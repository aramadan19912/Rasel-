import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
import { CdkVirtualScrollViewport } from '@angular/cdk/scrolling';
import { Subject, takeUntil } from 'rxjs';
import { InboxService } from '../../services/inbox.service';
import {
  Message,
  MessageCategory,
  MessageImportance,
  ReactionType
} from '../../models/message.model';
import { MessageFolder } from '../../models/folder.model';
import {
  InboxQueryParameters,
  FilterOptions,
  PaginatedList
} from '../../models/query-parameters.model';

@Component({
  selector: 'app-inbox',
  templateUrl: './inbox.component.html',
  styleUrls: ['./inbox.component.scss']
})
export class InboxComponent implements OnInit, OnDestroy {
  @ViewChild(CdkVirtualScrollViewport) messageList!: CdkVirtualScrollViewport;

  // Layout
  selectedLayout: 'compact' | 'normal' | 'preview' = 'normal';
  showReadingPane = true;
  readingPanePosition: 'right' | 'bottom' = 'right';

  // Messages
  messages: Message[] = [];
  selectedMessages = new Set<number>();
  selectedMessage: Message | null = null;
  isLoading = false;

  // Folders
  folders: MessageFolder[] = [];
  currentFolder: MessageFolder | null = null;

  // Categories
  categories: MessageCategory[] = [];

  // Search & Filter
  searchQuery = '';
  filterOptions: FilterOptions = {
    unreadOnly: false,
    flaggedOnly: false,
    hasAttachmentsOnly: false,
    importance: null,
    dateRange: null
  };

  // Sort
  sortBy: 'date' | 'sender' | 'subject' | 'importance' = 'date';
  sortDescending = true;

  // Pagination
  currentPage = 1;
  pageSize = 50;
  totalCount = 0;

  // Conversation threading
  groupByConversation = true;
  expandedConversations = new Set<string>();

  // Quick actions
  isComposing = false;
  replyingTo: Message | null = null;

  private destroy$ = new Subject<void>();

  constructor(private inboxService: InboxService) {}

  ngOnInit(): void {
    this.loadFolders();
    this.loadCategories();
    this.loadMessages();
    this.setupRealtimeUpdates();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadFolders(): void {
    this.inboxService.getFolders()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (folders) => {
          this.folders = folders;
          // Set inbox as default
          this.currentFolder = folders.find(f => f.type === 1) || null;
        },
        error: (error) => console.error('Failed to load folders', error)
      });
  }

  loadCategories(): void {
    this.inboxService.getCategories()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (categories) => {
          this.categories = categories;
        },
        error: (error) => console.error('Failed to load categories', error)
      });
  }

  loadMessages(): void {
    this.isLoading = true;

    const params: InboxQueryParameters = {
      folderId: this.currentFolder?.id,
      pageNumber: this.currentPage,
      pageSize: this.pageSize,
      sortBy: this.sortBy,
      sortDescending: this.sortDescending,
      search: this.searchQuery,
      isUnreadOnly: this.filterOptions.unreadOnly,
      isFlaggedOnly: this.filterOptions.flaggedOnly,
      hasAttachmentsOnly: this.filterOptions.hasAttachmentsOnly,
      categoryId: undefined,
      importance: this.filterOptions.importance || undefined
    };

    this.inboxService.getInbox(params)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result: PaginatedList<Message>) => {
          this.messages = result.items;
          this.totalCount = result.totalCount;
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Failed to load messages', error);
          this.isLoading = false;
        }
      });
  }

  selectMessage(message: Message): void {
    this.selectedMessage = message;
    this.inboxService.selectMessage(message);

    if (!message.isRead) {
      this.markAsRead(message.id);
    }
  }

  toggleMessageSelection(message: Message, event: Event): void {
    event.stopPropagation();

    if (this.selectedMessages.has(message.id)) {
      this.selectedMessages.delete(message.id);
    } else {
      this.selectedMessages.add(message.id);
    }
  }

  selectAll(): void {
    if (this.selectedMessages.size === this.messages.length) {
      this.selectedMessages.clear();
    } else {
      this.messages.forEach(m => this.selectedMessages.add(m.id));
    }
  }

  deleteSelected(): void {
    const messageIds = Array.from(this.selectedMessages);

    if (confirm(`Are you sure you want to delete ${messageIds.length} message(s)?`)) {
      this.inboxService.bulkDelete(messageIds)
        .pipe(takeUntil(this.destroy$))
        .subscribe({
          next: () => {
            this.loadMessages();
            this.selectedMessages.clear();
          },
          error: (error) => console.error('Failed to delete messages', error)
        });
    }
  }

  moveSelected(folderId: number): void {
    const messageIds = Array.from(this.selectedMessages);

    this.inboxService.bulkMove(messageIds, folderId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadMessages();
          this.selectedMessages.clear();
        },
        error: (error) => console.error('Failed to move messages', error)
      });
  }

  markAsRead(messageId: number): void {
    this.inboxService.markAsRead(messageId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          const message = this.messages.find(m => m.id === messageId);
          if (message) {
            message.isRead = true;
          }
        },
        error: (error) => console.error('Failed to mark as read', error)
      });
  }

  flagMessage(message: Message): void {
    this.inboxService.flagMessage(message.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          message.isFlagged = true;
        },
        error: (error) => console.error('Failed to flag message', error)
      });
  }

  reply(message: Message): void {
    this.replyingTo = message;
    this.openCompose('reply');
  }

  replyAll(message: Message): void {
    this.replyingTo = message;
    this.openCompose('replyAll');
  }

  forward(message: Message): void {
    this.replyingTo = message;
    this.openCompose('forward');
  }

  openCompose(mode: 'new' | 'reply' | 'replyAll' | 'forward' = 'new'): void {
    this.isComposing = true;
    // Open compose dialog (implementation depends on your dialog service)
  }

  assignCategory(message: Message, category: MessageCategory): void {
    this.inboxService.assignCategory(message.id, category.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          if (!message.categories) {
            message.categories = [];
          }
          message.categories.push(category);
        },
        error: (error) => console.error('Failed to assign category', error)
      });
  }

  addReaction(message: Message, reactionType: ReactionType): void {
    this.inboxService.addReaction(message.id, reactionType)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          // Update local state
          this.loadMessages();
        },
        error: (error) => console.error('Failed to add reaction', error)
      });
  }

  search(): void {
    this.currentPage = 1;
    this.loadMessages();
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadMessages();
  }

  changeSort(field: 'date' | 'sender' | 'subject' | 'importance'): void {
    if (this.sortBy === field) {
      this.sortDescending = !this.sortDescending;
    } else {
      this.sortBy = field;
      this.sortDescending = true;
    }
    this.loadMessages();
  }

  toggleConversation(conversationId: string): void {
    if (this.expandedConversations.has(conversationId)) {
      this.expandedConversations.delete(conversationId);
    } else {
      this.expandedConversations.add(conversationId);
    }
  }

  setupRealtimeUpdates(): void {
    this.inboxService.onNewMessage()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (message) => {
          this.messages.unshift(message);
          this.totalCount++;
          // Show notification
          console.log('New message received:', message.subject);
        }
      });
  }

  changeLayout(layout: 'compact' | 'normal' | 'preview'): void {
    this.selectedLayout = layout;
  }

  toggleReadingPane(): void {
    this.showReadingPane = !this.showReadingPane;
  }

  exportToPdf(message: Message): void {
    this.inboxService.exportToPdf(message.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `${message.subject}.pdf`;
          a.click();
          window.URL.revokeObjectURL(url);
        },
        error: (error) => console.error('Failed to export to PDF', error)
      });
  }

  onPageChange(event: any): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadMessages();
  }
}
