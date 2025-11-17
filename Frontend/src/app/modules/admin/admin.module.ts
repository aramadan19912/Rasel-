import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

// Angular Material Modules
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatListModule } from '@angular/material/list';

// NgxCharts for visualizations
import { NgxChartsModule } from '@swimlane/ngx-charts';

// Components
import { AdminDashboardComponent } from './components/dashboard/admin-dashboard.component';
import { UsersListComponent } from './components/users/users-list.component';
import { UserDialogComponent } from './components/users/user-dialog.component';
import { RolesListComponent } from './components/roles/roles-list.component';
import { RoleDialogComponent } from './components/roles/role-dialog.component';
import { DepartmentsListComponent } from './components/departments/departments-list.component';
import { DepartmentDialogComponent } from './components/departments/department-dialog.component';
import { EmployeesListComponent } from './components/employees/employees-list.component';
import { EmployeeDialogComponent } from './components/employees/employee-dialog.component';
import { AuditLogsComponent } from './components/audit-logs/audit-logs.component';
import { ReportsComponent } from './components/reports/reports.component';

const routes: Routes = [
  {
    path: '',
    component: AdminDashboardComponent
  },
  {
    path: 'users',
    component: UsersListComponent
  },
  {
    path: 'roles',
    component: RolesListComponent
  },
  {
    path: 'departments',
    component: DepartmentsListComponent
  },
  {
    path: 'employees',
    component: EmployeesListComponent
  },
  {
    path: 'audit-logs',
    component: AuditLogsComponent
  },
  {
    path: 'reports',
    component: ReportsComponent
  }
];

@NgModule({
  declarations: [
    AdminDashboardComponent,
    UsersListComponent,
    UserDialogComponent,
    RolesListComponent,
    RoleDialogComponent,
    DepartmentsListComponent,
    DepartmentDialogComponent,
    EmployeesListComponent,
    EmployeeDialogComponent,
    AuditLogsComponent,
    ReportsComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    TranslateModule,
    RouterModule.forChild(routes),
    NgxChartsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatMenuModule,
    MatBadgeModule,
    MatListModule
  ]
})
export class AdminModule { }
