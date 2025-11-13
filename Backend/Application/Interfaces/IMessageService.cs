using Backend.Application.DTOs.Messages;

namespace Backend.Application.Interfaces;

public interface IMessageService
{
    // ===== Message CRUD =====
    Task<MessageDto> GetByIdAsync(int id, string userId);
    Task<MessageDto> GetByMessageIdAsync(string messageId, string userId);
    Task<List<MessageDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50);
    Task<List<MessageDto>> GetByFolderAsync(int folderId, string userId, int pageNumber = 1, int pageSize = 50);
    Task<List<MessageDto>> GetUnreadAsync(string userId, int pageNumber = 1, int pageSize = 50);
    Task<List<MessageDto>> GetFlaggedAsync(string userId, int pageNumber = 1, int pageSize = 50);
    Task<List<MessageDto>> GetDraftsAsync(string userId, int pageNumber = 1, int pageSize = 50);
    Task<List<MessageDto>> SearchAsync(string query, string userId, int pageNumber = 1, int pageSize = 50);
    Task<MessageDto> CreateDraftAsync(CreateMessageDto dto, string userId);
    Task<MessageDto> SendAsync(SendMessageDto dto, string userId);
    Task<MessageDto> ReplyAsync(int messageId, ReplyMessageDto dto, string userId);
    Task<MessageDto> ForwardAsync(int messageId, ForwardMessageDto dto, string userId);
    Task<bool> DeleteAsync(int id, string userId);
    Task<bool> DeletePermanentlyAsync(int id, string userId);
    Task<bool> DeleteMultipleAsync(List<int> ids, string userId);

    // ===== Message Actions =====
    Task<bool> MarkAsReadAsync(int id, string userId);
    Task<bool> MarkAsUnreadAsync(int id, string userId);
    Task<bool> MarkMultipleAsReadAsync(List<int> ids, string userId);
    Task<bool> FlagAsync(int id, string userId);
    Task<bool> UnflagAsync(int id, string userId);
    Task<bool> MoveToFolderAsync(int id, int folderId, string userId);
    Task<bool> MoveMultipleToFolderAsync(List<int> ids, int folderId, string userId);
    Task<bool> UpdateAsync(int id, UpdateMessageDto dto, string userId);
    Task<bool> ArchiveAsync(int id, string userId);
    Task<bool> ArchiveMultipleAsync(List<int> ids, string userId);

    // ===== Categories =====
    Task<bool> AddCategoryAsync(int messageId, int categoryId, string userId);
    Task<bool> RemoveCategoryAsync(int messageId, int categoryId, string userId);
    Task<List<MessageCategoryDto>> GetCategoriesAsync(string userId);
    Task<MessageCategoryDto> CreateCategoryAsync(CreateMessageCategoryDto dto, string userId);
    Task<bool> UpdateCategoryAsync(int id, UpdateMessageCategoryDto dto, string userId);
    Task<bool> DeleteCategoryAsync(int id, string userId);

    // ===== Folders =====
    Task<List<MessageFolderDto>> GetFoldersAsync(string userId);
    Task<MessageFolderDto> GetFolderByIdAsync(int id, string userId);
    Task<MessageFolderDto> CreateFolderAsync(CreateMessageFolderDto dto, string userId);
    Task<bool> UpdateFolderAsync(int id, UpdateMessageFolderDto dto, string userId);
    Task<bool> DeleteFolderAsync(int id, string userId);
    Task<int> GetFolderUnreadCountAsync(int folderId, string userId);
    Task<int> GetTotalUnreadCountAsync(string userId);

    // ===== Attachments =====
    Task<List<MessageAttachmentDto>> GetAttachmentsAsync(int messageId, string userId);
    Task<MessageAttachmentDto> GetAttachmentByIdAsync(int attachmentId, string userId);
    Task<byte[]> DownloadAttachmentAsync(int attachmentId, string userId);

    // ===== Conversation Threads =====
    Task<List<MessageDto>> GetConversationThreadAsync(int threadId, string userId);
    Task<List<MessageDto>> GetRelatedMessagesAsync(int messageId, string userId);

    // ===== Statistics =====
    Task<MessageStatisticsDto> GetStatisticsAsync(string userId);
}

public class MessageStatisticsDto
{
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
    public int FlaggedMessages { get; set; }
    public int DraftMessages { get; set; }
    public int TotalFolders { get; set; }
    public Dictionary<string, int> MessagesByFolder { get; set; } = new();
    public Dictionary<string, int> MessagesByCategory { get; set; } = new();
}
