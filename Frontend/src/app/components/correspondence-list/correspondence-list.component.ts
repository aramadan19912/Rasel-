import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { CorrespondenceService } from '../../services/correspondence.service';
import { ArchiveCategoryService } from '../../services/archive-category.service';
import {
  CorrespondenceSummaryDto,
  CorrespondenceSearchRequest,
  ArchiveCategoryDto,
  CorrespondenceStatus,
  CorrespondencePriority
} from '../../models/correspondence.model';

@Component({
  selector: 'app-correspondence-list',
  templateUrl: './correspondence-list.component.html',
  styleUrls: ['./correspondence-list.component.css']
})
export class CorrespondenceListComponent implements OnInit {
  correspondences: CorrespondenceSummaryDto[] = [];
  categories: ArchiveCategoryDto[] = [];

  // Pagination
  currentPage = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  // Search and filter
  searchRequest: CorrespondenceSearchRequest = {
    pageNumber: 1,
    pageSize: 20,
    sortBy: 'CreatedAt',
    sortOrder: 'DESC'
  };

  // UI state
  loading = false;
  showFilters = false;
  viewMode: 'inbox' | 'outbox' | 'drafts' | 'all' | 'archived' = 'all';

  // Dropdown options
  statusOptions = Object.values(CorrespondenceStatus);
  priorityOptions = Object.values(CorrespondencePriority);

  // Arabic labels
  statusLabels: { [key: string]: string } = {
    'Draft': 'مسودة',
    'Pending': 'قيد الانتظار',
    'UnderReview': 'قيد المراجعة',
    'Approved': 'معتمد',
    'Rejected': 'مرفوض',
    'InProgress': 'قيد التنفيذ',
    'Completed': 'مكتمل',
    'Archived': 'مؤرشف',
    'Cancelled': 'ملغي'
  };

  priorityLabels: { [key: string]: string } = {
    'Low': 'منخفضة',
    'Normal': 'عادية',
    'High': 'عالية',
    'Urgent': 'عاجلة',
    'Critical': 'حرجة'
  };

  constructor(
    private correspondenceService: CorrespondenceService,
    private categoryService: ArchiveCategoryService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Check route parameter for view mode
    this.route.queryParams.subscribe(params => {
      if (params['view']) {
        this.viewMode = params['view'];
      }
      this.loadData();
    });

    this.loadCategories();
  }

  /**
   * Load correspondences based on view mode
   */
  loadData(): void {
    this.loading = true;

    switch (this.viewMode) {
      case 'inbox':
        this.loadInbox();
        break;
      case 'outbox':
        this.loadOutbox();
        break;
      case 'drafts':
        this.loadDrafts();
        break;
      case 'archived':
        this.loadArchived();
        break;
      default:
        this.loadAll();
        break;
    }
  }

  /**
   * Load all correspondences with search/filter
   */
  loadAll(): void {
    this.searchRequest.pageNumber = this.currentPage;
    this.searchRequest.pageSize = this.pageSize;

    this.correspondenceService.search(this.searchRequest).subscribe({
      next: (response) => {
        this.correspondences = response.items;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading correspondences:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Load inbox
   */
  loadInbox(): void {
    this.correspondenceService.getMyInbox(this.currentPage, this.pageSize).subscribe({
      next: (data) => {
        this.correspondences = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading inbox:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Load outbox
   */
  loadOutbox(): void {
    this.correspondenceService.getMyOutbox(this.currentPage, this.pageSize).subscribe({
      next: (data) => {
        this.correspondences = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading outbox:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Load drafts
   */
  loadDrafts(): void {
    this.correspondenceService.getMyDrafts(this.currentPage, this.pageSize).subscribe({
      next: (data) => {
        this.correspondences = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading drafts:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Load archived
   */
  loadArchived(): void {
    this.correspondenceService.getArchivedCorrespondences().subscribe({
      next: (data) => {
        this.correspondences = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading archived correspondences:', error);
        this.loading = false;
      }
    });
  }

  /**
   * Load categories for filter dropdown
   */
  loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  /**
   * Apply search and filters
   */
  applyFilters(): void {
    this.currentPage = 1;
    this.loadData();
  }

  /**
   * Clear all filters
   */
  clearFilters(): void {
    this.searchRequest = {
      pageNumber: 1,
      pageSize: this.pageSize,
      sortBy: 'CreatedAt',
      sortOrder: 'DESC'
    };
    this.currentPage = 1;
    this.loadData();
  }

  /**
   * Toggle filters panel
   */
  toggleFilters(): void {
    this.showFilters = !this.showFilters;
  }

  /**
   * Change view mode
   */
  changeViewMode(mode: 'inbox' | 'outbox' | 'drafts' | 'all' | 'archived'): void {
    this.viewMode = mode;
    this.currentPage = 1;
    this.loadData();
  }

  /**
   * Navigate to correspondence details
   */
  viewCorrespondence(id: number): void {
    this.router.navigate(['/correspondence', id]);
  }

  /**
   * Navigate to create new correspondence
   */
  createNew(): void {
    this.router.navigate(['/correspondence/new']);
  }

  /**
   * Pagination - Go to page
   */
  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadData();
    }
  }

  /**
   * Pagination - Next page
   */
  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadData();
    }
  }

  /**
   * Pagination - Previous page
   */
  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadData();
    }
  }

  /**
   * Get page numbers for pagination
   */
  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPages = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPages / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPages - 1);

    if (endPage - startPage < maxPages - 1) {
      startPage = Math.max(1, endPage - maxPages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  }

  /**
   * Get view mode title
   */
  getViewModeTitle(): string {
    const titles: { [key: string]: string } = {
      'inbox': 'الواردات',
      'outbox': 'الصادرات',
      'drafts': 'المسودات',
      'all': 'جميع المراسلات',
      'archived': 'الأرشيف'
    };
    return titles[this.viewMode] || 'المراسلات';
  }

  /**
   * Get status label in Arabic
   */
  getStatusLabel(status: string): string {
    return this.statusLabels[status] || status;
  }

  /**
   * Get priority label in Arabic
   */
  getPriorityLabel(priority: string): string {
    return this.priorityLabels[priority] || priority;
  }
}
