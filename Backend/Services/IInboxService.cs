using OutlookInboxManagement.DTOs;
using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.Services;

public interface IInboxService
{
    // Basic Operations
    Task<PaginatedList<MessageDto>> GetInboxAsync(InboxQueryParameters parameters);
    Task<MessageDto> GetMessageAsync(int id);
    Task<MessageDto> CreateDraftAsync(CreateMessageDto dto, string userId);
    Task<MessageDto> SendMessageAsync(int draftId);
    Task DeleteMessageAsync(int id, bool permanent = false);
    Task MoveToFolderAsync(int messageId, int folderId);
    Task<List<MessageDto>> MoveMultipleAsync(List<int> messageIds, int folderId);

    // Read/Unread
    Task MarkAsReadAsync(int messageId);
    Task MarkAsUnreadAsync(int messageId);
    Task MarkMultipleAsReadAsync(List<int> messageIds);

    // Folders
    Task<List<MessageFolderDto>> GetFoldersAsync(string userId);
    Task<MessageFolderDto> CreateFolderAsync(CreateFolderDto dto, string userId);
    Task<MessageFolderDto> UpdateFolderAsync(int id, UpdateFolderDto dto);
    Task DeleteFolderAsync(int id);
    Task<int> GetUnreadCountAsync(int folderId);

    // Categories
    Task<List<MessageCategoryDto>> GetCategoriesAsync(string userId);
    Task AssignCategoryAsync(int messageId, int categoryId);
    Task RemoveCategoryAsync(int messageId, int categoryId);
    Task<MessageCategoryDto> CreateCategoryAsync(CreateCategoryDto dto, string userId);

    // Flags & Follow-up
    Task FlagMessageAsync(int messageId, DateTime? dueDate);
    Task UnflagMessageAsync(int messageId);
    Task MarkFlagCompleteAsync(int messageId);
    Task SetReminderAsync(int messageId, DateTime reminderDate);

    // Search
    Task<PaginatedList<MessageDto>> SearchAsync(SearchParameters parameters);
    Task<List<MessageDto>> SearchByContentAsync(string query);
    Task<List<MessageDto>> SearchBySenderAsync(string email);
    Task<List<MessageDto>> SearchByDateRangeAsync(DateTime from, DateTime to);

    // Conversations/Threading
    Task<ConversationThreadDto> GetConversationAsync(string conversationId);
    Task<List<MessageDto>> GetRelatedMessagesAsync(int messageId);

    // Attachments
    Task<AttachmentDto> AddAttachmentAsync(int messageId, IFormFile file);
    Task RemoveAttachmentAsync(int attachmentId);
    Task<byte[]> DownloadAttachmentAsync(int attachmentId);
    Task<List<byte[]>> DownloadAllAttachmentsAsync(int messageId);

    // Rules
    Task<MessageRuleDto> CreateRuleAsync(CreateRuleDto dto);
    Task<List<MessageRuleDto>> GetRulesAsync(string userId);
    Task ApplyRuleAsync(int ruleId, int messageId);
    Task ApplyRulesAsync(int messageId);

    // Quick Actions
    Task QuickReplyAsync(int messageId, string replyText, string userId);
    Task ForwardAsync(int messageId, List<string> recipients, string userId);
    Task ReplyAsync(int messageId, ReplyDto dto, string userId);
    Task ReplyAllAsync(int messageId, ReplyDto dto, string userId);

    // Mentions
    Task<List<MessageDto>> GetMentionsAsync(string userId);
    Task AddMentionAsync(int messageId, string userId);

    // Reactions
    Task AddReactionAsync(int messageId, ReactionType reactionType, string userId);
    Task RemoveReactionAsync(int messageId, string userId);
    Task<List<MessageReactionDto>> GetReactionsAsync(int messageId);

    // Bulk Operations
    Task BulkDeleteAsync(List<int> messageIds);
    Task BulkMoveAsync(List<int> messageIds, int folderId);
    Task BulkCategorizeAsync(List<int> messageIds, int categoryId);
    Task BulkMarkAsReadAsync(List<int> messageIds);

    // Archive
    Task ArchiveMessageAsync(int messageId);
    Task UnarchiveMessageAsync(int messageId);

    // Junk/Spam
    Task MarkAsJunkAsync(int messageId);
    Task MarkAsNotJunkAsync(int messageId);
    Task<double> GetSpamScoreAsync(int messageId);

    // Delivery & Tracking
    Task<List<MessageTrackingDto>> GetTrackingInfoAsync(int messageId);
    Task RequestReadReceiptAsync(int messageId);
    Task RequestDeliveryReceiptAsync(int messageId);

    // Export/Import
    Task<byte[]> ExportToEmlAsync(int messageId);
    Task<byte[]> ExportToPdfAsync(int messageId);

    // Statistics
    Task<InboxStatisticsDto> GetStatisticsAsync(string userId);
    Task<List<TopSenderDto>> GetTopSendersAsync(string userId, int count);

    // Cleanup
    Task CleanupOldMessagesAsync(int olderThanDays);
    Task EmptyDeletedItemsAsync(string userId);
    Task EmptyJunkFolderAsync(string userId);

    // User initialization
    Task InitializeUserFoldersAsync(string userId);
}
