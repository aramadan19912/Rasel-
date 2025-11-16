import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DmsService } from '../../../services/dms.service';
import {
  Document,
  DocumentFolder,
  DocumentSearchDto,
  DocumentType,
  DocumentCategory,
  DocumentAccessLevel
} from '../../../models/dms.models';

@Component({
  selector: 'app-document-browser',
  templateUrl: './document-browser.component.html',
  styleUrls: ['./document-browser.component.scss']
})
export class DocumentBrowserComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  documents: Document[] = [];
  folders: DocumentFolder[] = [];
  currentFolder: DocumentFolder | null = null;
  breadcrumbs: DocumentFolder[] = [];
  selectedDocuments: Document[] = [];

  // Search & Filter
  searchTerm: string = '';
  selectedTypes: DocumentType[] = [];
  selectedCategories: DocumentCategory[] = [];
  selectedAccessLevel?: DocumentAccessLevel;

  // Pagination
  currentPage: number = 1;
  pageSize: number = 20;
  totalCount: number = 0;
  totalPages: number = 0;

  // View mode
  viewMode: 'grid' | 'list' = 'grid';
  sortBy: 'name' | 'date' | 'size' | 'type' = 'date';
  sortOrder: 'asc' | 'desc' = 'desc';

  // UI State
  loading: boolean = false;
  showUploadDialog: boolean = false;
  showCreateFolderDialog: boolean = false;
  showFilterPanel: boolean = false;

  // Enums for template
  documentTypes = Object.values(DocumentType);
  documentCategories = Object.values(DocumentCategory);
  accessLevels = Object.values(DocumentAccessLevel);

  constructor(
    private dmsService: DmsService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Watch for folder ID in route params
    this.route.queryParams
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        const folderId = params['folderId'];
        if (folderId) {
          this.loadFolder(parseInt(folderId));
        } else {
          this.loadRootFolder();
        }
      });

    // Subscribe to current folder changes
    this.dmsService.currentFolder$
      .pipe(takeUntil(this.destroy$))
      .subscribe(folder => {
        this.currentFolder = folder;
        this.buildBreadcrumbs();
      });

    // Subscribe to documents changes
    this.dmsService.documents$
      .pipe(takeUntil(this.destroy$))
      .subscribe(documents => {
        this.documents = documents;
      });

    // Initial load
    this.loadDocuments();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadRootFolder(): void {
    this.loading = true;
    this.dmsService.getRootFolders()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (folders) => {
          this.folders = folders;
          this.dmsService.setCurrentFolder(null);
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading root folders:', error);
          this.loading = false;
        }
      });
  }

  loadFolder(folderId: number): void {
    this.loading = true;
    this.dmsService.getFolder(folderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (folder) => {
          this.currentFolder = folder;
          this.loadSubFolders(folderId);
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading folder:', error);
          this.loading = false;
        }
      });
  }

  loadSubFolders(parentId: number): void {
    this.dmsService.getSubFolders(parentId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (folders) => {
          this.folders = folders;
        },
        error: (error) => {
          console.error('Error loading subfolders:', error);
        }
      });
  }

  loadDocuments(): void {
    this.loading = true;
    const searchDto: DocumentSearchDto = {
      searchTerm: this.searchTerm || undefined,
      documentTypes: this.selectedTypes.length > 0 ? this.selectedTypes : undefined,
      categories: this.selectedCategories.length > 0 ? this.selectedCategories : undefined,
      folderId: this.currentFolder?.id,
      pageNumber: this.currentPage,
      pageSize: this.pageSize
    };

    this.dmsService.searchDocuments(searchDto)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.documents = result.documents;
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading documents:', error);
          this.loading = false;
        }
      });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadDocuments();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadDocuments();
  }

  openDocument(document: Document): void {
    this.router.navigate(['/dms/view', document.id]);
  }

  openFolder(folder: DocumentFolder): void {
    this.router.navigate(['/dms/browse'], {
      queryParams: { folderId: folder.id }
    });
  }

  navigateToFolder(folder: DocumentFolder | null): void {
    if (folder) {
      this.router.navigate(['/dms/browse'], {
        queryParams: { folderId: folder.id }
      });
    } else {
      this.router.navigate(['/dms/browse']);
    }
  }

  buildBreadcrumbs(): void {
    this.breadcrumbs = [];
    if (!this.currentFolder) return;

    // Build breadcrumb trail from current folder to root
    let current: DocumentFolder | null = this.currentFolder;
    while (current) {
      this.breadcrumbs.unshift(current);
      // Would need to fetch parent folder to continue
      // For now, just show current folder
      break;
    }
  }

  toggleSelection(document: Document): void {
    const index = this.selectedDocuments.findIndex(d => d.id === document.id);
    if (index >= 0) {
      this.selectedDocuments.splice(index, 1);
    } else {
      this.selectedDocuments.push(document);
    }
  }

  isSelected(document: Document): boolean {
    return this.selectedDocuments.some(d => d.id === document.id);
  }

  selectAll(): void {
    this.selectedDocuments = [...this.documents];
  }

  deselectAll(): void {
    this.selectedDocuments = [];
  }

  downloadDocument(document: Document): void {
    this.dmsService.downloadDocument(document.id)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = document.originalFileName;
          link.click();
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          console.error('Error downloading document:', error);
        }
      });
  }

  deleteDocument(document: Document): void {
    if (confirm(`Are you sure you want to delete "${document.title}"?`)) {
      this.dmsService.deleteDocument(document.id)
        .subscribe({
          next: () => {
            this.loadDocuments();
          },
          error: (error) => {
            console.error('Error deleting document:', error);
          }
        });
    }
  }

  bulkDelete(): void {
    if (this.selectedDocuments.length === 0) return;

    if (confirm(`Are you sure you want to delete ${this.selectedDocuments.length} document(s)?`)) {
      const deletePromises = this.selectedDocuments.map(doc =>
        this.dmsService.deleteDocument(doc.id).toPromise()
      );

      Promise.all(deletePromises)
        .then(() => {
          this.selectedDocuments = [];
          this.loadDocuments();
        })
        .catch(error => {
          console.error('Error during bulk delete:', error);
        });
    }
  }

  bulkDownload(): void {
    if (this.selectedDocuments.length === 0) return;

    const documentIds = this.selectedDocuments.map(d => d.id);
    this.dmsService.exportAsZip(documentIds)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `documents_${Date.now()}.zip`;
          link.click();
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          console.error('Error exporting documents:', error);
        }
      });
  }

  getFileIcon(document: Document): string {
    return this.dmsService.getFileIcon(document.fileExtension);
  }

  formatFileSize(bytes: number): string {
    return this.dmsService.formatFileSize(bytes);
  }

  getAccessLevelColor(accessLevel: string): string {
    return this.dmsService.getAccessLevelColor(accessLevel);
  }

  toggleViewMode(): void {
    this.viewMode = this.viewMode === 'grid' ? 'list' : 'grid';
  }

  onCreateFolder(): void {
    this.showCreateFolderDialog = true;
  }

  onUploadDocuments(): void {
    this.showUploadDialog = true;
  }

  onFolderCreated(): void {
    this.showCreateFolderDialog = false;
    this.loadRootFolder();
  }

  onDocumentsUploaded(): void {
    this.showUploadDialog = false;
    this.loadDocuments();
  }
}
