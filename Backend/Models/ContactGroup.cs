namespace OutlookInboxManagement.Models;

public class ContactGroup
{
    public int Id { get; set; }
    public string GroupId { get; set; } = Guid.NewGuid().ToString();

    // Owner
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Group Information
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#1976D2";
    public string? Icon { get; set; }

    // Members
    public List<ContactGroupMembership> Members { get; set; } = new();

    // Settings
    public bool IsDistributionList { get; set; } // Can send email to all members
    public bool IsSmartGroup { get; set; } // Auto-populated based on rules
    public string? SmartGroupRules { get; set; } // JSON rules for smart groups

    // Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public int DisplayOrder { get; set; }
}

public class ContactGroupMembership
{
    public int Id { get; set; }

    public int ContactId { get; set; }
    public Contact? Contact { get; set; }

    public int GroupId { get; set; }
    public ContactGroup? Group { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
