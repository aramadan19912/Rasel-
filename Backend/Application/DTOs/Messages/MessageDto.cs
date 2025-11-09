namespace Backend.Application.DTOs.Messages;

public class MessageDto
{
    public int Id { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string BodyPreview { get; set; } = string.Empty;
    public MessageBodyType BodyType { get; set; }

    // Sender & Recipients
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderEmail { get; set; } = string.Empty;
    public List<MessageRecipientDto> ToRecipients { get; set; } = new();
    public List<MessageRecipientDto> CcRecipients { get; set; } = new();
    public List<MessageRecipientDto> BccRecipients { get; set; } = new();

    // Dates
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? ReceivedAt { get; set; }

    // Status
    public bool IsRead { get; set; }
    public bool IsFlagged { get; set; }
    public bool IsDraft { get; set; }
    public MessagePriority Priority { get; set; }
    public MessageSensitivity Sensitivity { get; set; }

    // Organization
    public int? FolderId { get; set; }
    public string? FolderName { get; set; }
    public List<int> CategoryIds { get; set; } = new();
    public List<string> CategoryNames { get; set; } = new();

    // Thread
    public int? ConversationThreadId { get; set; }
    public string? ConversationTopic { get; set; }
    public string? InReplyTo { get; set; }
    public bool HasAttachments { get; set; }
    public List<MessageAttachmentDto> Attachments { get; set; } = new();

    // Tracking
    public bool ReadReceiptRequested { get; set; }
    public bool DeliveryReceiptRequested { get; set; }
}

public class MessageRecipientDto
{
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public RecipientType Type { get; set; }
}

public class CreateMessageDto
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public MessageBodyType BodyType { get; set; } = MessageBodyType.Html;

    public List<string> ToRecipients { get; set; } = new();
    public List<string>? CcRecipients { get; set; }
    public List<string>? BccRecipients { get; set; }

    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    public MessageSensitivity Sensitivity { get; set; } = MessageSensitivity.Normal;

    public int? FolderId { get; set; }
    public List<int>? CategoryIds { get; set; }

    public string? InReplyTo { get; set; }
    public bool SaveToSentItems { get; set; } = true;
    public bool ReadReceiptRequested { get; set; }
    public bool DeliveryReceiptRequested { get; set; }

    public List<CreateMessageAttachmentDto>? Attachments { get; set; }
}

public class UpdateMessageDto
{
    public bool? IsRead { get; set; }
    public bool? IsFlagged { get; set; }
    public int? FolderId { get; set; }
    public List<int>? CategoryIds { get; set; }
    public MessagePriority? Priority { get; set; }
}

public class SendMessageDto : CreateMessageDto
{
    public DateTime? ScheduledSendTime { get; set; }
}

public class ReplyMessageDto
{
    public string Body { get; set; } = string.Empty;
    public MessageBodyType BodyType { get; set; } = MessageBodyType.Html;
    public bool ReplyAll { get; set; }
    public List<CreateMessageAttachmentDto>? Attachments { get; set; }
}

public class ForwardMessageDto
{
    public List<string> ToRecipients { get; set; } = new();
    public List<string>? CcRecipients { get; set; }
    public string? AdditionalBody { get; set; }
    public List<CreateMessageAttachmentDto>? AdditionalAttachments { get; set; }
}

public enum MessageBodyType
{
    Text = 0,
    Html = 1
}

public enum MessagePriority
{
    Low = 0,
    Normal = 1,
    High = 2
}

public enum MessageSensitivity
{
    Normal = 0,
    Personal = 1,
    Private = 2,
    Confidential = 3
}

public enum RecipientType
{
    To = 0,
    Cc = 1,
    Bcc = 2
}
