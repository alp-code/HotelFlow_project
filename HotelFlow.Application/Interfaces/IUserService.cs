using HotelFlow.Application.DTOs.Requests.Auth;
using HotelFlow.Application.DTOs.Responses.Auth;

public interface IUserService
{
    Task ChangeRoleAsync(ChangeUserRoleRequest request);
    Task<UserIdResponse> GetUserIdByEmailAsync(string email);
    Task DeleteUserAsync(Guid userId);        // admin
    Task DeleteOwnAccountAsync(Guid userId);  // guest
    Task RestoreUserAsync(Guid userId);       // admin
    Task<Guid> GetDeletedUserIdByEmailAsync(string email);
    Task<IEnumerable<UserResponse>> GetAllActiveUsersAsync();
    Task<UserResponse> GetProfileAsync(Guid userId);
    Task UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
}