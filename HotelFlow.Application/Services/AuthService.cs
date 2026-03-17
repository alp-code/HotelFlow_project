using BCrypt.Net;
using HotelFlow.Application.DTOs.Requests.Auth;
using HotelFlow.Application.DTOs.Responses.Auth;
using HotelFlow.Application.Interfaces;
using HotelFlow.Domain.Entities;
using HotelFlow.Domain.Enums;
using HotelFlow.Domain.Exceptions;
using HotelFlow.Infrastructure.Data.DbContext;
using HotelFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Security.Cryptography;

namespace HotelFlow.Application.Services;

public class AuthService : IAuthService
{
    private readonly HotelFlowDbContext _context;
    private readonly JwtTokenGenerator _jwtGenerator;

    public AuthService(
        HotelFlowDbContext context,
        JwtTokenGenerator jwtGenerator)
    {
        _context = context;
        _jwtGenerator = jwtGenerator;
    }

    // ================= REGISTER =================
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.Email);

        if (emailExists)
            throw new BadRequestException("Email already in use");

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phoneExists = await _context.UserProfiles
                .AnyAsync(up => up.Phone == request.Phone);

            if (phoneExists)
                throw new BadRequestException("Phone number already in use");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User(
            request.Email,
            passwordHash,
            UserRole.Guest
        );

        user.CreateProfile(
            request.FirstName,
            request.LastName,
            request.Phone
        );

        var refreshToken = user.CreateRefreshToken(
            GenerateRandomToken(),
            DateTime.UtcNow.AddDays(7)
        );

        _context.Users.Add(user);
        _context.RefreshTokens.Add(refreshToken);

        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _jwtGenerator.GenerateAccessToken(user),
            RefreshToken = refreshToken.Token
        };
    }

    // ================= LOGIN =================
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

        if (user == null)
            throw new UnauthorizedException("Invalid credentials");

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!passwordValid)
            throw new UnauthorizedException("Invalid credentials");

        foreach (var token in user.RefreshTokens.Where(rt => !rt.IsRevoked))
        {
            token.Revoke();
        }

        var refreshToken = user.CreateRefreshToken(
            GenerateRandomToken(),
            DateTime.UtcNow.AddDays(7)
        );

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _jwtGenerator.GenerateAccessToken(user),
            RefreshToken = refreshToken.Token
        };
    }

    // ================= REFRESH =================
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.RefreshTokens)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

        if (token == null || token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            throw new UnauthorizedException("Invalid refresh token");

        token.Revoke();

        var newRefreshToken = token.User.CreateRefreshToken(
            GenerateRandomToken(),
            DateTime.UtcNow.AddDays(7)
        );

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = _jwtGenerator.GenerateAccessToken(token.User),
            RefreshToken = newRefreshToken.Token
        };
    }

    // ================= HELPERS =================
    private static string GenerateRandomToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(randomBytes);
    }
}
