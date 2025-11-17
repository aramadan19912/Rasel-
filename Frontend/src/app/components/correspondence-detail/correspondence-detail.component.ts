import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { CorrespondenceService } from '../../services/correspondence.service';
import {
  CorrespondenceDto,
  CorrespondenceAttachmentDto,
  CorrespondenceRoutingDto,
  ArchiveCorrespondenceRequest,
  CorrespondenceStatus
} from '../../models/correspondence.model';

@Component({
  standalone: false,
  selector: 'app-correspondence-detail',
  templateUrl: './correspondence-detail.component.html',
  styleUrls: ['./correspondence-detail.component.scss']
})
export class CorrespondenceDetailComponent implements OnInit {
  correspondenceId!: number;
  correspondence: CorrespondenceDto | null = null;
  loading = true;
  activeTab = 'details';

  // Expose enums to template
  CorrespondenceStatus = CorrespondenceStatus;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private correspondenceService: CorrespondenceService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    public translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.correspondenceId = +id;
        this.loadCorrespondence();
      }
    });
  }

  loadCorrespondence(): void {
    this.loading = true;
    this.correspondenceService.getById(this.correspondenceId).subscribe({
      next: (data: CorrespondenceDto) => {
        this.correspondence = data;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading correspondence:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.loadFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.loading = false;
      }
    });
  }

  editCorrespondence(): void {
    if (this.correspondence) {
      this.router.navigate(['/correspondence', this.correspondenceId, 'edit']);
    }
  }

  deleteCorrespondence(): void {
    if (!this.correspondence) return;

    if (confirm(this.translate.instant('correspondence.confirmDelete'))) {
      this.correspondenceService.delete(this.correspondenceId).subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant('correspondence.deleteSuccess'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
          this.router.navigate(['/correspondence']);
        },
        error: (error: any) => {
          console.error('Error deleting correspondence:', error);
          this.snackBar.open(
            this.translate.instant('correspondence.errors.deleteFailed'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
        }
      });
    }
  }

  routeCorrespondence(): void {
    if (!this.correspondence) return;

    import('../correspondence-routing-dialog/correspondence-routing-dialog.component').then(m => {
      const dialogRef = this.dialog.open(m.CorrespondenceRoutingDialogComponent, {
        width: '600px',
        data: {
          correspondenceId: this.correspondenceId,
          referenceNumber: this.correspondence!.referenceNumber
        }
      });

      dialogRef.afterClosed().subscribe((result: any) => {
        if (result) {
          // Reload correspondence to show new routing
          this.loadCorrespondence();
        }
      });
    });
  }

  archiveCorrespondence(): void {
    if (!this.correspondence) return;

    const request: ArchiveCorrespondenceRequest = {
      correspondenceId: this.correspondenceId,
      generatePdf: true,
      includeAttachments: true,
      applyWatermark: true,
      applyDigitalSignature: true
    };

    this.correspondenceService.archiveCorrespondence(this.correspondenceId, request).subscribe({
      next: () => {
        this.snackBar.open(
          this.translate.instant('correspondence.archiveSuccess'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.loadCorrespondence();
      },
      error: (error: any) => {
        console.error('Error archiving correspondence:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.archiveFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
      }
    });
  }

  downloadAttachment(attachment: CorrespondenceAttachmentDto): void {
    this.correspondenceService.downloadAttachment(attachment.id).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = attachment.originalFileName;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error: any) => {
        console.error('Error downloading attachment:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.downloadFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
      }
    });
  }

  deleteAttachment(attachment: CorrespondenceAttachmentDto): void {
    if (confirm(this.translate.instant('correspondence.confirmDeleteAttachment'))) {
      this.correspondenceService.deleteAttachment(attachment.id).subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant('correspondence.attachmentDeleteSuccess'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
          this.loadCorrespondence();
        },
        error: (error: any) => {
          console.error('Error deleting attachment:', error);
          this.snackBar.open(
            this.translate.instant('correspondence.errors.attachmentDeleteFailed'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
        }
      });
    }
  }

  downloadPdf(): void {
    if (!this.correspondence) return;

    this.correspondenceService.downloadCorrespondencePdf(this.correspondenceId).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `${this.correspondence!.referenceNumber}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (error: any) => {
        console.error('Error downloading PDF:', error);
        this.snackBar.open(
          this.translate.instant('correspondence.errors.pdfDownloadFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
      }
    });
  }

  printCorrespondence(): void {
    window.print();
  }

  getStatusColor(status: string): string {
    const statusColors: { [key: string]: string } = {
      'Draft': 'gray',
      'Pending': 'orange',
      'UnderReview': 'blue',
      'Approved': 'green',
      'Rejected': 'red',
      'InProgress': 'cyan',
      'Completed': 'teal',
      'Archived': 'purple',
      'Cancelled': 'dark-gray'
    };
    return statusColors[status] || 'gray';
  }

  getPriorityColor(priority: string): string {
    const priorityColors: { [key: string]: string } = {
      'Low': 'gray',
      'Normal': 'blue',
      'High': 'orange',
      'Urgent': 'red',
      'Critical': 'purple'
    };
    return priorityColors[priority] || 'gray';
  }

  getConfidentialityColor(level: string): string {
    const confidentialityColors: { [key: string]: string } = {
      'Public': 'green',
      'Internal': 'blue',
      'Confidential': 'orange',
      'Secret': 'red',
      'TopSecret': 'purple'
    };
    return confidentialityColors[level] || 'gray';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  canEdit(): boolean {
    return this.correspondence?.status === 'Draft' || this.correspondence?.status === 'Pending';
  }

  canDelete(): boolean {
    return this.correspondence?.status === 'Draft';
  }

  canRoute(): boolean {
    return this.correspondence?.status !== 'Draft' &&
           this.correspondence?.status !== 'Cancelled' &&
           !this.correspondence?.isArchived;
  }

  canArchive(): boolean {
    return this.correspondence?.status === 'Completed' &&
           !this.correspondence?.isArchived;
  }

  backToList(): void {
    this.router.navigate(['/correspondence']);
  }
}
