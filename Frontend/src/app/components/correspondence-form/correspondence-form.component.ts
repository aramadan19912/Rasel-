import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { CorrespondenceService } from '../../services/correspondence.service';
import { ArchiveCategoryService } from '../../services/archive-category.service';
import {
  CorrespondenceDto,
  CreateCorrespondenceRequest,
  UpdateCorrespondenceRequest,
  ArchiveCategoryDto,
  CorrespondenceStatus,
  CorrespondencePriority,
  ConfidentialityLevel,
  ArchiveClassification
} from '../../models/correspondence.model';

@Component({
  selector: 'app-correspondence-form',
  templateUrl: './correspondence-form.component.html',
  styleUrls: ['./correspondence-form.component.scss']
})
export class CorrespondenceFormComponent implements OnInit {
  correspondenceForm!: FormGroup;
  isEditMode = false;
  correspondenceId?: number;
  loading = true;
  submitting = false;
  categories: ArchiveCategoryDto[] = [];
  selectedFiles: File[] = [];

  // Enums for dropdowns
  statuses = Object.values(CorrespondenceStatus);
  priorities = Object.values(CorrespondencePriority);
  confidentialityLevels = Object.values(ConfidentialityLevel);
  classifications = Object.values(ArchiveClassification);

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private correspondenceService: CorrespondenceService,
    private archiveCategoryService: ArchiveCategoryService,
    private snackBar: MatSnackBar,
    public translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.loadCategories();

    // Check if we're in edit mode
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.isEditMode = true;
        this.correspondenceId = +id;
        this.loadCorrespondence(this.correspondenceId);
      } else {
        this.loading = false;
      }
    });
  }

  initializeForm(): void {
    this.correspondenceForm = this.fb.group({
      subjectAr: ['', [Validators.required, Validators.maxLength(500)]],
      subjectEn: ['', Validators.maxLength(500)],
      contentAr: ['', [Validators.required]],
      contentEn: [''],
      categoryId: [null, Validators.required],
      status: [CorrespondenceStatus.Draft, Validators.required],
      priority: [CorrespondencePriority.Normal, Validators.required],
      confidentialityLevel: [ConfidentialityLevel.Internal, Validators.required],
      correspondenceDate: [new Date(), Validators.required],
      dueDate: [null],
      fromEmployeeId: [null],
      externalSenderName: ['', Validators.maxLength(200)],
      externalSenderOrganization: ['', Validators.maxLength(200)],
      toDepartmentId: [null],
      toEmployeeId: [null],
      formSubmissionId: [null],
      relatedCorrespondenceId: [null],
      keywords: ['', Validators.maxLength(500)],
      tags: ['', Validators.maxLength(500)],
      notes: ['']
    });
  }

  loadCategories(): void {
    this.archiveCategoryService.getAllCategories().subscribe({
      next: (categories) => {
        this.categories = categories.filter(c => c.isActive);
      },
      error: (error) => {
        console.error('Error loading categories:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.categoriesLoadFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
      }
    });
  }

  loadCorrespondence(id: number): void {
    this.correspondenceService.getCorrespondenceById(id).subscribe({
      next: (correspondence) => {
        this.patchFormValues(correspondence);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading correspondence:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.loadFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.loading = false;
        this.router.navigate(['/correspondence']);
      }
    });
  }

  patchFormValues(correspondence: CorrespondenceDto): void {
    this.correspondenceForm.patchValue({
      subjectAr: correspondence.subjectAr,
      subjectEn: correspondence.subjectEn,
      contentAr: correspondence.contentAr,
      contentEn: correspondence.contentEn,
      categoryId: correspondence.categoryId,
      status: correspondence.status,
      priority: correspondence.priority,
      confidentialityLevel: correspondence.confidentialityLevel,
      correspondenceDate: new Date(correspondence.correspondenceDate),
      dueDate: correspondence.dueDate ? new Date(correspondence.dueDate) : null,
      fromEmployeeId: correspondence.fromEmployeeId,
      externalSenderName: correspondence.externalSenderName,
      externalSenderOrganization: correspondence.externalSenderOrganization,
      toDepartmentId: correspondence.toDepartmentId,
      toEmployeeId: correspondence.toEmployeeId,
      formSubmissionId: correspondence.formSubmissionId,
      relatedCorrespondenceId: correspondence.relatedCorrespondenceId,
      keywords: correspondence.keywords,
      tags: correspondence.tags,
      notes: correspondence.notes
    });
  }

  onFileSelected(event: any): void {
    const files = event.target.files;
    if (files) {
      this.selectedFiles = Array.from(files);
    }
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  submitForm(): void {
    if (this.correspondenceForm.invalid) {
      this.markFormGroupTouched(this.correspondenceForm);
      this.snackBar.open(
        this.translate.instant('correspondence.errors.invalidForm'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
      return;
    }

    this.submitting = true;

    if (this.isEditMode && this.correspondenceId) {
      this.updateCorrespondence();
    } else {
      this.createCorrespondence();
    }
  }

  createCorrespondence(): void {
    const request: CreateCorrespondenceRequest = this.correspondenceForm.value;

    this.correspondenceService.createCorrespondence(request).subscribe({
      next: (correspondence) => {
        if (this.selectedFiles.length > 0) {
          this.uploadAttachments(correspondence.id);
        } else {
          this.submitting = false;
          this.snackBar.open(
            this.translate.instant('correspondence.createSuccess'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
          this.router.navigate(['/correspondence', correspondence.id]);
        }
      },
      error: (error) => {
        console.error('Error creating correspondence:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.createFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.submitting = false;
      }
    });
  }

  updateCorrespondence(): void {
    const request: UpdateCorrespondenceRequest = this.correspondenceForm.value;

    this.correspondenceService.updateCorrespondence(this.correspondenceId!, request).subscribe({
      next: () => {
        if (this.selectedFiles.length > 0) {
          this.uploadAttachments(this.correspondenceId!);
        } else {
          this.submitting = false;
          this.snackBar.open(
            this.translate.instant('correspondence.updateSuccess'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
          this.router.navigate(['/correspondence', this.correspondenceId]);
        }
      },
      error: (error) => {
        console.error('Error updating correspondence:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.updateFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.submitting = false;
      }
    });
  }

  uploadAttachments(correspondenceId: number): void {
    const formData = new FormData();
    this.selectedFiles.forEach((file, index) => {
      formData.append('files', file, file.name);
    });

    this.correspondenceService.uploadAttachments(correspondenceId, this.selectedFiles).subscribe({
      next: () => {
        this.submitting = false;
        this.snackBar.open(
          this.isEditMode
            ? this.translate.instant('correspondence.updateSuccess')
            : this.translate.instant('correspondence.createSuccess'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.router.navigate(['/correspondence', correspondenceId]);
      },
      error: (error) => {
        console.error('Error uploading attachments:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.uploadFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.submitting = false;
        // Still navigate to the correspondence even if attachments failed
        this.router.navigate(['/correspondence', correspondenceId]);
      }
    });
  }

  saveDraft(): void {
    this.correspondenceForm.patchValue({ status: CorrespondenceStatus.Draft });
    this.submitForm();
  }

  cancel(): void {
    if (this.isEditMode && this.correspondenceId) {
      this.router.navigate(['/correspondence', this.correspondenceId]);
    } else {
      this.router.navigate(['/correspondence']);
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  getErrorMessage(fieldName: string): string {
    const control = this.correspondenceForm.get(fieldName);
    if (control?.hasError('required')) {
      return this.translate.instant('validation.required');
    }
    if (control?.hasError('maxlength')) {
      const maxLength = control.errors?.['maxlength'].requiredLength;
      return this.translate.instant('validation.maxLength', { max: maxLength });
    }
    return '';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}
