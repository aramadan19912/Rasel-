import { Component } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-department-dialog',
  template: `<h2 mat-dialog-title>{{ 'admin.departments.addDepartment' | translate }}</h2><mat-dialog-content><p>Department form coming soon...</p></mat-dialog-content><mat-dialog-actions align="end"><button mat-button (click)="close()">{{ 'common.cancel' | translate }}</button><button mat-raised-button color="primary">{{ 'common.save' | translate }}</button></mat-dialog-actions>`
})
export class DepartmentDialogComponent {
  constructor(private dialogRef: MatDialogRef<DepartmentDialogComponent>) {}
  close(): void { this.dialogRef.close(); }
}
