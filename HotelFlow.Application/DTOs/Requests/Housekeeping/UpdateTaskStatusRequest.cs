namespace HotelFlow.Application.DTOs.Requests.Housekeeping;

public class UpdateTaskStatusRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}