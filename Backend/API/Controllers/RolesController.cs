using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.DTOs.Roles;
using Backend.Application.Interfaces;
using Backend.Domain.Enums;
using Backend.Infrastructure.Authorization;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Permission(SystemPermission.RolesRead)]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleDto>>> GetAll()
    {
        var roles = await _roleService.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("{roleId}")]
    public async Task<ActionResult<RoleDto>> GetById(string roleId)
    {
        var role = await _roleService.GetByIdAsync(roleId);
        if (role == null)
            return NotFound();

        return Ok(role);
    }

    [HttpPost]
    [Permission(SystemPermission.RolesCreate)]
    public async Task<ActionResult<RoleDto>> Create([FromBody] CreateRoleRequest request)
    {
        try
        {
            var role = await _roleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { roleId = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{roleId}")]
    [Permission(SystemPermission.RolesUpdate)]
    public async Task<ActionResult<RoleDto>> Update(string roleId, [FromBody] CreateRoleRequest request)
    {
        try
        {
            var role = await _roleService.UpdateAsync(roleId, request);
            return Ok(role);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{roleId}")]
    [Permission(SystemPermission.RolesDelete)]
    public async Task<IActionResult> Delete(string roleId)
    {
        try
        {
            var result = await _roleService.DeleteAsync(roleId);
            if (!result)
                return NotFound();

            return Ok(new { message = "Role deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{roleId}/permissions/{permissionId}")]
    [Permission(SystemPermission.RolesManage)]
    public async Task<IActionResult> AssignPermission(string roleId, int permissionId)
    {
        var result = await _roleService.AssignPermissionAsync(roleId, permissionId);
        if (!result)
            return BadRequest(new { message = "Failed to assign permission" });

        return Ok(new { message = "Permission assigned successfully" });
    }

    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [Permission(SystemPermission.RolesManage)]
    public async Task<IActionResult> RemovePermission(string roleId, int permissionId)
    {
        var result = await _roleService.RemovePermissionAsync(roleId, permissionId);
        if (!result)
            return BadRequest(new { message = "Failed to remove permission" });

        return Ok(new { message = "Permission removed successfully" });
    }

    [HttpGet("{roleId}/permissions")]
    public async Task<ActionResult<List<string>>> GetRolePermissions(string roleId)
    {
        var permissions = await _roleService.GetRolePermissionsAsync(roleId);
        return Ok(permissions);
    }
}
