using HotelFlow.Application.DTOs.Requests.Auth;
using HotelFlow.Application.DTOs.Responses.Auth;

namespace HotelFlow.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}

