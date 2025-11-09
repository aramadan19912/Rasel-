using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.Data;
using OutlookInboxManagement.DTOs;
using OutlookInboxManagement.Models;
using System.Text;

namespace OutlookInboxManagement.Services;

public class ContactsService : IContactsService
{
    private readonly ApplicationDbContext _context;

    public ContactsService(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Contact Management

    public async Task<ContactDto?> GetContactAsync(int contactId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.Addresses)
            .Include(c => c.Websites)
            .Include(c => c.GroupMemberships).ThenInclude(gm => gm.Group)
            .Include(c => c.Relationships).ThenInclude(r => r.RelatedContact)
            .Include(c => c.CustomFields)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        return contact == null ? null : MapToContactDto(contact);
    }

    public async Task<ContactDto?> GetContactByContactIdAsync(string contactId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.Addresses)
            .Include(c => c.Websites)
            .Include(c => c.GroupMemberships).ThenInclude(gm => gm.Group)
            .Include(c => c.Relationships).ThenInclude(r => r.RelatedContact)
            .Include(c => c.CustomFields)
            .FirstOrDefaultAsync(c => c.ContactId == contactId && c.UserId == userId);

        return contact == null ? null : MapToContactDto(contact);
    }

    public async Task<PaginatedList<ContactDto>> GetContactsAsync(ContactQueryParameters parameters, string userId)
    {
        var query = _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.GroupMemberships).ThenInclude(gm => gm.Group)
            .Where(c => c.UserId == userId)
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
        {
            var searchTerm = parameters.SearchTerm.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(searchTerm) ||
                c.LastName.ToLower().Contains(searchTerm) ||
                c.DisplayName.ToLower().Contains(searchTerm) ||
                c.Company!.ToLower().Contains(searchTerm) ||
                c.EmailAddresses.Any(e => e.Email.ToLower().Contains(searchTerm)));
        }

        // Filter by categories
        if (parameters.Categories != null && parameters.Categories.Any())
        {
            query = query.Where(c => c.Categories.Any(cat => parameters.Categories.Contains(cat)));
        }

        // Filter by groups
        if (parameters.GroupIds != null && parameters.GroupIds.Any())
        {
            query = query.Where(c => c.GroupMemberships.Any(gm => parameters.GroupIds.Contains(gm.GroupId)));
        }

        // Filter by favorite
        if (parameters.IsFavorite.HasValue)
        {
            query = query.Where(c => c.IsFavorite == parameters.IsFavorite.Value);
        }

        // Filter by company
        if (!string.IsNullOrWhiteSpace(parameters.Company))
        {
            query = query.Where(c => c.Company == parameters.Company);
        }

        // Sorting
        query = parameters.SortBy?.ToLower() switch
        {
            "firstname" => parameters.SortDescending ? query.OrderByDescending(c => c.FirstName) : query.OrderBy(c => c.FirstName),
            "lastname" => parameters.SortDescending ? query.OrderByDescending(c => c.LastName) : query.OrderBy(c => c.LastName),
            "company" => parameters.SortDescending ? query.OrderByDescending(c => c.Company) : query.OrderBy(c => c.Company),
            "createdat" => parameters.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => parameters.SortDescending ? query.OrderByDescending(c => c.DisplayName) : query.OrderBy(c => c.DisplayName)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PaginatedList<ContactDto>(
            items.Select(MapToContactDto).ToList(),
            totalCount,
            parameters.PageNumber,
            parameters.PageSize
        );
    }

    public async Task<List<ContactDto>> GetAllContactsAsync(string userId)
    {
        var contacts = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.DisplayName)
            .ToListAsync();

        return contacts.Select(MapToContactDto).ToList();
    }

    public async Task<List<ContactDto>> GetFavoriteContactsAsync(string userId)
    {
        var contacts = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Where(c => c.UserId == userId && c.IsFavorite)
            .OrderBy(c => c.DisplayName)
            .ToListAsync();

        return contacts.Select(MapToContactDto).ToList();
    }

    public async Task<List<ContactDto>> GetRecentContactsAsync(string userId, int count = 10)
    {
        var contacts = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .Take(count)
            .ToListAsync();

        return contacts.Select(MapToContactDto).ToList();
    }

    public async Task<List<ContactDto>> GetFrequentContactsAsync(string userId, int count = 10)
    {
        var contacts = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ContactFrequency)
            .Take(count)
            .ToListAsync();

        return contacts.Select(MapToContactDto).ToList();
    }

    public async Task<ContactDto> CreateContactAsync(CreateContactDto dto, string userId)
    {
        var contact = new Contact
        {
            ContactId = Guid.NewGuid().ToString(),
            UserId = userId,
            Title = dto.Title,
            FirstName = dto.FirstName,
            MiddleName = dto.MiddleName,
            LastName = dto.LastName,
            Suffix = dto.Suffix,
            Nickname = dto.Nickname,
            DisplayName = GenerateDisplayName(dto.FirstName, dto.MiddleName, dto.LastName),
            FileAs = GenerateFileAs(dto.FirstName, dto.LastName),
            JobTitle = dto.JobTitle,
            Department = dto.Department,
            Company = dto.Company,
            Manager = dto.Manager,
            Assistant = dto.Assistant,
            OfficeLocation = dto.OfficeLocation,
            Birthday = dto.Birthday,
            SpouseName = dto.SpouseName,
            Children = dto.Children,
            Gender = dto.Gender,
            Notes = dto.Notes,
            Categories = dto.Categories?.ToList() ?? new List<string>(),
            Tags = dto.Tags?.ToList() ?? new List<string>(),
            LinkedInUrl = dto.LinkedInUrl,
            TwitterHandle = dto.TwitterHandle,
            FacebookUrl = dto.FacebookUrl,
            InstagramHandle = dto.InstagramHandle,
            Privacy = dto.Privacy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        // Add emails
        if (dto.EmailAddresses != null && dto.EmailAddresses.Any())
        {
            foreach (var email in dto.EmailAddresses)
            {
                _context.Set<ContactEmail>().Add(new ContactEmail
                {
                    ContactId = contact.Id,
                    Type = email.Type,
                    Email = email.Email,
                    IsPrimary = email.IsPrimary,
                    DisplayOrder = email.DisplayOrder
                });
            }
        }

        // Add phone numbers
        if (dto.PhoneNumbers != null && dto.PhoneNumbers.Any())
        {
            foreach (var phone in dto.PhoneNumbers)
            {
                _context.Set<ContactPhone>().Add(new ContactPhone
                {
                    ContactId = contact.Id,
                    Type = phone.Type,
                    PhoneNumber = phone.PhoneNumber,
                    Extension = phone.Extension,
                    IsPrimary = phone.IsPrimary,
                    DisplayOrder = phone.DisplayOrder
                });
            }
        }

        // Add addresses
        if (dto.Addresses != null && dto.Addresses.Any())
        {
            foreach (var address in dto.Addresses)
            {
                _context.Set<ContactAddress>().Add(new ContactAddress
                {
                    ContactId = contact.Id,
                    Type = address.Type,
                    Street = address.Street,
                    Street2 = address.Street2,
                    City = address.City,
                    State = address.State,
                    PostalCode = address.PostalCode,
                    Country = address.Country,
                    IsPrimary = address.IsPrimary,
                    DisplayOrder = address.DisplayOrder
                });
            }
        }

        // Add websites
        if (dto.Websites != null && dto.Websites.Any())
        {
            foreach (var website in dto.Websites)
            {
                _context.Set<ContactWebsite>().Add(new ContactWebsite
                {
                    ContactId = contact.Id,
                    Type = website.Type,
                    Url = website.Url,
                    DisplayOrder = website.DisplayOrder
                });
            }
        }

        // Add custom fields
        if (dto.CustomFields != null && dto.CustomFields.Any())
        {
            foreach (var field in dto.CustomFields)
            {
                _context.Set<ContactCustomField>().Add(new ContactCustomField
                {
                    ContactId = contact.Id,
                    FieldName = field.FieldName,
                    FieldValue = field.FieldValue,
                    FieldType = field.FieldType
                });
            }
        }

        await _context.SaveChangesAsync();

        return (await GetContactAsync(contact.Id, userId))!;
    }

    public async Task<ContactDto?> UpdateContactAsync(int contactId, UpdateContactDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.Addresses)
            .Include(c => c.Websites)
            .Include(c => c.CustomFields)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null) return null;

        // Update basic info
        if (dto.Title != null) contact.Title = dto.Title;
        if (dto.FirstName != null) contact.FirstName = dto.FirstName;
        if (dto.MiddleName != null) contact.MiddleName = dto.MiddleName;
        if (dto.LastName != null) contact.LastName = dto.LastName;
        if (dto.Suffix != null) contact.Suffix = dto.Suffix;
        if (dto.Nickname != null) contact.Nickname = dto.Nickname;

        // Update display name
        contact.DisplayName = GenerateDisplayName(contact.FirstName, contact.MiddleName, contact.LastName);
        contact.FileAs = GenerateFileAs(contact.FirstName, contact.LastName);

        // Update professional info
        if (dto.JobTitle != null) contact.JobTitle = dto.JobTitle;
        if (dto.Department != null) contact.Department = dto.Department;
        if (dto.Company != null) contact.Company = dto.Company;
        if (dto.Manager != null) contact.Manager = dto.Manager;
        if (dto.Assistant != null) contact.Assistant = dto.Assistant;
        if (dto.OfficeLocation != null) contact.OfficeLocation = dto.OfficeLocation;

        // Update personal info
        if (dto.Birthday.HasValue) contact.Birthday = dto.Birthday;
        if (dto.SpouseName != null) contact.SpouseName = dto.SpouseName;
        if (dto.Children != null) contact.Children = dto.Children;
        if (dto.Gender != null) contact.Gender = dto.Gender;

        // Update additional details
        if (dto.Notes != null) contact.Notes = dto.Notes;
        if (dto.Categories != null) contact.Categories = dto.Categories.ToList();
        if (dto.Tags != null) contact.Tags = dto.Tags.ToList();

        // Update social media
        if (dto.LinkedInUrl != null) contact.LinkedInUrl = dto.LinkedInUrl;
        if (dto.TwitterHandle != null) contact.TwitterHandle = dto.TwitterHandle;
        if (dto.FacebookUrl != null) contact.FacebookUrl = dto.FacebookUrl;
        if (dto.InstagramHandle != null) contact.InstagramHandle = dto.InstagramHandle;

        // Update metadata
        if (dto.IsFavorite.HasValue) contact.IsFavorite = dto.IsFavorite.Value;
        if (dto.IsBlocked.HasValue) contact.IsBlocked = dto.IsBlocked.Value;
        if (dto.Privacy.HasValue) contact.Privacy = dto.Privacy.Value;

        contact.LastModified = DateTime.UtcNow;

        // Update contact details
        if (dto.EmailAddresses != null)
        {
            _context.Set<ContactEmail>().RemoveRange(contact.EmailAddresses);
            foreach (var email in dto.EmailAddresses)
            {
                _context.Set<ContactEmail>().Add(new ContactEmail
                {
                    ContactId = contact.Id,
                    Type = email.Type,
                    Email = email.Email,
                    IsPrimary = email.IsPrimary,
                    DisplayOrder = email.DisplayOrder
                });
            }
        }

        if (dto.PhoneNumbers != null)
        {
            _context.Set<ContactPhone>().RemoveRange(contact.PhoneNumbers);
            foreach (var phone in dto.PhoneNumbers)
            {
                _context.Set<ContactPhone>().Add(new ContactPhone
                {
                    ContactId = contact.Id,
                    Type = phone.Type,
                    PhoneNumber = phone.PhoneNumber,
                    Extension = phone.Extension,
                    IsPrimary = phone.IsPrimary,
                    DisplayOrder = phone.DisplayOrder
                });
            }
        }

        await _context.SaveChangesAsync();
        return await GetContactAsync(contact.Id, userId);
    }

    public async Task<bool> DeleteContactAsync(int contactId, string userId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null) return false;

        _context.Contacts.Remove(contact);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleFavoriteAsync(int contactId, string userId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null) return false;

        contact.IsFavorite = !contact.IsFavorite;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleBlockAsync(int contactId, string userId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null) return false;

        contact.IsBlocked = !contact.IsBlocked;
        await _context.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Photo Management

    public async Task<bool> UploadPhotoAsync(int contactId, Stream photoStream, string fileName, string userId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null) return false;

        using var memoryStream = new MemoryStream();
        await photoStream.CopyToAsync(memoryStream);
        contact.Photo = memoryStream.ToArray();
        contact.PhotoUrl = $"/api/contacts/{contactId}/photo";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePhotoAsync(int contactId, string userId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null) return false;

        contact.Photo = null;
        contact.PhotoUrl = null;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<(Stream? stream, string contentType)?> GetPhotoAsync(int contactId, string userId)
    {
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null || contact.Photo == null)
            return null;

        var stream = new MemoryStream(contact.Photo);
        return (stream, "image/jpeg");
    }

    #endregion

    #region Groups (simplified - full implementation would be larger)

    public async Task<ContactGroupDto?> GetGroupAsync(int groupId, string userId)
    {
        var group = await _context.Set<ContactGroup>()
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId);

        return group == null ? null : MapToGroupDto(group);
    }

    public async Task<List<ContactGroupDto>> GetGroupsAsync(string userId)
    {
        var groups = await _context.Set<ContactGroup>()
            .Include(g => g.Members)
            .Where(g => g.UserId == userId)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();

        return groups.Select(MapToGroupDto).ToList();
    }

    public async Task<ContactGroupDto> CreateGroupAsync(CreateContactGroupDto dto, string userId)
    {
        var group = new ContactGroup
        {
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            Icon = dto.Icon,
            IsDistributionList = dto.IsDistributionList,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<ContactGroup>().Add(group);
        await _context.SaveChangesAsync();

        return MapToGroupDto(group);
    }

    public async Task<bool> AddContactToGroupAsync(int contactId, int groupId, string userId)
    {
        var contact = await _context.Contacts.FindAsync(contactId);
        var group = await _context.Set<ContactGroup>().FindAsync(groupId);

        if (contact == null || group == null || contact.UserId != userId || group.UserId != userId)
            return false;

        var existing = await _context.Set<ContactGroupMembership>()
            .AnyAsync(m => m.ContactId == contactId && m.GroupId == groupId);

        if (existing) return true;

        _context.Set<ContactGroupMembership>().Add(new ContactGroupMembership
        {
            ContactId = contactId,
            GroupId = groupId,
            AddedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Helpers

    private string GenerateDisplayName(string firstName, string? middleName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(middleName))
            return $"{firstName} {lastName}".Trim();
        return $"{firstName} {middleName} {lastName}".Trim();
    }

    private string GenerateFileAs(string firstName, string lastName)
    {
        return $"{lastName}, {firstName}".Trim();
    }

    private ContactDto MapToContactDto(Contact contact)
    {
        return new ContactDto
        {
            Id = contact.Id,
            ContactId = contact.ContactId,
            UserId = contact.UserId,
            Title = contact.Title,
            FirstName = contact.FirstName,
            MiddleName = contact.MiddleName,
            LastName = contact.LastName,
            Suffix = contact.Suffix,
            Nickname = contact.Nickname,
            DisplayName = contact.DisplayName,
            FileAs = contact.FileAs,
            EmailAddresses = contact.EmailAddresses.Select(e => new ContactEmailDto
            {
                Id = e.Id,
                Type = e.Type,
                Email = e.Email,
                IsPrimary = e.IsPrimary,
                DisplayOrder = e.DisplayOrder
            }).ToList(),
            PhoneNumbers = contact.PhoneNumbers.Select(p => new ContactPhoneDto
            {
                Id = p.Id,
                Type = p.Type,
                PhoneNumber = p.PhoneNumber,
                Extension = p.Extension,
                IsPrimary = p.IsPrimary,
                DisplayOrder = p.DisplayOrder
            }).ToList(),
            JobTitle = contact.JobTitle,
            Company = contact.Company,
            PhotoUrl = contact.PhotoUrl,
            IsFavorite = contact.IsFavorite,
            IsBlocked = contact.IsBlocked,
            CreatedAt = contact.CreatedAt,
            LastModified = contact.LastModified
        };
    }

    private ContactGroupDto MapToGroupDto(ContactGroup group)
    {
        return new ContactGroupDto
        {
            Id = group.Id,
            GroupId = group.GroupId,
            UserId = group.UserId,
            Name = group.Name,
            Description = group.Description,
            Color = group.Color,
            Icon = group.Icon,
            IsDistributionList = group.IsDistributionList,
            IsSmartGroup = group.IsSmartGroup,
            MemberCount = group.Members?.Count ?? 0,
            CreatedAt = group.CreatedAt,
            LastModified = group.LastModified,
            DisplayOrder = group.DisplayOrder
        };
    }

    // Stub implementations for remaining interface methods
    public Task<ContactGroupDto?> UpdateGroupAsync(int groupId, UpdateContactGroupDto dto, string userId) => throw new NotImplementedException();
    public Task<bool> DeleteGroupAsync(int groupId, string userId) => throw new NotImplementedException();
    public Task<bool> RemoveContactFromGroupAsync(int contactId, int groupId, string userId) => throw new NotImplementedException();
    public Task<List<ContactDto>> GetGroupMembersAsync(int groupId, string userId) => throw new NotImplementedException();
    public Task<bool> SendEmailToGroupAsync(int groupId, string subject, string body, string userId) => throw new NotImplementedException();
    public Task<List<ContactInteractionDto>> GetContactInteractionsAsync(int contactId, string userId) => throw new NotImplementedException();
    public Task<ContactInteractionDto> AddInteractionAsync(int contactId, CreateInteractionDto dto, string userId) => throw new NotImplementedException();
    public Task<bool> DeleteInteractionAsync(int interactionId, string userId) => throw new NotImplementedException();
    public Task<bool> AddRelationshipAsync(int contactId, int relatedContactId, RelationshipType type, string? description, string userId) => throw new NotImplementedException();
    public Task<bool> RemoveRelationshipAsync(int relationshipId, string userId) => throw new NotImplementedException();
    public Task<List<ContactDto>> SearchContactsAsync(string searchTerm, string userId) => throw new NotImplementedException();
    public Task<List<ContactDto>> GetContactsByCompanyAsync(string company, string userId) => throw new NotImplementedException();
    public Task<List<ContactDto>> GetContactsByCategoryAsync(string category, string userId) => throw new NotImplementedException();
    public Task<List<ContactDto>> GetContactsWithBirthdayAsync(DateTime startDate, DateTime endDate, string userId) => throw new NotImplementedException();
    public Task<List<ContactDto>> GetContactsWithUpcomingBirthdaysAsync(string userId, int days = 30) => throw new NotImplementedException();
    public Task<List<List<ContactDto>>> FindDuplicateContactsAsync(string userId) => throw new NotImplementedException();
    public Task<ContactDto> MergeContactsAsync(MergeContactsDto dto, string userId) => throw new NotImplementedException();
    public Task<string> ExportContactToVCardAsync(int contactId, string userId) => throw new NotImplementedException();
    public Task<string> ExportContactsToVCardAsync(List<int> contactIds, string userId) => throw new NotImplementedException();
    public Task<ContactDto> ImportContactFromVCardAsync(string vCardContent, string userId) => throw new NotImplementedException();
    public Task<List<ContactDto>> ImportContactsFromVCardAsync(string vCardContent, string userId) => throw new NotImplementedException();
    public Task<byte[]> ExportContactsToCsvAsync(string userId) => throw new NotImplementedException();
    public Task<ContactStatisticsDto> GetStatisticsAsync(string userId) => throw new NotImplementedException();
    public Task<bool> BulkAddToGroupAsync(List<int> contactIds, int groupId, string userId) => throw new NotImplementedException();
    public Task<bool> BulkRemoveFromGroupAsync(List<int> contactIds, int groupId, string userId) => throw new NotImplementedException();
    public Task<bool> BulkAddCategoryAsync(List<int> contactIds, string category, string userId) => throw new NotImplementedException();
    public Task<bool> BulkDeleteAsync(List<int> contactIds, string userId) => throw new NotImplementedException();

    #endregion
}
