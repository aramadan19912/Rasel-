export interface MessageFolder {
  id: number;
  name: string;
  displayName: string;
  parentFolderId?: number;
  subFolders: MessageFolder[];
  type: FolderType;
  unreadCount: number;
  totalCount: number;
  color: string;
  icon: string;
  displayOrder: number;
}

export enum FolderType {
  Inbox = 1,
  Drafts = 2,
  Sent = 3,
  Deleted = 4,
  Junk = 5,
  Archive = 6,
  Outbox = 7,
  Custom = 99
}

export interface CreateFolderDto {
  name: string;
  displayName: string;
  parentFolderId?: number;
  color: string;
  icon: string;
}

export interface UpdateFolderDto {
  displayName?: string;
  color?: string;
  icon?: string;
  displayOrder?: number;
}
