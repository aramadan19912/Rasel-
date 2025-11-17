import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import {
  Document,
  DocumentVersion,
  DocumentAnnotation,
  DocumentActivity,
  DocumentFolder,
  DocumentMetadata,
  CreateDocumentDto,
  UpdateDocumentDto,
  CreateVersionDto,
  CreateAnnotationDto,
  CreateFolderDto,
  DocumentSearchDto,
  DocumentSearchResult,
  ShareDocumentDto,
  BulkOperationResult
} from '../models/dms.models';

@Injectable({
  providedIn: 'root'
})
export class DmsService {
  private apiUrl = 'http://localhost:5000/api/document';

  // State management
  private currentFolderSubject = new BehaviorSubject<DocumentFolder | null>(null);
  public currentFolder$ = this.currentFolderSubject.asObservable();

  private documentsSubject = new BehaviorSubject<Document[]>([]);
  public documents$ = this.documentsSubject.asObservable();

  constructor(private http: HttpClient) {}

  // ==================== Document Management ====================

  getDocument(id: number): Observable<Document> {
    return this.http.get<Document>(`${this.apiUrl}/${id}`);
  }

  searchDocuments(searchDto: DocumentSearchDto): Observable<DocumentSearchResult> {
    return this.http.post<DocumentSearchResult>(`${this.apiUrl}/search`, searchDto)
      .pipe(tap(result => this.documentsSubject.next(result.documents)));
  }

  createDocument(createDto: CreateDocumentDto, file: File): Observable<Document> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('title', createDto.title);
    if (createDto.description) formData.append('description', createDto.description);
    if (createDto.category) formData.append('category', createDto.category);
    if (createDto.accessLevel) formData.append('accessLevel', createDto.accessLevel);
    if (createDto.correspondenceId) formData.append('correspondenceId', createDto.correspondenceId.toString());
    if (createDto.folderId) formData.append('folderId', createDto.folderId.toString());
    if (createDto.tags && createDto.tags.length > 0) {
      createDto.tags.forEach(tag => formData.append('tags', tag));
    }
    if (createDto.metadata) {
      Object.keys(createDto.metadata).forEach(key => {
        formData.append(`metadata[${key}]`, createDto.metadata![key]);
      });
    }

    return this.http.post<Document>(this.apiUrl, formData);
  }

  updateDocument(id: number, updateDto: UpdateDocumentDto): Observable<Document> {
    return this.http.put<Document>(`${this.apiUrl}/${id}`, updateDto);
  }

  deleteDocument(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  downloadDocument(id: number, versionId?: number): Observable<Blob> {
    const params = versionId ? new HttpParams().set('versionId', versionId.toString()) : undefined;
    return this.http.get(`${this.apiUrl}/${id}/download`, {
      params,
      responseType: 'blob'
    });
  }

  getDocumentPreviewUrl(id: number, versionId?: number): Observable<string> {
    const params = versionId ? new HttpParams().set('versionId', versionId.toString()) : undefined;
    return this.http.get<string>(`${this.apiUrl}/${id}/preview`, { params });
  }

  // ==================== Versioning ====================

  getVersionHistory(documentId: number): Observable<DocumentVersion[]> {
    return this.http.get<DocumentVersion[]>(`${this.apiUrl}/${documentId}/versions`);
  }

  createNewVersion(documentId: number, versionDto: CreateVersionDto, file: File): Observable<DocumentVersion> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('versionComment', versionDto.versionComment);
    formData.append('changeType', versionDto.changeType);

    return this.http.post<DocumentVersion>(`${this.apiUrl}/${documentId}/versions`, formData);
  }

  restoreVersion(documentId: number, versionId: number): Observable<Document> {
    return this.http.post<Document>(`${this.apiUrl}/${documentId}/versions/${versionId}/restore`, {});
  }

  downloadVersion(documentId: number, versionId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${documentId}/versions/${versionId}/download`, {
      responseType: 'blob'
    });
  }

  // ==================== Locking ====================

  lockDocument(documentId: number): Observable<Document> {
    return this.http.post<Document>(`${this.apiUrl}/${documentId}/lock`, {});
  }

  unlockDocument(documentId: number): Observable<Document> {
    return this.http.post<Document>(`${this.apiUrl}/${documentId}/unlock`, {});
  }

  isDocumentLocked(documentId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/${documentId}/locked`);
  }

  // ==================== Folders ====================

  getFolder(id: number): Observable<DocumentFolder> {
    return this.http.get<DocumentFolder>(`${this.apiUrl}/folders/${id}`)
      .pipe(tap(folder => this.currentFolderSubject.next(folder)));
  }

  getRootFolders(): Observable<DocumentFolder[]> {
    return this.http.get<DocumentFolder[]>(`${this.apiUrl}/folders`);
  }

  getSubFolders(parentId: number): Observable<DocumentFolder[]> {
    return this.http.get<DocumentFolder[]>(`${this.apiUrl}/folders/${parentId}/subfolders`);
  }

  createFolder(createDto: CreateFolderDto): Observable<DocumentFolder> {
    return this.http.post<DocumentFolder>(`${this.apiUrl}/folders`, createDto);
  }

  updateFolder(id: number, updateDto: CreateFolderDto): Observable<DocumentFolder> {
    return this.http.put<DocumentFolder>(`${this.apiUrl}/folders/${id}`, updateDto);
  }

  deleteFolder(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/folders/${id}`);
  }

  moveDocument(documentId: number, targetFolderId: number | null): Observable<Document> {
    return this.http.post<Document>(`${this.apiUrl}/${documentId}/move`, { targetFolderId });
  }

  // ==================== Annotations ====================

  getAnnotations(documentId: number, versionId?: number): Observable<DocumentAnnotation[]> {
    const params = versionId ? new HttpParams().set('versionId', versionId.toString()) : undefined;
    return this.http.get<DocumentAnnotation[]>(`${this.apiUrl}/${documentId}/annotations`, { params });
  }

  createAnnotation(documentId: number, createDto: CreateAnnotationDto): Observable<DocumentAnnotation> {
    return this.http.post<DocumentAnnotation>(`${this.apiUrl}/${documentId}/annotations`, createDto);
  }

  updateAnnotation(annotationId: number, updateDto: CreateAnnotationDto): Observable<DocumentAnnotation> {
    return this.http.put<DocumentAnnotation>(`${this.apiUrl}/annotations/${annotationId}`, updateDto);
  }

  deleteAnnotation(annotationId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/annotations/${annotationId}`);
  }

  // ==================== Activity Log ====================

  getActivity(documentId: number): Observable<DocumentActivity[]> {
    return this.getDocumentActivities(documentId);
  }

  getDocumentActivities(documentId: number, limit: number = 50): Observable<DocumentActivity[]> {
    const params = new HttpParams().set('limit', limit.toString());
    return this.http.get<DocumentActivity[]>(`${this.apiUrl}/${documentId}/activities`, { params });
  }

  // ==================== Metadata ====================

  getDocumentMetadata(documentId: number): Observable<DocumentMetadata[]> {
    return this.http.get<DocumentMetadata[]>(`${this.apiUrl}/${documentId}/metadata`);
  }

  updateDocumentMetadata(documentId: number, metadata: { [key: string]: string }): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${documentId}/metadata`, metadata);
  }

  // ==================== Permissions ====================

  canAccessDocument(documentId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/${documentId}/can-access`);
  }

  canEditDocument(documentId: number): Observable<boolean> {
    return this.http.get<boolean>(`${this.apiUrl}/${documentId}/can-edit`);
  }

  shareDocument(documentId: number, shareDto: ShareDocumentDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${documentId}/share`, shareDto);
  }

  // ==================== Bulk Operations ====================

  bulkUpload(files: File[], folderId?: number): Observable<Document[]> {
    const formData = new FormData();
    files.forEach(file => formData.append('files', file));
    if (folderId) formData.append('folderId', folderId.toString());

    return this.http.post<Document[]>(`${this.apiUrl}/bulk-upload`, formData);
  }

  exportAsZip(documentIds: number[]): Observable<Blob> {
    return this.http.post(`${this.apiUrl}/export-zip`, documentIds, {
      responseType: 'blob'
    });
  }

  bulkDelete(documentIds: number[]): Observable<BulkOperationResult> {
    return this.http.post<BulkOperationResult>(`${this.apiUrl}/bulk-delete`, documentIds);
  }

  bulkMove(documentIds: number[], targetFolderId: number | null): Observable<BulkOperationResult> {
    return this.http.post<BulkOperationResult>(`${this.apiUrl}/bulk-move`, {
      documentIds,
      targetFolderId
    });
  }

  // ==================== Statistics ====================

  getStatistics(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/statistics`);
  }

  // ==================== Helper Methods ====================

  setCurrentFolder(folder: DocumentFolder | null): void {
    this.currentFolderSubject.next(folder);
  }

  getFileIcon(fileExtension: string): string {
    const ext = fileExtension.toLowerCase().replace('.', '');
    const iconMap: { [key: string]: string } = {
      'pdf': 'picture_as_pdf',
      'doc': 'description',
      'docx': 'description',
      'xls': 'grid_on',
      'xlsx': 'grid_on',
      'ppt': 'slideshow',
      'pptx': 'slideshow',
      'jpg': 'image',
      'jpeg': 'image',
      'png': 'image',
      'gif': 'image',
      'txt': 'text_fields',
      'zip': 'folder_zip',
      'rar': 'folder_zip'
    };
    return iconMap[ext] || 'insert_drive_file';
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }

  getActivityIcon(activityType: string): string {
    const iconMap: { [key: string]: string } = {
      'Created': 'add_circle',
      'Viewed': 'visibility',
      'Downloaded': 'download',
      'Edited': 'edit',
      'VersionCreated': 'update',
      'Renamed': 'drive_file_rename_outline',
      'Moved': 'drive_file_move',
      'Deleted': 'delete',
      'Restored': 'restore',
      'Shared': 'share',
      'Locked': 'lock',
      'Unlocked': 'lock_open',
      'PermissionsChanged': 'admin_panel_settings',
      'Annotated': 'note_add',
      'CommentAdded': 'comment',
      'MetadataChanged': 'label'
    };
    return iconMap[activityType] || 'info';
  }

  getAccessLevelColor(accessLevel: string): string {
    const colorMap: { [key: string]: string } = {
      'Public': 'green',
      'Internal': 'blue',
      'Restricted': 'orange',
      'Confidential': 'red',
      'Secret': 'purple'
    };
    return colorMap[accessLevel] || 'gray';
  }
}
