import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AdminService } from '../../../../services/admin/admin.service';
import { AuthService } from '../../../../services/auth.service';

@Component({
  selector: 'app-admin-dashboard',
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  loading = true;
  selectedDashboardLevel = 'executive';

  // Executive Dashboard Data
  executiveDashboard: any = null;

  // Department Dashboard Data
  departmentDashboard: any = null;
  selectedDepartmentId: number | null = null;
  departments: any[] = [];

  // Employee Dashboard Data
  employeeDashboard: any = null;
  selectedEmployeeId: number | null = null;
  employees: any[] = [];

  // Common Data
  recentActivities: any[] = [];
  correspondenceStats: any = null;

  // Date range for statistics
  startDate: Date | null = null;
  endDate: Date | null = null;

  constructor(
    private adminService: AdminService,
    private authService: AuthService,
    private router: Router
  ) {
    // Set default date range (last 30 days)
    this.endDate = new Date();
    this.startDate = new Date();
    this.startDate.setDate(this.startDate.getDate() - 30);
  }

  ngOnInit(): void {
    this.loadDepartments();
    this.loadEmployees();
    this.loadDashboard();
    this.loadRecentActivities();
  }

  loadDashboard(): void {
    switch (this.selectedDashboardLevel) {
      case 'executive':
        this.loadExecutiveDashboard();
        break;
      case 'department':
        if (this.selectedDepartmentId) {
          this.loadDepartmentDashboard(this.selectedDepartmentId);
        }
        break;
      case 'employee':
        if (this.selectedEmployeeId) {
          this.loadEmployeeDashboard(this.selectedEmployeeId);
        }
        break;
    }
    this.loadCorrespondenceStatistics();
  }

  loadExecutiveDashboard(): void {
    this.loading = true;
    this.adminService.getExecutiveDashboard().subscribe({
      next: (data) => {
        this.executiveDashboard = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading executive dashboard:', error);
        this.loading = false;
      }
    });
  }

  loadDepartmentDashboard(departmentId: number): void {
    this.loading = true;
    this.adminService.getDepartmentDashboard(departmentId).subscribe({
      next: (data) => {
        this.departmentDashboard = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading department dashboard:', error);
        this.loading = false;
      }
    });
  }

  loadEmployeeDashboard(employeeId: number): void {
    this.loading = true;
    this.adminService.getEmployeeDashboard(employeeId).subscribe({
      next: (data) => {
        this.employeeDashboard = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading employee dashboard:', error);
        this.loading = false;
      }
    });
  }

  loadRecentActivities(): void {
    this.adminService.getRecentActivities(10).subscribe({
      next: (data) => {
        this.recentActivities = data;
      },
      error: (error) => {
        console.error('Error loading recent activities:', error);
      }
    });
  }

  loadCorrespondenceStatistics(): void {
    this.adminService.getCorrespondenceStatistics(this.startDate || undefined, this.endDate || undefined).subscribe({
      next: (data) => {
        this.correspondenceStats = data;
      },
      error: (error) => {
        console.error('Error loading correspondence statistics:', error);
      }
    });
  }

  loadDepartments(): void {
    this.adminService.getAllDepartments().subscribe({
      next: (data) => {
        this.departments = data;
        if (data.length > 0) {
          this.selectedDepartmentId = data[0].id;
        }
      },
      error: (error) => {
        console.error('Error loading departments:', error);
      }
    });
  }

  loadEmployees(): void {
    this.adminService.searchEmployees({ pageNumber: 1, pageSize: 100 }).subscribe({
      next: (data) => {
        this.employees = data.items || data;
        if (this.employees.length > 0) {
          this.selectedEmployeeId = this.employees[0].id;
        }
      },
      error: (error) => {
        console.error('Error loading employees:', error);
      }
    });
  }

  onDashboardLevelChange(level: string): void {
    this.selectedDashboardLevel = level;
    this.loadDashboard();
  }

  onDepartmentChange(departmentId: number): void {
    this.selectedDepartmentId = departmentId;
    this.loadDepartmentDashboard(departmentId);
  }

  onEmployeeChange(employeeId: number): void {
    this.selectedEmployeeId = employeeId;
    this.loadEmployeeDashboard(employeeId);
  }

  onDateRangeChange(): void {
    this.loadCorrespondenceStatistics();
  }

  navigateTo(route: string): void {
    this.router.navigate(['/admin', route]);
  }

  refreshDashboard(): void {
    this.loadDashboard();
    this.loadRecentActivities();
  }
}
