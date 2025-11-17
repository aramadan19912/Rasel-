using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlookInboxManagement.DTOs.Admin;
using OutlookInboxManagement.Services.Admin;

namespace OutlookInboxManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(IReportService reportService, ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    #region Correspondence Reports

    /// <summary>
    /// Get correspondence report with filtering options
    /// </summary>
    [HttpPost("correspondence")]
    [Authorize(Policy = "correspondence.read")]
    public async Task<ActionResult<CorrespondenceReportDto>> GetCorrespondenceReport([FromBody] ReportFilterDto filter)
    {
        try
        {
            var report = await _reportService.GetCorrespondenceReportAsync(filter);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating correspondence report");
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Export correspondence report in specified format (PDF, Excel, CSV)
    /// </summary>
    [HttpPost("correspondence/export")]
    [Authorize(Policy = "correspondence.read")]
    public async Task<ActionResult> ExportCorrespondenceReport([FromBody] ReportFilterDto filter, [FromQuery] ExportFormat format = ExportFormat.PDF)
    {
        try
        {
            var fileBytes = await _reportService.ExportCorrespondenceReportAsync(filter, format);

            var contentType = format switch
            {
                ExportFormat.PDF => "application/pdf",
                ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ExportFormat.CSV => "text/csv",
                _ => "application/octet-stream"
            };

            var fileName = $"correspondence-report-{DateTime.UtcNow:yyyyMMdd}.{format.ToString().ToLower()}";

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting correspondence report");
            return StatusCode(500, new { message = "An error occurred while exporting the report" });
        }
    }

    #endregion

    #region Employee Performance Reports

    /// <summary>
    /// Get performance report for a specific employee
    /// </summary>
    [HttpGet("employee/{employeeId}")]
    [Authorize(Policy = "employee.read")]
    public async Task<ActionResult<EmployeePerformanceReportDto>> GetEmployeePerformanceReport(
        int employeeId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var report = await _reportService.GetEmployeePerformanceReportAsync(employeeId, startDate, endDate);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating employee performance report for employee {EmployeeId}", employeeId);
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Get performance reports for all employees
    /// </summary>
    [HttpGet("employee/all")]
    [Authorize(Policy = "employee.read")]
    public async Task<ActionResult<List<EmployeePerformanceReportDto>>> GetAllEmployeesPerformanceReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var reports = await _reportService.GetAllEmployeesPerformanceReportAsync(startDate, endDate);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating all employees performance report");
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    #endregion

    #region Department Reports

    /// <summary>
    /// Get report for a specific department
    /// </summary>
    [HttpGet("department/{departmentId}")]
    [Authorize(Policy = "department.read")]
    public async Task<ActionResult<DepartmentReportDto>> GetDepartmentReport(
        int departmentId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var report = await _reportService.GetDepartmentReportAsync(departmentId, startDate, endDate);
            return Ok(report);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating department report for department {DepartmentId}", departmentId);
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    /// <summary>
    /// Get reports for all departments
    /// </summary>
    [HttpGet("department/all")]
    [Authorize(Policy = "department.read")]
    public async Task<ActionResult<List<DepartmentReportDto>>> GetAllDepartmentsReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var reports = await _reportService.GetAllDepartmentsReportAsync(startDate, endDate);
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating all departments report");
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    #endregion

    #region Archive Reports

    /// <summary>
    /// Get archive statistics report
    /// </summary>
    [HttpGet("archive")]
    [Authorize(Policy = "archive.read")]
    public async Task<ActionResult<ArchiveReportDto>> GetArchiveReport(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var report = await _reportService.GetArchiveReportAsync(startDate, endDate);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating archive report");
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    #endregion

    #region Audit Log Reports

    /// <summary>
    /// Get audit log report with filtering options
    /// </summary>
    [HttpPost("audit-log")]
    [Authorize(Policy = "admin.audit.view")]
    public async Task<ActionResult<AuditLogReportDto>> GetAuditLogReport([FromBody] AuditLogFilterDto filter)
    {
        try
        {
            var report = await _reportService.GetAuditLogReportAsync(filter);
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating audit log report");
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    #endregion

    #region Custom Reports

    /// <summary>
    /// Generate a custom report based on request parameters
    /// </summary>
    [HttpPost("custom")]
    [Authorize(Policy = "admin.reports.custom")]
    public async Task<ActionResult> GenerateCustomReport([FromBody] CustomReportRequest request)
    {
        try
        {
            var fileBytes = await _reportService.GenerateCustomReportAsync(request);

            var contentType = request.Format switch
            {
                ExportFormat.PDF => "application/pdf",
                ExportFormat.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ExportFormat.CSV => "text/csv",
                _ => "application/octet-stream"
            };

            var fileName = $"custom-report-{DateTime.UtcNow:yyyyMMdd}.{request.Format.ToString().ToLower()}";

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating custom report");
            return StatusCode(500, new { message = "An error occurred while generating the report" });
        }
    }

    #endregion
}
