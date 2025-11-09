using Microsoft.AspNetCore.Identity;

namespace OutlookInboxManagement.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public List<Message> SentMessages { get; set; } = new();
    public List<MessageFolder> Folders { get; set; } = new();
    public List<MessageCategory> Categories { get; set; } = new();
    public List<MessageRule> Rules { get; set; } = new();
    public List<Calendar> Calendars { get; set; } = new();
    public List<Contact> Contacts { get; set; } = new();
    public List<ContactGroup> ContactGroups { get; set; } = new();
}
