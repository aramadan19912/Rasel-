using OutlookInboxManagement.DTOs;

namespace OutlookInboxManagement.Services;

public interface IContactsService
{
    // Contact Management
    Task<ContactDto?> GetContactAsync(int contactId, string userId);
    Task<ContactDto?> GetContactByContactIdAsync(string contactId, string userId);
    Task<PaginatedList<ContactDto>> GetContactsAsync(ContactQueryParameters parameters, string userId);
    Task<List<ContactDto>> GetAllContactsAsync(string userId);
    Task<List<ContactDto>> GetFavoriteContactsAsync(string userId);
    Task<List<ContactDto>> GetRecentContactsAsync(string userId, int count = 10);
    Task<List<ContactDto>> GetFrequentContactsAsync(string userId, int count = 10);
    Task<ContactDto> CreateContactAsync(CreateContactDto dto, string userId);
    Task<ContactDto?> UpdateContactAsync(int contactId, UpdateContactDto dto, string userId);
    Task<bool> DeleteContactAsync(int contactId, string userId);
    Task<bool> ToggleFavoriteAsync(int contactId, string userId);
    Task<bool> ToggleBlockAsync(int contactId, string userId);

    // Photo Management
    Task<bool> UploadPhotoAsync(int contactId, Stream photoStream, string fileName, string userId);
    Task<bool> DeletePhotoAsync(int contactId, string userId);
    Task<(Stream? stream, string contentType)?> GetPhotoAsync(int contactId, string userId);

    // Contact Groups
    Task<ContactGroupDto?> GetGroupAsync(int groupId, string userId);
    Task<List<ContactGroupDto>> GetGroupsAsync(string userId);
    Task<ContactGroupDto> CreateGroupAsync(CreateContactGroupDto dto, string userId);
    Task<ContactGroupDto?> UpdateGroupAsync(int groupId, UpdateContactGroupDto dto, string userId);
    Task<bool> DeleteGroupAsync(int groupId, string userId);
    Task<bool> AddContactToGroupAsync(int contactId, int groupId, string userId);
    Task<bool> RemoveContactFromGroupAsync(int contactId, int groupId, string userId);
    Task<List<ContactDto>> GetGroupMembersAsync(int groupId, string userId);
    Task<bool> SendEmailToGroupAsync(int groupId, string subject, string body, string userId);

    // Interactions
    Task<List<ContactInteractionDto>> GetContactInteractionsAsync(int contactId, string userId);
    Task<ContactInteractionDto> AddInteractionAsync(int contactId, CreateInteractionDto dto, string userId);
    Task<bool> DeleteInteractionAsync(int interactionId, string userId);

    // Relationships
    Task<bool> AddRelationshipAsync(int contactId, int relatedContactId, RelationshipType type, string? description, string userId);
    Task<bool> RemoveRelationshipAsync(int relationshipId, string userId);

    // Search & Filter
    Task<List<ContactDto>> SearchContactsAsync(string searchTerm, string userId);
    Task<List<ContactDto>> GetContactsByCompanyAsync(string company, string userId);
    Task<List<ContactDto>> GetContactsByCategoryAsync(string category, string userId);
    Task<List<ContactDto>> GetContactsWithBirthdayAsync(DateTime startDate, DateTime endDate, string userId);
    Task<List<ContactDto>> GetContactsWithUpcomingBirthdaysAsync(string userId, int days = 30);

    // Duplicate Management
    Task<List<List<ContactDto>>> FindDuplicateContactsAsync(string userId);
    Task<ContactDto> MergeContactsAsync(MergeContactsDto dto, string userId);

    // Import/Export
    Task<string> ExportContactToVCardAsync(int contactId, string userId);
    Task<string> ExportContactsToVCardAsync(List<int> contactIds, string userId);
    Task<ContactDto> ImportContactFromVCardAsync(string vCardContent, string userId);
    Task<List<ContactDto>> ImportContactsFromVCardAsync(string vCardContent, string userId);
    Task<byte[]> ExportContactsToCsvAsync(string userId);

    // Statistics
    Task<ContactStatisticsDto> GetStatisticsAsync(string userId);

    // Bulk Operations
    Task<bool> BulkAddToGroupAsync(List<int> contactIds, int groupId, string userId);
    Task<bool> BulkRemoveFromGroupAsync(List<int> contactIds, int groupId, string userId);
    Task<bool> BulkAddCategoryAsync(List<int> contactIds, string category, string userId);
    Task<bool> BulkDeleteAsync(List<int> contactIds, string userId);
}
