using HotelFlow.Application.DTOs.Requests.Housekeeping;
using HotelFlow.Application.DTOs.Responses.Housekeeping;
using HotelFlow.Domain.Entities;

namespace HotelFlow.Application.Interfaces;

public interface IHousekeepingService
{
    // Osnovne operacije
    Task<HousekeepingTaskResponse> CreateTaskAsync(CreateHousekeepingTaskRequest request, Guid createdBy);
    Task<HousekeepingTaskResponse> GetTaskAsync(Guid taskId);
    Task<IEnumerable<HousekeepingTaskResponse>> GetAllTasksAsync();
    Task<IEnumerable<HousekeepingTaskResponse>> GetTasksByStatusAsync(string status);
    Task<IEnumerable<HousekeepingTaskResponse>> GetTasksByRoomNumberAsync(string RoomNumber);

    // Dodela i upravljanje zadacima
    Task<HousekeepingTaskResponse> AssignTaskAsync(Guid taskId, Guid assignedToUserId, Guid assignedBy);
    Task<HousekeepingTaskResponse> TakeTaskAsync(Guid taskId, Guid housekeeperId);
    Task<HousekeepingTaskResponse> UpdateTaskStatusAsync(Guid taskId, string status, string? notes, Guid updatedBy);
    Task<HousekeepingTaskResponse> CompleteTaskAsync(Guid taskId, string? notes, Guid completedBy);

    // Za housekeeping osoblje
    Task<IEnumerable<HousekeepingTaskResponse>> GetMyTasksAsync(Guid housekeeperId);
    Task<IEnumerable<HousekeepingTaskResponse>> GetAvailableTasksAsync();
    Task<IEnumerable<HousekeepingTaskResponse>> GetTodayTasksAsync(Guid? housekeeperId = null);

    // Automatsko generisanje zadataka
    Task GenerateCleaningTasksForDirtyRoomsAsync();

    Task<HousekeepingTaskResponse> CompleteInspectionTaskAsync(Guid taskId, CompleteInspectionRequest request, Guid completedBy);

    Task HandleExpiredTasksAsync();
    Task<HousekeepingTaskResponse> CancelTaskWithoutRecreationAsync(Guid taskId, string? reason, Guid cancelledBy);

    Task<IEnumerable<HousekeeperResponse>> GetHousekeepersAsync(bool includeStats = false);
    Task<HousekeeperResponse?> GetHousekeeperAsync(Guid housekeeperId, bool includeStats = false);
    Task<IEnumerable<HousekeeperResponse>> GetAvailableHousekeepersAsync();
    Task<IEnumerable<HousekeeperResponse>> GetHousekeepersByPerformanceAsync(DateTime fromDate, DateTime toDate);

}