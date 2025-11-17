import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  standalone: false,
  selector: 'app-user-dialog',
  template: `
    <h2 mat-dialog-title>{{ 'admin.users.addUser' | translate }}</h2>
    <mat-dialog-content>
      <p>User form coming soon...</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="close()">{{ 'common.cancel' | translate }}</button>
      <button mat-raised-button color="primary">{{ 'common.save' | translate }}</button>
    </mat-dialog-actions>
  `
})
export class UserDialogComponent {
  constructor(private dialogRef: MatDialogRef<UserDialogComponent>) {}

  close(): void {
    this.dialogRef.close();
  }
}
