namespace Backend.Application.DTOs.Contacts;

public class ContactDto
{
    public int Id { get; set; }
    public string ContactId { get; set; } = string.Empty;

    // Basic Information
    public string? Title { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? Suffix { get; set; }
    public string? Nickname { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? FileAs { get; set; }

    // Contact Details
    public List<ContactEmailDto> EmailAddresses { get; set; } = new();
    public List<ContactPhoneDto> PhoneNumbers { get; set; } = new();
    public List<ContactAddressDto> Addresses { get; set; } = new();
    public List<ContactWebsiteDto> Websites { get; set; } = new();

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
    public string? PhotoUrl { get; set; }

    // Groups
    public List<ContactGroupMembershipDto> GroupMemberships { get; set; } = new();

    // Relationships
    public List<ContactRelationshipDto> Relationships { get; set; } = new();

    // Custom Fields
    public List<ContactCustomFieldDto> CustomFields { get; set; } = new();

    // Metadata
    public bool IsFavorite { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
    public DateTime? LastContacted { get; set; }
    public int ContactFrequency { get; set; }

    // Source
    public ContactSource Source { get; set; }
    public string? SourceId { get; set; }

    // Privacy
    public ContactPrivacy Privacy { get; set; }
}

public class CreateContactDto
{
    public string? Title { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? Suffix { get; set; }
    public string? Nickname { get; set; }

    public string? Email { get; set; }
    public string? Phone { get; set; }

    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? Manager { get; set; }

    public DateTime? Birthday { get; set; }
    public string? Notes { get; set; }
    public List<string> Categories { get; set; } = new();
    public List<string> Tags { get; set; } = new();
}

public class UpdateContactDto
{
    public string? Title { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
    public string? Nickname { get; set; }

    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public string? Company { get; set; }
    public string? Manager { get; set; }
    public string? Assistant { get; set; }
    public string? OfficeLocation { get; set; }

    public DateTime? Birthday { get; set; }
    public string? SpouseName { get; set; }
    public string? Children { get; set; }
    public string? Gender { get; set; }

    public string? Notes { get; set; }
    public List<string>? Categories { get; set; }
    public List<string>? Tags { get; set; }

    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramHandle { get; set; }

    public bool? IsFavorite { get; set; }
    public bool? IsBlocked { get; set; }
    public ContactPrivacy? Privacy { get; set; }
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
