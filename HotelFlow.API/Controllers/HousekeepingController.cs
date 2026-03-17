using HotelFlow.Application.DTOs.Requests.Housekeeping;
using HotelFlow.Application.Interfaces;
using HotelFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HotelFlow.API.Controllers;

[ApiController]
[Route("api/housekeeping")]
[Authorize(Roles = "Staff,Housekeeping")] 
public class HousekeepingController : ControllerBase
{
    private readonly IHousekeepingService _housekeepingService;
    private readonly ILogger<HousekeepingController> _logger;

    public HousekeepingController(
        IHousekeepingService housekeepingService,
        ILogger<HousekeepingController> logger)
    {
        _housekeepingService = housekeepingService;
        _logger = logger;
    }

    // 🔹 GET: /api/housekeeping/today-tasks (Današnji zadaci)
    [HttpGet("today-tasks")]
    public async Task<IActionResult> GetTodayTasks()
    {
        var userId = GetUserIdFromToken();
        var tasks = await _housekeepingService.GetTodayTasksAsync(userId);
        return Ok(tasks);
    }

    // 🔹 POST: /api/housekeeping/tasks - Kreiranje novog zadatka
    [HttpPost("tasks")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> CreateTask([FromBody] CreateHousekeepingTaskRequest request)
    {
        var createdBy = GetUserIdFromToken();
        var task = await _housekeepingService.CreateTaskAsync(request, createdBy);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    // 🔹 GET: /api/housekeeping/tasks/{id} - Dobijanje određenog zadatka
    [HttpGet("tasks/{id}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var task = await _housekeepingService.GetTaskAsync(id);
        return Ok(task);
    }

    // 🔹 GET: /api/housekeeping/tasks - Svi zadaci
    [HttpGet("tasks")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetAllTasks()
    {
        var tasks = await _housekeepingService.GetAllTasksAsync();
        return Ok(tasks);
    }

    // 🔹 GET: /api/housekeeping/tasks/available - Slobodni zadaci
    [HttpGet("tasks/available")]
    [Authorize(Roles = "Housekeeping,Staff")]
    public async Task<IActionResult> GetAvailableTasks()
    {
        var tasks = await _housekeepingService.GetAvailableTasksAsync();
        return Ok(tasks);
    }

    // 🔹 GET: /api/housekeeping/tasks/my-tasks - Moji zadaci
    [HttpGet("tasks/my-tasks")]
    [Authorize(Roles = "Housekeeping")]
    public async Task<IActionResult> GetMyTasks()
    {
        var housekeeperId = GetUserIdFromToken();
        var tasks = await _housekeepingService.GetMyTasksAsync(housekeeperId);
        return Ok(tasks);
    }

    // 🔹 POST: /api/housekeeping/tasks/{taskId}/take - Preuzimanje zadatka
    [HttpPost("tasks/{taskId}/take")]
    [Authorize(Roles = "Housekeeping")]
    public async Task<IActionResult> TakeTask(Guid taskId)
    {
        var housekeeperId = GetUserIdFromToken();
        var task = await _housekeepingService.TakeTaskAsync(taskId, housekeeperId);
        return Ok(task);
    }

    // 🔹 POST: /api/housekeeping/tasks/{taskId}/assign - Dodeljivanje zadatka
    [HttpPost("tasks/{taskId}/assign")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> AssignTask(Guid taskId, [FromBody] AssignTaskRequest request)
    {
        var assignedBy = GetUserIdFromToken();
        var task = await _housekeepingService.AssignTaskAsync(taskId, request.AssignedToUserId, assignedBy);
        return Ok(task);
    }

    // 🔹 PUT: /api/housekeeping/tasks/{taskId}/status - Ažuriranje statusa zadatka
    [HttpPut("tasks/{taskId}/status")]
    [Authorize(Roles = "Housekeeping,Staff")]
    public async Task<IActionResult> UpdateTaskStatus(Guid taskId, [FromBody] UpdateTaskStatusRequest request)
    {
        var updatedBy = GetUserIdFromToken();
        var task = await _housekeepingService.UpdateTaskStatusAsync(taskId, request.Status, request.Notes, updatedBy);
        return Ok(task);
    }

    // 🔹 POST: /api/housekeeping/tasks/{taskId}/complete - Završavanje zadatka
    [HttpPost("tasks/{taskId}/complete")]
    [Authorize(Roles = "Housekeeping")]
    public async Task<IActionResult> CompleteTask(Guid taskId, [FromBody] string? notes = null)
    {
        var completedBy = GetUserIdFromToken();
        var task = await _housekeepingService.CompleteTaskAsync(taskId, notes, completedBy);
        return Ok(task);
    }

    // 🔹 POST: /api/housekeeping/generate-tasks - Generisanje zadataka za prljave sobe
    [HttpPost("generate-tasks")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GenerateTasks()
    {
        await _housekeepingService.GenerateCleaningTasksForDirtyRoomsAsync();
        return Ok(new { message = "Tasks generated successfully" });
    }

    // 🔹 POST: /api/housekeeping/tasks/{taskId}/complete-inspection - Završavanje inspekcije
    [HttpPost("tasks/{taskId}/complete-inspection")]
    [Authorize(Roles = "Housekeeping,Staff")]
    public async Task<IActionResult> CompleteInspection(Guid taskId, [FromBody] CompleteInspectionRequest request)
    {
        var completedBy = GetUserIdFromToken();
        var task = await _housekeepingService.CompleteInspectionTaskAsync(taskId, request, completedBy);
        return Ok(task);
    }

    // 🔹 GET: /api/housekeeping/tasks/room/{roomId} - Zadaci za određenu sobu
    [HttpGet("tasks/room/{roomId}")]
    public async Task<IActionResult> GetTasksByRoom(string RoomNumber)
    {
        var tasks = await _housekeepingService.GetTasksByRoomNumberAsync(RoomNumber);
        return Ok(tasks);
    }

    // 🔹 GET: /api/housekeeping/tasks/status/{status} - Zadaci po statusu
    [HttpGet("tasks/status/{status}")]
    public async Task<IActionResult> GetTasksByStatus(string status)
    {
        var tasks = await _housekeepingService.GetTasksByStatusAsync(status);
        return Ok(tasks);
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            throw new UnauthorizedException("User ID not found in token");

        return Guid.Parse(userIdClaim);
    }

    // 🔹 POST: /api/housekeeping/handle-expired-tasks - Rukovanje isteklim zadacima
    [HttpPost("handle-expired-tasks")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> HandleExpiredTasks()
    {
        await _housekeepingService.HandleExpiredTasksAsync();
        return Ok(new { message = "Expired tasks handled successfully" });
    }

    // 🔹 POST: /api/housekeeping/tasks/{taskId}/cancel-without-recreation - Otkazivanje zadataka bez rekreiranja
    [HttpPost("tasks/{taskId}/cancel-without-recreation")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> CancelTaskWithoutRecreation(Guid taskId, [FromBody] string? reason = null)
    {
        var cancelledBy = GetUserIdFromToken();
        var task = await _housekeepingService.CancelTaskWithoutRecreationAsync(taskId, reason, cancelledBy);
        return Ok(task);
    }

    // 🔹 NOVO: GET: /api/housekeeping/housekeepers - Svi housekeeperi
    [HttpGet("housekeepers")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetHousekeepers([FromQuery] bool includeStats = false)
    {
        var housekeepers = await _housekeepingService.GetHousekeepersAsync(includeStats);
        return Ok(housekeepers);
    }

    // 🔹 NOVO: GET: /api/housekeeping/housekeepers/{id} - Konkretan housekeeper
    [HttpGet("housekeepers/{id}")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetHousekeeper(Guid id, [FromQuery] bool includeStats = false)
    {
        var currentUserId = GetUserIdFromToken();
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Housekeeping može videti samo svoje podatke
        if (currentUserRole == "Housekeeping" && currentUserId != id)
        {
            return Forbid("Housekeeping can only view their own information");
        }

        var housekeeper = await _housekeepingService.GetHousekeeperAsync(id, includeStats);

        if (housekeeper == null)
            return NotFound(new { message = "Housekeeper not found" });

        return Ok(housekeeper);
    }

    // 🔹 NOVO: GET: /api/housekeeping/housekeepers/available - Dostupni housekeeperi
    [HttpGet("housekeepers/available")]
    [Authorize(Roles = "Staff,Housekeeping")]
    public async Task<IActionResult> GetAvailableHousekeepers()
    {
        var housekeepers = await _housekeepingService.GetAvailableHousekeepersAsync();
        return Ok(housekeepers);
    }

    // 🔹 NOVO: GET: /api/housekeeping/housekeepers/performance - Performanse housekeepera po periodu
    [HttpGet("housekeepers/performance")]
    [Authorize(Roles = "Staff")]
    public async Task<IActionResult> GetHousekeepersByPerformance(
        [FromQuery] DateTime fromDate,
        [FromQuery] DateTime toDate)
    {
        // Provera validnosti datuma
        if (fromDate > toDate)
        {
            return BadRequest(new { message = "FromDate cannot be after ToDate" });
        }

        var housekeepers = await _housekeepingService.GetHousekeepersByPerformanceAsync(fromDate, toDate);
        return Ok(housekeepers);
    }

    // 🔹 NOVO: GET: /api/housekeeping/housekeepers/me - Moji podaci kao housekeeper
    [HttpGet("housekeepers/me")]
    [Authorize(Roles = "Housekeeping")]
    public async Task<IActionResult> GetMyHousekeeperInfo([FromQuery] bool includeStats = false)
    {
        var housekeeperId = GetUserIdFromToken();
        var housekeeper = await _housekeepingService.GetHousekeeperAsync(housekeeperId, includeStats);

        if (housekeeper == null)
            return NotFound(new { message = "Housekeeper not found" });

        return Ok(housekeeper);
    }

    // 🔹 NOVO: GET: /api/housekeeping/housekeepers/stats - Statistika za housekeepera
    [HttpGet("housekeepers/{id}/stats")]
    [Authorize(Roles = "Staff,Housekeeping")]
    public async Task<IActionResult> GetHousekeeperStats(Guid id)
    {
        var currentUserId = GetUserIdFromToken();
        var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Housekeeping može videti samo svoje statistike
        if (currentUserRole == "Housekeeping" && currentUserId != id)
        {
            return Forbid("Housekeeping can only view their own statistics");
        }

        var housekeeper = await _housekeepingService.GetHousekeeperAsync(id, true);

        if (housekeeper == null)
            return NotFound(new { message = "Housekeeper not found" });

        var stats = new
        {
            housekeeper.ActiveTasksCount,
            housekeeper.CompletedTasksToday,
            housekeeper.Id,
            housekeeper.FullName
        };

        return Ok(stats);
    }
}