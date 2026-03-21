using HotelFlow.Application.DTOs.Requests.Housekeeping;
using HotelFlow.Application.DTOs.Responses.Housekeeping;
using HotelFlow.Application.Interfaces;
using HotelFlow.Domain.Entities;
using HotelFlow.Domain.Enums;
using HotelFlow.Domain.Exceptions;
using HotelFlow.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HotelFlow.Application.Services;

public class HousekeepingService : IHousekeepingService
{
    private readonly HotelFlowDbContext _context;
    private readonly ILogger<HousekeepingService> _logger;

    public HousekeepingService(
        HotelFlowDbContext context,
        ILogger<HousekeepingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HousekeepingTaskResponse> CreateTaskAsync(CreateHousekeepingTaskRequest request, Guid createdBy)
    {
        // Pronađi sobu po broju sobe
        var room = await _context.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.RoomNumber == request.RoomNumber);

        if (room == null)
            throw new NotFoundException($"Room with number {request.RoomNumber} not found");

        // Proveri da li već postoji pending task za ovu sobu
        var existingActiveTask = await _context.HousekeepingTasks
            .FirstOrDefaultAsync(t =>
                t.RoomId == room.Id &&
                (t.Status == HousekeepingTaskStatus.Pending ||
                 t.Status == HousekeepingTaskStatus.InProgress) &&
                 t.Type == (HousekeepingTaskType)Enum.Parse(typeof(HousekeepingTaskType), request.TaskType));

        if (existingActiveTask != null)
            throw new BadRequestException(
                $"Active {request.TaskType} task already exists for room {room.RoomNumber}. " +
                $"Task ID: {existingActiveTask.Id}, Status: {existingActiveTask.Status}");

        var taskType = (HousekeepingTaskType)Enum.Parse(typeof(HousekeepingTaskType), request.TaskType);
        var deadline = request.Deadline ?? DateTime.UtcNow.AddHours(2);

        if (request.AssignedToUserId.HasValue)
        {
            var user = await _context.Users.FindAsync(request.AssignedToUserId.Value);
            if (user == null)
                throw new BadRequestException($"User with ID {request.AssignedToUserId} not found");

            if (user.Role != UserRole.Housekeeping)
                throw new BadRequestException("Task can only be assigned to housekeeping staff");
        }

        var task = new HousekeepingTask(
            room.Id,
            request.AssignedToUserId,
            taskType,
            deadline,
            request.Description
        );

        room.CheckOut();

        _context.HousekeepingTasks.Add(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Housekeeping task {task.Id} created for room {room.RoomNumber}");

        return await MapToResponseAsync(task);
    }

    public async Task<HousekeepingTaskResponse> AssignTaskAsync(Guid taskId, Guid assignedToUserId, Guid assignedBy)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new NotFoundException("Task not found");

        // Proveri da li je korisnik housekeeping
        var user = await _context.Users.FindAsync(assignedToUserId);
        if (user == null || user.Role != UserRole.Housekeeping)
            throw new Exception("User is not a housekeeper");

        task.Reassign(assignedToUserId);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Task {taskId} assigned to user {assignedToUserId}");

        return await MapToResponseAsync(task);
    }

    public async Task<HousekeepingTaskResponse> TakeTaskAsync(Guid taskId, Guid housekeeperId)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new NotFoundException("Task not found");

        // Proveri da li je task slobodan ili već dodeljen ovom korisniku
        if (task.AssignedToUserId != null && task.AssignedToUserId != housekeeperId)
            throw new BadRequestException("Task is already assigned to another housekeeper");

        // Proveri da li je korisnik housekeeping
        var user = await _context.Users.FindAsync(housekeeperId);
        if (user == null || user.Role != UserRole.Housekeeping)
            throw new Exception("User is not a housekeeper");

        task.Reassign(housekeeperId);
        task.Start();
        task.Room.ChangeStatus(RoomStatus.Cleaning);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Task {taskId} taken by housekeeper {housekeeperId}");

        return await MapToResponseAsync(task);
    }

    public async Task<IEnumerable<HousekeepingTaskResponse>> GetMyTasksAsync(Guid housekeeperId)
    {
        var tasks = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .Where(t => t.AssignedToUserId == housekeeperId)
            .OrderBy(t => t.Deadline)
            .ToListAsync();

        var responses = new List<HousekeepingTaskResponse>();
        foreach (var task in tasks)
        {
            responses.Add(await MapToResponseAsync(task));
        }

        return responses;
    }

    public async Task<IEnumerable<HousekeepingTaskResponse>> GetTodayTasksAsync(Guid? housekeeperId = null)
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var query = _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .Where(t => t.Deadline >= today && t.Deadline < tomorrow &&
                       t.Status != HousekeepingTaskStatus.Completed &&
                       t.Status != HousekeepingTaskStatus.Cancelled);

        if (housekeeperId.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == housekeeperId.Value);
        }

        var tasks = await query
            .OrderBy(t => t.Deadline)
            .ToListAsync();

        var responses = new List<HousekeepingTaskResponse>();
        foreach (var task in tasks)
        {
            responses.Add(await MapToResponseAsync(task));
        }

        return responses;
    }

    public async Task<IEnumerable<HousekeepingTaskResponse>> GetAvailableTasksAsync()
    {
        var tasks = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .Where(t => (t.Status == HousekeepingTaskStatus.Pending &&
                   t.AssignedToUserId == null))
            .OrderBy(t => t.Deadline)
            .ToListAsync();

        var responses = new List<HousekeepingTaskResponse>();
        foreach (var task in tasks)
        {
            responses.Add(await MapToResponseAsync(task));
        }

        return responses;
    }

    public async Task<HousekeepingTaskResponse> UpdateTaskStatusAsync(Guid taskId, string status, string? notes, Guid updatedBy)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new NotFoundException("Task not found");

        var taskStatus = (HousekeepingTaskStatus)Enum.Parse(typeof(HousekeepingTaskStatus), status);

        switch (taskStatus)
        {
            case HousekeepingTaskStatus.InProgress:
                task.Start();
                break;
            case HousekeepingTaskStatus.Completed:

                if (task.Type == HousekeepingTaskType.Inspection)
                    throw new Exception(
                        "Inspection tasks cannot be completed via this method");

                task.Complete(notes);

                // Ažuriranje statusa sobe na osnovu tipa zadatka
                UpdateRoomStatusBasedOnTaskType(task);
                break;
            case HousekeepingTaskStatus.Cancelled:
                
                await HandleTaskCancellationAsync(task, notes, updatedBy);
                break;
            default:
                throw new Exception($"Invalid status transition to {status}");
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Task {taskId} status updated to {status} by user {updatedBy}");

        return await MapToResponseAsync(task);
    }

    private void UpdateRoomStatusBasedOnTaskType(HousekeepingTask task){
    if (task.Room == null) return;

    bool shouldMarkAvailable =
        task.Room.Status == RoomStatus.NeedsCleaning ||
        task.Room.Status == RoomStatus.OutOfService ||
        task.Room.Status == RoomStatus.Cleaning;

    switch (task.Type){
        case HousekeepingTaskType.Cleaning:
        case HousekeepingTaskType.Maintenance:
        case HousekeepingTaskType.Restocking:
        case HousekeepingTaskType.Setup:
            if (shouldMarkAvailable)
                task.Room.MarkAsCleaned();
            break;

        case HousekeepingTaskType.Inspection:
            // Inspection is handled separately via CompleteInspectionTaskAsync
            break;

        default:
            _logger.LogWarning($"Unknown task type: {task.Type} for room {task.Room.RoomNumber}");
            break;
        }
    }

    public async Task<HousekeepingTaskResponse> CompleteTaskAsync(Guid taskId, string? notes, Guid completedBy)
    {
        return await UpdateTaskStatusAsync(taskId, HousekeepingTaskStatus.Completed.ToString(), notes, completedBy);
    }

    public async Task GenerateCleaningTasksForDirtyRoomsAsync()
    {
        var dirtyRooms = await _context.Rooms
            .Where(r => r.Status == RoomStatus.NeedsCleaning)
            .ToListAsync();

        foreach (var room in dirtyRooms)
        {
            // Proveri da li već postoji pending cleaning task za ovu sobu
            var existingTask = await _context.HousekeepingTasks
                .FirstOrDefaultAsync(t =>
                    t.RoomId == room.Id &&
                    t.Status == HousekeepingTaskStatus.Pending &&
                    t.Type == HousekeepingTaskType.Cleaning);

            if (existingTask == null)
            {
                var task = new HousekeepingTask(
                    room.Id,
                    null,
                    HousekeepingTaskType.Cleaning,
                    DateTime.UtcNow.AddHours(2),
                    "Automatic cleaning task for dirty room"
                );

                _context.HousekeepingTasks.Add(task);
                _logger.LogInformation($"Generated cleaning task for room {room.RoomNumber}");
            }
        }

        await _context.SaveChangesAsync();
    }

    // Pomocne metode
    private async Task<HousekeepingTaskResponse> MapToResponseAsync(HousekeepingTask task)
    {
        var room = await _context.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == task.RoomId);

        var assignedToUser = task.AssignedToUserId != Guid.Empty
            ? await _context.Users.FindAsync(task.AssignedToUserId)
            : null;

        return new HousekeepingTaskResponse
        {
            Id = task.Id,
            RoomId = task.RoomId,
            RoomNumber = room?.RoomNumber ?? "Unknown",
            RoomType = room?.RoomType?.Name ?? "Unknown",
            AssignedToUserId = task.AssignedToUserId,
            AssignedToUser = assignedToUser?.Email ?? "Unassigned",
            TaskType = task.Type.ToString(),
            Status = task.Status.ToString(),
            Description = task.Description,
            CompletedAt = task.CompletedAt,
            Deadline = task.Deadline,
            Notes = task.Notes,
            CreatedAt = task.CreatedAt
        };
    }

    // Implementacija ostalih metoda iz interfejsa...
    public async Task<HousekeepingTaskResponse> GetTaskAsync(Guid taskId)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new NotFoundException("Task not found");

        return await MapToResponseAsync(task);
    }

    public async Task<IEnumerable<HousekeepingTaskResponse>> GetAllTasksAsync()
    {
        var tasks = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var responses = new List<HousekeepingTaskResponse>();
        foreach (var task in tasks)
        {
            responses.Add(await MapToResponseAsync(task));
        }

        return responses;
    }

    public async Task<IEnumerable<HousekeepingTaskResponse>> GetTasksByStatusAsync(string status)
    {
        var taskStatus = (HousekeepingTaskStatus)Enum.Parse(typeof(HousekeepingTaskStatus), status);

        var tasks = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .Where(t => t.Status == taskStatus)
            .OrderBy(t => t.Deadline)
            .ToListAsync();

        var responses = new List<HousekeepingTaskResponse>();
        foreach (var task in tasks)
        {
            responses.Add(await MapToResponseAsync(task));
        }

        return responses;
    }

    public async Task<IEnumerable<HousekeepingTaskResponse>> GetTasksByRoomNumberAsync(string roomNumber)
    {
        // Pronađi sobu po broju
        var room = await _context.Rooms
            .FirstOrDefaultAsync(r => r.RoomNumber == roomNumber);

        if (room == null)
            throw new NotFoundException($"Room with number {roomNumber} not found");

        // Pronađi sve zadatke za tu sobu
        var tasks = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .Where(t => t.RoomId == room.Id)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        var responses = new List<HousekeepingTaskResponse>();
        foreach (var task in tasks)
        {
            responses.Add(await MapToResponseAsync(task));
        }

        return responses;
    }

    public async Task<HousekeepingTaskResponse> CompleteInspectionTaskAsync(Guid taskId, CompleteInspectionRequest request, Guid completedBy)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new NotFoundException("Task not found");

        if (task.Type != HousekeepingTaskType.Inspection)
            throw new Exception("Only inspection tasks can be completed with this method");

        // Provera RoomStatus
        if (!Enum.TryParse<RoomStatus>(request.RoomStatus, out var roomStatus))
            throw new BadRequestException($"Invalid room status: {request.RoomStatus}");

        // Provera da li je task u toku
        if (task.Status != HousekeepingTaskStatus.InProgress)
            throw new Exception("Only tasks in progress can be completed");

        // Ažuriraj task
        task.Complete(request.Notes);
        task.Room.ChangeStatus(roomStatus);

        // Ako je naveden nextTaskType, kreiraj novi zadatak
        if (!string.IsNullOrEmpty(request.NextTaskType))
        {
            if (!Enum.TryParse<HousekeepingTaskType>(request.NextTaskType, out var nextTaskType))
                throw new Exception($"Invalid task type: {request.NextTaskType}");

            if (nextTaskType == HousekeepingTaskType.Inspection)
                throw new Exception(
                    "Inspection task cannot generate another inspection task");

            // Provera da li već postoji pending task istog tipa za ovu sobu
            var existingActiveTask = await _context.HousekeepingTasks
                .FirstOrDefaultAsync(t =>
                    t.RoomId == task.RoomId &&
                    (t.Status == HousekeepingTaskStatus.Pending ||
                     t.Status == HousekeepingTaskStatus.InProgress) &&
                     t.Type == nextTaskType);

            if (existingActiveTask == null)
            {
                var newTask = new HousekeepingTask(
                    task.RoomId,
                    null, // nije dodeljeno
                    nextTaskType,
                    DateTime.UtcNow.AddHours(2), // podrazumevani rok
                    $"Auto-generated {request.NextTaskType} task after inspection"
                );

                _context.HousekeepingTasks.Add(newTask);
                _logger.LogInformation($"Generated {nextTaskType} task for room {task.Room.RoomNumber} after inspection");
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Inspection task {taskId} completed by user {completedBy}. Room status set to {request.RoomStatus}");

        return await MapToResponseAsync(task);
    }

    public async Task HandleExpiredTasksAsync()
    {
        var now = DateTime.UtcNow;

        // Naci sve zadatke ciji rok je istekao, a nisu zavrseni ili otkazani
        var expiredTasks = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Where(t => t.Deadline < now &&
                       t.Status != HousekeepingTaskStatus.Completed &&
                       t.Status != HousekeepingTaskStatus.Cancelled &&
                       t.Status != HousekeepingTaskStatus.Failed)
            .ToListAsync();

        foreach (var task in expiredTasks)
        {
            _logger.LogInformation($"Task {task.Id} for room {task.Room?.RoomNumber} has expired. Marking as Failed.");

            // Pznaci stari zadatak Failed
            task.Fail($"Task expired at {task.Deadline}. Automatically marked as failed and new task created.");

            // Proveri da li vec postoji aktivan zadatak istog tipa za ovu sobu 
            var existingActiveTask = await _context.HousekeepingTasks
                .FirstOrDefaultAsync(t =>
                    t.RoomId == task.RoomId &&
                    (t.Status == HousekeepingTaskStatus.Pending ||
                     t.Status == HousekeepingTaskStatus.InProgress) &&
                    t.Type == task.Type &&
                    t.Id != task.Id);

            // Ako n epostoji aktivan zadatak kreiraj novi
            if (existingActiveTask == null)
            {
                var newTask = new HousekeepingTask(
                    task.RoomId,
                    null,
                    task.Type,
                    DateTime.UtcNow.AddHours(2),
                    $"Auto-recreated task after previous one expired. Original: {task.Description}"
                );

                _context.HousekeepingTasks.Add(newTask);
                _logger.LogInformation($"New task {newTask.Id} created for room {task.Room?.RoomNumber} after expiration of task {task.Id}");
            }
            else
            {
                _logger.LogInformation($"Active task {existingActiveTask.Id} already exists for room {task.Room?.RoomNumber}. Skipping creation of new task.");
            }
        }

        await _context.SaveChangesAsync();
    }

    private async Task<HousekeepingTask> HandleTaskCancellationAsync(HousekeepingTask task, string? reason, Guid cancelledBy)
    {
        
        task.Cancel(reason);


        if (ShouldRecreateTaskAfterCancellation(task))
        {
            await RecreateTaskAsync(task, $"Task cancelled by user {cancelledBy}. Reason: {reason}");
        }

        return task;
    }

    private bool ShouldRecreateTaskAfterCancellation(HousekeepingTask task)
    {
        
        switch (task.Type)
        {
            case HousekeepingTaskType.Cleaning:
                return task.Room.Status == RoomStatus.NeedsCleaning;

            case HousekeepingTaskType.Maintenance:
                return task.Room.Status == RoomStatus.OutOfService;

            case HousekeepingTaskType.Restocking:
            case HousekeepingTaskType.Setup:
                return task.Room.Status == RoomStatus.NeedsCleaning;

            case HousekeepingTaskType.Inspection:

                return false;

            default:
                return false;
        }
    }

    private async Task<HousekeepingTask?> RecreateTaskAsync(HousekeepingTask originalTask, string reason)
    {
  
        var existingActiveTask = await _context.HousekeepingTasks
            .FirstOrDefaultAsync(t =>
                t.RoomId == originalTask.RoomId &&
                (t.Status == HousekeepingTaskStatus.Pending ||
                 t.Status == HousekeepingTaskStatus.InProgress) &&
                t.Type == originalTask.Type &&
                t.Id != originalTask.Id);

        if (existingActiveTask != null)
        {
            _logger.LogInformation($"Active task {existingActiveTask.Id} already exists for room {originalTask.Room.RoomNumber}. Skipping recreation.");
            return null;
        }


        var newTask = new HousekeepingTask(
            originalTask.RoomId,
            null, 
            originalTask.Type,
            DateTime.UtcNow.AddHours(2), 
            $"Auto-recreated task after cancellation. Original: {originalTask.Description}. Reason: {reason}"
        );

        _context.HousekeepingTasks.Add(newTask);
        _logger.LogInformation($"Recreated task {newTask.Id} for room {originalTask.Room.RoomNumber} after cancellation of task {originalTask.Id}");

        return newTask;
    }

    public async Task<HousekeepingTaskResponse> CancelTaskWithoutRecreationAsync(Guid taskId, string? reason, Guid cancelledBy)
    {
        var task = await _context.HousekeepingTasks
            .Include(t => t.Room)
            .ThenInclude(r => r.RoomType)
            .Include(t => t.AssignedToUser)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new NotFoundException("Task not found");

        task.Cancel(reason);
        UpdateRoomStatusBasedOnTaskType(task);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Task {taskId} cancelled without recreation by user {cancelledBy}. Reason: {reason}");

        return await MapToResponseAsync(task);
    }

    public async Task<IEnumerable<HousekeeperResponse>> GetHousekeepersAsync(bool includeStats = false)
    {
        var query = _context.Users
            .Include(u => u.Profile)
            .Where(u => u.Role == UserRole.Housekeeping && !u.IsDeleted)
            .OrderBy(u => u.Profile != null ? u.Profile.LastName : u.Email);

        var housekeepers = await query.ToListAsync();
        var responses = new List<HousekeeperResponse>();

        foreach (var housekeeper in housekeepers)
        {
            var response = new HousekeeperResponse
            {
                Id = housekeeper.Id,
                Email = housekeeper.Email,
                FirstName = housekeeper.Profile?.FirstName ?? string.Empty,
                LastName = housekeeper.Profile?.LastName ?? string.Empty,
                Phone = housekeeper.Profile?.Phone ?? string.Empty,
                CreatedAt = housekeeper.CreatedAt
            };

            if (includeStats)
            {
                // Broj aktivnih taskova
                response.ActiveTasksCount = await _context.HousekeepingTasks
                    .CountAsync(t => t.AssignedToUserId == housekeeper.Id &&
                                    t.Status != HousekeepingTaskStatus.Completed &&
                                    t.Status != HousekeepingTaskStatus.Cancelled);

                // Broj završenih taskova danas
                var today = DateTime.Today;
                response.CompletedTasksToday = await _context.HousekeepingTasks
                    .CountAsync(t => t.AssignedToUserId == housekeeper.Id &&
                                    t.Status == HousekeepingTaskStatus.Completed &&
                                    t.CompletedAt.HasValue &&
                                    t.CompletedAt.Value.Date == today);
            }

            responses.Add(response);
        }

        return responses;
    }

 
    public async Task<HousekeeperResponse?> GetHousekeeperAsync(Guid housekeeperId, bool includeStats = false)
    {
        var housekeeper = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == housekeeperId &&
                                     u.Role == UserRole.Housekeeping &&
                                     !u.IsDeleted);

        if (housekeeper == null)
            return null;

        var response = new HousekeeperResponse
        {
            Id = housekeeper.Id,
            Email = housekeeper.Email,
            FirstName = housekeeper.Profile?.FirstName ?? string.Empty,
            LastName = housekeeper.Profile?.LastName ?? string.Empty,
            Phone = housekeeper.Profile?.Phone ?? string.Empty,
            CreatedAt = housekeeper.CreatedAt
        };

        if (includeStats)
        {
            response.ActiveTasksCount = await _context.HousekeepingTasks
                .CountAsync(t => t.AssignedToUserId == housekeeper.Id &&
                                t.Status != HousekeepingTaskStatus.Completed &&
                                t.Status != HousekeepingTaskStatus.Cancelled);

            var todayUtc = DateTime.UtcNow.Date;
            var tomorrowUtc = todayUtc.AddDays(1);
            response.CompletedTasksToday = await _context.HousekeepingTasks
                .CountAsync(t => t.AssignedToUserId == housekeeper.Id &&
                                t.Status == HousekeepingTaskStatus.Completed &&
                                t.CompletedAt >= todayUtc &&
                                t.CompletedAt < tomorrowUtc);
        }

        return response;
    }

    public async Task<IEnumerable<HousekeeperResponse>> GetAvailableHousekeepersAsync()
    {
        // Housekeeper-i koji imaju manje od 3 aktivna taska
        var housekeepers = await GetHousekeepersAsync(true);

        return housekeepers
            .Where(h => h.ActiveTasksCount < 3) 
            .OrderBy(h => h.ActiveTasksCount)
            .ThenBy(h => h.LastName);
    }

    public async Task<IEnumerable<HousekeeperResponse>> GetHousekeepersByPerformanceAsync(DateTime fromDate, DateTime toDate)
    {
        var housekeepers = await GetHousekeepersAsync(true);
        var responses = new List<HousekeeperResponse>();

        foreach (var housekeeper in housekeepers)
        {
            var completedTasksCount = await _context.HousekeepingTasks
                .CountAsync(t => t.AssignedToUserId == housekeeper.Id &&
                                t.Status == HousekeepingTaskStatus.Completed &&
                                t.CompletedAt.HasValue &&
                                t.CompletedAt.Value >= fromDate &&
                                t.CompletedAt.Value <= toDate);

            var response = new HousekeeperResponse
            {
                Id = housekeeper.Id,
                Email = housekeeper.Email,
                FirstName = housekeeper.FirstName,
                LastName = housekeeper.LastName,
                Phone = housekeeper.Phone,
                CreatedAt = housekeeper.CreatedAt,
                ActiveTasksCount = housekeeper.ActiveTasksCount,
                CompletedTasksToday = completedTasksCount 
            };

            responses.Add(response);
        }

        return responses.OrderByDescending(h => h.CompletedTasksToday);
    }
}