import { MessageImportance } from './message.model';

export interface InboxQueryParameters {
  folderId?: number;
  pageNumber: number;
  pageSize: number;
  sortBy: string;
  sortDescending: boolean;
  search?: string;
  isUnreadOnly: boolean;
  isFlaggedOnly: boolean;
  hasAttachmentsOnly: boolean;
  categoryId?: number;
  importance?: MessageImportance;
  fromDate?: Date;
  toDate?: Date;
}

export interface SearchParameters extends InboxQueryParameters {
  sender?: string;
  subject?: string;
  content?: string;
  hasAttachments?: boolean;
}

export interface PaginatedList<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface FilterOptions {
  unreadOnly: boolean;
  flaggedOnly: boolean;
  hasAttachmentsOnly: boolean;
  importance: MessageImportance | null;
  dateRange: { from: Date; to: Date } | null;
}
