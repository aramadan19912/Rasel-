using OutlookInboxManagement.DTOs.Admin;

namespace OutlookInboxManagement.Services.Admin;

public interface IReportService
{
    // Correspondence Reports
    Task<CorrespondenceReportDto> GetCorrespondenceReportAsync(ReportFilterDto filter);
    Task<byte[]> ExportCorrespondenceReportAsync(ReportFilterDto filter, ExportFormat format);

    // Employee Performance Reports
    Task<EmployeePerformanceReportDto> GetEmployeePerformanceReportAsync(int employeeId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<EmployeePerformanceReportDto>> GetAllEmployeesPerformanceReportAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Department Reports
    Task<DepartmentReportDto> GetDepartmentReportAsync(int departmentId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<DepartmentReportDto>> GetAllDepartmentsReportAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Archive Reports
    Task<ArchiveReportDto> GetArchiveReportAsync(DateTime? startDate = null, DateTime? endDate = null);

    // Audit Log Reports
    Task<AuditLogReportDto> GetAuditLogReportAsync(AuditLogFilterDto filter);

    // Custom Reports
    Task<byte[]> GenerateCustomReportAsync(CustomReportRequest request);
}

public enum ExportFormat
{
    PDF,
    Excel,
    CSV
}
