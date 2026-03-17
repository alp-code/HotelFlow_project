using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Application.DTOs.Responses.Auth;
public class UserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public UserProfileResponse? Profile { get; set; }
}

