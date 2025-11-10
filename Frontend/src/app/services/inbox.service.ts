import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import {
  Message,
  CreateMessageDto,
  ReplyDto,
  ForwardDto,
  MessageCategory,
  ReactionType
} from '../models/message.model';
import { MessageFolder, CreateFolderDto, UpdateFolderDto } from '../models/folder.model';
import {
  InboxQueryParameters,
  SearchParameters,
  PaginatedList
} from '../models/query-parameters.model';
import { MessageRule, CreateRuleDto } from '../models/rule.model';
import { InboxStatistics, ConversationThread } from '../models/statistics.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class InboxService {
  private readonly apiUrl = `${environment.apiUrl}/api/Messages`;

  // State management
  private messagesSubject = new BehaviorSubject<Message[]>([]);
  public messages$ = this.messagesSubject.asObservable();

  private foldersSubject = new BehaviorSubject<MessageFolder[]>([]);
  public folders$ = this.foldersSubject.asObservable();

  private categoriesSubject = new BehaviorSubject<MessageCategory[]>([]);
  public categories$ = this.categoriesSubject.asObservable();

  private selectedMessageSubject = new BehaviorSubject<Message | null>(null);
  public selectedMessage$ = this.selectedMessageSubject.asObservable();

  // Real-time notifications
  private newMessageSubject = new Subject<Message>();
  public newMessage$ = this.newMessageSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Basic Operations
  getInbox(parameters: InboxQueryParameters): Observable<PaginatedList<Message>> {
    let params = new HttpParams()
      .set('pageNumber', parameters.pageNumber.toString())
      .set('pageSize', parameters.pageSize.toString())
      .set('sortBy', parameters.sortBy)
      .set('sortDescending', parameters.sortDescending.toString())
      .set('isUnreadOnly', parameters.isUnreadOnly.toString())
      .set('isFlaggedOnly', parameters.isFlaggedOnly.toString())
      .set('hasAttachmentsOnly', parameters.hasAttachmentsOnly.toString());

    if (parameters.folderId) {
      params = params.set('folderId', parameters.folderId.toString());
    }
    if (parameters.search) {
      params = params.set('search', parameters.search);
    }
    if (parameters.categoryId) {
      params = params.set('categoryId', parameters.categoryId.toString());
    }
    if (parameters.importance !== undefined && parameters.importance !== null) {
      params = params.set('importance', parameters.importance.toString());
    }

    return this.http.get<PaginatedList<Message>>(this.apiUrl, { params }).pipe(
      tap(result => this.messagesSubject.next(result.items))
    );
  }

  getMessage(id: number): Observable<Message> {
    return this.http.get<Message>(`${this.apiUrl}/${id}`).pipe(
      tap(message => this.selectedMessageSubject.next(message))
    );
  }

  createDraft(dto: CreateMessageDto): Observable<Message> {
    return this.http.post<Message>(this.apiUrl, dto);
  }

  sendMessage(draftId: number): Observable<Message> {
    return this.http.post<Message>(`${this.apiUrl}/${draftId}/send`, {});
  }

  deleteMessage(id: number, permanent: boolean = false): Observable<void> {
    const params = new HttpParams().set('permanent', permanent.toString());
    return this.http.delete<void>(`${this.apiUrl}/${id}`, { params });
  }

  moveToFolder(messageId: number, folderId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/move`, { folderId });
  }

  // Read/Unread Operations
  markAsRead(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/read`, {});
  }

  markAsUnread(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/unread`, {});
  }

  bulkMarkAsRead(messageIds: number[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/bulk/read`, { messageIds });
  }

  // Reply/Forward Operations
  reply(messageId: number, dto: ReplyDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/reply`, dto);
  }

  replyAll(messageId: number, dto: ReplyDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/reply-all`, dto);
  }

  forward(messageId: number, dto: ForwardDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/forward`, dto);
  }

  quickReply(messageId: number, text: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/quick-reply`, { text });
  }

  // Folder Operations
  getFolders(): Observable<MessageFolder[]> {
    return this.http.get<MessageFolder[]>(`${this.apiUrl}/folders`).pipe(
      tap(folders => this.foldersSubject.next(folders))
    );
  }

  createFolder(dto: CreateFolderDto): Observable<MessageFolder> {
    return this.http.post<MessageFolder>(`${this.apiUrl}/folders`, dto);
  }

  updateFolder(id: number, dto: UpdateFolderDto): Observable<MessageFolder> {
    return this.http.put<MessageFolder>(`${this.apiUrl}/folders/${id}`, dto);
  }

  deleteFolder(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/folders/${id}`);
  }

  getUnreadCount(folderId: number): Observable<number> {
    return this.http.get<number>(`${this.apiUrl}/folders/${folderId}/unread-count`);
  }

  // Category Operations
  getCategories(): Observable<MessageCategory[]> {
    return this.http.get<MessageCategory[]>(`${this.apiUrl}/categories`).pipe(
      tap(categories => this.categoriesSubject.next(categories))
    );
  }

  createCategory(name: string, color: string): Observable<MessageCategory> {
    return this.http.post<MessageCategory>(`${this.apiUrl}/categories`, { name, color });
  }

  assignCategory(messageId: number, categoryId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/categories/${categoryId}`, {});
  }

  removeCategory(messageId: number, categoryId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${messageId}/categories/${categoryId}`);
  }

  // Flag Operations
  flagMessage(messageId: number, dueDate?: Date): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/flag`, { dueDate });
  }

  unflagMessage(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/unflag`, {});
  }

  markFlagComplete(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/flag/complete`, {});
  }

  setReminder(messageId: number, reminderDate: Date): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/reminder`, reminderDate);
  }

  // Search Operations
  search(parameters: SearchParameters): Observable<PaginatedList<Message>> {
    let params = new HttpParams()
      .set('pageNumber', parameters.pageNumber.toString())
      .set('pageSize', parameters.pageSize.toString());

    if (parameters.search) params = params.set('search', parameters.search);
    if (parameters.sender) params = params.set('sender', parameters.sender);
    if (parameters.subject) params = params.set('subject', parameters.subject);
    if (parameters.content) params = params.set('content', parameters.content);
    if (parameters.hasAttachments !== undefined) {
      params = params.set('hasAttachments', parameters.hasAttachments.toString());
    }

    return this.http.get<PaginatedList<Message>>(`${this.apiUrl}/search`, { params });
  }

  searchByContent(query: string): Observable<Message[]> {
    const params = new HttpParams().set('query', query);
    return this.http.get<Message[]>(`${this.apiUrl}/search/content`, { params });
  }

  searchBySender(email: string): Observable<Message[]> {
    const params = new HttpParams().set('email', email);
    return this.http.get<Message[]>(`${this.apiUrl}/search/sender`, { params });
  }

  // Conversation Operations
  getConversation(conversationId: string): Observable<ConversationThread> {
    return this.http.get<ConversationThread>(`${this.apiUrl}/conversations/${conversationId}`);
  }

  getRelatedMessages(messageId: number): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/${messageId}/related`);
  }

  // Attachment Operations
  addAttachment(messageId: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post(`${this.apiUrl}/${messageId}/attachments`, formData);
  }

  removeAttachment(attachmentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/attachments/${attachmentId}`);
  }

  downloadAttachment(attachmentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/attachments/${attachmentId}/download`, {
      responseType: 'blob'
    });
  }

  // Rule Operations
  getRules(): Observable<MessageRule[]> {
    return this.http.get<MessageRule[]>(`${this.apiUrl}/rules`);
  }

  createRule(dto: CreateRuleDto): Observable<MessageRule> {
    return this.http.post<MessageRule>(`${this.apiUrl}/rules`, dto);
  }

  applyRules(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/apply-rules`, {});
  }

  // Reaction Operations
  addReaction(messageId: number, reactionType: ReactionType): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/reactions`, { reactionType });
  }

  removeReaction(messageId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${messageId}/reactions`);
  }

  getReactions(messageId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${messageId}/reactions`);
  }

  // Bulk Operations
  bulkDelete(messageIds: number[]): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/bulk/delete`, { messageIds });
  }

  bulkMove(messageIds: number[], folderId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/bulk/move`, { messageIds, folderId });
  }

  bulkCategorize(messageIds: number[], categoryId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/bulk/categorize`, { messageIds, categoryId });
  }

  // Archive/Junk Operations
  archiveMessage(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/archive`, {});
  }

  unarchiveMessage(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/unarchive`, {});
  }

  markAsJunk(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/junk`, {});
  }

  markAsNotJunk(messageId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${messageId}/not-junk`, {});
  }

  // Statistics & Export
  getStatistics(): Observable<InboxStatistics> {
    return this.http.get<InboxStatistics>(`${this.apiUrl}/statistics`);
  }

  exportToEml(messageId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${messageId}/export/eml`, {
      responseType: 'blob'
    });
  }

  exportToPdf(messageId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${messageId}/export/pdf`, {
      responseType: 'blob'
    });
  }

  // Maintenance Operations
  cleanupOldMessages(olderThanDays: number): Observable<void> {
    const params = new HttpParams().set('olderThanDays', olderThanDays.toString());
    return this.http.post<void>(`${this.apiUrl}/cleanup/old-messages`, {}, { params });
  }

  emptyDeletedItems(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/cleanup/deleted-items`, {});
  }

  emptyJunkFolder(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/cleanup/junk`, {});
  }

  // Mentions
  getMentions(): Observable<Message[]> {
    return this.http.get<Message[]>(`${this.apiUrl}/mentions`);
  }

  // Tracking
  getTrackingInfo(messageId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${messageId}/tracking`);
  }

  // Real-time notification handlers
  onNewMessage(): Observable<Message> {
    return this.newMessage$.asObservable();
  }

  notifyNewMessage(message: Message): void {
    this.newMessageSubject.next(message);
  }

  // Helper methods
  selectMessage(message: Message | null): void {
    this.selectedMessageSubject.next(message);
  }

  getSelectedMessage(): Message | null {
    return this.selectedMessageSubject.value;
  }
}
