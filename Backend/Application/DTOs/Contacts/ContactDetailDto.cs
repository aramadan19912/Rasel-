namespace Backend.Application.DTOs.Contacts;

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
