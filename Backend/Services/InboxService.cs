using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.Data;
using OutlookInboxManagement.DTOs;
using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.Services;

public class InboxService : IInboxService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IMessageRuleEngine _ruleEngine;
    private readonly INotificationService _notifications;

    public InboxService(
        ApplicationDbContext context,
        IMapper mapper,
        IMessageRuleEngine ruleEngine,
        INotificationService notifications)
    {
        _context = context;
        _mapper = mapper;
        _ruleEngine = ruleEngine;
        _notifications = notifications;
    }

    public async Task<PaginatedList<MessageDto>> GetInboxAsync(InboxQueryParameters parameters)
    {
        var query = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ToRecipients)
            .Include(m => m.Categories)
            .Include(m => m.Attachments)
            .Include(m => m.Reactions)
            .Include(m => m.Mentions)
            .AsQueryable();

        if (parameters.FolderId.HasValue)
        {
            query = query.Where(m => m.FolderId == parameters.FolderId);
        }

        // Apply filters
        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Where(m =>
                m.Subject.Contains(parameters.Search) ||
                m.Body.Contains(parameters.Search) ||
                m.Sender.FullName.Contains(parameters.Search) ||
                m.Sender.Email.Contains(parameters.Search));
        }

        if (parameters.IsUnreadOnly)
        {
            query = query.Where(m => !m.IsRead);
        }

        if (parameters.IsFlaggedOnly)
        {
            query = query.Where(m => m.IsFlagged);
        }

        if (parameters.HasAttachmentsOnly)
        {
            query = query.Where(m => m.HasAttachments);
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(m => m.Categories.Any(c => c.Id == parameters.CategoryId));
        }

        if (parameters.Importance.HasValue)
        {
            query = query.Where(m => m.Importance == parameters.Importance);
        }

        if (parameters.FromDate.HasValue)
        {
            query = query.Where(m => m.ReceivedAt >= parameters.FromDate);
        }

        if (parameters.ToDate.HasValue)
        {
            query = query.Where(m => m.ReceivedAt <= parameters.ToDate);
        }

        // Sorting
        query = parameters.SortBy switch
        {
            "date" => parameters.SortDescending
                ? query.OrderByDescending(m => m.ReceivedAt)
                : query.OrderBy(m => m.ReceivedAt),
            "sender" => parameters.SortDescending
                ? query.OrderByDescending(m => m.Sender.FullName)
                : query.OrderBy(m => m.Sender.FullName),
            "subject" => parameters.SortDescending
                ? query.OrderByDescending(m => m.Subject)
                : query.OrderBy(m => m.Subject),
            "importance" => parameters.SortDescending
                ? query.OrderByDescending(m => m.Importance)
                : query.OrderBy(m => m.Importance),
            _ => query.OrderByDescending(m => m.ReceivedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PaginatedList<MessageDto>(
            _mapper.Map<List<MessageDto>>(items),
            totalCount,
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<MessageDto> GetMessageAsync(int id)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ToRecipients)
            .Include(m => m.CcRecipients)
            .Include(m => m.BccRecipients)
            .Include(m => m.Categories)
            .Include(m => m.Attachments)
            .Include(m => m.Reactions).ThenInclude(r => r.User)
            .Include(m => m.Mentions).ThenInclude(m => m.MentionedUser)
            .Include(m => m.Folder)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
            throw new KeyNotFoundException($"Message with ID {id} not found");

        return _mapper.Map<MessageDto>(message);
    }

    public async Task<MessageDto> CreateDraftAsync(CreateMessageDto dto, string userId)
    {
        var draft = _mapper.Map<Message>(dto);
        draft.SenderId = userId;
        draft.IsDraft = true;
        draft.CreatedAt = DateTime.UtcNow;

        // Get Drafts folder
        var draftsFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f => f.UserId == userId && f.Type == FolderType.Drafts);

        if (draftsFolder != null)
        {
            draft.FolderId = draftsFolder.Id;
        }

        _context.Messages.Add(draft);
        await _context.SaveChangesAsync();

        return await GetMessageAsync(draft.Id);
    }

    public async Task<MessageDto> SendMessageAsync(int draftId)
    {
        var draft = await _context.Messages
            .Include(m => m.ToRecipients)
            .Include(m => m.CcRecipients)
            .Include(m => m.BccRecipients)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == draftId);

        if (draft == null)
            throw new KeyNotFoundException("Draft not found");

        if (!draft.ToRecipients.Any())
            throw new InvalidOperationException("At least one recipient is required");

        draft.IsDraft = false;
        draft.SentAt = DateTime.UtcNow;
        draft.DeliveryStatus = DeliveryStatus.Sent;

        // Move to Sent folder
        var sentFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f =>
                f.UserId == draft.SenderId &&
                f.Type == FolderType.Sent);

        if (sentFolder != null)
        {
            draft.FolderId = sentFolder.Id;
        }

        // Create tracking records
        var allRecipients = draft.ToRecipients.Concat(draft.CcRecipients).Concat(draft.BccRecipients);
        foreach (var recipient in allRecipients)
        {
            var tracking = new MessageTracking
            {
                MessageId = draft.Id,
                RecipientEmail = recipient.Email,
                Status = TrackingStatus.Sent,
                StatusDate = DateTime.UtcNow
            };
            _context.MessageTrackings.Add(tracking);
        }

        await _context.SaveChangesAsync();

        // Send notifications
        await _notifications.NotifyNewMessageAsync(draft);

        return await GetMessageAsync(draft.Id);
    }

    public async Task DeleteMessageAsync(int id, bool permanent = false)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        if (permanent)
        {
            _context.Messages.Remove(message);
        }
        else
        {
            // Move to Deleted folder
            var deletedFolder = await _context.MessageFolders
                .FirstOrDefaultAsync(f =>
                    f.UserId == message.SenderId &&
                    f.Type == FolderType.Deleted);

            if (deletedFolder != null)
            {
                message.FolderId = deletedFolder.Id;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task MoveToFolderAsync(int messageId, int folderId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.FolderId = folderId;
        await _context.SaveChangesAsync();
    }

    public async Task<List<MessageDto>> MoveMultipleAsync(List<int> messageIds, int folderId)
    {
        var messages = await _context.Messages
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
        {
            message.FolderId = folderId;
        }

        await _context.SaveChangesAsync();
        return _mapper.Map<List<MessageDto>>(messages);
    }

    public async Task MarkAsReadAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.IsRead = true;
        message.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsUnreadAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.IsRead = false;
        message.ReadAt = null;
        await _context.SaveChangesAsync();
    }

    public async Task MarkMultipleAsReadAsync(List<int> messageIds)
    {
        var messages = await _context.Messages
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<MessageFolderDto>> GetFoldersAsync(string userId)
    {
        var folders = await _context.MessageFolders
            .Where(f => f.UserId == userId)
            .Include(f => f.SubFolders)
            .OrderBy(f => f.DisplayOrder)
            .ToListAsync();

        return _mapper.Map<List<MessageFolderDto>>(folders);
    }

    public async Task<MessageFolderDto> CreateFolderAsync(CreateFolderDto dto, string userId)
    {
        var folder = _mapper.Map<MessageFolder>(dto);
        folder.UserId = userId;
        folder.IsSystem = false;

        _context.MessageFolders.Add(folder);
        await _context.SaveChangesAsync();

        return _mapper.Map<MessageFolderDto>(folder);
    }

    public async Task<MessageFolderDto> UpdateFolderAsync(int id, UpdateFolderDto dto)
    {
        var folder = await _context.MessageFolders.FindAsync(id);
        if (folder == null)
            throw new KeyNotFoundException("Folder not found");

        if (dto.DisplayName != null) folder.DisplayName = dto.DisplayName;
        if (dto.Color != null) folder.Color = dto.Color;
        if (dto.Icon != null) folder.Icon = dto.Icon;
        if (dto.DisplayOrder.HasValue) folder.DisplayOrder = dto.DisplayOrder.Value;

        await _context.SaveChangesAsync();
        return _mapper.Map<MessageFolderDto>(folder);
    }

    public async Task DeleteFolderAsync(int id)
    {
        var folder = await _context.MessageFolders.FindAsync(id);
        if (folder == null)
            throw new KeyNotFoundException("Folder not found");

        if (folder.IsSystem)
            throw new InvalidOperationException("Cannot delete system folders");

        _context.MessageFolders.Remove(folder);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUnreadCountAsync(int folderId)
    {
        return await _context.Messages
            .Where(m => m.FolderId == folderId && !m.IsRead)
            .CountAsync();
    }

    public async Task<List<MessageCategoryDto>> GetCategoriesAsync(string userId)
    {
        var categories = await _context.MessageCategories
            .Where(c => c.UserId == userId || c.IsDefault)
            .ToListAsync();

        return _mapper.Map<List<MessageCategoryDto>>(categories);
    }

    public async Task AssignCategoryAsync(int messageId, int categoryId)
    {
        var message = await _context.Messages
            .Include(m => m.Categories)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            throw new KeyNotFoundException("Message not found");

        var category = await _context.MessageCategories.FindAsync(categoryId);
        if (category == null)
            throw new KeyNotFoundException("Category not found");

        if (!message.Categories.Contains(category))
        {
            message.Categories.Add(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveCategoryAsync(int messageId, int categoryId)
    {
        var message = await _context.Messages
            .Include(m => m.Categories)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            throw new KeyNotFoundException("Message not found");

        var category = message.Categories.FirstOrDefault(c => c.Id == categoryId);
        if (category != null)
        {
            message.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<MessageCategoryDto> CreateCategoryAsync(CreateCategoryDto dto, string userId)
    {
        var category = _mapper.Map<MessageCategory>(dto);
        category.UserId = userId;
        category.IsDefault = false;

        _context.MessageCategories.Add(category);
        await _context.SaveChangesAsync();

        return _mapper.Map<MessageCategoryDto>(category);
    }

    public async Task FlagMessageAsync(int messageId, DateTime? dueDate)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.IsFlagged = true;
        message.FlagDueDate = dueDate;
        message.FlagStatus = FlagStatus.Flagged;

        await _context.SaveChangesAsync();
    }

    public async Task UnflagMessageAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.IsFlagged = false;
        message.FlagDueDate = null;
        message.FlagStatus = FlagStatus.NotFlagged;

        await _context.SaveChangesAsync();
    }

    public async Task MarkFlagCompleteAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.FlagStatus = FlagStatus.Complete;
        await _context.SaveChangesAsync();
    }

    public async Task SetReminderAsync(int messageId, DateTime reminderDate)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.ReminderDate = reminderDate;
        message.ReminderIsSet = true;

        await _context.SaveChangesAsync();
    }

    public async Task<PaginatedList<MessageDto>> SearchAsync(SearchParameters parameters)
    {
        var query = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ToRecipients)
            .Include(m => m.Categories)
            .Include(m => m.Attachments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(parameters.Search))
        {
            query = query.Where(m =>
                m.Subject.Contains(parameters.Search) ||
                m.Body.Contains(parameters.Search));
        }

        if (!string.IsNullOrEmpty(parameters.Sender))
        {
            query = query.Where(m => m.Sender.Email.Contains(parameters.Sender));
        }

        if (!string.IsNullOrEmpty(parameters.Subject))
        {
            query = query.Where(m => m.Subject.Contains(parameters.Subject));
        }

        if (parameters.HasAttachments.HasValue)
        {
            query = query.Where(m => m.HasAttachments == parameters.HasAttachments.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.ReceivedAt)
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PaginatedList<MessageDto>(
            _mapper.Map<List<MessageDto>>(items),
            totalCount,
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<List<MessageDto>> SearchByContentAsync(string query)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.Body.Contains(query) || m.Subject.Contains(query))
            .OrderByDescending(m => m.ReceivedAt)
            .Take(50)
            .ToListAsync();

        return _mapper.Map<List<MessageDto>>(messages);
    }

    public async Task<List<MessageDto>> SearchBySenderAsync(string email)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.Sender.Email == email)
            .OrderByDescending(m => m.ReceivedAt)
            .Take(50)
            .ToListAsync();

        return _mapper.Map<List<MessageDto>>(messages);
    }

    public async Task<List<MessageDto>> SearchByDateRangeAsync(DateTime from, DateTime to)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ReceivedAt >= from && m.ReceivedAt <= to)
            .OrderByDescending(m => m.ReceivedAt)
            .ToListAsync();

        return _mapper.Map<List<MessageDto>>(messages);
    }

    public async Task<ConversationThreadDto> GetConversationAsync(string conversationId)
    {
        var thread = await _context.ConversationThreads
            .Include(t => t.Messages).ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(t => t.ConversationId == conversationId);

        if (thread == null)
            throw new KeyNotFoundException("Conversation not found");

        return _mapper.Map<ConversationThreadDto>(thread);
    }

    public async Task<List<MessageDto>> GetRelatedMessagesAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        var related = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ConversationId == message.ConversationId)
            .OrderBy(m => m.ReceivedAt)
            .ToListAsync();

        return _mapper.Map<List<MessageDto>>(related);
    }

    public async Task<AttachmentDto> AddAttachmentAsync(int messageId, IFormFile file)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        var attachment = new MessageAttachment
        {
            MessageId = messageId,
            Name = file.FileName,
            ContentType = file.ContentType,
            Size = file.Length,
            ContentBytes = memoryStream.ToArray()
        };

        _context.MessageAttachments.Add(attachment);
        message.HasAttachments = true;

        await _context.SaveChangesAsync();

        return _mapper.Map<AttachmentDto>(attachment);
    }

    public async Task RemoveAttachmentAsync(int attachmentId)
    {
        var attachment = await _context.MessageAttachments.FindAsync(attachmentId);
        if (attachment == null)
            throw new KeyNotFoundException("Attachment not found");

        _context.MessageAttachments.Remove(attachment);

        // Check if message still has attachments
        var message = await _context.Messages
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == attachment.MessageId);

        if (message != null)
        {
            message.HasAttachments = message.Attachments.Count > 1;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<byte[]> DownloadAttachmentAsync(int attachmentId)
    {
        var attachment = await _context.MessageAttachments.FindAsync(attachmentId);
        if (attachment == null)
            throw new KeyNotFoundException("Attachment not found");

        return attachment.ContentBytes ?? Array.Empty<byte>();
    }

    public async Task<List<byte[]>> DownloadAllAttachmentsAsync(int messageId)
    {
        var attachments = await _context.MessageAttachments
            .Where(a => a.MessageId == messageId)
            .ToListAsync();

        return attachments.Select(a => a.ContentBytes ?? Array.Empty<byte>()).ToList();
    }

    public async Task<MessageRuleDto> CreateRuleAsync(CreateRuleDto dto)
    {
        var rule = _mapper.Map<MessageRule>(dto);
        rule.CreatedAt = DateTime.UtcNow;

        _context.MessageRules.Add(rule);
        await _context.SaveChangesAsync();

        return _mapper.Map<MessageRuleDto>(rule);
    }

    public async Task<List<MessageRuleDto>> GetRulesAsync(string userId)
    {
        var rules = await _context.MessageRules
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Priority)
            .ToListAsync();

        return _mapper.Map<List<MessageRuleDto>>(rules);
    }

    public async Task ApplyRuleAsync(int ruleId, int messageId)
    {
        var rule = await _context.MessageRules.FindAsync(ruleId);
        if (rule == null)
            throw new KeyNotFoundException("Rule not found");

        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        await _ruleEngine.ExecuteRuleAsync(rule, message);
        await _context.SaveChangesAsync();
    }

    public async Task ApplyRulesAsync(int messageId)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null) return;

        var rules = await _context.MessageRules
            .Where(r => r.IsEnabled)
            .OrderBy(r => r.Priority)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (await _ruleEngine.EvaluateConditionsAsync(rule, message))
            {
                await _ruleEngine.ExecuteRuleAsync(rule, message);
                message.ProcessedByRule = true;
                message.RuleId = rule.Id;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task QuickReplyAsync(int messageId, string replyText, string userId)
    {
        var originalMessage = await _context.Messages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (originalMessage == null)
            throw new KeyNotFoundException("Message not found");

        var reply = new Message
        {
            Subject = "RE: " + originalMessage.Subject,
            Body = replyText + "\n\n---Original Message---\n" + originalMessage.Body,
            SenderId = userId,
            ParentMessageId = messageId,
            ConversationId = originalMessage.ConversationId,
            IsDraft = false,
            SentAt = DateTime.UtcNow,
            ToRecipients = new List<MessageRecipient>
            {
                new MessageRecipient
                {
                    Email = originalMessage.Sender.Email,
                    DisplayName = originalMessage.Sender.FullName,
                    Type = RecipientType.To
                }
            }
        };

        _context.Messages.Add(reply);
        await _context.SaveChangesAsync();

        await _notifications.NotifyReplyAsync(reply);
    }

    public async Task ForwardAsync(int messageId, List<string> recipients, string userId)
    {
        var originalMessage = await _context.Messages
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (originalMessage == null)
            throw new KeyNotFoundException("Message not found");

        var forward = new Message
        {
            Subject = "FW: " + originalMessage.Subject,
            Body = originalMessage.Body,
            SenderId = userId,
            IsDraft = false,
            SentAt = DateTime.UtcNow,
            ToRecipients = recipients.Select(r => new MessageRecipient
            {
                Email = r,
                Type = RecipientType.To
            }).ToList()
        };

        _context.Messages.Add(forward);
        await _context.SaveChangesAsync();
    }

    public async Task ReplyAsync(int messageId, ReplyDto dto, string userId)
    {
        await QuickReplyAsync(messageId, dto.Body, userId);
    }

    public async Task ReplyAllAsync(int messageId, ReplyDto dto, string userId)
    {
        var originalMessage = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.ToRecipients)
            .Include(m => m.CcRecipients)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (originalMessage == null)
            throw new KeyNotFoundException("Message not found");

        var reply = new Message
        {
            Subject = "RE: " + originalMessage.Subject,
            Body = dto.Body + "\n\n---Original Message---\n" + originalMessage.Body,
            SenderId = userId,
            ParentMessageId = messageId,
            ConversationId = originalMessage.ConversationId,
            IsDraft = false,
            SentAt = DateTime.UtcNow
        };

        // Add all original recipients
        reply.ToRecipients = originalMessage.ToRecipients.Select(r => new MessageRecipient
        {
            Email = r.Email,
            DisplayName = r.DisplayName,
            Type = RecipientType.To
        }).ToList();

        reply.CcRecipients = originalMessage.CcRecipients.Select(r => new MessageRecipient
        {
            Email = r.Email,
            DisplayName = r.DisplayName,
            Type = RecipientType.Cc
        }).ToList();

        _context.Messages.Add(reply);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MessageDto>> GetMentionsAsync(string userId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Mentions)
            .Where(m => m.Mentions.Any(mention => mention.MentionedUserId == userId))
            .OrderByDescending(m => m.ReceivedAt)
            .ToListAsync();

        return _mapper.Map<List<MessageDto>>(messages);
    }

    public async Task AddMentionAsync(int messageId, string userId)
    {
        var message = await _context.Messages
            .Include(m => m.Mentions)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            throw new KeyNotFoundException("Message not found");

        var mention = new MessageMention
        {
            MessageId = messageId,
            MentionedUserId = userId
        };

        _context.MessageMentions.Add(mention);
        await _context.SaveChangesAsync();
    }

    public async Task AddReactionAsync(int messageId, ReactionType reactionType, string userId)
    {
        var message = await _context.Messages
            .Include(m => m.Reactions)
            .FirstOrDefaultAsync(m => m.Id == messageId);

        if (message == null)
            throw new KeyNotFoundException("Message not found");

        var existingReaction = message.Reactions.FirstOrDefault(r => r.UserId == userId);
        if (existingReaction != null)
        {
            existingReaction.ReactionType = reactionType;
        }
        else
        {
            var reaction = new MessageReaction
            {
                MessageId = messageId,
                UserId = userId,
                ReactionType = reactionType,
                ReactedAt = DateTime.UtcNow
            };
            _context.MessageReactions.Add(reaction);
        }

        await _context.SaveChangesAsync();
    }

    public async Task RemoveReactionAsync(int messageId, string userId)
    {
        var reaction = await _context.MessageReactions
            .FirstOrDefaultAsync(r => r.MessageId == messageId && r.UserId == userId);

        if (reaction != null)
        {
            _context.MessageReactions.Remove(reaction);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<MessageReactionDto>> GetReactionsAsync(int messageId)
    {
        var reactions = await _context.MessageReactions
            .Include(r => r.User)
            .Where(r => r.MessageId == messageId)
            .ToListAsync();

        return _mapper.Map<List<MessageReactionDto>>(reactions);
    }

    public async Task BulkDeleteAsync(List<int> messageIds)
    {
        var messages = await _context.Messages
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
        {
            // Move to deleted folder instead of permanent delete
            var deletedFolder = await _context.MessageFolders
                .FirstOrDefaultAsync(f =>
                    f.UserId == message.SenderId &&
                    f.Type == FolderType.Deleted);

            if (deletedFolder != null)
            {
                message.FolderId = deletedFolder.Id;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task BulkMoveAsync(List<int> messageIds, int folderId)
    {
        var messages = await _context.Messages
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
        {
            message.FolderId = folderId;
        }

        await _context.SaveChangesAsync();
    }

    public async Task BulkCategorizeAsync(List<int> messageIds, int categoryId)
    {
        var category = await _context.MessageCategories.FindAsync(categoryId);
        if (category == null)
            throw new KeyNotFoundException("Category not found");

        var messages = await _context.Messages
            .Include(m => m.Categories)
            .Where(m => messageIds.Contains(m.Id))
            .ToListAsync();

        foreach (var message in messages)
        {
            if (!message.Categories.Contains(category))
            {
                message.Categories.Add(category);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task BulkMarkAsReadAsync(List<int> messageIds)
    {
        await MarkMultipleAsReadAsync(messageIds);
    }

    public async Task ArchiveMessageAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.IsArchived = true;
        message.ArchivedAt = DateTime.UtcNow;

        // Move to Archive folder
        var archiveFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f =>
                f.UserId == message.SenderId &&
                f.Type == FolderType.Archive);

        if (archiveFolder != null)
        {
            message.FolderId = archiveFolder.Id;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UnarchiveMessageAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.IsArchived = false;
        message.ArchivedAt = null;

        // Move back to Inbox
        var inboxFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f =>
                f.UserId == message.SenderId &&
                f.Type == FolderType.Inbox);

        if (inboxFolder != null)
        {
            message.FolderId = inboxFolder.Id;
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsJunkAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.IsJunk = true;

        // Move to Junk folder
        var junkFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f =>
                f.UserId == message.SenderId &&
                f.Type == FolderType.Junk);

        if (junkFolder != null)
        {
            message.FolderId = junkFolder.Id;
        }

        await _context.SaveChangesAsync();
    }

    public async Task MarkAsNotJunkAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.IsJunk = false;

        // Move back to Inbox
        var inboxFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f =>
                f.UserId == message.SenderId &&
                f.Type == FolderType.Inbox);

        if (inboxFolder != null)
        {
            message.FolderId = inboxFolder.Id;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<double> GetSpamScoreAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        return message.SpamScore;
    }

    public async Task<List<MessageTrackingDto>> GetTrackingInfoAsync(int messageId)
    {
        var tracking = await _context.MessageTrackings
            .Where(t => t.MessageId == messageId)
            .ToListAsync();

        return _mapper.Map<List<MessageTrackingDto>>(tracking);
    }

    public async Task RequestReadReceiptAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.RequestReadReceipt = true;
        await _context.SaveChangesAsync();
    }

    public async Task RequestDeliveryReceiptAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message == null)
            throw new KeyNotFoundException("Message not found");

        message.RequestDeliveryReceipt = true;
        await _context.SaveChangesAsync();
    }

    public async Task<byte[]> ExportToEmlAsync(int messageId)
    {
        // Implementation for EML export
        var message = await GetMessageAsync(messageId);
        // Convert to EML format (simplified)
        var eml = $"From: {message.Sender?.Email}\nSubject: {message.Subject}\n\n{message.Body}";
        return System.Text.Encoding.UTF8.GetBytes(eml);
    }

    public async Task<byte[]> ExportToPdfAsync(int messageId)
    {
        // Implementation for PDF export
        // This would require a PDF library like iTextSharp or similar
        var message = await GetMessageAsync(messageId);
        return System.Text.Encoding.UTF8.GetBytes($"PDF Export: {message.Subject}");
    }

    public async Task<InboxStatisticsDto> GetStatisticsAsync(string userId)
    {
        var messages = await _context.Messages
            .Where(m => m.SenderId == userId)
            .ToListAsync();

        return new InboxStatisticsDto
        {
            TotalMessages = messages.Count,
            UnreadMessages = messages.Count(m => !m.IsRead),
            FlaggedMessages = messages.Count(m => m.IsFlagged),
            DraftMessages = messages.Count(m => m.IsDraft),
            TodayMessages = messages.Count(m =>
                m.ReceivedAt.HasValue &&
                m.ReceivedAt.Value.Date == DateTime.Today),
            ThisWeekMessages = messages.Count(m =>
                m.ReceivedAt.HasValue &&
                m.ReceivedAt.Value >= DateTime.Today.AddDays(-7)),
            AverageResponseTime = CalculateAverageResponseTime(messages),
            TopSenders = await GetTopSendersAsync(userId, 5)
        };
    }

    public async Task<List<TopSenderDto>> GetTopSendersAsync(string userId, int count)
    {
        var topSenders = await _context.Messages
            .Where(m => m.SenderId == userId)
            .GroupBy(m => new { m.Sender.Email, m.Sender.FullName })
            .Select(g => new TopSenderDto
            {
                Email = g.Key.Email,
                Name = g.Key.FullName,
                MessageCount = g.Count()
            })
            .OrderByDescending(s => s.MessageCount)
            .Take(count)
            .ToListAsync();

        return topSenders;
    }

    public async Task CleanupOldMessagesAsync(int olderThanDays)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        var oldMessages = await _context.Messages
            .Where(m => m.CreatedAt < cutoffDate && m.FolderId != null)
            .ToListAsync();

        _context.Messages.RemoveRange(oldMessages);
        await _context.SaveChangesAsync();
    }

    public async Task EmptyDeletedItemsAsync(string userId)
    {
        var deletedFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f => f.UserId == userId && f.Type == FolderType.Deleted);

        if (deletedFolder != null)
        {
            var messages = await _context.Messages
                .Where(m => m.FolderId == deletedFolder.Id)
                .ToListAsync();

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }
    }

    public async Task EmptyJunkFolderAsync(string userId)
    {
        var junkFolder = await _context.MessageFolders
            .FirstOrDefaultAsync(f => f.UserId == userId && f.Type == FolderType.Junk);

        if (junkFolder != null)
        {
            var messages = await _context.Messages
                .Where(m => m.FolderId == junkFolder.Id)
                .ToListAsync();

            _context.Messages.RemoveRange(messages);
            await _context.SaveChangesAsync();
        }
    }

    public async Task InitializeUserFoldersAsync(string userId)
    {
        var systemFolders = new List<MessageFolder>
        {
            new MessageFolder
            {
                Name = "Inbox",
                DisplayName = "Inbox",
                Type = FolderType.Inbox,
                UserId = userId,
                IsSystem = true,
                Icon = "inbox",
                DisplayOrder = 1
            },
            new MessageFolder
            {
                Name = "Drafts",
                DisplayName = "Drafts",
                Type = FolderType.Drafts,
                UserId = userId,
                IsSystem = true,
                Icon = "drafts",
                DisplayOrder = 2
            },
            new MessageFolder
            {
                Name = "Sent",
                DisplayName = "Sent Items",
                Type = FolderType.Sent,
                UserId = userId,
                IsSystem = true,
                Icon = "send",
                DisplayOrder = 3
            },
            new MessageFolder
            {
                Name = "Deleted",
                DisplayName = "Deleted Items",
                Type = FolderType.Deleted,
                UserId = userId,
                IsSystem = true,
                Icon = "delete",
                DisplayOrder = 4
            },
            new MessageFolder
            {
                Name = "Junk",
                DisplayName = "Junk Email",
                Type = FolderType.Junk,
                UserId = userId,
                IsSystem = true,
                Icon = "report",
                DisplayOrder = 5
            },
            new MessageFolder
            {
                Name = "Archive",
                DisplayName = "Archive",
                Type = FolderType.Archive,
                UserId = userId,
                IsSystem = true,
                Icon = "archive",
                DisplayOrder = 6
            }
        };

        _context.MessageFolders.AddRange(systemFolders);
        await _context.SaveChangesAsync();
    }

    private TimeSpan CalculateAverageResponseTime(List<Message> messages)
    {
        var repliesWithParent = messages
            .Where(m => m.ParentMessageId.HasValue && m.SentAt.HasValue)
            .Select(m => new
            {
                Reply = m,
                Original = messages.FirstOrDefault(om => om.Id == m.ParentMessageId)
            })
            .Where(x => x.Original != null && x.Original.SentAt.HasValue)
            .Select(x => x.Reply.SentAt!.Value - x.Original.SentAt!.Value)
            .ToList();

        return repliesWithParent.Any()
            ? TimeSpan.FromTicks((long)repliesWithParent.Average(t => t.Ticks))
            : TimeSpan.Zero;
    }
}
