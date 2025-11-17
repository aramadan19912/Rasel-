import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { ArchiveCategoryService } from '../../services/archive-category.service';
import {
  ArchiveCategoryDto,
  ArchiveCategoryHierarchyDto,
  ArchiveCategoryStatsDto,
  CreateArchiveCategoryRequest
} from '../../models/correspondence.model';

interface TreeNode {
  id: number;
  name: string;
  classification: string;
  icon?: string;
  color?: string;
  children: TreeNode[];
  expanded: boolean;
  level: number;
}

@Component({
  standalone: false,
  selector: 'app-archive-management',
  templateUrl: './archive-management.component.html',
  styleUrls: ['./archive-management.component.scss']
})
export class ArchiveManagementComponent implements OnInit {
  categories: ArchiveCategoryDto[] = [];
  categoryHierarchy: TreeNode[] = [];
  categoryStats: ArchiveCategoryStatsDto[] = [];
  loading = true;
  activeView: 'list' | 'tree' | 'stats' = 'tree';
  selectedCategory: ArchiveCategoryDto | null = null;

  displayedColumns: string[] = ['categoryCode', 'name', 'classification', 'retentionPeriod', 'isActive', 'actions'];

  constructor(
    private archiveCategoryService: ArchiveCategoryService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    public translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadCategories();
    this.loadCategoryHierarchy();
    this.loadCategoryStats();
  }

  loadCategories(): void {
    this.loading = true;
    this.archiveCategoryService.getAll().subscribe({
      next: (categories: ArchiveCategoryDto[]) => {
        this.categories = categories;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading categories:', error);
        this.snackBar.open(
          this.translate.instant('archive.errors.loadCategoriesFailed'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
        this.loading = false;
      }
    });
  }

  loadCategoryHierarchy(): void {
    this.archiveCategoryService.getCategoryTree().subscribe({
      next: (hierarchy: ArchiveCategoryHierarchyDto) => {
        // getCategoryTree returns a single root object, not an array
        this.categoryHierarchy = hierarchy.subCategories ? this.buildTreeNodes(hierarchy.subCategories, 0) : [];
      },
      error: (error: any) => {
        console.error('Error loading category hierarchy:', error);
      }
    });
  }

  loadCategoryStats(): void {
    this.archiveCategoryService.getAllCategoriesStats().subscribe({
      next: (stats: ArchiveCategoryStatsDto[]) => {
        this.categoryStats = stats;
      },
      error: (error: any) => {
        console.error('Error loading category stats:', error);
      }
    });
  }

  buildTreeNodes(hierarchyItems: ArchiveCategoryHierarchyDto[], level: number): TreeNode[] {
    return hierarchyItems.map(item => ({
      id: item.id || 0,
      name: this.translate.currentLang === 'ar' ? (item.nameAr || '') : (item.nameEn || ''),
      classification: item.classification || '',
      icon: item.icon,
      color: item.color,
      children: this.buildTreeNodes(item.subCategories || [], level + 1),
      expanded: level < 2,
      level
    }));
  }

  toggleNode(node: TreeNode): void {
    node.expanded = !node.expanded;
  }

  selectCategory(category: ArchiveCategoryDto): void {
    this.selectedCategory = category;
  }

  addCategory(): void {
    // TODO: Open dialog to add new category
    console.log('Add category');
  }

  editCategory(category: ArchiveCategoryDto): void {
    // TODO: Open dialog to edit category
    console.log('Edit category', category);
  }

  deleteCategory(category: ArchiveCategoryDto): void {
    if (confirm(this.translate.instant('archive.confirmDelete'))) {
      this.archiveCategoryService.delete(category.id!).subscribe({
        next: () => {
          this.snackBar.open(
            this.translate.instant('archive.deleteSuccess'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
          this.loadCategories();
          this.loadCategoryHierarchy();
        },
        error: (error: any) => {
          console.error('Error deleting category:', error);
          this.snackBar.open(
            this.translate.instant('archive.errors.deleteFailed'),
            this.translate.instant('common.close'),
            { duration: 3000 }
          );
        }
      });
    }
  }

  getCategoryName(category: ArchiveCategoryDto): string {
    return this.translate.currentLang === 'ar' ? category.nameAr : category.nameEn;
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  getClassificationColor(classification: string): string {
    const colors: { [key: string]: string } = {
      'Contract': '#2196f3',
      'Complaint': '#f44336',
      'Meeting': '#ff9800',
      'Memorandum': '#9c27b0',
      'Decision': '#4caf50',
      'Report': '#00bcd4',
      'Invoice': '#ff5722',
      'PurchaseOrder': '#3f51b5',
      'HR_Document': '#e91e63',
      'Legal': '#795548',
      'Financial': '#009688',
      'Technical': '#607d8b',
      'Administrative': '#8bc34a',
      'Correspondence': '#673ab7',
      'Other': '#9e9e9e'
    };
    return colors[classification] || '#9e9e9e';
  }

  refreshData(): void {
    this.loadCategories();
    this.loadCategoryHierarchy();
    this.loadCategoryStats();
  }
}
