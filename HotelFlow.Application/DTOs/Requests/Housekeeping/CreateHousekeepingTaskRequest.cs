namespace HotelFlow.Application.DTOs.Requests.Housekeeping;

public class CreateHousekeepingTaskRequest
{
    public string RoomNumber { get; set; } = string.Empty;
    public Guid? AssignedToUserId { get; set; }
    public string TaskType { get; set; } = "Cleaning";
    public string Description { get; set; } = string.Empty;
    public DateTime? Deadline { get; set; }
}