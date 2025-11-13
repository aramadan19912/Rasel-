export interface Message {
  id: number;
  messageId: string;
  subject: string;
  body: string;
  bodyPreview: string;
  bodyType: MessageBodyType;

  // Sender & Recipients
  sender: User | null;
  toRecipients: Recipient[];
  ccRecipients: Recipient[];
  bccRecipients: Recipient[];

  // Dates
  createdAt: Date;
  sentAt?: Date;
  receivedAt?: Date;
  readAt?: Date;

  // Status & Flags
  isRead: boolean;
  isDraft: boolean;
  importance: MessageImportance;
  sensitivity: MessageSensitivity;
  hasAttachments: boolean;
  isFlagged: boolean;
  flagDueDate?: Date;
  flagStatus: FlagStatus;

  // Organization
  categories: MessageCategory[];
  folderId?: number;
  folderName?: string;

  // Threading
  conversationId?: string;
  conversationTopic?: string;
  parentMessageId?: number;
  replyCount: number;

  // Attachments
  attachments: Attachment[];

  // Metadata
  isArchived: boolean;
  isJunk: boolean;
  mentions: MessageMention[];
  reactions: MessageReaction[];
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  avatar?: string;
}

export interface Recipient {
  email: string;
  displayName: string;
  type: RecipientType;
}

export interface Attachment {
  id: number;
  name: string;
  contentType: string;
  size: number;
  isInline: boolean;
}

export interface MessageCategory {
  id: number;
  name: string;
  color: string;
  isDefault: boolean;
}

export interface MessageMention {
  id: number;
  mentionedUserId: string;
  mentionedUserName: string;
  mentionText: string;
}

export interface MessageReaction {
  id: number;
  userId: string;
  userName: string;
  reactionType: ReactionType;
  reactedAt: Date;
}

export enum MessageBodyType {
  Text = 0,
  HTML = 1,
  RTF = 2
}

export enum MessageImportance {
  Low = 0,
  Normal = 1,
  High = 2
}

export enum MessageSensitivity {
  Normal = 0,
  Personal = 1,
  Private = 2,
  Confidential = 3
}

export enum FlagStatus {
  NotFlagged = 0,
  Flagged = 1,
  Complete = 2
}

export enum RecipientType {
  To = 0,
  Cc = 1,
  Bcc = 2
}

export enum ReactionType {
  Like = 1,
  Love = 2,
  Laugh = 3,
  Wow = 4,
  Sad = 5,
  Angry = 6
}

export interface CreateMessageDto {
  subject: string;
  body: string;
  bodyType: MessageBodyType;
  toRecipients: Recipient[];
  ccRecipients: Recipient[];
  bccRecipients: Recipient[];
  importance: MessageImportance;
  sensitivity: MessageSensitivity;
  requestReadReceipt: boolean;
  requestDeliveryReceipt: boolean;
  isDraft: boolean;
}

export interface ReplyDto {
  body: string;
  bodyType: MessageBodyType;
  additionalRecipients: Recipient[];
}

export interface ForwardDto {
  recipients: string[];
  additionalMessage?: string;
}
