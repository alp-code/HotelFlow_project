using HotelFlow.Domain.Enums;

public class ChangeUserRoleRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}
