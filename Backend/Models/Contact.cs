using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.Models;

public class Contact
{
    public int Id { get; set; }
    public string ContactId { get; set; } = Guid.NewGuid().ToString();

    // Owner
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    // Basic Information
    public string? Title { get; set; } // Mr., Mrs., Dr., etc.
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? Suffix { get; set; } // Jr., Sr., III, etc.
    public string? Nickname { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? FileAs { get; set; } // How to file/sort this contact

    // Contact Details
    public List<ContactEmail> EmailAddresses { get; set; } = new();
    public List<ContactPhone> PhoneNumbers { get; set; } = new();
    public List<ContactAddress> Addresses { get; set; } = new();
    public List<ContactWebsite> Websites { get; set; } = new();

    // Professional Information
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? Manager { get; set; }
    public string? Assistant { get; set; }
    public string? OfficeLocation { get; set; }

    // Personal Information
    public DateTime? Birthday { get; set; }
    public string? SpouseName { get; set; }
    public string? Children { get; set; }
    public string? Gender { get; set; }

    // Additional Details
    public string? Notes { get; set; }
    public List<string> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();

    // Social Media
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramHandle { get; set; }

    // Photo
    public byte[]? Photo { get; set; }
    public string? PhotoUrl { get; set; }

    // Groups
    public List<ContactGroupMembership> GroupMemberships { get; set; } = new();

    // Relationships
    public List<ContactRelationship> Relationships { get; set; } = new();

    // Custom Fields
    public List<ContactCustomField> CustomFields { get; set; } = new();

    // Metadata
    public bool IsFavorite { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public DateTime? LastContacted { get; set; }
    public int ContactFrequency { get; set; } // How often contacted

    // Source
    public ContactSource Source { get; set; } = ContactSource.Manual;
    public string? SourceId { get; set; } // External source ID

    // Privacy
    public ContactPrivacy Privacy { get; set; } = ContactPrivacy.Private;
}

public class ContactEmail
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public Contact? Contact { get; set; }

    public EmailType Type { get; set; } = EmailType.Personal;
    public string Email { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class ContactPhone
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public Contact? Contact { get; set; }

    public PhoneType Type { get; set; } = PhoneType.Mobile;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Extension { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class ContactAddress
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public Contact? Contact { get; set; }

    public AddressType Type { get; set; } = AddressType.Home;
    public string? Street { get; set; }
    public string? Street2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class ContactWebsite
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public Contact? Contact { get; set; }

    public WebsiteType Type { get; set; } = WebsiteType.Personal;
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class ContactCustomField
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public Contact? Contact { get; set; }

    public string FieldName { get; set; } = string.Empty;
    public string FieldValue { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; } = CustomFieldType.Text;
}

public class ContactRelationship
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public Contact? Contact { get; set; }

    public int RelatedContactId { get; set; }
    public Contact? RelatedContact { get; set; }

    public RelationshipType Type { get; set; }
    public string? Description { get; set; }
}

// Enums
public enum EmailType
{
    Personal = 0,
    Work = 1,
    Other = 2
}

public enum PhoneType
{
    Mobile = 0,
    Home = 1,
    Work = 2,
    Main = 3,
    HomeFax = 4,
    WorkFax = 5,
    Pager = 6,
    Other = 7
}

public enum AddressType
{
    Home = 0,
    Work = 1,
    Other = 2
}

public enum WebsiteType
{
    Personal = 0,
    Work = 1,
    Blog = 2,
    Portfolio = 3,
    Other = 4
}

public enum CustomFieldType
{
    Text = 0,
    Number = 1,
    Date = 2,
    Boolean = 3,
    Url = 4
}

public enum RelationshipType
{
    Spouse = 0,
    Partner = 1,
    Child = 2,
    Parent = 3,
    Sibling = 4,
    Relative = 5,
    Friend = 6,
    Assistant = 7,
    Manager = 8,
    Colleague = 9,
    ReferredBy = 10,
    Other = 11
}

public enum ContactSource
{
    Manual = 0,
    Import = 1,
    Exchange = 2,
    ActiveDirectory = 3,
    LinkedIn = 4,
    Google = 5,
    Other = 6
}

public enum ContactPrivacy
{
    Private = 0,
    Public = 1,
    Shared = 2
}
