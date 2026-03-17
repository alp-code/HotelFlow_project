namespace HotelFlow.Application.DTOs.Responses.Housekeeping;

public class HousekeepingTaskResponse
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public Guid? AssignedToUserId { get; set; }
    public string AssignedToUser { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public DateTime Deadline { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}