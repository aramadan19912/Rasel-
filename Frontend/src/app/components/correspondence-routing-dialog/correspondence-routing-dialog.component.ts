import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { CorrespondenceService } from '../../services/correspondence.service';
import {
  RouteCorrespondenceRequest,
  RoutingAction,
  CorrespondencePriority
} from '../../models/correspondence.model';

export interface RoutingDialogData {
  correspondenceId: number;
  referenceNumber: string;
}

@Component({
  selector: 'app-correspondence-routing-dialog',
  templateUrl: './correspondence-routing-dialog.component.html',
  styleUrls: ['./correspondence-routing-dialog.component.scss']
})
export class CorrespondenceRoutingDialogComponent implements OnInit {
  routingForm!: FormGroup;
  submitting = false;

  // Enums for dropdowns
  routingActions = Object.values(RoutingAction);
  priorities = Object.values(CorrespondencePriority);

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CorrespondenceRoutingDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: RoutingDialogData,
    private correspondenceService: CorrespondenceService,
    private snackBar: MatSnackBar,
    public translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(): void {
    this.routingForm = this.fb.group({
      toEmployeeId: [null, [Validators.required]],
      toDepartmentId: [null],
      action: [RoutingAction.ForReview, Validators.required],
      priority: [CorrespondencePriority.Normal, Validators.required],
      instructions: ['', Validators.maxLength(1000)],
      dueDate: [null]
    });
  }

  submitRouting(): void {
    if (this.routingForm.invalid) {
      this.markFormGroupTouched(this.routingForm);
      return;
    }

    this.submitting = true;

    const request: RouteCorrespondenceRequest = {
      correspondenceId: this.data.correspondenceId,
      ...this.routingForm.value
    };

    this.correspondenceService.routeCorrespondence(request).subscribe({
      next: (routing) => {
        this.snackBar.open(
          this.translate.instant('correspondence.routingSuccess'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.dialogRef.close(routing);
      },
      error: (error) => {
        console.error('Error routing correspondence:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.routingFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.submitting = false;
      }
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  getErrorMessage(fieldName: string): string {
    const control = this.routingForm.get(fieldName);
    if (control?.hasError('required')) {
      return this.translate.instant('validation.required');
    }
    if (control?.hasError('maxlength')) {
      const maxLength = control.errors?.['maxlength'].requiredLength;
      return this.translate.instant('validation.maxLength', { max: maxLength });
    }
    return '';
  }
}
