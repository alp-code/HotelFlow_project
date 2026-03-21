using HotelFlow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HotelFlow.Application.DTOs.Requests.Auth;

namespace HotelFlow.API.Controllers;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // ADMIN DELETE
    [Authorize(Roles = "Staff")]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await _userService.DeleteUserAsync(userId);
        return NoContent();
    }

    // DELETE OWN ACCOUNT
    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyAccount()
    {
        var userId = Guid.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!
        );

        await _userService.DeleteOwnAccountAsync(userId);
        return NoContent();
    }

    // ADMIN RESTORE
    [Authorize(Roles = "Staff")]
    [HttpPost("{userId}/restore")]
    public async Task<IActionResult> RestoreUser(Guid userId)
    {
        await _userService.RestoreUserAsync(userId);
        return NoContent();
    }

    [Authorize(Roles = "Staff")]
    [HttpPut("change-role")]
    public async Task<IActionResult> ChangeRole(ChangeUserRoleRequest request)
    {
        await _userService.ChangeRoleAsync(request);
        return NoContent();
    }

    [HttpGet("user-id")]
    public async Task<IActionResult> GetUserIdByEmail([FromQuery] string email)
    {
        var result = await _userService.GetUserIdByEmailAsync(email);
        return Ok(result);
    }
    [Authorize(Roles = "Staff")]
    [HttpGet("deleted-user-id")]
    public async Task<IActionResult> GetDeletedUserIdByEmail([FromQuery] string email)
    {
        var userId = await _userService.GetDeletedUserIdByEmailAsync(email);
        return Ok(new { UserId = userId, Email = email, IsDeleted = true });
    }

    [Authorize(Roles = "Staff")]
    [HttpGet("all-active-users")]
    public async Task<IActionResult> GetAllActiveUsers()
    {
        var users = await _userService.GetAllActiveUsersAsync();
        return Ok(users);
    }
        
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _userService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userService.UpdateProfileAsync(userId, request);
        return NoContent();
    }

    [Authorize]
    [HttpPut("me/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _userService.ChangePasswordAsync(userId, request);
        return NoContent();
}
}
