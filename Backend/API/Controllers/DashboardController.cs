using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlookInboxManagement.DTOs.Admin;
using OutlookInboxManagement.Services.Admin;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get executive-level dashboard with organization-wide metrics
    /// </summary>
    [HttpGet("executive")]
    [Authorize(Policy = "admin.dashboard.view")]
    public async Task<ActionResult<ExecutiveDashboardDto>> GetExecutiveDashboard()
    {
        var dashboard = await _dashboardService.GetExecutiveDashboardAsync();
        return Ok(dashboard);
    }

    /// <summary>
    /// Get department-specific dashboard
    /// </summary>
    [HttpGet("department/{departmentId}")]
    [Authorize(Policy = "admin.dashboard.view")]
    public async Task<ActionResult<DepartmentDashboardDto>> GetDepartmentDashboard(int departmentId)
    {
        try
        {
            var dashboard = await _dashboardService.GetDepartmentDashboardAsync(departmentId);
            return Ok(dashboard);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get employee-specific dashboard
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<EmployeeDashboardDto>> GetEmployeeDashboard(int employeeId)
    {
        try
        {
            var dashboard = await _dashboardService.GetEmployeeDashboardAsync(employeeId);
            return Ok(dashboard);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get recent system activities
    /// </summary>
    [HttpGet("activities")]
    [Authorize(Policy = "admin.audit.view")]
    public async Task<ActionResult<List<ActivityLogDto>>> GetRecentActivities([FromQuery] int limit = 10)
    {
        var activities = await _dashboardService.GetRecentActivitiesAsync(limit);
        return Ok(activities);
    }

    /// <summary>
    /// Get correspondence statistics for a date range
    /// </summary>
    [HttpGet("correspondence-statistics")]
    [Authorize(Policy = "correspondence.read")]
    public async Task<ActionResult<CorrespondenceStatisticsDto>> GetCorrespondenceStatistics(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var statistics = await _dashboardService.GetCorrespondenceStatisticsAsync(startDate, endDate);
        return Ok(statistics);
    }
}
