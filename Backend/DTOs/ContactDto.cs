using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.DTOs;

public class ContactDto
{
    public int Id { get; set; }
    public string ContactId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;

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
    public List<ContactGroupSummaryDto> Groups { get; set; } = new();

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
    // Basic Information
    public string? Title { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? Suffix { get; set; }
    public string? Nickname { get; set; }

    // Contact Details
    public List<ContactEmailDto>? EmailAddresses { get; set; }
    public List<ContactPhoneDto>? PhoneNumbers { get; set; }
    public List<ContactAddressDto>? Addresses { get; set; }
    public List<ContactWebsiteDto>? Websites { get; set; }

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
    public List<string>? Categories { get; set; }
    public List<string>? Tags { get; set; }

    // Social Media
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramHandle { get; set; }

    // Custom Fields
    public List<ContactCustomFieldDto>? CustomFields { get; set; }

    // Privacy
    public ContactPrivacy Privacy { get; set; } = ContactPrivacy.Private;
}

public class UpdateContactDto
{
    // Basic Information
    public string? Title { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Suffix { get; set; }
    public string? Nickname { get; set; }

    // Contact Details
    public List<ContactEmailDto>? EmailAddresses { get; set; }
    public List<ContactPhoneDto>? PhoneNumbers { get; set; }
    public List<ContactAddressDto>? Addresses { get; set; }
    public List<ContactWebsiteDto>? Websites { get; set; }

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
    public List<string>? Categories { get; set; }
    public List<string>? Tags { get; set; }

    // Social Media
    public string? LinkedInUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookUrl { get; set; }
    public string? InstagramHandle { get; set; }

    // Custom Fields
    public List<ContactCustomFieldDto>? CustomFields { get; set; }

    // Metadata
    public bool? IsFavorite { get; set; }
    public bool? IsBlocked { get; set; }

    // Privacy
    public ContactPrivacy? Privacy { get; set; }
}

public class ContactEmailDto
{
    public int Id { get; set; }
    public EmailType Type { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class ContactPhoneDto
{
    public int Id { get; set; }
    public PhoneType Type { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Extension { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class ContactAddressDto
{
    public int Id { get; set; }
    public AddressType Type { get; set; }
    public string? Street { get; set; }
    public string? Street2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class ContactWebsiteDto
{
    public int Id { get; set; }
    public WebsiteType Type { get; set; }
    public string Url { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class ContactCustomFieldDto
{
    public int Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldValue { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; }
}

public class ContactRelationshipDto
{
    public int Id { get; set; }
    public int RelatedContactId { get; set; }
    public string RelatedContactName { get; set; } = string.Empty;
    public RelationshipType Type { get; set; }
    public string? Description { get; set; }
}

public class ContactGroupSummaryDto
{
    public int Id { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class ContactGroupDto
{
    public int Id { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool IsDistributionList { get; set; }
    public bool IsSmartGroup { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModified { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateContactGroupDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#1976D2";
    public string? Icon { get; set; }
    public bool IsDistributionList { get; set; }
}

public class UpdateContactGroupDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool? IsDistributionList { get; set; }
    public int? DisplayOrder { get; set; }
}

public class ContactInteractionDto
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public InteractionType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime InteractionDate { get; set; }
    public int? RelatedMessageId { get; set; }
    public int? RelatedEventId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInteractionDto
{
    public InteractionType Type { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime? InteractionDate { get; set; }
    public int? RelatedMessageId { get; set; }
    public int? RelatedEventId { get; set; }
}

public class ContactQueryParameters
{
    public string? SearchTerm { get; set; }
    public List<string>? Categories { get; set; }
    public List<string>? Tags { get; set; }
    public List<int>? GroupIds { get; set; }
    public bool? IsFavorite { get; set; }
    public ContactSource? Source { get; set; }
    public string? Company { get; set; }
    public string? SortBy { get; set; } = "DisplayName";
    public bool SortDescending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class ContactStatisticsDto
{
    public int TotalContacts { get; set; }
    public int FavoriteContacts { get; set; }
    public int ContactsWithBirthday { get; set; }
    public int TotalGroups { get; set; }
    public int RecentlyAdded { get; set; }
    public int FrequentlyContacted { get; set; }
    public Dictionary<string, int> ContactsByCompany { get; set; } = new();
    public Dictionary<string, int> ContactsBySource { get; set; } = new();
    public Dictionary<string, int> ContactsByCategory { get; set; } = new();
}

public class MergeContactsDto
{
    public int PrimaryContactId { get; set; }
    public List<int> ContactIdsToMerge { get; set; } = new();
    public bool KeepAllEmails { get; set; } = true;
    public bool KeepAllPhones { get; set; } = true;
    public bool KeepAllAddresses { get; set; } = true;
}
