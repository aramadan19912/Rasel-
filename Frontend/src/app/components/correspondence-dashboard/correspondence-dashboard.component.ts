import { Component, OnInit } from '@angular/core';
import { CorrespondenceService } from '../../services/correspondence.service';
import { ArchiveCategoryService } from '../../services/archive-category.service';
import { ArchiveCategoryStatsDto, CorrespondenceSummaryDto } from '../../models/correspondence.model';

interface StatusStat {
  status: string;
  count: number;
  color: string;
  icon: string;
  arabicLabel: string;
}

interface ClassificationStat {
  classification: string;
  count: number;
  color: string;
  arabicLabel: string;
}

@Component({
  standalone: false,
  selector: 'app-correspondence-dashboard',
  templateUrl: './correspondence-dashboard.component.html',
  styleUrls: ['./correspondence-dashboard.component.css']
})
export class CorrespondenceDashboardComponent implements OnInit {
  // Statistics
  statusStats: StatusStat[] = [];
  classificationStats: ClassificationStat[] = [];
  categoryStats: ArchiveCategoryStatsDto[] = [];

  // Recent correspondences
  recentInbox: CorrespondenceSummaryDto[] = [];
  recentOutbox: CorrespondenceSummaryDto[] = [];
  pendingActionsCount: number = 0;

  // Totals
  totalCorrespondences: number = 0;
  totalArchived: number = 0;
  totalPending: number = 0;

  // Loading states
  loadingStatusStats = true;
  loadingClassificationStats = true;
  loadingCategoryStats = true;
  loadingRecentCorrespondences = true;

  // Status color mapping
  private statusColors: { [key: string]: string } = {
    'Draft': '#6c757d',
    'Pending': '#ffc107',
    'UnderReview': '#17a2b8',
    'Approved': '#28a745',
    'Rejected': '#dc3545',
    'InProgress': '#007bff',
    'Completed': '#28a745',
    'Archived': '#6c757d',
    'Cancelled': '#dc3545'
  };

  // Status icon mapping
  private statusIcons: { [key: string]: string } = {
    'Draft': 'edit',
    'Pending': 'hourglass_empty',
    'UnderReview': 'rate_review',
    'Approved': 'check_circle',
    'Rejected': 'cancel',
    'InProgress': 'autorenew',
    'Completed': 'done_all',
    'Archived': 'archive',
    'Cancelled': 'block'
  };

  // Status Arabic labels
  private statusArabicLabels: { [key: string]: string } = {
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

  // Classification Arabic labels
  private classificationArabicLabels: { [key: string]: string } = {
    'Contract': 'عقود',
    'Complaint': 'شكاوى',
    'Meeting': 'اجتماعات',
    'Memorandum': 'مذكرات',
    'Decision': 'قرارات',
    'Report': 'تقارير',
    'Invoice': 'فواتير',
    'PurchaseOrder': 'أوامر شراء',
    'HR_Document': 'موارد بشرية',
    'Legal': 'قانونية',
    'Financial': 'مالية',
    'Technical': 'فنية',
    'Administrative': 'إدارية',
    'Correspondence': 'مراسلات',
    'Other': 'أخرى'
  };

  constructor(
    private correspondenceService: CorrespondenceService,
    private categoryService: ArchiveCategoryService
  ) {}

  ngOnInit(): void {
    this.loadStatistics();
    this.loadRecentCorrespondences();
  }

  /**
   * Load all statistics
   */
  loadStatistics(): void {
    this.loadStatusStats();
    this.loadClassificationStats();
    this.loadCategoryStats();
  }

  /**
   * Load correspondence statistics by status
   */
  loadStatusStats(): void {
    this.loadingStatusStats = true;
    this.correspondenceService.getStatsByStatus().subscribe({
      next: (stats) => {
        this.statusStats = Object.keys(stats).map(status => ({
          status,
          count: stats[status],
          color: this.statusColors[status] || '#6c757d',
          icon: this.statusIcons[status] || 'description',
          arabicLabel: this.statusArabicLabels[status] || status
        })).sort((a, b) => b.count - a.count);

        this.totalCorrespondences = Object.values(stats).reduce((sum, count) => sum + count, 0);
        this.totalPending = stats['Pending'] || 0;
        this.totalArchived = stats['Archived'] || 0;

        this.loadingStatusStats = false;
      },
      error: (error) => {
        console.error('Error loading status statistics:', error);
        this.loadingStatusStats = false;
      }
    });
  }

  /**
   * Load correspondence statistics by classification
   */
  loadClassificationStats(): void {
    this.loadingClassificationStats = true;
    this.correspondenceService.getStatsByClassification().subscribe({
      next: (stats) => {
        this.classificationStats = Object.keys(stats).map(classification => ({
          classification,
          count: stats[classification],
          color: this.getClassificationColor(classification),
          arabicLabel: this.classificationArabicLabels[classification] || classification
        })).sort((a, b) => b.count - a.count);

        this.loadingClassificationStats = false;
      },
      error: (error) => {
        console.error('Error loading classification statistics:', error);
        this.loadingClassificationStats = false;
      }
    });
  }

  /**
   * Load category statistics
   */
  loadCategoryStats(): void {
    this.loadingCategoryStats = true;
    this.categoryService.getAllCategoriesStats().subscribe({
      next: (stats) => {
        this.categoryStats = stats.sort((a, b) => b.totalCorrespondences - a.totalCorrespondences).slice(0, 10);
        this.loadingCategoryStats = false;
      },
      error: (error) => {
        console.error('Error loading category statistics:', error);
        this.loadingCategoryStats = false;
      }
    });
  }

  /**
   * Load recent correspondences
   */
  loadRecentCorrespondences(): void {
    this.loadingRecentCorrespondences = true;

    // Load inbox
    this.correspondenceService.getMyInbox(1, 5).subscribe({
      next: (inbox) => {
        this.recentInbox = inbox;
      },
      error: (error) => {
        console.error('Error loading inbox:', error);
      }
    });

    // Load outbox
    this.correspondenceService.getMyOutbox(1, 5).subscribe({
      next: (outbox) => {
        this.recentOutbox = outbox;
      },
      error: (error) => {
        console.error('Error loading outbox:', error);
      }
    });

    // Load pending actions count
    this.correspondenceService.getMyPendingActions().subscribe({
      next: (actions) => {
        this.pendingActionsCount = actions.length;
        this.loadingRecentCorrespondences = false;
      },
      error: (error) => {
        console.error('Error loading pending actions:', error);
        this.loadingRecentCorrespondences = false;
      }
    });
  }

  /**
   * Get classification color
   */
  getClassificationColor(classification: string): string {
    const colors: { [key: string]: string } = {
      'Contract': '#007bff',
      'Complaint': '#dc3545',
      'Meeting': '#17a2b8',
      'Memorandum': '#6c757d',
      'Decision': '#28a745',
      'Report': '#ffc107',
      'Invoice': '#e83e8c',
      'PurchaseOrder': '#fd7e14',
      'HR_Document': '#20c997',
      'Legal': '#6f42c1',
      'Financial': '#fd7e14',
      'Technical': '#17a2b8',
      'Administrative': '#6c757d',
      'Correspondence': '#007bff',
      'Other': '#6c757d'
    };
    return colors[classification] || '#6c757d';
  }

  /**
   * Navigate to correspondence details
   */
  viewCorrespondence(id: number): void {
    // Navigate to correspondence detail page
    // Implementation depends on your routing configuration
  }

  /**
   * Refresh statistics
   */
  refresh(): void {
    this.loadStatistics();
    this.loadRecentCorrespondences();
  }

  /**
   * Format file size
   */
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}
