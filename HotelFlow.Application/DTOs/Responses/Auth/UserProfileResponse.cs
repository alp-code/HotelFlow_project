using System;
using System.Collections.Generic;
using System.Text;

namespace HotelFlow.Application.DTOs.Responses.Auth;
public class UserProfileResponse
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}