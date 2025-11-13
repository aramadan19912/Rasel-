using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs.Messages;
using Backend.Application.Interfaces;
using Backend.Domain.Entities.Messages;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Services;

public class MessageService : IMessageService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;

    public MessageService(ApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    // ===== Message CRUD =====
    public async Task<MessageDto> GetByIdAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.read"))
            throw new UnauthorizedAccessException("Permission denied: messages.read");

        var message = await _context.Messages
            .Include(m => m.Folder)
            .Include(m => m.Attachments)
            .Include(m => m.Recipients)
            .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

        if (message == null)
            throw new KeyNotFoundException($"Message with ID {id} not found");

        return MapToDto(message);
    }

    public async Task<MessageDto> GetByMessageIdAsync(string messageId, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.read"))
            throw new UnauthorizedAccessException("Permission denied: messages.read");

        var message = await _context.Messages
            .Include(m => m.Folder)
            .Include(m => m.Attachments)
            .Include(m => m.Recipients)
            .FirstOrDefaultAsync(m => m.MessageId == messageId && m.UserId == userId);

        if (message == null)
            throw new KeyNotFoundException($"Message with MessageId {messageId} not found");

        return MapToDto(message);
    }

    public async Task<List<MessageDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.read"))
            throw new UnauthorizedAccessException("Permission denied: messages.read");

        var messages = await _context.Messages
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Include(m => m.Folder)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(MapToDto).ToList();
    }

    public async Task<List<MessageDto>> GetByFolderAsync(int folderId, string userId, int pageNumber = 1, int pageSize = 50)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.read"))
            throw new UnauthorizedAccessException("Permission denied: messages.read");

        var messages = await _context.Messages
            .Where(m => m.UserId == userId && m.FolderId == folderId && !m.IsDeleted)
            .Include(m => m.Folder)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(MapToDto).ToList();
    }

    public async Task<List<MessageDto>> GetUnreadAsync(string userId, int pageNumber = 1, int pageSize = 50)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.read"))
            throw new UnauthorizedAccessException("Permission denied: messages.read");

        var messages = await _context.Messages
            .Where(m => m.UserId == userId && !m.IsRead && !m.IsDeleted)
            .Include(m => m.Folder)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(MapToDto).ToList();
    }

    public async Task<List<MessageDto>> GetFlaggedAsync(string userId, int pageNumber = 1, int pageSize = 50)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.read"))
            throw new UnauthorizedAccessException("Permission denied: messages.read");

        var messages = await _context.Messages
            .Where(m => m.UserId == userId && m.IsFlagged && !m.IsDeleted)
            .Include(m => m.Folder)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(MapToDto).ToList();
    }

    public async Task<List<MessageDto>> GetDraftsAsync(string userId, int pageNumber = 1, int pageSize = 50)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.read"))
            throw new UnauthorizedAccessException("Permission denied: messages.read");

        var messages = await _context.Messages
            .Where(m => m.UserId == userId && m.IsDraft && !m.IsDeleted)
            .Include(m => m.Folder)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(MapToDto).ToList();
    }

    public async Task<List<MessageDto>> SearchAsync(string query, string userId, int pageNumber = 1, int pageSize = 50)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.read"))
            throw new UnauthorizedAccessException("Permission denied: messages.read");

        var messages = await _context.Messages
            .Where(m => m.UserId == userId && !m.IsDeleted &&
                (m.Subject.Contains(query) ||
                 m.Body.Contains(query) ||
                 m.FromEmail.Contains(query)))
            .Include(m => m.Folder)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return messages.Select(MapToDto).ToList();
    }

    public async Task<MessageDto> CreateDraftAsync(CreateMessageDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.create"))
            throw new UnauthorizedAccessException("Permission denied: messages.create");

        var message = new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            UserId = userId,
            Subject = dto.Subject,
            Body = dto.Body,
            BodyType = (int)dto.BodyType,
            IsDraft = true,
            Priority = (int)dto.Priority,
            Sensitivity = (int)dto.Sensitivity,
            FolderId = dto.FolderId,
            CreatedAt = DateTime.UtcNow
        };

        // Add recipients
        foreach (var recipientEmail in dto.ToRecipients)
        {
            message.Recipients.Add(new MessageRecipient
            {
                Email = recipientEmail,
                RecipientType = 0 // To
            });
        }

        if (dto.CcRecipients != null)
        {
            foreach (var recipientEmail in dto.CcRecipients)
            {
                message.Recipients.Add(new MessageRecipient
                {
                    Email = recipientEmail,
                    RecipientType = 1 // Cc
                });
            }
        }

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return MapToDto(message);
    }

    public async Task<MessageDto> SendAsync(SendMessageDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.send"))
            throw new UnauthorizedAccessException("Permission denied: messages.send");

        var message = new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            UserId = userId,
            Subject = dto.Subject,
            Body = dto.Body,
            BodyType = (int)dto.BodyType,
            IsDraft = false,
            Priority = (int)dto.Priority,
            Sensitivity = (int)dto.Sensitivity,
            FolderId = dto.FolderId,
            SentAt = dto.ScheduledSendTime ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Add recipients
        foreach (var recipientEmail in dto.ToRecipients)
        {
            message.Recipients.Add(new MessageRecipient { Email = recipientEmail, RecipientType = 0 });
        }

        if (dto.CcRecipients != null)
        {
            foreach (var email in dto.CcRecipients)
            {
                message.Recipients.Add(new MessageRecipient { Email = email, RecipientType = 1 });
            }
        }

        if (dto.BccRecipients != null)
        {
            foreach (var email in dto.BccRecipients)
            {
                message.Recipients.Add(new MessageRecipient { Email = email, RecipientType = 2 });
            }
        }

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return MapToDto(message);
    }

    public async Task<MessageDto> ReplyAsync(int messageId, ReplyMessageDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.send"))
            throw new UnauthorizedAccessException("Permission denied: messages.send");

        var originalMessage = await _context.Messages
            .Include(m => m.Recipients)
            .FirstOrDefaultAsync(m => m.Id == messageId && m.UserId == userId);

        if (originalMessage == null)
            throw new KeyNotFoundException($"Message with ID {messageId} not found");

        var replyMessage = new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            UserId = userId,
            Subject = originalMessage.Subject.StartsWith("Re:") ? originalMessage.Subject : $"Re: {originalMessage.Subject}",
            Body = dto.Body,
            BodyType = (int)dto.BodyType,
            IsDraft = false,
            InReplyTo = originalMessage.MessageId,
            ConversationThreadId = originalMessage.ConversationThreadId,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Add original sender as recipient
        replyMessage.Recipients.Add(new MessageRecipient
        {
            Email = originalMessage.FromEmail,
            RecipientType = 0
        });

        // If reply all, add all original recipients
        if (dto.ReplyAll)
        {
            foreach (var recipient in originalMessage.Recipients.Where(r => r.RecipientType == 0))
            {
                replyMessage.Recipients.Add(new MessageRecipient
                {
                    Email = recipient.Email,
                    RecipientType = 1 // Cc
                });
            }
        }

        _context.Messages.Add(replyMessage);
        await _context.SaveChangesAsync();

        return MapToDto(replyMessage);
    }

    public async Task<MessageDto> ForwardAsync(int messageId, ForwardMessageDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.send"))
            throw new UnauthorizedAccessException("Permission denied: messages.send");

        var originalMessage = await _context.Messages
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == messageId && m.UserId == userId);

        if (originalMessage == null)
            throw new KeyNotFoundException($"Message with ID {messageId} not found");

        var forwardMessage = new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            UserId = userId,
            Subject = originalMessage.Subject.StartsWith("Fw:") ? originalMessage.Subject : $"Fw: {originalMessage.Subject}",
            Body = $"{dto.AdditionalBody}\n\n--- Forwarded Message ---\n{originalMessage.Body}",
            BodyType = originalMessage.BodyType,
            IsDraft = false,
            SentAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Add recipients
        foreach (var recipientEmail in dto.ToRecipients)
        {
            forwardMessage.Recipients.Add(new MessageRecipient { Email = recipientEmail, RecipientType = 0 });
        }

        if (dto.CcRecipients != null)
        {
            foreach (var email in dto.CcRecipients)
            {
                forwardMessage.Recipients.Add(new MessageRecipient { Email = email, RecipientType = 1 });
            }
        }

        _context.Messages.Add(forwardMessage);
        await _context.SaveChangesAsync();

        return MapToDto(forwardMessage);
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.delete"))
            throw new UnauthorizedAccessException("Permission denied: messages.delete");

        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (message == null) return false;

        message.IsDeleted = true;
        message.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeletePermanentlyAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.delete"))
            throw new UnauthorizedAccessException("Permission denied: messages.delete");

        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (message == null) return false;

        _context.Messages.Remove(message);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteMultipleAsync(List<int> ids, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "messages.delete"))
            throw new UnauthorizedAccessException("Permission denied: messages.delete");

        var messages = await _context.Messages
            .Where(m => ids.Contains(m.Id) && m.UserId == userId)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Message Actions =====
    public async Task<bool> MarkAsReadAsync(int id, string userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (message == null) return false;

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAsUnreadAsync(int id, string userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (message == null) return false;

        message.IsRead = false;
        message.ReadAt = null;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkMultipleAsReadAsync(List<int> ids, string userId)
    {
        var messages = await _context.Messages
            .Where(m => ids.Contains(m.Id) && m.UserId == userId)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> FlagAsync(int id, string userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (message == null) return false;

        message.IsFlagged = true;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnflagAsync(int id, string userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (message == null) return false;

        message.IsFlagged = false;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MoveToFolderAsync(int id, int folderId, string userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (message == null) return false;

        var folder = await _context.MessageFolders.FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);
        if (folder == null) return false;

        message.FolderId = folderId;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MoveMultipleToFolderAsync(List<int> ids, int folderId, string userId)
    {
        var folder = await _context.MessageFolders.FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);
        if (folder == null) return false;

        var messages = await _context.Messages
            .Where(m => ids.Contains(m.Id) && m.UserId == userId)
            .ToListAsync();

        foreach (var message in messages)
        {
            message.FolderId = folderId;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(int id, UpdateMessageDto dto, string userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
        if (message == null) return false;

        if (dto.IsRead.HasValue) message.IsRead = dto.IsRead.Value;
        if (dto.IsFlagged.HasValue) message.IsFlagged = dto.IsFlagged.Value;
        if (dto.FolderId.HasValue) message.FolderId = dto.FolderId.Value;
        if (dto.Priority.HasValue) message.Priority = (int)dto.Priority.Value;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ArchiveAsync(int id, string userId)
    {
        var archiveFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f => f.UserId == userId && f.Type == FolderType.Archive);

        if (archiveFolder == null) return false;

        return await MoveToFolderAsync(id, archiveFolder.Id, userId);
    }

    public async Task<bool> ArchiveMultipleAsync(List<int> ids, string userId)
    {
        var archiveFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f => f.UserId == userId && f.Type == FolderType.Archive);

        if (archiveFolder == null) return false;

        return await MoveMultipleToFolderAsync(ids, archiveFolder.Id, userId);
    }

    // ===== Categories =====
    public async Task<bool> AddCategoryAsync(int messageId, int categoryId, string userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && m.UserId == userId);
        if (message == null) return false;

        var category = await _context.MessageCategories.FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);
        if (category == null) return false;

        message.Categories.Add(category);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveCategoryAsync(int messageId, int categoryId, string userId)
    {
        var message = await _context.Messages
            .Include(m => m.Categories)
            .FirstOrDefaultAsync(m => m.Id == messageId && m.UserId == userId);
        if (message == null) return false;

        var category = message.Categories.FirstOrDefault(c => c.Id == categoryId);
        if (category != null)
        {
            message.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<List<MessageCategoryDto>> GetCategoriesAsync(string userId)
    {
        var categories = await _context.MessageCategories
            .Where(c => c.UserId == userId)
            .ToListAsync();

        return categories.Select(c => new MessageCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Color = c.Color,
            IsDefault = c.IsDefault
        }).ToList();
    }

    public async Task<MessageCategoryDto> CreateCategoryAsync(CreateMessageCategoryDto dto, string userId)
    {
        var category = new MessageCategory
        {
            Name = dto.Name,
            Color = dto.Color,
            UserId = userId
        };

        _context.MessageCategories.Add(category);
        await _context.SaveChangesAsync();

        return new MessageCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Color = category.Color,
            IsDefault = category.IsDefault
        };
    }

    public async Task<bool> UpdateCategoryAsync(int id, UpdateMessageCategoryDto dto, string userId)
    {
        var category = await _context.MessageCategories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (category == null) return false;

        if (dto.Name != null) category.Name = dto.Name;
        if (dto.Color != null) category.Color = dto.Color;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id, string userId)
    {
        var category = await _context.MessageCategories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (category == null || category.IsDefault) return false;

        _context.MessageCategories.Remove(category);
        await _context.SaveChangesAsync();

        return true;
    }

    // ===== Folders =====
    public async Task<List<MessageFolderDto>> GetFoldersAsync(string userId)
    {
        var folders = await _context.MessageFolders
            .Where(f => f.UserId == userId)
            .Include(f => f.SubFolders)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();

        return folders.Select(MapFolderToDto).ToList();
    }

    public async Task<MessageFolderDto> GetFolderByIdAsync(int id, string userId)
    {
        var folder = await _context.MessageFolders
            .Include(f => f.SubFolders)
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);

        if (folder == null)
            throw new KeyNotFoundException($"Folder with ID {id} not found");

        return MapFolderToDto(folder);
    }

    public async Task<MessageFolderDto> CreateFolderAsync(CreateMessageFolderDto dto, string userId)
    {
        var folder = new MessageFolder
        {
            Name = dto.Name,
            DisplayName = dto.DisplayName,
            ParentFolderId = dto.ParentFolderId,
            Color = dto.Color,
            Icon = dto.Icon,
            UserId = userId,
            Type = FolderType.Custom
        };

        _context.MessageFolders.Add(folder);
        await _context.SaveChangesAsync();

        return MapFolderToDto(folder);
    }

    public async Task<bool> UpdateFolderAsync(int id, UpdateMessageFolderDto dto, string userId)
    {
        var folder = await _context.MessageFolders.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if (folder == null || folder.IsSystem) return false;

        if (dto.DisplayName != null) folder.DisplayName = dto.DisplayName;
        if (dto.Color != null) folder.Color = dto.Color;
        if (dto.Icon != null) folder.Icon = dto.Icon;
        if (dto.DisplayOrder.HasValue) folder.DisplayOrder = dto.DisplayOrder.Value;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteFolderAsync(int id, string userId)
    {
        var folder = await _context.MessageFolders.FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        if (folder == null || folder.IsSystem) return false;

        _context.MessageFolders.Remove(folder);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetFolderUnreadCountAsync(int folderId, string userId)
    {
        return await _context.Messages
            .CountAsync(m => m.FolderId == folderId && m.UserId == userId && !m.IsRead && !m.IsDeleted);
    }

    public async Task<int> GetTotalUnreadCountAsync(string userId)
    {
        return await _context.Messages
            .CountAsync(m => m.UserId == userId && !m.IsRead && !m.IsDeleted);
    }

    // ===== Attachments =====
    public async Task<List<MessageAttachmentDto>> GetAttachmentsAsync(int messageId, string userId)
    {
        var message = await _context.Messages
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == messageId && m.UserId == userId);

        if (message == null)
            throw new KeyNotFoundException($"Message with ID {messageId} not found");

        return message.Attachments.Select(a => new MessageAttachmentDto
        {
            Id = a.Id,
            Name = a.Name,
            ContentType = a.ContentType,
            Size = a.Size,
            FilePath = a.FilePath,
            IsInline = a.IsInline,
            ContentId = a.ContentId
        }).ToList();
    }

    public async Task<MessageAttachmentDto> GetAttachmentByIdAsync(int attachmentId, string userId)
    {
        var attachment = await _context.MessageAttachments
            .Include(a => a.Message)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.Message!.UserId == userId);

        if (attachment == null)
            throw new KeyNotFoundException($"Attachment with ID {attachmentId} not found");

        return new MessageAttachmentDto
        {
            Id = attachment.Id,
            Name = attachment.Name,
            ContentType = attachment.ContentType,
            Size = attachment.Size,
            FilePath = attachment.FilePath,
            IsInline = attachment.IsInline,
            ContentId = attachment.ContentId
        };
    }

    public async Task<byte[]> DownloadAttachmentAsync(int attachmentId, string userId)
    {
        var attachment = await _context.MessageAttachments
            .Include(a => a.Message)
            .FirstOrDefaultAsync(a => a.Id == attachmentId && a.Message!.UserId == userId);

        if (attachment == null)
            throw new KeyNotFoundException($"Attachment with ID {attachmentId} not found");

        return attachment.ContentBytes ?? Array.Empty<byte>();
    }

    // ===== Conversation Threads =====
    public async Task<List<MessageDto>> GetConversationThreadAsync(int threadId, string userId)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationThreadId == threadId && m.UserId == userId)
            .Include(m => m.Folder)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return messages.Select(MapToDto).ToList();
    }

    public async Task<List<MessageDto>> GetRelatedMessagesAsync(int messageId, string userId)
    {
        var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && m.UserId == userId);
        if (message == null || !message.ConversationThreadId.HasValue)
            return new List<MessageDto>();

        return await GetConversationThreadAsync(message.ConversationThreadId.Value, userId);
    }

    // ===== Statistics =====
    public async Task<MessageStatisticsDto> GetStatisticsAsync(string userId)
    {
        var totalMessages = await _context.Messages.CountAsync(m => m.UserId == userId && !m.IsDeleted);
        var unreadMessages = await _context.Messages.CountAsync(m => m.UserId == userId && !m.IsRead && !m.IsDeleted);
        var flaggedMessages = await _context.Messages.CountAsync(m => m.UserId == userId && m.IsFlagged && !m.IsDeleted);
        var draftMessages = await _context.Messages.CountAsync(m => m.UserId == userId && m.IsDraft && !m.IsDeleted);
        var totalFolders = await _context.MessageFolders.CountAsync(f => f.UserId == userId);

        return new MessageStatisticsDto
        {
            TotalMessages = totalMessages,
            UnreadMessages = unreadMessages,
            FlaggedMessages = flaggedMessages,
            DraftMessages = draftMessages,
            TotalFolders = totalFolders
        };
    }

    // ===== Helper Methods =====
    private MessageDto MapToDto(Message message)
    {
        return new MessageDto
        {
            Id = message.Id,
            MessageId = message.MessageId,
            Subject = message.Subject,
            Body = message.Body,
            BodyPreview = message.BodyPreview,
            BodyType = (MessageBodyType)message.BodyType,
            SenderId = message.UserId,
            SenderName = message.FromName,
            SenderEmail = message.FromEmail,
            ToRecipients = message.Recipients?.Where(r => r.RecipientType == 0).Select(r => new MessageRecipientDto
            {
                Email = r.Email,
                DisplayName = r.DisplayName,
                Type = (RecipientType)r.RecipientType
            }).ToList() ?? new List<MessageRecipientDto>(),
            CreatedAt = message.CreatedAt,
            SentAt = message.SentAt,
            ReadAt = message.ReadAt,
            IsRead = message.IsRead,
            IsFlagged = message.IsFlagged,
            IsDraft = message.IsDraft,
            Priority = (MessagePriority)message.Priority,
            Sensitivity = (MessageSensitivity)message.Sensitivity,
            FolderId = message.FolderId,
            FolderName = message.Folder?.DisplayName,
            ConversationThreadId = message.ConversationThreadId,
            InReplyTo = message.InReplyTo,
            HasAttachments = message.Attachments?.Any() ?? false,
            Attachments = message.Attachments?.Select(a => new MessageAttachmentDto
            {
                Id = a.Id,
                Name = a.Name,
                ContentType = a.ContentType,
                Size = a.Size,
                FilePath = a.FilePath,
                IsInline = a.IsInline,
                ContentId = a.ContentId
            }).ToList() ?? new List<MessageAttachmentDto>()
        };
    }

    private MessageFolderDto MapFolderToDto(MessageFolder folder)
    {
        return new MessageFolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            DisplayName = folder.DisplayName,
            ParentFolderId = folder.ParentFolderId,
            ParentFolderName = folder.ParentFolder?.DisplayName,
            Type = folder.Type,
            UnreadCount = folder.UnreadCount,
            TotalCount = folder.TotalCount,
            Color = folder.Color,
            Icon = folder.Icon,
            DisplayOrder = folder.DisplayOrder,
            SubFolders = folder.SubFolders?.Select(MapFolderToDto).ToList() ?? new List<MessageFolderDto>()
        };
    }
}
