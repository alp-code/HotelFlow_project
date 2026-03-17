
namespace HotelFlow.Application.DTOs.Responses.Housekeeping;

public class HousekeeperResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    // Statistika
    public int ActiveTasksCount { get; set; }
    public int CompletedTasksToday { get; set; }

}