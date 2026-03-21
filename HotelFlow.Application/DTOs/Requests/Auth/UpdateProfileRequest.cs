namespace HotelFlow.Application.DTOs.Requests.Auth;

public class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    public string? Gender { get; set; }
    public string? Address { get; set; }
    public string? ProfilePicture { get; set; }

}