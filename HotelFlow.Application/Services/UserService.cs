using HotelFlow.Application.DTOs.Responses.Auth;
using HotelFlow.Domain.Enums;
using HotelFlow.Domain.Exceptions;
using HotelFlow.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using HotelFlow.Application.DTOs.Requests.Auth;
namespace HotelFlow.Application.Services;

public class UserService : IUserService
{
    private readonly HotelFlowDbContext _context;

    public UserService(HotelFlowDbContext context)
    {
        _context = context;
    }

    public async Task ChangeRoleAsync(ChangeUserRoleRequest request)
    {
        var user = await _context.Users.FindAsync(request.UserId);

        if (user == null)
            throw new NotFoundException("User not found");

        if (!Enum.TryParse<UserRole>(request.Role, true, out var userRole))
        {
            throw new BadRequestException($"Invalid role: {request.Role}. Valid values are: {string.Join(", ", Enum.GetNames(typeof(UserRole)))}");
        }

        user.ChangeRole(userRole);

        await _context.SaveChangesAsync();
    }
    public async Task<UserIdResponse> GetUserIdByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email cannot be empty");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user == null)
            throw new NotFoundException($"User with email '{email}' not found");

        return new UserIdResponse
        {
            UserId = user.Id,
            Email = user.Email
        };
    }
    // ADMIN DELETE
    public async Task DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
            throw new NotFoundException("User not found");

        user.SoftDelete();
        await _context.SaveChangesAsync();
    }

    // GUEST DELETE OWN ACCOUNT
    public async Task DeleteOwnAccountAsync(Guid userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new NotFoundException("User not found");

        if (user.Role != UserRole.Guest)
            throw new ForbiddenException("Only guests can delete their account");

        user.SoftDelete();
        await _context.SaveChangesAsync();
    }

    // ADMIN RESTORE
    public async Task RestoreUserAsync(Guid userId)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsDeleted);

        if (user == null)
            throw new NotFoundException("Deleted user not found");

        user.Restore();
        await _context.SaveChangesAsync();
    }
    public async Task<Guid> GetDeletedUserIdByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new BadRequestException("Email cannot be empty");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && u.IsDeleted);

        if (user == null)
            throw new NotFoundException($"Deleted user with email '{email}' not found");

        return user.Id;
    }
    public async Task<IEnumerable<UserResponse>> GetAllActiveUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .Include(u => u.Profile)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt,
                Profile = u.Profile != null ? new UserProfileResponse
                {
                    FirstName = u.Profile.FirstName,
                    LastName = u.Profile.LastName,
                    Phone = u.Profile.Phone
                } : null
            })
            .ToListAsync();
    }

    public async Task<UserResponse> GetProfileAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new NotFoundException("User not found");

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt,
            Profile = user.Profile != null ? new UserProfileResponse
            {
                FirstName = user.Profile.FirstName,
                LastName = user.Profile.LastName,
                Phone = user.Profile.Phone
            } : null
        };
    }

    public async Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new NotFoundException("User not found");

        if (user.Profile == null)
            user.CreateProfile(request.FirstName, request.LastName, request.Phone);
        else
            user.Profile.Update(request.FirstName, request.LastName, request.Phone);

        await _context.SaveChangesAsync();
    }

}