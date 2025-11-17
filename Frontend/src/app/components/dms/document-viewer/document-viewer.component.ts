import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DmsService } from '../../../services/dms.service';
import {
  Document,
  DocumentVersion,
  DocumentAnnotation,
  AnnotationType,
  CreateAnnotationDto
} from '../../../models/dms.models';

@Component({
  standalone: false,
  selector: 'app-document-viewer',
  templateUrl: './document-viewer.component.html',
  styleUrls: ['./document-viewer.component.scss']
})
export class DocumentViewerComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  document: Document | null = null;
  currentVersion: DocumentVersion | null = null;
  versions: DocumentVersion[] = [];
  annotations: DocumentAnnotation[] = [];

  documentUrl: string | null = null;
  loading = false;
  error: string | null = null;

  // Viewer state
  currentPage = 1;
  totalPages = 1;
  zoom = 100;
  rotation = 0;

  // Annotation mode
  annotationMode = false;
  selectedAnnotationType: AnnotationType = AnnotationType.Highlight;
  annotationTypes = AnnotationType;

  // Sidebar tabs
  activeTab: 'info' | 'versions' | 'annotations' | 'activity' = 'info';

  // Permissions
  canEdit = false;
  canAnnotate = false;
  canDownload = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private dmsService: DmsService
  ) {}

  ngOnInit(): void {
    this.route.params.pipe(takeUntil(this.destroy$)).subscribe(params => {
      const documentId = +params['id'];
      if (documentId) {
        this.loadDocument(documentId);
      }
    });

    // Check for version query param
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      if (params['version']) {
        const versionId = +params['version'];
        this.loadVersion(versionId);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();

    // Cleanup document URL
    if (this.documentUrl) {
      window.URL.revokeObjectURL(this.documentUrl);
    }
  }

  // ========== Document Loading ==========

  loadDocument(documentId: number): void {
    this.loading = true;
    this.error = null;

    this.dmsService.getDocument(documentId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (doc) => {
        this.document = doc;
        this.checkPermissions();
        this.loadDocumentContent(documentId);
        this.loadVersions(documentId);
        this.loadAnnotations(documentId);
      },
      error: (err) => {
        this.error = 'Failed to load document';
        this.loading = false;
        console.error('Error loading document:', err);
      }
    });
  }

  loadDocumentContent(documentId: number, versionId?: number): void {
    this.dmsService.downloadDocument(documentId, versionId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (blob) => {
        // Revoke old URL if exists
        if (this.documentUrl) {
          window.URL.revokeObjectURL(this.documentUrl);
        }

        this.documentUrl = window.URL.createObjectURL(blob);
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load document content';
        this.loading = false;
        console.error('Error loading document content:', err);
      }
    });
  }

  loadVersions(documentId: number): void {
    this.dmsService.getVersionHistory(documentId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (versions) => {
        this.versions = versions;
      },
      error: (err) => {
        console.error('Error loading versions:', err);
      }
    });
  }

  loadAnnotations(documentId: number): void {
    this.dmsService.getAnnotations(documentId).pipe(takeUntil(this.destroy$)).subscribe({
      next: (annotations) => {
        this.annotations = annotations;
      },
      error: (err) => {
        console.error('Error loading annotations:', err);
      }
    });
  }

  loadVersion(versionId: number): void {
    if (!this.document) return;

    const version = this.versions.find(v => v.id === versionId);
    if (version) {
      this.currentVersion = version;
      this.loadDocumentContent(this.document.id, versionId);
    }
  }

  checkPermissions(): void {
    // TODO: Implement actual permission checks based on user roles
    this.canEdit = true;
    this.canAnnotate = true;
    this.canDownload = true;
  }

  // ========== Viewer Controls ==========

  zoomIn(): void {
    this.zoom = Math.min(this.zoom + 25, 200);
  }

  zoomOut(): void {
    this.zoom = Math.max(this.zoom - 25, 25);
  }

  resetZoom(): void {
    this.zoom = 100;
  }

  rotateLeft(): void {
    this.rotation = (this.rotation - 90) % 360;
  }

  rotateRight(): void {
    this.rotation = (this.rotation + 90) % 360;
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
    }
  }

  // ========== Document Actions ==========

  downloadDocument(): void {
    if (!this.document || !this.documentUrl) return;

    const link = window.document.createElement('a');
    link.href = this.documentUrl;
    link.download = this.document.originalFileName;
    link.click();
  }

  printDocument(): void {
    if (this.documentUrl) {
      const printWindow = window.open(this.documentUrl);
      if (printWindow) {
        printWindow.print();
      }
    }
  }

  shareDocument(): void {
    // TODO: Implement share functionality
    console.log('Share document');
  }

  editDocument(): void {
    if (this.document) {
      // Navigate to edit mode or open editor
      console.log('Edit document:', this.document.id);
    }
  }

  deleteDocument(): void {
    if (!this.document) return;

    if (confirm(`Are you sure you want to delete "${this.document.title}"?`)) {
      this.dmsService.deleteDocument(this.document.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.router.navigate(['/dms']);
        },
        error: (err) => {
          console.error('Error deleting document:', err);
          alert('Failed to delete document');
        }
      });
    }
  }

  // ========== Version Management ==========

  viewVersion(version: DocumentVersion): void {
    if (this.document) {
      this.currentVersion = version;
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { version: version.id },
        queryParamsHandling: 'merge'
      });
    }
  }

  restoreVersion(version: DocumentVersion): void {
    if (!this.document) return;

    if (confirm(`Restore version ${version.versionNumber}? This will create a new version.`)) {
      this.dmsService.restoreVersion(this.document.id, version.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: (doc) => {
          this.document = doc;
          this.loadVersions(doc.id);
          this.loadDocumentContent(doc.id);
          alert('Version restored successfully');
        },
        error: (err) => {
          console.error('Error restoring version:', err);
          alert('Failed to restore version');
        }
      });
    }
  }

  // ========== Annotations ==========

  toggleAnnotationMode(): void {
    this.annotationMode = !this.annotationMode;
  }

  selectAnnotationType(type: AnnotationType): void {
    this.selectedAnnotationType = type;
  }

  addAnnotation(data: Partial<CreateAnnotationDto>): void {
    if (!this.document) return;

    const dto: CreateAnnotationDto = {
      type: this.selectedAnnotationType,
      pageNumber: this.currentPage,
      content: data.content || '',
      x: data.x || 0,
      y: data.y || 0,
      width: data.width || 0,
      height: data.height || 0,
      color: data.color || '#FFFF00'
    };

    this.dmsService.createAnnotation(this.document.id, dto).pipe(takeUntil(this.destroy$)).subscribe({
      next: (annotation) => {
        this.annotations.push(annotation);
      },
      error: (err) => {
        console.error('Error creating annotation:', err);
        alert('Failed to create annotation');
      }
    });
  }

  deleteAnnotation(annotation: DocumentAnnotation): void {
    if (!this.document) return;

    if (confirm('Delete this annotation?')) {
      this.dmsService.deleteAnnotation(annotation.id).pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.annotations = this.annotations.filter(a => a.id !== annotation.id);
        },
        error: (err) => {
          console.error('Error deleting annotation:', err);
          alert('Failed to delete annotation');
        }
      });
    }
  }

  // ========== UI Helpers ==========

  getFileTypeIcon(): string {
    if (!this.document) return 'insert_drive_file';

    const ext = this.document.fileExtension.toLowerCase();
    if (ext === '.pdf') return 'picture_as_pdf';
    if (['.doc', '.docx'].includes(ext)) return 'description';
    if (['.xls', '.xlsx'].includes(ext)) return 'table_chart';
    if (['.ppt', '.pptx'].includes(ext)) return 'slideshow';
    if (['.jpg', '.jpeg', '.png', '.gif'].includes(ext)) return 'image';
    return 'insert_drive_file';
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + ' KB';
    if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
    return (bytes / (1024 * 1024 * 1024)).toFixed(2) + ' GB';
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleString();
  }

  getAnnotationTypeLabel(type: AnnotationType): string {
    const labels: { [key in AnnotationType]: string } = {
      [AnnotationType.Text]: 'Text',
      [AnnotationType.Highlight]: 'Highlight',
      [AnnotationType.Underline]: 'Underline',
      [AnnotationType.Strikethrough]: 'Strikethrough',
      [AnnotationType.FreeHand]: 'Free Hand',
      [AnnotationType.Rectangle]: 'Rectangle',
      [AnnotationType.Circle]: 'Circle',
      [AnnotationType.Arrow]: 'Arrow',
      [AnnotationType.Stamp]: 'Stamp',
      [AnnotationType.Image]: 'Image',
      [AnnotationType.Comment]: 'Comment'
    };
    return labels[type] || 'Unknown';
  }

  close(): void {
    this.router.navigate(['/dms']);
  }
}
