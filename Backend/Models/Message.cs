using System.ComponentModel.DataAnnotations;

namespace OutlookInboxManagement.Models;

public class Message
{
    public int Id { get; set; }
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    // Basic Info
    [Required]
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string BodyPreview { get; set; } = string.Empty;
    public MessageBodyType BodyType { get; set; } = MessageBodyType.HTML;

    // Sender & Recipients
    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser? Sender { get; set; }
    public List<MessageRecipient> ToRecipients { get; set; } = new();
    public List<MessageRecipient> CcRecipients { get; set; } = new();
    public List<MessageRecipient> BccRecipients { get; set; } = new();

    // Dates
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? LastModified { get; set; }

    // Status & Flags
    public bool IsRead { get; set; }
    public bool IsDraft { get; set; } = true;
    public MessageImportance Importance { get; set; } = MessageImportance.Normal;
    public MessageSensitivity Sensitivity { get; set; } = MessageSensitivity.Normal;
    public bool HasAttachments { get; set; }
    public bool IsFlagged { get; set; }
    public DateTime? FlagDueDate { get; set; }
    public FlagStatus FlagStatus { get; set; } = FlagStatus.NotFlagged;

    // Categories & Organization
    public List<MessageCategory> Categories { get; set; } = new();
    public int? FolderId { get; set; }
    public MessageFolder? Folder { get; set; }
    public List<string> Tags { get; set; } = new();

    // Threading
    public string? ConversationId { get; set; }
    public string? ConversationTopic { get; set; }
    public int? ParentMessageId { get; set; }
    public Message? ParentMessage { get; set; }
    public List<Message> Replies { get; set; } = new();

    // Attachments
    public List<MessageAttachment> Attachments { get; set; } = new();

    // Delivery & Tracking
    public bool RequestReadReceipt { get; set; }
    public bool RequestDeliveryReceipt { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;
    public List<MessageTracking> TrackingInfo { get; set; } = new();

    // Security
    public bool IsEncrypted { get; set; }
    public bool IsSigned { get; set; }
    public string? DigitalSignature { get; set; }

    // Rules & Auto-processing
    public bool ProcessedByRule { get; set; }
    public int? RuleId { get; set; }

    // Search & Indexing
    public string SearchableContent { get; set; } = string.Empty;
    public DateTime? IndexedAt { get; set; }

    // Metadata
    public Dictionary<string, string> CustomProperties { get; set; } = new();
    public string? InternetMessageId { get; set; }
    public List<string> InternetMessageHeaders { get; set; } = new();

    // Retention & Archive
    public DateTime? RetentionDate { get; set; }
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }

    // Spam & Junk
    public bool IsJunk { get; set; }
    public double SpamScore { get; set; }

    // Mentions & @
    public List<MessageMention> Mentions { get; set; } = new();
    public bool MentionsCurrentUser { get; set; }

    // Reactions (like Outlook)
    public List<MessageReaction> Reactions { get; set; } = new();

    // Follow-up & Reminders
    public DateTime? FollowUpDate { get; set; }
    public DateTime? ReminderDate { get; set; }
    public bool ReminderIsSet { get; set; }
}

public enum MessageBodyType
{
    Text = 0,
    HTML = 1,
    RTF = 2
}

public enum MessageImportance
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

public enum FlagStatus
{
    NotFlagged = 0,
    Flagged = 1,
    Complete = 2
}

public enum DeliveryStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Read = 3,
    Failed = 4
}
