namespace HotelFlow.Application.DTOs.Requests.Housekeeping;

public class CompleteInspectionRequest
{
    public string RoomStatus { get; set; } = string.Empty; // Available, NeedsCleaning, OutOfService
    public string? NextTaskType { get; set; } // Cleaning, Maintenance, Restocking, Setup
    public string? Notes { get; set; }
}