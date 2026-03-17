namespace HotelFlow.Application.DTOs.Responses.Auth;

public class UserIdResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}