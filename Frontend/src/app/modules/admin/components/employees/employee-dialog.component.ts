import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-employee-dialog',
  template: `<h2 mat-dialog-title>{{ 'admin.employees.addEmployee' | translate }}</h2><mat-dialog-content><p>Employee form coming soon...</p></mat-dialog-content><mat-dialog-actions align="end"><button mat-button (click)="close()">{{ 'common.cancel' | translate }}</button><button mat-raised-button color="primary">{{ 'common.save' | translate }}</button></mat-dialog-actions>`
})
export class EmployeeDialogComponent {
  constructor(private dialogRef: MatDialogRef<EmployeeDialogComponent>) {}
  close(): void { this.dialogRef.close(); }
}
