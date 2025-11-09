using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.DTOs.Permissions;
using Backend.Application.Interfaces;
using Backend.Domain.Enums;
using Backend.Infrastructure.Authorization;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Permission(SystemPermission.PermissionsRead)]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PermissionDto>>> GetAll()
    {
        var permissions = await _permissionService.GetAllAsync();
        return Ok(permissions);
    }

    [HttpGet("{permissionId}")]
    public async Task<ActionResult<PermissionDto>> GetById(int permissionId)
    {
        var permission = await _permissionService.GetByIdAsync(permissionId);
        if (permission == null)
            return NotFound();

        return Ok(permission);
    }

    [HttpGet("module/{module}")]
    public async Task<ActionResult<List<PermissionDto>>> GetByModule(string module)
    {
        var permissions = await _permissionService.GetByModuleAsync(module);
        return Ok(permissions);
    }

    [HttpPost("seed")]
    [Permission(SystemPermission.PermissionsManage)]
    public async Task<IActionResult> SeedPermissions()
    {
        await _permissionService.SeedPermissionsAsync();
        return Ok(new { message = "Permissions seeded successfully" });
    }
}
