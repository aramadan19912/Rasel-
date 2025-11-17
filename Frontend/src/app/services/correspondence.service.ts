import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CorrespondenceDto,
  CorrespondenceSummaryDto,
  CreateCorrespondenceRequest,
  UpdateCorrespondenceRequest,
  CorrespondenceSearchRequest,
  CorrespondenceSearchResponse,
  CorrespondenceAttachmentDto,
  UploadAttachmentRequest,
  CorrespondenceRoutingDto,
  RouteCorrespondenceRequest,
  RespondToRoutingRequest,
  RoutingChainDto,
  ArchiveCorrespondenceRequest
} from '../models/correspondence.model';

@Injectable({
  providedIn: 'root'
})
export class CorrespondenceService {
  private readonly apiUrl = `${environment.apiUrl}/api/Correspondence`;

  constructor(private http: HttpClient) {}

  // ==================== CRUD Operations ====================

  /**
   * Get correspondence by ID
   */
  getById(id: number): Observable<CorrespondenceDto> {
    return this.http.get<CorrespondenceDto>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get correspondence by reference number
   */
  getByReferenceNumber(referenceNumber: string): Observable<CorrespondenceDto> {
    return this.http.get<CorrespondenceDto>(`${this.apiUrl}/reference/${referenceNumber}`);
  }

  /**
   * Get all correspondences with pagination
   */
  getAll(pageNumber: number = 1, pageSize: number = 20): Observable<CorrespondenceSummaryDto[]> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<CorrespondenceSummaryDto[]>(this.apiUrl, { params });
  }

  /**
   * Create new correspondence
   */
  create(request: CreateCorrespondenceRequest): Observable<CorrespondenceDto> {
    return this.http.post<CorrespondenceDto>(this.apiUrl, request);
  }

  /**
   * Update correspondence
   */
  update(id: number, request: UpdateCorrespondenceRequest): Observable<CorrespondenceDto> {
    return this.http.put<CorrespondenceDto>(`${this.apiUrl}/${id}`, request);
  }

  /**
   * Delete correspondence (soft delete)
   */
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // ==================== Search & Filter ====================

  /**
   * Advanced search with multiple filters
   */
  search(request: CorrespondenceSearchRequest): Observable<CorrespondenceSearchResponse> {
    return this.http.post<CorrespondenceSearchResponse>(`${this.apiUrl}/search`, request);
  }

  /**
   * Get correspondences by category
   */
  getByCategory(categoryId: number): Observable<CorrespondenceSummaryDto[]> {
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/category/${categoryId}`);
  }

  /**
   * Get correspondences by classification
   */
  getByClassification(classification: string): Observable<CorrespondenceSummaryDto[]> {
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/classification/${classification}`);
  }

  /**
   * Get correspondences by status
   */
  getByStatus(status: string): Observable<CorrespondenceSummaryDto[]> {
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/status/${status}`);
  }

  /**
   * Update correspondence status
   */
  updateStatus(id: number, status: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, { status });
  }

  // ==================== Employee/Department Operations ====================

  /**
   * Get correspondences sent by an employee
   */
  getByFromEmployee(employeeId: number): Observable<CorrespondenceSummaryDto[]> {
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/from-employee/${employeeId}`);
  }

  /**
   * Get correspondences sent to an employee
   */
  getByToEmployee(employeeId: number): Observable<CorrespondenceSummaryDto[]> {
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/to-employee/${employeeId}`);
  }

  /**
   * Get correspondences by department
   */
  getByDepartment(departmentId: number): Observable<CorrespondenceSummaryDto[]> {
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/department/${departmentId}`);
  }

  // ==================== My Correspondence (Inbox/Outbox) ====================

  /**
   * Get my inbox (correspondences addressed to me)
   */
  getMyInbox(pageNumber: number = 1, pageSize: number = 20): Observable<CorrespondenceSummaryDto[]> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/my-inbox`, { params });
  }

  /**
   * Get my outbox (correspondences sent by me)
   */
  getMyOutbox(pageNumber: number = 1, pageSize: number = 20): Observable<CorrespondenceSummaryDto[]> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/my-outbox`, { params });
  }

  /**
   * Get my draft correspondences
   */
  getMyDrafts(pageNumber: number = 1, pageSize: number = 20): Observable<CorrespondenceSummaryDto[]> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/my-drafts`, { params });
  }

  /**
   * Get my pending actions (routings assigned to me)
   */
  getMyPendingActions(): Observable<CorrespondenceRoutingDto[]> {
    return this.http.get<CorrespondenceRoutingDto[]>(`${this.apiUrl}/my-pending-actions`);
  }

  // ==================== Attachment Operations ====================

  /**
   * Upload attachment to correspondence
   */
  uploadAttachment(
    correspondenceId: number,
    file: File,
    request: UploadAttachmentRequest
  ): Observable<CorrespondenceAttachmentDto> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('correspondenceId', request.correspondenceId.toString());
    formData.append('isMainDocument', request.isMainDocument.toString());
    if (request.description) {
      formData.append('description', request.description);
    }

    return this.http.post<CorrespondenceAttachmentDto>(
      `${this.apiUrl}/${correspondenceId}/attachments`,
      formData
    );
  }

  /**
   * Get attachments of a correspondence
   */
  getAttachments(correspondenceId: number): Observable<CorrespondenceAttachmentDto[]> {
    return this.http.get<CorrespondenceAttachmentDto[]>(`${this.apiUrl}/${correspondenceId}/attachments`);
  }

  /**
   * Delete attachment
   */
  deleteAttachment(attachmentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/attachments/${attachmentId}`);
  }

  /**
   * Download attachment file
   */
  downloadAttachment(attachmentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/attachments/${attachmentId}/download`, {
      responseType: 'blob'
    });
  }

  /**
   * Download correspondence as PDF
   */
  downloadCorrespondencePdf(correspondenceId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${correspondenceId}/pdf`, {
      responseType: 'blob'
    });
  }

  // ==================== Routing Operations ====================

  /**
   * Route correspondence to another employee (إحالة)
   */
  routeCorrespondence(
    correspondenceId: number,
    request: RouteCorrespondenceRequest
  ): Observable<CorrespondenceRoutingDto> {
    return this.http.post<CorrespondenceRoutingDto>(
      `${this.apiUrl}/${correspondenceId}/route`,
      request
    );
  }

  /**
   * Get routing history of a correspondence
   */
  getRoutingHistory(correspondenceId: number): Observable<CorrespondenceRoutingDto[]> {
    return this.http.get<CorrespondenceRoutingDto[]>(`${this.apiUrl}/${correspondenceId}/routing-history`);
  }

  /**
   * Get routing chain of a correspondence
   */
  getRoutingChain(correspondenceId: number): Observable<RoutingChainDto> {
    return this.http.get<RoutingChainDto>(`${this.apiUrl}/${correspondenceId}/routing-chain`);
  }

  /**
   * Respond to a routing
   */
  respondToRouting(routingId: number, request: RespondToRoutingRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/routing/${routingId}/respond`, request);
  }

  // ==================== Archive Operations ====================

  /**
   * Archive a correspondence
   */
  archiveCorrespondence(
    correspondenceId: number,
    request: ArchiveCorrespondenceRequest
  ): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${correspondenceId}/archive`, request);
  }

  /**
   * Get archived correspondences
   */
  getArchivedCorrespondences(): Observable<CorrespondenceSummaryDto[]> {
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/archived`);
  }

  // ==================== Related Correspondence ====================

  /**
   * Get related correspondences (replies, follow-ups)
   */
  getRelatedCorrespondences(correspondenceId: number): Observable<CorrespondenceSummaryDto[]> {
    return this.http.get<CorrespondenceSummaryDto[]>(`${this.apiUrl}/${correspondenceId}/related`);
  }

  // ==================== Statistics ====================

  /**
   * Get correspondence statistics by status
   */
  getStatsByStatus(): Observable<{ [key: string]: number }> {
    return this.http.get<{ [key: string]: number }>(`${this.apiUrl}/stats/by-status`);
  }

  /**
   * Get correspondence statistics by classification
   */
  getStatsByClassification(): Observable<{ [key: string]: number }> {
    return this.http.get<{ [key: string]: number }>(`${this.apiUrl}/stats/by-classification`);
  }

  // ==================== Reference Number Generation ====================

  /**
   * Generate reference number for a category
   */
  generateReferenceNumber(categoryId: number): Observable<{ referenceNumber: string }> {
    return this.http.get<{ referenceNumber: string }>(`${this.apiUrl}/generate-reference/${categoryId}`);
  }
}
