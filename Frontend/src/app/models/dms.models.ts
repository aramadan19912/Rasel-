// Document Management System Models

export interface Document {
  id: number;
  fileName: string;
  originalFileName: string;
  fileExtension: string;
  mimeType: string;
  fileSize: number;
  filePath: string;
  title: string;
  description?: string;
  documentType: DocumentType;
  category?: DocumentCategory;
  tags?: string[];
  currentVersion: number;
  accessLevel: DocumentAccessLevel;
  correspondenceId?: number;
  folderId?: number;
  folderName?: string;
  ownerId: string;
  isLocked: boolean;
  lockedBy?: string;
  lockedAt?: Date;
  allowedUsers?: string[];
  allowedRoles?: string[];
  createdAt: Date;
  updatedAt?: Date;
  versionsCount?: number;
  annotationsCount?: number;
}

export interface DocumentVersion {
  id: number;
  documentId: number;
  versionNumber: number;
  fileName: string;
  filePath: string;
  fileSize: number;
  fileHash: string;
  versionComment: string;
  changeType: VersionChangeType;
  createdBy: string;
  createdAt: Date;
  isActive: boolean;
}

export interface DocumentAnnotation {
  id: number;
  documentId: number;
  versionId?: number;
  type: AnnotationType;
  pageNumber: number;
  x: number;
  y: number;
  width: number;
  height: number;
  content: string;
  color?: string;
  opacity?: number;
  fontSize?: number;
  createdBy: string;
  createdByName?: string;
  createdAt: Date;
  parentAnnotationId?: number;
  replies?: DocumentAnnotation[];
}

export interface DocumentActivity {
  id: number;
  documentId: number;
  versionId?: number;
  activityType: DocumentActivityType;
  description: string;
  userId: string;
  userName: string;
  ipAddress?: string;
  userAgent?: string;
  createdAt: Date;
  additionalData?: any;
}

export interface DocumentFolder {
  id: number;
  name: string;
  description?: string;
  path: string;
  parentFolderId?: number;
  ownerId: string;
  accessLevel: FolderAccessLevel;
  allowedUsers?: string[];
  allowedRoles?: string[];
  createdAt: Date;
  subFolders?: DocumentFolder[];
  documentsCount?: number;
}

export interface DocumentMetadata {
  id: number;
  documentId: number;
  key: string;
  value: string;
  type: MetadataType;
}

// DTOs for Create/Update operations
export interface CreateDocumentDto {
  title: string;
  description?: string;
  category?: DocumentCategory;
  tags?: string[];
  accessLevel: DocumentAccessLevel;
  correspondenceId?: number;
  folderId?: number;
  metadata?: { [key: string]: string };
}

export interface UpdateDocumentDto {
  title?: string;
  description?: string;
  category?: DocumentCategory;
  tags?: string[];
  accessLevel?: DocumentAccessLevel;
  folderId?: number;
}

export interface CreateVersionDto {
  versionComment: string;
  changeType: VersionChangeType;
}

export interface CreateAnnotationDto {
  versionId?: number;
  type: AnnotationType;
  pageNumber: number;
  x: number;
  y: number;
  width: number;
  height: number;
  content: string;
  color?: string;
  opacity?: number;
  fontSize?: number;
  parentAnnotationId?: number;
}

export interface CreateFolderDto {
  name: string;
  description?: string;
  parentFolderId?: number;
  accessLevel: FolderAccessLevel;
  allowedUsers?: string[];
  allowedRoles?: string[];
}

export interface DocumentSearchDto {
  searchTerm?: string;
  documentTypes?: DocumentType[];
  categories?: DocumentCategory[];
  tags?: string[];
  folderId?: number;
  correspondenceId?: number;
  createdFrom?: Date;
  createdTo?: Date;
  ownerId?: string;
  pageNumber: number;
  pageSize: number;
}

export interface DocumentSearchResult {
  documents: Document[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ShareDocumentDto {
  userIds: string[];
  roleNames: string[];
}

// Enums
export enum DocumentType {
  Word = 'Word',
  Excel = 'Excel',
  PowerPoint = 'PowerPoint',
  PDF = 'PDF',
  Image = 'Image',
  Text = 'Text',
  Other = 'Other'
}

export enum DocumentCategory {
  General = 'General',
  Correspondence = 'Correspondence',
  Contract = 'Contract',
  Report = 'Report',
  Form = 'Form',
  Presentation = 'Presentation',
  Financial = 'Financial',
  Legal = 'Legal',
  HR = 'HR',
  IT = 'IT',
  Other = 'Other'
}

export enum DocumentAccessLevel {
  Public = 'Public',
  Internal = 'Internal',
  Restricted = 'Restricted',
  Confidential = 'Confidential',
  Secret = 'Secret'
}

export enum VersionChangeType {
  Created = 'Created',
  MinorEdit = 'MinorEdit',
  MajorEdit = 'MajorEdit',
  Annotation = 'Annotation',
  ImageEdit = 'ImageEdit',
  Restored = 'Restored'
}

export enum AnnotationType {
  Text = 'Text',
  Highlight = 'Highlight',
  Underline = 'Underline',
  Strikethrough = 'Strikethrough',
  FreeHand = 'FreeHand',
  Rectangle = 'Rectangle',
  Circle = 'Circle',
  Arrow = 'Arrow',
  Stamp = 'Stamp',
  Image = 'Image',
  Comment = 'Comment'
}

export enum DocumentActivityType {
  Created = 'Created',
  Viewed = 'Viewed',
  Downloaded = 'Downloaded',
  Edited = 'Edited',
  VersionCreated = 'VersionCreated',
  Renamed = 'Renamed',
  Moved = 'Moved',
  Deleted = 'Deleted',
  Restored = 'Restored',
  Shared = 'Shared',
  Locked = 'Locked',
  Unlocked = 'Unlocked',
  PermissionsChanged = 'PermissionsChanged',
  Annotated = 'Annotated',
  CommentAdded = 'CommentAdded',
  MetadataChanged = 'MetadataChanged'
}

export enum FolderAccessLevel {
  Public = 'Public',
  Internal = 'Internal',
  Restricted = 'Restricted',
  Private = 'Private'
}

export enum MetadataType {
  String = 'String',
  Number = 'Number',
  Date = 'Date',
  Boolean = 'Boolean',
  JSON = 'JSON'
}

// Helper interfaces for UI
export interface DocumentViewConfig {
  showVersionHistory: boolean;
  showAnnotations: boolean;
  showActivityLog: boolean;
  showMetadata: boolean;
  editMode: boolean;
}

export interface AnnotationTool {
  type: AnnotationType;
  color: string;
  opacity: number;
  fontSize?: number;
  active: boolean;
}

export interface BulkOperationResult {
  successCount: number;
  failureCount: number;
  errors: string[];
}
