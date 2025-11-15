import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private apiUrl = `${environment.apiUrl}/api/admin`;

  constructor(private http: HttpClient) {}

  // Dashboard
  getDashboardData(): Observable<any> {
    return this.http.get(`${this.apiUrl}/dashboard`);
  }

  getSystemStatistics(): Observable<any> {
    return this.http.get(`${this.apiUrl}/dashboard/statistics`);
  }

  getExecutiveDashboard(): Observable<any> {
    return this.http.get(`${environment.apiUrl}/api/dashboard/executive`);
  }

  getDepartmentDashboard(departmentId: number): Observable<any> {
    return this.http.get(`${environment.apiUrl}/api/dashboard/department/${departmentId}`);
  }

  getEmployeeDashboard(employeeId: number): Observable<any> {
    return this.http.get(`${environment.apiUrl}/api/dashboard/employee/${employeeId}`);
  }

  getRecentActivities(limit: number = 10): Observable<any> {
    return this.http.get(`${environment.apiUrl}/api/dashboard/activities`, {
      params: { limit: limit.toString() }
    });
  }

  getCorrespondenceStatistics(startDate?: Date, endDate?: Date): Observable<any> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }
    return this.http.get(`${environment.apiUrl}/api/dashboard/correspondence-statistics`, { params });
  }

  // Users
  searchUsers(params: any): Observable<any> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key] !== null && params[key] !== undefined) {
        httpParams = httpParams.set(key, params[key].toString());
      }
    });
    return this.http.get(`${this.apiUrl}/users/search`, { params: httpParams });
  }

  getUserById(userId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/users/${userId}`);
  }

  createUser(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/users`, data);
  }

  updateUser(userId: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/users/${userId}`, data);
  }

  deleteUser(userId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/users/${userId}`);
  }

  lockUser(userId: string, lockoutEnd?: Date): Observable<any> {
    return this.http.post(`${this.apiUrl}/users/${userId}/lock`, { lockoutEnd });
  }

  unlockUser(userId: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/users/${userId}/unlock`, {});
  }

  resetPassword(userId: string, newPassword: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/users/${userId}/reset-password`, { newPassword });
  }

  // Roles
  getAllRoles(): Observable<any> {
    return this.http.get(`${this.apiUrl}/roles`);
  }

  getRoleById(roleId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/roles/${roleId}`);
  }

  createRole(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/roles`, data);
  }

  updateRole(roleId: string, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/roles/${roleId}`, data);
  }

  deleteRole(roleId: string): Observable<any> {
    return this.http.delete(`${this.apiUrl}/roles/${roleId}`);
  }

  getRolePermissions(roleId: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/roles/${roleId}/permissions`);
  }

  assignPermissions(roleId: string, permissionIds: number[]): Observable<any> {
    return this.http.post(`${this.apiUrl}/roles/${roleId}/permissions`, { permissionIds });
  }

  // Permissions
  getAllPermissions(): Observable<any> {
    return this.http.get(`${this.apiUrl}/permissions`);
  }

  getPermissionsByModule(): Observable<any> {
    return this.http.get(`${this.apiUrl}/permissions/by-module`);
  }

  // Departments
  getAllDepartments(): Observable<any> {
    return this.http.get(`${this.apiUrl}/departments`);
  }

  getDepartmentHierarchy(): Observable<any> {
    return this.http.get(`${this.apiUrl}/departments/hierarchy`);
  }

  getDepartmentById(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/departments/${id}`);
  }

  createDepartment(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/departments`, data);
  }

  updateDepartment(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/departments/${id}`, data);
  }

  deleteDepartment(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/departments/${id}`);
  }

  // Employees
  searchEmployees(params: any): Observable<any> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key] !== null && params[key] !== undefined) {
        httpParams = httpParams.set(key, params[key].toString());
      }
    });
    return this.http.get(`${this.apiUrl}/employees/search`, { params: httpParams });
  }

  getEmployeeById(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/employees/${id}`);
  }

  createEmployee(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/employees`, data);
  }

  updateEmployee(id: number, data: any): Observable<any> {
    return this.http.put(`${this.apiUrl}/employees/${id}`, data);
  }

  deleteEmployee(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/employees/${id}`);
  }

  getEmployeesByDepartment(departmentId: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/employees/by-department/${departmentId}`);
  }

  // Audit Logs
  searchAuditLogs(params: any): Observable<any> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      if (params[key] !== null && params[key] !== undefined) {
        httpParams = httpParams.set(key, params[key].toString());
      }
    });
    return this.http.get(`${this.apiUrl}/audit-logs/search`, { params: httpParams });
  }

  getAuditLogById(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/audit-logs/${id}`);
  }

  getAuditLogStatistics(): Observable<any> {
    return this.http.get(`${this.apiUrl}/audit-logs/statistics`);
  }
}
