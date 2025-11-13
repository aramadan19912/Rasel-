namespace Backend.Application.DTOs.Contacts;

public class ContactGroupDto
{
    public int Id { get; set; }
    public string GroupId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = "#1976D2";
    public string? Icon { get; set; }
    public int MemberCount { get; set; }
    public bool IsDistributionList { get; set; }
    public bool IsSmartGroup { get; set; }
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
    public bool IsSmartGroup { get; set; }
    public string? SmartGroupRules { get; set; }
}

public class UpdateContactGroupDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public int? DisplayOrder { get; set; }
}

public class ContactGroupMembershipDto
{
    public int Id { get; set; }
    public int ContactId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public string? Notes { get; set; }
}

public class AddContactToGroupDto
{
    public int ContactId { get; set; }
    public string? Notes { get; set; }
}
