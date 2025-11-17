import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DmsService } from '../../../services/dms.service';
import {
  Document,
  DocumentFolder,
  DocumentType,
  DocumentCategory,
  DocumentAccessLevel
} from '../../../models/dms.models';

interface DmsStatistics {
  totalDocuments: number;
  totalFolders: number;
  totalStorage: number;
  documentsByType: { [key: string]: number };
  documentsByCategory: { [key: string]: number };
  recentDocuments: Document[];
  popularDocuments: Document[];
  storageByCategory: { category: string; size: number }[];
}

@Component({
  standalone: false,
  selector: 'app-dms-dashboard',
  templateUrl: './dms-dashboard.component.html',
  styleUrls: ['./dms-dashboard.component.scss']
})
export class DmsDashboardComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  statistics: DmsStatistics | null = null;
  loading = false;

  // Chart data
  storageChartData: any[] = [];
  typeChartData: any[] = [];
  categoryChartData: any[] = [];

  constructor(
    private dmsService: DmsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadStatistics();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadStatistics(): void {
    this.loading = true;

    this.dmsService.getStatistics()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (stats: any) => {
          this.statistics = stats as any;
          this.prepareChartData();
          this.loading = false;
        },
        error: (err: any) => {
          console.error('Error loading statistics:', err);
          this.loading = false;
        }
      });
  }

  prepareChartData(): void {
    if (!this.statistics) return;

    // Storage by category
    this.storageChartData = this.statistics.storageByCategory.map(item => ({
      name: item.category,
      value: item.size
    }));

    // Documents by type
    this.typeChartData = Object.entries(this.statistics.documentsByType).map(([key, value]) => ({
      name: this.getDocumentTypeLabel(key),
      value: value
    }));

    // Documents by category
    this.categoryChartData = Object.entries(this.statistics.documentsByCategory).map(([key, value]) => ({
      name: key,
      value: value
    }));
  }

  getDocumentTypeLabel(type: string): string {
    const labels: { [key: string]: string } = {
      'General': 'General',
      'Contract': 'Contract',
      'Invoice': 'Invoice',
      'Report': 'Report',
      'Presentation': 'Presentation',
      'Spreadsheet': 'Spreadsheet',
      'Image': 'Image',
      'Video': 'Video',
      'Audio': 'Audio',
      'Archive': 'Archive',
      'Other': 'Other'
    };
    return labels[type] || type;
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(2) + ' KB';
    if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)).toFixed(2) + ' MB';
    return (bytes / (1024 * 1024 * 1024)).toFixed(2) + ' GB';
  }

  formatDate(date: Date | string): string {
    return new Date(date).toLocaleDateString();
  }

  viewDocument(document: Document): void {
    this.router.navigate(['/dms/viewer', document.id]);
  }

  goToDocuments(): void {
    this.router.navigate(['/dms']);
  }

  goToUpload(): void {
    this.router.navigate(['/dms/upload']);
  }

  getFileIcon(doc: Document): string {
    const ext = doc.fileExtension.toLowerCase();
    if (ext === '.pdf') return 'picture_as_pdf';
    if (['.doc', '.docx'].includes(ext)) return 'description';
    if (['.xls', '.xlsx'].includes(ext)) return 'table_chart';
    if (['.ppt', '.pptx'].includes(ext)) return 'slideshow';
    if (['.jpg', '.jpeg', '.png', '.gif'].includes(ext)) return 'image';
    return 'insert_drive_file';
  }
}
