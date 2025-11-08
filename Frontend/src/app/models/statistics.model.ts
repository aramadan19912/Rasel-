export interface InboxStatistics {
  totalMessages: number;
  unreadMessages: number;
  flaggedMessages: number;
  draftMessages: number;
  todayMessages: number;
  thisWeekMessages: number;
  averageResponseTime: string;
  topSenders: TopSender[];
}

export interface TopSender {
  email: string;
  name: string;
  messageCount: number;
}

export interface ConversationThread {
  id: number;
  conversationId: string;
  topic: string;
  startedAt: Date;
  lastMessageAt: Date;
  messageCount: number;
  participants: string[];
  hasAttachments: boolean;
  importance: number;
  messages: any[];
}
