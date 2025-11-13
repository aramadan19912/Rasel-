using Backend.Application.DTOs.Contacts;

namespace Backend.Application.Interfaces;

public interface IContactsService
{
    // ===== Contact CRUD =====
    Task<List<ContactDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50);
    Task<ContactDto> GetByIdAsync(int id, string userId);
    Task<ContactDto> GetByContactIdAsync(string contactId, string userId);
    Task<ContactDto> CreateAsync(CreateContactDto dto, string userId);
    Task<bool> UpdateAsync(int id, UpdateContactDto dto, string userId);
    Task<bool> DeleteAsync(int id, string userId);
    Task<bool> DeleteMultipleAsync(List<int> ids, string userId);

    // ===== Contact Search & Filter =====
    Task<List<ContactDto>> SearchAsync(string query, string userId, int pageNumber = 1, int pageSize = 50);
    Task<List<ContactDto>> GetFavoritesAsync(string userId);
    Task<List<ContactDto>> GetByCompanyAsync(string company, string userId);
    Task<List<ContactDto>> GetByTagAsync(string tag, string userId);
    Task<List<ContactDto>> GetByCategoryAsync(string category, string userId);
    Task<List<ContactDto>> GetRecentlyContactedAsync(string userId, int count = 10);
    Task<List<ContactDto>> GetFrequentlyContactedAsync(string userId, int count = 10);
    Task<List<ContactDto>> GetByBirthdayAsync(DateTime startDate, DateTime endDate, string userId);
    Task<List<ContactDto>> GetUpcomingBirthdaysAsync(string userId, int days = 30);

    // ===== Contact Actions =====
    Task<bool> MarkAsFavoriteAsync(int id, string userId);
    Task<bool> UnmarkAsFavoriteAsync(int id, string userId);
    Task<bool> BlockContactAsync(int id, string userId);
    Task<bool> UnblockContactAsync(int id, string userId);
    Task<bool> UpdateLastContactedAsync(int id, string userId);

    // ===== Contact Details =====
    Task<bool> AddEmailAsync(int contactId, ContactEmailDto dto, string userId);
    Task<bool> UpdateEmailAsync(int contactId, int emailId, ContactEmailDto dto, string userId);
    Task<bool> RemoveEmailAsync(int contactId, int emailId, string userId);
    Task<bool> SetPrimaryEmailAsync(int contactId, int emailId, string userId);

    Task<bool> AddPhoneAsync(int contactId, ContactPhoneDto dto, string userId);
    Task<bool> UpdatePhoneAsync(int contactId, int phoneId, ContactPhoneDto dto, string userId);
    Task<bool> RemovePhoneAsync(int contactId, int phoneId, string userId);
    Task<bool> SetPrimaryPhoneAsync(int contactId, int phoneId, string userId);

    Task<bool> AddAddressAsync(int contactId, ContactAddressDto dto, string userId);
    Task<bool> UpdateAddressAsync(int contactId, int addressId, ContactAddressDto dto, string userId);
    Task<bool> RemoveAddressAsync(int contactId, int addressId, string userId);
    Task<bool> SetPrimaryAddressAsync(int contactId, int addressId, string userId);

    Task<bool> AddWebsiteAsync(int contactId, ContactWebsiteDto dto, string userId);
    Task<bool> RemoveWebsiteAsync(int contactId, int websiteId, string userId);

    Task<bool> AddCustomFieldAsync(int contactId, ContactCustomFieldDto dto, string userId);
    Task<bool> UpdateCustomFieldAsync(int contactId, int fieldId, ContactCustomFieldDto dto, string userId);
    Task<bool> RemoveCustomFieldAsync(int contactId, int fieldId, string userId);

    // ===== Contact Relationships =====
    Task<List<ContactRelationshipDto>> GetRelationshipsAsync(int contactId, string userId);
    Task<bool> AddRelationshipAsync(int contactId, int relatedContactId, RelationshipType type, string? description, string userId);
    Task<bool> RemoveRelationshipAsync(int contactId, int relationshipId, string userId);

    // ===== Contact Groups =====
    Task<List<ContactGroupDto>> GetGroupsAsync(string userId);
    Task<ContactGroupDto> GetGroupByIdAsync(int id, string userId);
    Task<ContactGroupDto> CreateGroupAsync(CreateContactGroupDto dto, string userId);
    Task<bool> UpdateGroupAsync(int id, UpdateContactGroupDto dto, string userId);
    Task<bool> DeleteGroupAsync(int id, string userId);

    Task<List<ContactDto>> GetGroupMembersAsync(int groupId, string userId);
    Task<bool> AddContactToGroupAsync(int groupId, AddContactToGroupDto dto, string userId);
    Task<bool> RemoveContactFromGroupAsync(int groupId, int contactId, string userId);
    Task<bool> AddMultipleContactsToGroupAsync(int groupId, List<int> contactIds, string userId);

    // ===== Smart Groups =====
    Task<List<ContactDto>> GetSmartGroupMembersAsync(int groupId, string userId);
    Task<bool> UpdateSmartGroupRulesAsync(int groupId, string rules, string userId);
    Task<bool> RefreshSmartGroupAsync(int groupId, string userId);

    // ===== Import/Export =====
    Task<List<ContactDto>> ImportFromCsvAsync(Stream csvStream, string userId);
    Task<Stream> ExportToCsvAsync(List<int> contactIds, string userId);
    Task<Stream> ExportToVCardAsync(List<int> contactIds, string userId);
    Task<List<ContactDto>> ImportFromVCardAsync(Stream vcardStream, string userId);

    // ===== Merge & Duplicate Detection =====
    Task<List<ContactDuplicateDto>> FindDuplicatesAsync(string userId);
    Task<ContactDto> MergeContactsAsync(int primaryId, int duplicateId, string userId);

    // ===== Statistics =====
    Task<ContactStatisticsDto> GetStatisticsAsync(string userId);
}

public class ContactDuplicateDto
{
    public ContactDto Contact1 { get; set; } = null!;
    public ContactDto Contact2 { get; set; } = null!;
    public double SimilarityScore { get; set; }
    public List<string> MatchedFields { get; set; } = new();
}

public class ContactStatisticsDto
{
    public int TotalContacts { get; set; }
    public int FavoriteContacts { get; set; }
    public int BlockedContacts { get; set; }
    public int TotalGroups { get; set; }
    public int ContactsWithBirthdays { get; set; }
    public Dictionary<string, int> ContactsBySource { get; set; } = new();
    public Dictionary<string, int> ContactsByCompany { get; set; } = new();
    public List<ContactDto> TopContacted { get; set; } = new();
}
