namespace Application.DTOs.Archive;

/// <summary>
/// Correspondence routing DTO
/// </summary>
public class CorrespondenceRoutingDto
{
    public int Id { get; set; }
    public int CorrespondenceId { get; set; }
    public string CorrespondenceReferenceNumber { get; set; } = string.Empty;

    public int FromEmployeeId { get; set; }
    public string FromEmployeeName { get; set; } = string.Empty;
    public string? FromEmployeePosition { get; set; }

    public int ToEmployeeId { get; set; }
    public string ToEmployeeName { get; set; } = string.Empty;
    public string? ToEmployeePosition { get; set; }

    public int? ToDepartmentId { get; set; }
    public string? ToDepartmentName { get; set; }

    public string Action { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public DateTime? DueDate { get; set; }

    public DateTime RoutedDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public bool IsRead { get; set; }

    public string? Response { get; set; }
    public DateTime? ResponseDate { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedDate { get; set; }

    public int? ParentRoutingId { get; set; }
    public int SequenceNumber { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Route correspondence request
/// </summary>
public class RouteCorrespondenceRequest
{
    public int CorrespondenceId { get; set; }
    public int ToEmployeeId { get; set; }
    public int? ToDepartmentId { get; set; }
    public string Action { get; set; } = "ForReview";
    public string Priority { get; set; } = "Normal";
    public string? Instructions { get; set; }
    public DateTime? DueDate { get; set; }
}

/// <summary>
/// Respond to routing request
/// </summary>
public class RespondToRoutingRequest
{
    public int RoutingId { get; set; }
    public string Response { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
}

/// <summary>
/// Routing chain (complete history)
/// </summary>
public class RoutingChainDto
{
    public int CorrespondenceId { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public List<CorrespondenceRoutingDto> RoutingHistory { get; set; } = new();
    public int TotalRoutings { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;
    public int? CurrentAssigneeId { get; set; }
    public string? CurrentAssigneeName { get; set; }
}
