import { Component } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { TranslateService } from '@ngx-translate/core';
import { AdminService } from '../../../../services/admin/admin.service';
import { RoleDialogComponent } from './role-dialog.component';

@Component({
  selector: 'app-roles-list',
  template: `
    <div class="admin-container">
      <div class="page-header">
        <h2>{{ 'admin.roles.title' | translate }}</h2>
        <button mat-raised-button color="primary" (click)="openDialog()"
                *appHasPermission="'admin.roles.manage'">
          <mat-icon>add</mat-icon>
          {{ 'admin.roles.addRole' | translate }}
        </button>
      </div>
      <mat-card>
        <mat-card-content>
          <p class="coming-soon">{{ 'admin.roles.comingSoon' | translate }}</p>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`.admin-container { padding: 24px; } .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; } .coming-soon { text-align: center; padding: 48px; color: #666; }`]
})
export class RolesListComponent {
  constructor(
    private adminService: AdminService,
    private dialog: MatDialog,
    public translate: TranslateService
  ) {}

  openDialog(): void {
    this.dialog.open(RoleDialogComponent, { width: '600px' });
  }
}
