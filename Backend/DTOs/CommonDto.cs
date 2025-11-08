using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.DTOs;

public class MessageMentionDto
{
    public int Id { get; set; }
    public string MentionedUserId { get; set; } = string.Empty;
    public string MentionedUserName { get; set; } = string.Empty;
    public string MentionText { get; set; } = string.Empty;
}

public class MessageReactionDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public ReactionType ReactionType { get; set; }
    public DateTime ReactedAt { get; set; }
}

public class MessageTrackingDto
{
    public string RecipientEmail { get; set; } = string.Empty;
    public TrackingStatus Status { get; set; }
    public DateTime StatusDate { get; set; }
    public string? StatusMessage { get; set; }
}

public class ConversationThreadDto
{
    public int Id { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public int MessageCount { get; set; }
    public List<string> Participants { get; set; } = new();
    public bool HasAttachments { get; set; }
    public MessageImportance Importance { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
}

public class InboxStatisticsDto
{
    public int TotalMessages { get; set; }
    public int UnreadMessages { get; set; }
    public int FlaggedMessages { get; set; }
    public int DraftMessages { get; set; }
    public int TodayMessages { get; set; }
    public int ThisWeekMessages { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public List<TopSenderDto> TopSenders { get; set; } = new();
}

public class TopSenderDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int MessageCount { get; set; }
}

public class MoveToFolderDto
{
    public int FolderId { get; set; }
}

public class BulkMoveDto
{
    public List<int> MessageIds { get; set; } = new();
    public int FolderId { get; set; }
}

public class BulkActionDto
{
    public List<int> MessageIds { get; set; } = new();
}

public class FlagMessageDto
{
    public DateTime? DueDate { get; set; }
}

public class AddReactionDto
{
    public ReactionType ReactionType { get; set; }
}

public class PaginatedList<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PaginatedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}

public class InboxQueryParameters
{
    public int? FolderId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? SortBy { get; set; } = "date";
    public bool SortDescending { get; set; } = true;
    public string? Search { get; set; }
    public bool IsUnreadOnly { get; set; }
    public bool IsFlaggedOnly { get; set; }
    public bool HasAttachmentsOnly { get; set; }
    public int? CategoryId { get; set; }
    public MessageImportance? Importance { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class SearchParameters : InboxQueryParameters
{
    public string? Sender { get; set; }
    public string? Subject { get; set; }
    public string? Content { get; set; }
    public bool? HasAttachments { get; set; }
}
