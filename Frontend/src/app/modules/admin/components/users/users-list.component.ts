import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { AdminService } from '../../../../services/admin/admin.service';
import { UserDialogComponent } from './user-dialog.component';

@Component({
  selector: 'app-users-list',
  template: `
    <div class="admin-container">
      <div class="page-header">
        <h2>{{ 'admin.users.title' | translate }}</h2>
        <button mat-raised-button color="primary" (click)="openDialog()"
                *appHasPermission="'admin.users.manage'">
          <mat-icon>add</mat-icon>
          {{ 'admin.users.addUser' | translate }}
        </button>
      </div>
      <mat-card>
        <mat-card-content>
          <p class="coming-soon">{{ 'admin.users.comingSoon' | translate }}</p>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .admin-container { padding: 24px; }
    .page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 24px; }
    .coming-soon { text-align: center; padding: 48px; color: #666; }
  `]
})
export class UsersListComponent implements OnInit {
  constructor(
    private adminService: AdminService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    public translate: TranslateService
  ) {}

  ngOnInit(): void {}

  openDialog(): void {
    this.dialog.open(UserDialogComponent, {
      width: '600px'
    });
  }
}
