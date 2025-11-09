using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.DTOs.Users;
using Backend.Application.Interfaces;
using Backend.Domain.Enums;
using Backend.Infrastructure.Authorization;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Permission(SystemPermission.UsersRead)]
    public async Task<ActionResult<List<UserDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var users = await _userService.GetAllAsync(pageNumber, pageSize);
        return Ok(users);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<UserDto>> GetById(string userId)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != userId && !User.HasClaim("permission", SystemPermission.UsersRead))
            return Forbid();

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPut("{userId}")]
    public async Task<ActionResult<UserDto>> Update(string userId, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (currentUserId != userId && !User.HasClaim("permission", SystemPermission.UsersUpdate))
            return Forbid();

        try
        {
            var user = await _userService.UpdateAsync(userId, request);
            return Ok(user);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{userId}")]
    [Permission(SystemPermission.UsersDelete)]
    public async Task<IActionResult> Delete(string userId)
    {
        var result = await _userService.DeleteAsync(userId);
        if (!result)
            return NotFound();

        return Ok(new { message = "User deleted successfully" });
    }

    [HttpPost("{userId}/activate")]
    [Permission(SystemPermission.UsersManage)]
    public async Task<IActionResult> Activate(string userId)
    {
        var result = await _userService.ActivateAsync(userId);
        if (!result)
            return NotFound();

        return Ok(new { message = "User activated successfully" });
    }

    [HttpPost("{userId}/deactivate")]
    [Permission(SystemPermission.UsersManage)]
    public async Task<IActionResult> Deactivate(string userId)
    {
        var result = await _userService.DeactivateAsync(userId);
        if (!result)
            return NotFound();

        return Ok(new { message = "User deactivated successfully" });
    }

    [HttpPost("{userId}/roles/{roleName}")]
    [Permission(SystemPermission.UsersManage)]
    public async Task<IActionResult> AssignRole(string userId, string roleName)
    {
        var result = await _userService.AssignRoleAsync(userId, roleName);
        if (!result)
            return BadRequest(new { message = "Failed to assign role" });

        return Ok(new { message = "Role assigned successfully" });
    }

    [HttpDelete("{userId}/roles/{roleName}")]
    [Permission(SystemPermission.UsersManage)]
    public async Task<IActionResult> RemoveRole(string userId, string roleName)
    {
        var result = await _userService.RemoveRoleAsync(userId, roleName);
        if (!result)
            return BadRequest(new { message = "Failed to remove role" });

        return Ok(new { message = "Role removed successfully" });
    }

    [HttpGet("{userId}/roles")]
    public async Task<ActionResult<List<string>>> GetUserRoles(string userId)
    {
        var roles = await _userService.GetUserRolesAsync(userId);
        return Ok(roles);
    }

    [HttpGet("{userId}/permissions")]
    public async Task<ActionResult<List<string>>> GetUserPermissions(string userId)
    {
        var permissions = await _userService.GetUserPermissionsAsync(userId);
        return Ok(permissions);
    }
}
