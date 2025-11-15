import { Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AdminService } from '../../../../services/admin/admin.service';
import { DepartmentDialogComponent } from './department-dialog.component';

@Component({
  selector: 'app-departments-list',
  template: `<div class="admin-container"><div class="page-header"><h2>{{ 'admin.departments.title' | translate }}</h2><button mat-raised-button color="primary" (click)="openDialog()"><mat-icon>add</mat-icon>{{ 'admin.departments.addDepartment' | translate }}</button></div><mat-card><mat-card-content><p class="coming-soon">{{ 'admin.departments.comingSoon' | translate }}</p></mat-card-content></mat-card></div>`,
  styles: [`.admin-container{padding:24px}.page-header{display:flex;justify-content:space-between;align-items:center;margin-bottom:24px}.coming-soon{text-align:center;padding:48px;color:#666}`]
})
export class DepartmentsListComponent {
  constructor(private adminService: AdminService, private dialog: MatDialog) {}
  openDialog(): void { this.dialog.open(DepartmentDialogComponent, { width: '600px' }); }
}
