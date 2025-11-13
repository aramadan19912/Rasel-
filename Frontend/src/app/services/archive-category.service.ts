import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ArchiveCategoryDto,
  ArchiveCategoryHierarchyDto,
  ArchiveCategoryStatsDto,
  CreateArchiveCategoryRequest
} from '../models/correspondence.model';

@Injectable({
  providedIn: 'root'
})
export class ArchiveCategoryService {
  private readonly apiUrl = `${environment.apiUrl}/api/ArchiveCategory`;

  constructor(private http: HttpClient) {}

  /**
   * Get archive category by ID
   */
  getById(id: number): Observable<ArchiveCategoryDto> {
    return this.http.get<ArchiveCategoryDto>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get all archive categories
   */
  getAll(): Observable<ArchiveCategoryDto[]> {
    return this.http.get<ArchiveCategoryDto[]>(this.apiUrl);
  }

  /**
   * Get category tree (hierarchical structure)
   */
  getCategoryTree(): Observable<ArchiveCategoryHierarchyDto> {
    return this.http.get<ArchiveCategoryHierarchyDto>(`${this.apiUrl}/tree`);
  }

  /**
   * Get root categories (categories without parent)
   */
  getRootCategories(): Observable<ArchiveCategoryDto[]> {
    return this.http.get<ArchiveCategoryDto[]>(`${this.apiUrl}/root`);
  }

  /**
   * Get sub-categories of a parent category
   */
  getSubCategories(parentId: number): Observable<ArchiveCategoryDto[]> {
    return this.http.get<ArchiveCategoryDto[]>(`${this.apiUrl}/${parentId}/subcategories`);
  }

  /**
   * Get parent category of a category
   */
  getParentCategory(categoryId: number): Observable<ArchiveCategoryDto> {
    return this.http.get<ArchiveCategoryDto>(`${this.apiUrl}/${categoryId}/parent`);
  }

  /**
   * Get categories by classification
   */
  getByClassification(classification: string): Observable<ArchiveCategoryDto[]> {
    return this.http.get<ArchiveCategoryDto[]>(`${this.apiUrl}/classification/${classification}`);
  }

  /**
   * Search categories
   */
  search(searchTerm: string): Observable<ArchiveCategoryDto[]> {
    const params = new HttpParams().set('searchTerm', searchTerm);
    return this.http.get<ArchiveCategoryDto[]>(`${this.apiUrl}/search`, { params });
  }

  /**
   * Get category statistics
   */
  getCategoryStats(id: number): Observable<ArchiveCategoryStatsDto> {
    return this.http.get<ArchiveCategoryStatsDto>(`${this.apiUrl}/${id}/stats`);
  }

  /**
   * Get all categories statistics
   */
  getAllCategoriesStats(): Observable<ArchiveCategoryStatsDto[]> {
    return this.http.get<ArchiveCategoryStatsDto[]>(`${this.apiUrl}/stats`);
  }

  /**
   * Create new archive category
   */
  create(request: CreateArchiveCategoryRequest): Observable<ArchiveCategoryDto> {
    return this.http.post<ArchiveCategoryDto>(this.apiUrl, request);
  }

  /**
   * Update archive category
   */
  update(id: number, request: CreateArchiveCategoryRequest): Observable<ArchiveCategoryDto> {
    return this.http.put<ArchiveCategoryDto>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete archive category
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  /**
   * Check if category code exists
   */
  checkCodeExists(code: string): Observable<{ exists: boolean }> {
    return this.http.get<{ exists: boolean }>(`${this.apiUrl}/check-code/${code}`);
  }

  /**
   * Check if category can be deleted
   */
  canDelete(id: number): Observable<{ canDelete: boolean }> {
    return this.http.get<{ canDelete: boolean }>(`${this.apiUrl}/${id}/can-delete`);
  }
}
