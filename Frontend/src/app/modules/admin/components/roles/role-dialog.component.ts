import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-role-dialog',
  template: `
    <h2 mat-dialog-title>{{ 'admin.roles.addRole' | translate }}</h2>
    <mat-dialog-content><p>Role form coming soon...</p></mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="close()">{{ 'common.cancel' | translate }}</button>
      <button mat-raised-button color="primary">{{ 'common.save' | translate }}</button>
    </mat-dialog-actions>
  `
})
export class RoleDialogComponent {
  constructor(private dialogRef: MatDialogRef<RoleDialogComponent>) {}
  close(): void { this.dialogRef.close(); }
}
