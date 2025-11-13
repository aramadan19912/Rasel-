// Archive Category Models

export interface ArchiveCategoryDto {
  id: number;
  categoryCode: string;
  nameAr: string;
  nameEn: string;
  descriptionAr?: string;
  descriptionEn?: string;
  classification: string;
  parentCategoryId?: number;
  parentCategoryName?: string;
  retentionPeriod: string;
  allowAttachments: boolean;
  requireApproval: boolean;
  icon?: string;
  color?: string;
  sortOrder: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  createdBy: string;
  updatedBy?: string;
}

export interface ArchiveCategoryHierarchyDto {
  id?: number;
  categoryCode?: string;
  nameAr?: string;
  nameEn?: string;
  classification?: string;
  icon?: string;
  color?: string;
  subCategories: ArchiveCategoryHierarchyDto[];
}

export interface ArchiveCategoryStatsDto {
  categoryId: number;
  categoryName: string;
  totalCorrespondences: number;
  archivedCorrespondences: number;
  pendingCorrespondences: number;
  completedCorrespondences: number;
  totalSize: number;
  lastCorrespondenceDate?: Date;
  oldestCorrespondenceDate?: Date;
}

export interface CreateArchiveCategoryRequest {
  categoryCode: string;
  nameAr: string;
  nameEn: string;
  descriptionAr?: string;
  descriptionEn?: string;
  classification: string;
  parentCategoryId?: number;
  retentionPeriod: string;
  allowAttachments: boolean;
  requireApproval: boolean;
  icon?: string;
  color?: string;
  sortOrder: number;
}

// Correspondence Models

export interface CorrespondenceDto {
  id: number;
  referenceNumber: string;
  subjectAr: string;
  subjectEn?: string;
  contentAr: string;
  contentEn?: string;
  categoryId: number;
  categoryName: string;
  classification: string;
  status: string;
  priority: string;
  confidentialityLevel: string;
  correspondenceDate: Date;
  dueDate?: Date;
  fromEmployeeId?: number;
  fromEmployeeName?: string;
  externalSenderName?: string;
  externalSenderOrganization?: string;
  toDepartmentId?: number;
  toDepartmentName?: string;
  toEmployeeId?: number;
  toEmployeeName?: string;
  formId?: number;
  formName?: string;
  formSubmissionId?: number;
  relatedCorrespondenceId?: number;
  relatedReferenceNumber?: string;
  attachments: CorrespondenceAttachmentDto[];
  routings: CorrespondenceRoutingDto[];
  archivedDocumentId?: number;
  archiveNumber?: string;
  pdfFilePath?: string;
  keywords?: string;
  tags?: string;
  notes?: string;
  isArchived: boolean;
  archivedAt?: Date;
  archivedBy?: string;
  createdAt: Date;
  updatedAt: Date;
  createdBy: string;
  updatedBy?: string;
}

export interface CorrespondenceSummaryDto {
  id: number;
  referenceNumber: string;
  subjectAr: string;
  categoryName: string;
  status: string;
  priority: string;
  correspondenceDate: Date;
  fromEmployeeName?: string;
  toEmployeeName?: string;
  attachmentCount: number;
  isArchived: boolean;
  createdAt: Date;
}

export interface CreateCorrespondenceRequest {
  subjectAr: string;
  subjectEn?: string;
  contentAr: string;
  contentEn?: string;
  categoryId: number;
  status?: string;
  priority?: string;
  confidentialityLevel?: string;
  correspondenceDate?: Date;
  dueDate?: Date;
  fromEmployeeId?: number;
  externalSenderName?: string;
  externalSenderOrganization?: string;
  toDepartmentId?: number;
  toEmployeeId?: number;
  formSubmissionId?: number;
  relatedCorrespondenceId?: number;
  keywords?: string;
  tags?: string;
  notes?: string;
}

export interface UpdateCorrespondenceRequest extends CreateCorrespondenceRequest {}

export interface CorrespondenceSearchRequest {
  searchTerm?: string;
  categoryId?: number;
  classification?: string;
  status?: string;
  priority?: string;
  dateFrom?: Date;
  dateTo?: Date;
  fromEmployeeId?: number;
  toEmployeeId?: number;
  toDepartmentId?: number;
  isArchived?: boolean;
  tags?: string;
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: string;
}

export interface CorrespondenceSearchResponse {
  items: CorrespondenceSummaryDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Correspondence Attachment Models

export interface CorrespondenceAttachmentDto {
  id: number;
  correspondenceId: number;
  fileName: string;
  originalFileName: string;
  filePath: string;
  fileSize: number;
  mimeType: string;
  fileExtension: string;
  description?: string;
  version: number;
  isMainDocument: boolean;
  sortOrder: number;
  createdAt: Date;
  createdBy: string;
}

export interface UploadAttachmentRequest {
  correspondenceId: number;
  description?: string;
  isMainDocument: boolean;
}

// Correspondence Routing Models

export interface CorrespondenceRoutingDto {
  id: number;
  correspondenceId: number;
  correspondenceReferenceNumber: string;
  fromEmployeeId: number;
  fromEmployeeName: string;
  fromEmployeePosition?: string;
  toEmployeeId: number;
  toEmployeeName: string;
  toEmployeePosition?: string;
  toDepartmentId?: number;
  toDepartmentName?: string;
  action: string;
  priority: string;
  instructions?: string;
  dueDate?: Date;
  routedDate: Date;
  receivedDate?: Date;
  isRead: boolean;
  response?: string;
  responseDate?: Date;
  status: string;
  completedDate?: Date;
  parentRoutingId?: number;
  sequenceNumber: number;
  createdAt: Date;
}

export interface RouteCorrespondenceRequest {
  correspondenceId: number;
  toEmployeeId: number;
  toDepartmentId?: number;
  action: string;
  priority: string;
  instructions?: string;
  dueDate?: Date;
}

export interface RespondToRoutingRequest {
  routingId: number;
  response: string;
  status: string;
}

export interface RoutingChainDto {
  correspondenceId: number;
  referenceNumber: string;
  routingHistory: CorrespondenceRoutingDto[];
  totalRoutings: number;
  currentStatus: string;
  currentAssigneeId?: number;
  currentAssigneeName?: string;
}

// Archive Operations

export interface ArchiveCorrespondenceRequest {
  correspondenceId: number;
  generatePdf: boolean;
  includeAttachments: boolean;
  applyWatermark: boolean;
  applyDigitalSignature: boolean;
  notes?: string;
}

// Enums

export enum CorrespondenceStatus {
  Draft = 'Draft',
  Pending = 'Pending',
  UnderReview = 'UnderReview',
  Approved = 'Approved',
  Rejected = 'Rejected',
  InProgress = 'InProgress',
  Completed = 'Completed',
  Archived = 'Archived',
  Cancelled = 'Cancelled'
}

export enum CorrespondencePriority {
  Low = 'Low',
  Normal = 'Normal',
  High = 'High',
  Urgent = 'Urgent',
  Critical = 'Critical'
}

export enum ConfidentialityLevel {
  Public = 'Public',
  Internal = 'Internal',
  Confidential = 'Confidential',
  Secret = 'Secret',
  TopSecret = 'TopSecret'
}

export enum RoutingAction {
  ForReview = 'ForReview',
  ForApproval = 'ForApproval',
  ForAction = 'ForAction',
  ForInformation = 'ForInformation',
  ForComment = 'ForComment',
  ForSignature = 'ForSignature'
}

export enum ArchiveClassification {
  Contract = 'Contract',
  Complaint = 'Complaint',
  Meeting = 'Meeting',
  Memorandum = 'Memorandum',
  Decision = 'Decision',
  Report = 'Report',
  Invoice = 'Invoice',
  PurchaseOrder = 'PurchaseOrder',
  HR_Document = 'HR_Document',
  Legal = 'Legal',
  Financial = 'Financial',
  Technical = 'Technical',
  Administrative = 'Administrative',
  Correspondence = 'Correspondence',
  Other = 'Other'
}

export enum RetentionPeriod {
  OneYear = 'OneYear',
  TwoYears = 'TwoYears',
  ThreeYears = 'ThreeYears',
  FiveYears = 'FiveYears',
  SevenYears = 'SevenYears',
  TenYears = 'TenYears',
  Permanent = 'Permanent'
}
