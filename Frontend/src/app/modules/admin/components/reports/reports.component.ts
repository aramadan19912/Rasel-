import { Component, OnInit } from '@angular/core';
import { AdminService } from '../../../../services/admin/admin.service';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  loading = false;
  selectedReportType = 'correspondence';

  // Correspondence Report Data
  correspondenceReport: any = null;
  correspondenceFilter: any = {
    startDate: new Date(new Date().setMonth(new Date().getMonth() - 1)),
    endDate: new Date(),
    statuses: [],
    priorities: [],
    departmentIds: [],
    employeeIds: []
  };

  // Employee Performance Report Data
  employeePerformanceReports: any[] = [];
  selectedEmployeeId: number | null = null;
  employees: any[] = [];

  // Department Report Data
  departmentReports: any[] = [];
  selectedDepartmentId: number | null = null;
  departments: any[] = [];

  // Archive Report Data
  archiveReport: any = null;

  // Audit Log Report Data
  auditLogReport: any = null;
  auditLogFilter: any = {
    startDate: new Date(new Date().setMonth(new Date().getMonth() - 1)),
    endDate: new Date(),
    actions: [],
    entityTypes: [],
    userId: null,
    ipAddress: null,
    pageNumber: 1,
    pageSize: 50
  };

  // Chart Data
  statusChartData: any[] = [];
  priorityChartData: any[] = [];
  departmentChartData: any[] = [];

  // Chart options
  view: [number, number] = [700, 300];
  showXAxis = true;
  showYAxis = true;
  gradient = false;
  showLegend = true;
  showXAxisLabel = true;
  showYAxisLabel = true;
  xAxisLabel = 'Category';
  yAxisLabel = 'Count';
  colorScheme = {
    domain: ['#667eea', '#764ba2', '#4facfe', '#00f2fe', '#43e97b', '#38f9d7', '#fa709a', '#fee140']
  };

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.loadDepartments();
    this.loadEmployees();
    this.loadReport();
  }

  loadDepartments(): void {
    // Load departments for filters
    this.adminService.getDepartments().subscribe({
      next: (data) => {
        this.departments = data;
      },
      error: (error) => {
        console.error('Error loading departments:', error);
      }
    });
  }

  loadEmployees(): void {
    // Load employees for filters
    this.adminService.getEmployees().subscribe({
      next: (data) => {
        this.employees = data;
      },
      error: (error) => {
        console.error('Error loading employees:', error);
      }
    });
  }

  loadReport(): void {
    this.loading = true;

    switch (this.selectedReportType) {
      case 'correspondence':
        this.loadCorrespondenceReport();
        break;
      case 'employee':
        this.loadEmployeePerformanceReports();
        break;
      case 'department':
        this.loadDepartmentReports();
        break;
      case 'archive':
        this.loadArchiveReport();
        break;
      case 'audit':
        this.loadAuditLogReport();
        break;
    }
  }

  loadCorrespondenceReport(): void {
    this.adminService.getCorrespondenceReport(this.correspondenceFilter).subscribe({
      next: (data) => {
        this.correspondenceReport = data;
        this.prepareCorrespondenceChartData();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading correspondence report:', error);
        this.loading = false;
      }
    });
  }

  loadEmployeePerformanceReports(): void {
    if (this.selectedEmployeeId) {
      this.adminService.getEmployeePerformanceReport(
        this.selectedEmployeeId,
        this.correspondenceFilter.startDate,
        this.correspondenceFilter.endDate
      ).subscribe({
        next: (data) => {
          this.employeePerformanceReports = [data];
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading employee performance report:', error);
          this.loading = false;
        }
      });
    } else {
      this.adminService.getAllEmployeesPerformanceReport(
        this.correspondenceFilter.startDate,
        this.correspondenceFilter.endDate
      ).subscribe({
        next: (data) => {
          this.employeePerformanceReports = data;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading all employees performance report:', error);
          this.loading = false;
        }
      });
    }
  }

  loadDepartmentReports(): void {
    if (this.selectedDepartmentId) {
      this.adminService.getDepartmentReport(
        this.selectedDepartmentId,
        this.correspondenceFilter.startDate,
        this.correspondenceFilter.endDate
      ).subscribe({
        next: (data) => {
          this.departmentReports = [data];
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading department report:', error);
          this.loading = false;
        }
      });
    } else {
      this.adminService.getAllDepartmentsReport(
        this.correspondenceFilter.startDate,
        this.correspondenceFilter.endDate
      ).subscribe({
        next: (data) => {
          this.departmentReports = data;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading all departments report:', error);
          this.loading = false;
        }
      });
    }
  }

  loadArchiveReport(): void {
    this.adminService.getArchiveReport(
      this.correspondenceFilter.startDate,
      this.correspondenceFilter.endDate
    ).subscribe({
      next: (data) => {
        this.archiveReport = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading archive report:', error);
        this.loading = false;
      }
    });
  }

  loadAuditLogReport(): void {
    this.adminService.getAuditLogReport(this.auditLogFilter).subscribe({
      next: (data) => {
        this.auditLogReport = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading audit log report:', error);
        this.loading = false;
      }
    });
  }

  prepareCorrespondenceChartData(): void {
    if (!this.correspondenceReport) return;

    // Status chart data
    this.statusChartData = Object.entries(this.correspondenceReport.byStatus || {}).map(([name, value]) => ({
      name,
      value
    }));

    // Priority chart data
    this.priorityChartData = Object.entries(this.correspondenceReport.byPriority || {}).map(([name, value]) => ({
      name,
      value
    }));

    // Department chart data
    this.departmentChartData = Object.entries(this.correspondenceReport.byDepartment || {}).map(([name, value]) => ({
      name,
      value
    }));
  }

  onReportTypeChange(type: string): void {
    this.selectedReportType = type;
    this.loadReport();
  }

  onDateRangeChange(): void {
    this.loadReport();
  }

  exportReport(format: string): void {
    this.loading = true;

    switch (this.selectedReportType) {
      case 'correspondence':
        this.adminService.exportCorrespondenceReport(this.correspondenceFilter, format).subscribe({
          next: (blob) => {
            this.downloadFile(blob, `correspondence-report.${format.toLowerCase()}`);
            this.loading = false;
          },
          error: (error) => {
            console.error('Error exporting correspondence report:', error);
            this.loading = false;
          }
        });
        break;
      // Add other export cases as needed
    }
  }

  private downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  refreshReport(): void {
    this.loadReport();
  }
}
