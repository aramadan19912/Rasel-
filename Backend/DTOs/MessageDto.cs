using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string BodyPreview { get; set; } = string.Empty;
    public MessageBodyType BodyType { get; set; }

    // Sender
    public UserDto? Sender { get; set; }

    // Recipients
    public List<RecipientDto> ToRecipients { get; set; } = new();
    public List<RecipientDto> CcRecipients { get; set; } = new();
    public List<RecipientDto> BccRecipients { get; set; } = new();

    // Dates
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    // Status
    public bool IsRead { get; set; }
    public bool IsDraft { get; set; }
    public MessageImportance Importance { get; set; }
    public MessageSensitivity Sensitivity { get; set; }
    public bool HasAttachments { get; set; }
    public bool IsFlagged { get; set; }
    public DateTime? FlagDueDate { get; set; }
    public FlagStatus FlagStatus { get; set; }

    // Organization
    public List<MessageCategoryDto> Categories { get; set; } = new();
    public int? FolderId { get; set; }
    public string? FolderName { get; set; }

    // Threading
    public string? ConversationId { get; set; }
    public string? ConversationTopic { get; set; }
    public int? ParentMessageId { get; set; }
    public int ReplyCount { get; set; }

    // Attachments
    public List<AttachmentDto> Attachments { get; set; } = new();

    // Metadata
    public bool IsArchived { get; set; }
    public bool IsJunk { get; set; }
    public List<MessageMentionDto> Mentions { get; set; } = new();
    public List<MessageReactionDto> Reactions { get; set; } = new();
}

public class CreateMessageDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public MessageBodyType BodyType { get; set; } = MessageBodyType.HTML;
    public List<RecipientDto> ToRecipients { get; set; } = new();
    public List<RecipientDto> CcRecipients { get; set; } = new();
    public List<RecipientDto> BccRecipients { get; set; } = new();
    public MessageImportance Importance { get; set; } = MessageImportance.Normal;
    public MessageSensitivity Sensitivity { get; set; } = MessageSensitivity.Normal;
    public bool RequestReadReceipt { get; set; }
    public bool RequestDeliveryReceipt { get; set; }
    public bool IsDraft { get; set; } = true;
}

public class ReplyDto
{
    public string Body { get; set; } = string.Empty;
    public MessageBodyType BodyType { get; set; } = MessageBodyType.HTML;
    public List<RecipientDto> AdditionalRecipients { get; set; } = new();
}

public class ForwardDto
{
    public List<string> Recipients { get; set; } = new();
    public string? AdditionalMessage { get; set; }
}

public class QuickReplyDto
{
    public string Text { get; set; } = string.Empty;
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Avatar { get; set; }
}

public class RecipientDto
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public RecipientType Type { get; set; }
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public bool IsInline { get; set; }
}
