using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs.Contacts;
using Backend.Application.Interfaces;
using Backend.Domain.Entities.Contacts;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Services;

public class ContactsService : IContactsService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;

    public ContactsService(ApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    // ===== Contact CRUD =====
    public async Task<List<ContactDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50)
    {
        if (!await _userService.HasPermissionAsync(userId, "contacts.read"))
            throw new UnauthorizedAccessException("Permission denied: contacts.read");

        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.Addresses)
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<ContactDto> GetByIdAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "contacts.read"))
            throw new UnauthorizedAccessException("Permission denied: contacts.read");

        var contact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.Addresses)
            .Include(c => c.Websites)
            .Include(c => c.CustomFields)
            .Include(c => c.GroupMemberships)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);

        if (contact == null)
            throw new KeyNotFoundException($"Contact with ID {id} not found");

        return MapToDto(contact);
    }

    public async Task<ContactDto> GetByContactIdAsync(string contactId, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "contacts.read"))
            throw new UnauthorizedAccessException("Permission denied: contacts.read");

        var contact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.ContactId == contactId && c.UserId == userId && !c.IsDeleted);

        if (contact == null)
            throw new KeyNotFoundException($"Contact with ContactId {contactId} not found");

        return MapToDto(contact);
    }

    public async Task<ContactDto> CreateAsync(CreateContactDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "contacts.create"))
            throw new UnauthorizedAccessException("Permission denied: contacts.create");

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
            DisplayName = $"{dto.FirstName} {dto.LastName}".Trim(),
            JobTitle = dto.JobTitle,
            Department = dto.Department,
            Company = dto.Company,
            Manager = dto.Manager,
            Birthday = dto.Birthday,
            Notes = dto.Notes,
            Categories = dto.Categories != null ? string.Join(",", dto.Categories) : null,
            Tags = dto.Tags != null ? string.Join(",", dto.Tags) : null,
            CreatedAt = DateTime.UtcNow,
            Source = ContactSource.Manual
        };

        // Add primary email if provided
        if (!string.IsNullOrEmpty(dto.Email))
        {
            contact.EmailAddresses.Add(new ContactEmail
            {
                Email = dto.Email,
                Type = EmailType.Personal,
                IsPrimary = true,
                DisplayOrder = 0
            });
        }

        // Add primary phone if provided
        if (!string.IsNullOrEmpty(dto.Phone))
        {
            contact.PhoneNumbers.Add(new ContactPhone
            {
                PhoneNumber = dto.Phone,
                Type = PhoneType.Mobile,
                IsPrimary = true,
                DisplayOrder = 0
            });
        }

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();

        return MapToDto(contact);
    }

    public async Task<bool> UpdateAsync(int id, UpdateContactDto dto, string userId)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsDeleted);
        if (contact == null) return false;

        if (dto.Title != null) contact.Title = dto.Title;
        if (dto.FirstName != null) contact.FirstName = dto.FirstName;
        if (dto.MiddleName != null) contact.MiddleName = dto.MiddleName;
        if (dto.LastName != null) contact.LastName = dto.LastName;
        if (dto.Suffix != null) contact.Suffix = dto.Suffix;
        if (dto.Nickname != null) contact.Nickname = dto.Nickname;
        if (dto.JobTitle != null) contact.JobTitle = dto.JobTitle;
        if (dto.Department != null) contact.Department = dto.Department;
        if (dto.Company != null) contact.Company = dto.Company;
        if (dto.Manager != null) contact.Manager = dto.Manager;
        if (dto.Assistant != null) contact.Assistant = dto.Assistant;
        if (dto.OfficeLocation != null) contact.OfficeLocation = dto.OfficeLocation;
        if (dto.Birthday.HasValue) contact.Birthday = dto.Birthday.Value;
        if (dto.SpouseName != null) contact.SpouseName = dto.SpouseName;
        if (dto.Children != null) contact.Children = dto.Children;
        if (dto.Gender != null) contact.Gender = dto.Gender;
        if (dto.Notes != null) contact.Notes = dto.Notes;
        if (dto.Categories != null) contact.Categories = string.Join(",", dto.Categories);
        if (dto.Tags != null) contact.Tags = string.Join(",", dto.Tags);
        if (dto.LinkedInUrl != null) contact.LinkedInUrl = dto.LinkedInUrl;
        if (dto.TwitterHandle != null) contact.TwitterHandle = dto.TwitterHandle;
        if (dto.FacebookUrl != null) contact.FacebookUrl = dto.FacebookUrl;
        if (dto.InstagramHandle != null) contact.InstagramHandle = dto.InstagramHandle;
        if (dto.IsFavorite.HasValue) contact.IsFavorite = dto.IsFavorite.Value;
        if (dto.IsBlocked.HasValue) contact.IsBlocked = dto.IsBlocked.Value;
        if (dto.Privacy.HasValue) contact.Privacy = dto.Privacy.Value;

        contact.DisplayName = $"{contact.FirstName} {contact.LastName}".Trim();
        contact.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "contacts.delete"))
            throw new UnauthorizedAccessException("Permission denied: contacts.delete");

        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (contact == null) return false;

        contact.IsDeleted = true;
        contact.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteMultipleAsync(List<int> ids, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "contacts.delete"))
            throw new UnauthorizedAccessException("Permission denied: contacts.delete");

        var contacts = await _context.Contacts
            .Where(c => ids.Contains(c.Id) && c.UserId == userId)
            .ToListAsync();

        foreach (var contact in contacts)
        {
            contact.IsDeleted = true;
            contact.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Contact Search & Filter =====
    public async Task<List<ContactDto>> SearchAsync(string query, string userId, int pageNumber = 1, int pageSize = 50)
    {
        if (!await _userService.HasPermissionAsync(userId, "contacts.read"))
            throw new UnauthorizedAccessException("Permission denied: contacts.read");

        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted &&
                       (c.FirstName.Contains(query) ||
                        c.LastName.Contains(query) ||
                        c.DisplayName.Contains(query) ||
                        (c.Company != null && c.Company.Contains(query)) ||
                        c.EmailAddresses.Any(e => e.Email.Contains(query))))
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<List<ContactDto>> GetFavoritesAsync(string userId)
    {
        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && c.IsFavorite && !c.IsDeleted)
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<List<ContactDto>> GetByCompanyAsync(string company, string userId)
    {
        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && c.Company == company && !c.IsDeleted)
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<List<ContactDto>> GetByTagAsync(string tag, string userId)
    {
        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted && c.Tags != null && c.Tags.Contains(tag))
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<List<ContactDto>> GetByCategoryAsync(string category, string userId)
    {
        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted && c.Categories != null && c.Categories.Contains(category))
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<List<ContactDto>> GetRecentlyContactedAsync(string userId, int count = 10)
    {
        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted && c.LastContacted.HasValue)
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .OrderByDescending(c => c.LastContacted)
            .Take(count)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<List<ContactDto>> GetFrequentlyContactedAsync(string userId, int count = 10)
    {
        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .OrderByDescending(c => c.ContactFrequency)
            .Take(count)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<List<ContactDto>> GetByBirthdayAsync(DateTime startDate, DateTime endDate, string userId)
    {
        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted && c.Birthday.HasValue &&
                       c.Birthday.Value >= startDate && c.Birthday.Value <= endDate)
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .OrderBy(c => c.Birthday)
            .ToListAsync();

        return contacts.Select(MapToDto).ToList();
    }

    public async Task<List<ContactDto>> GetUpcomingBirthdaysAsync(string userId, int days = 30)
    {
        var today = DateTime.Today;
        var endDate = today.AddDays(days);

        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted && c.Birthday.HasValue)
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .ToListAsync();

        // Filter by birthday month/day within next X days
        var upcomingBirthdays = contacts
            .Where(c => c.Birthday.HasValue)
            .Where(c =>
            {
                var birthday = c.Birthday!.Value;
                var nextBirthday = new DateTime(today.Year, birthday.Month, birthday.Day);
                if (nextBirthday < today)
                    nextBirthday = nextBirthday.AddYears(1);
                return nextBirthday <= endDate;
            })
            .OrderBy(c => c.Birthday!.Value.Month).ThenBy(c => c.Birthday!.Value.Day)
            .ToList();

        return upcomingBirthdays.Select(MapToDto).ToList();
    }

    // ===== Contact Actions =====
    public async Task<bool> MarkAsFavoriteAsync(int id, string userId)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (contact == null) return false;

        contact.IsFavorite = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnmarkAsFavoriteAsync(int id, string userId)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (contact == null) return false;

        contact.IsFavorite = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BlockContactAsync(int id, string userId)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (contact == null) return false;

        contact.IsBlocked = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnblockContactAsync(int id, string userId)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (contact == null) return false;

        contact.IsBlocked = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLastContactedAsync(int id, string userId)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (contact == null) return false;

        contact.LastContacted = DateTime.UtcNow;
        contact.ContactFrequency++;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Contact Details =====
    public async Task<bool> AddEmailAsync(int contactId, ContactEmailDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        // If this is set as primary, unset other primary emails
        if (dto.IsPrimary)
        {
            foreach (var email in contact.EmailAddresses)
            {
                email.IsPrimary = false;
            }
        }

        contact.EmailAddresses.Add(new ContactEmail
        {
            ContactId = contactId,
            Type = dto.Type,
            Email = dto.Email,
            IsPrimary = dto.IsPrimary,
            DisplayOrder = dto.DisplayOrder
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateEmailAsync(int contactId, int emailId, ContactEmailDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        var email = contact.EmailAddresses.FirstOrDefault(e => e.Id == emailId);
        if (email == null) return false;

        // If this is set as primary, unset other primary emails
        if (dto.IsPrimary)
        {
            foreach (var e in contact.EmailAddresses.Where(e => e.Id != emailId))
            {
                e.IsPrimary = false;
            }
        }

        email.Type = dto.Type;
        email.Email = dto.Email;
        email.IsPrimary = dto.IsPrimary;
        email.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveEmailAsync(int contactId, int emailId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        var email = contact.EmailAddresses.FirstOrDefault(e => e.Id == emailId);
        if (email == null) return false;

        contact.EmailAddresses.Remove(email);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetPrimaryEmailAsync(int contactId, int emailId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        foreach (var email in contact.EmailAddresses)
        {
            email.IsPrimary = email.Id == emailId;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddPhoneAsync(int contactId, ContactPhoneDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.PhoneNumbers)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        if (dto.IsPrimary)
        {
            foreach (var phone in contact.PhoneNumbers)
            {
                phone.IsPrimary = false;
            }
        }

        contact.PhoneNumbers.Add(new ContactPhone
        {
            ContactId = contactId,
            Type = dto.Type,
            PhoneNumber = dto.PhoneNumber,
            Extension = dto.Extension,
            IsPrimary = dto.IsPrimary,
            DisplayOrder = dto.DisplayOrder
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePhoneAsync(int contactId, int phoneId, ContactPhoneDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.PhoneNumbers)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        var phone = contact.PhoneNumbers.FirstOrDefault(p => p.Id == phoneId);
        if (phone == null) return false;

        if (dto.IsPrimary)
        {
            foreach (var p in contact.PhoneNumbers.Where(p => p.Id != phoneId))
            {
                p.IsPrimary = false;
            }
        }

        phone.Type = dto.Type;
        phone.PhoneNumber = dto.PhoneNumber;
        phone.Extension = dto.Extension;
        phone.IsPrimary = dto.IsPrimary;
        phone.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemovePhoneAsync(int contactId, int phoneId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.PhoneNumbers)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        var phone = contact.PhoneNumbers.FirstOrDefault(p => p.Id == phoneId);
        if (phone == null) return false;

        contact.PhoneNumbers.Remove(phone);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetPrimaryPhoneAsync(int contactId, int phoneId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.PhoneNumbers)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        foreach (var phone in contact.PhoneNumbers)
        {
            phone.IsPrimary = phone.Id == phoneId;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddAddressAsync(int contactId, ContactAddressDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        if (dto.IsPrimary)
        {
            foreach (var address in contact.Addresses)
            {
                address.IsPrimary = false;
            }
        }

        contact.Addresses.Add(new ContactAddress
        {
            ContactId = contactId,
            Type = dto.Type,
            Street = dto.Street,
            Street2 = dto.Street2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            IsPrimary = dto.IsPrimary,
            DisplayOrder = dto.DisplayOrder
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAddressAsync(int contactId, int addressId, ContactAddressDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        var address = contact.Addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null) return false;

        if (dto.IsPrimary)
        {
            foreach (var a in contact.Addresses.Where(a => a.Id != addressId))
            {
                a.IsPrimary = false;
            }
        }

        address.Type = dto.Type;
        address.Street = dto.Street;
        address.Street2 = dto.Street2;
        address.City = dto.City;
        address.State = dto.State;
        address.PostalCode = dto.PostalCode;
        address.Country = dto.Country;
        address.IsPrimary = dto.IsPrimary;
        address.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAddressAsync(int contactId, int addressId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        var address = contact.Addresses.FirstOrDefault(a => a.Id == addressId);
        if (address == null) return false;

        contact.Addresses.Remove(address);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetPrimaryAddressAsync(int contactId, int addressId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        foreach (var address in contact.Addresses)
        {
            address.IsPrimary = address.Id == addressId;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddWebsiteAsync(int contactId, ContactWebsiteDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Websites)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        contact.Websites.Add(new ContactWebsite
        {
            ContactId = contactId,
            Type = dto.Type,
            Url = dto.Url,
            DisplayOrder = dto.DisplayOrder
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveWebsiteAsync(int contactId, int websiteId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Websites)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        var website = contact.Websites.FirstOrDefault(w => w.Id == websiteId);
        if (website == null) return false;

        contact.Websites.Remove(website);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddCustomFieldAsync(int contactId, ContactCustomFieldDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.CustomFields)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        contact.CustomFields.Add(new ContactCustomField
        {
            ContactId = contactId,
            FieldName = dto.FieldName,
            FieldValue = dto.FieldValue,
            FieldType = dto.FieldType
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCustomFieldAsync(int contactId, int fieldId, ContactCustomFieldDto dto, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.CustomFields)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        var field = contact.CustomFields.FirstOrDefault(f => f.Id == fieldId);
        if (field == null) return false;

        field.FieldName = dto.FieldName;
        field.FieldValue = dto.FieldValue;
        field.FieldType = dto.FieldType;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveCustomFieldAsync(int contactId, int fieldId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.CustomFields)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        if (contact == null) return false;

        var field = contact.CustomFields.FirstOrDefault(f => f.Id == fieldId);
        if (field == null) return false;

        contact.CustomFields.Remove(field);
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Contact Relationships =====
    public async Task<List<ContactRelationshipDto>> GetRelationshipsAsync(int contactId, string userId)
    {
        var contact = await _context.Contacts
            .Include(c => c.Relationships)
            .FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);

        if (contact == null)
            throw new KeyNotFoundException($"Contact with ID {contactId} not found");

        return contact.Relationships.Select(r => new ContactRelationshipDto
        {
            Id = r.Id,
            RelatedContactId = r.RelatedContactId,
            Type = r.Type,
            Description = r.Description
        }).ToList();
    }

    public async Task<bool> AddRelationshipAsync(int contactId, int relatedContactId, RelationshipType type, string? description, string userId)
    {
        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == contactId && c.UserId == userId);
        var relatedContact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == relatedContactId && c.UserId == userId);

        if (contact == null || relatedContact == null) return false;

        var relationship = new ContactRelationship
        {
            ContactId = contactId,
            RelatedContactId = relatedContactId,
            Type = type,
            Description = description
        };

        _context.ContactRelationships.Add(relationship);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveRelationshipAsync(int contactId, int relationshipId, string userId)
    {
        var relationship = await _context.ContactRelationships
            .Include(r => r.Contact)
            .FirstOrDefaultAsync(r => r.Id == relationshipId && r.ContactId == contactId && r.Contact!.UserId == userId);

        if (relationship == null) return false;

        _context.ContactRelationships.Remove(relationship);
        await _context.SaveChangesAsync();

        return true;
    }

    // ===== Contact Groups =====
    public async Task<List<ContactGroupDto>> GetGroupsAsync(string userId)
    {
        var groups = await _context.ContactGroups
            .Where(g => g.UserId == userId && !g.IsDeleted)
            .Include(g => g.Members)
            .OrderBy(g => g.DisplayOrder)
            .ToListAsync();

        return groups.Select(MapGroupToDto).ToList();
    }

    public async Task<ContactGroupDto> GetGroupByIdAsync(int id, string userId)
    {
        var group = await _context.ContactGroups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId && !g.IsDeleted);

        if (group == null)
            throw new KeyNotFoundException($"Group with ID {id} not found");

        return MapGroupToDto(group);
    }

    public async Task<ContactGroupDto> CreateGroupAsync(CreateContactGroupDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "contacts.create"))
            throw new UnauthorizedAccessException("Permission denied: contacts.create");

        var group = new ContactGroup
        {
            GroupId = Guid.NewGuid().ToString(),
            UserId = userId,
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            Icon = dto.Icon,
            IsDistributionList = dto.IsDistributionList,
            IsSmartGroup = dto.IsSmartGroup,
            SmartGroupRules = dto.SmartGroupRules,
            CreatedAt = DateTime.UtcNow
        };

        _context.ContactGroups.Add(group);
        await _context.SaveChangesAsync();

        return MapGroupToDto(group);
    }

    public async Task<bool> UpdateGroupAsync(int id, UpdateContactGroupDto dto, string userId)
    {
        var group = await _context.ContactGroups.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId && !g.IsDeleted);
        if (group == null) return false;

        if (dto.Name != null) group.Name = dto.Name;
        if (dto.Description != null) group.Description = dto.Description;
        if (dto.Color != null) group.Color = dto.Color;
        if (dto.Icon != null) group.Icon = dto.Icon;
        if (dto.DisplayOrder.HasValue) group.DisplayOrder = dto.DisplayOrder.Value;

        group.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteGroupAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "contacts.delete"))
            throw new UnauthorizedAccessException("Permission denied: contacts.delete");

        var group = await _context.ContactGroups.FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);
        if (group == null) return false;

        group.IsDeleted = true;
        group.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ContactDto>> GetGroupMembersAsync(int groupId, string userId)
    {
        var group = await _context.ContactGroups
            .Include(g => g.Members)
            .ThenInclude(m => m.Contact)
            .ThenInclude(c => c!.EmailAddresses)
            .FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId && !g.IsDeleted);

        if (group == null)
            throw new KeyNotFoundException($"Group with ID {groupId} not found");

        return group.Members
            .Where(m => m.Contact != null && !m.Contact.IsDeleted)
            .Select(m => MapToDto(m.Contact!))
            .ToList();
    }

    public async Task<bool> AddContactToGroupAsync(int groupId, AddContactToGroupDto dto, string userId)
    {
        var group = await _context.ContactGroups.FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId && !g.IsDeleted);
        var contact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == dto.ContactId && c.UserId == userId && !c.IsDeleted);

        if (group == null || contact == null) return false;

        // Check if already member
        var exists = await _context.ContactGroupMemberships
            .AnyAsync(m => m.GroupId == groupId && m.ContactId == dto.ContactId);

        if (exists) return false;

        var membership = new ContactGroupMembership
        {
            GroupId = groupId,
            ContactId = dto.ContactId,
            Notes = dto.Notes,
            AddedAt = DateTime.UtcNow
        };

        _context.ContactGroupMemberships.Add(membership);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveContactFromGroupAsync(int groupId, int contactId, string userId)
    {
        var membership = await _context.ContactGroupMemberships
            .Include(m => m.Group)
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.ContactId == contactId && m.Group!.UserId == userId);

        if (membership == null) return false;

        _context.ContactGroupMemberships.Remove(membership);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddMultipleContactsToGroupAsync(int groupId, List<int> contactIds, string userId)
    {
        var group = await _context.ContactGroups.FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId && !g.IsDeleted);
        if (group == null) return false;

        foreach (var contactId in contactIds)
        {
            var exists = await _context.ContactGroupMemberships
                .AnyAsync(m => m.GroupId == groupId && m.ContactId == contactId);

            if (!exists)
            {
                _context.ContactGroupMemberships.Add(new ContactGroupMembership
                {
                    GroupId = groupId,
                    ContactId = contactId,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Smart Groups =====
    public async Task<List<ContactDto>> GetSmartGroupMembersAsync(int groupId, string userId)
    {
        // Smart groups would require parsing rules and filtering contacts
        // For now, return regular group members
        return await GetGroupMembersAsync(groupId, userId);
    }

    public async Task<bool> UpdateSmartGroupRulesAsync(int groupId, string rules, string userId)
    {
        var group = await _context.ContactGroups.FirstOrDefaultAsync(g => g.Id == groupId && g.UserId == userId && g.IsSmartGroup);
        if (group == null) return false;

        group.SmartGroupRules = rules;
        group.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RefreshSmartGroupAsync(int groupId, string userId)
    {
        // This would parse rules and rebuild group membership
        // For now, return true
        return true;
    }

    // ===== Import/Export =====
    public async Task<List<ContactDto>> ImportFromCsvAsync(Stream csvStream, string userId)
    {
        // CSV import would require CSV parsing library
        // Placeholder implementation
        return new List<ContactDto>();
    }

    public async Task<Stream> ExportToCsvAsync(List<int> contactIds, string userId)
    {
        // CSV export implementation
        return new MemoryStream();
    }

    public async Task<Stream> ExportToVCardAsync(List<int> contactIds, string userId)
    {
        // vCard export implementation
        return new MemoryStream();
    }

    public async Task<List<ContactDto>> ImportFromVCardAsync(Stream vcardStream, string userId)
    {
        // vCard import implementation
        return new List<ContactDto>();
    }

    // ===== Merge & Duplicate Detection =====
    public async Task<List<ContactDuplicateDto>> FindDuplicatesAsync(string userId)
    {
        // Simple duplicate detection based on email or full name
        var contacts = await _context.Contacts
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Include(c => c.EmailAddresses)
            .ToListAsync();

        var duplicates = new List<ContactDuplicateDto>();

        // Find contacts with same email
        var emailGroups = contacts
            .Where(c => c.EmailAddresses.Any())
            .SelectMany(c => c.EmailAddresses.Select(e => new { Contact = c, Email = e.Email }))
            .GroupBy(x => x.Email)
            .Where(g => g.Count() > 1);

        foreach (var group in emailGroups)
        {
            var contactList = group.Select(x => x.Contact).Distinct().ToList();
            for (int i = 0; i < contactList.Count - 1; i++)
            {
                duplicates.Add(new ContactDuplicateDto
                {
                    Contact1 = MapToDto(contactList[i]),
                    Contact2 = MapToDto(contactList[i + 1]),
                    SimilarityScore = 0.9,
                    MatchedFields = new List<string> { "Email" }
                });
            }
        }

        return duplicates;
    }

    public async Task<ContactDto> MergeContactsAsync(int primaryId, int duplicateId, string userId)
    {
        var primaryContact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == primaryId && c.UserId == userId && !c.IsDeleted);

        var duplicateContact = await _context.Contacts
            .Include(c => c.EmailAddresses)
            .Include(c => c.PhoneNumbers)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == duplicateId && c.UserId == userId && !c.IsDeleted);

        if (primaryContact == null || duplicateContact == null)
            throw new KeyNotFoundException("One or both contacts not found");

        // Merge emails
        foreach (var email in duplicateContact.EmailAddresses)
        {
            if (!primaryContact.EmailAddresses.Any(e => e.Email == email.Email))
            {
                email.ContactId = primaryId;
                email.IsPrimary = false;
                primaryContact.EmailAddresses.Add(email);
            }
        }

        // Merge phones
        foreach (var phone in duplicateContact.PhoneNumbers)
        {
            if (!primaryContact.PhoneNumbers.Any(p => p.PhoneNumber == phone.PhoneNumber))
            {
                phone.ContactId = primaryId;
                phone.IsPrimary = false;
                primaryContact.PhoneNumbers.Add(phone);
            }
        }

        // Mark duplicate as deleted
        duplicateContact.IsDeleted = true;
        duplicateContact.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(primaryContact);
    }

    // ===== Statistics =====
    public async Task<ContactStatisticsDto> GetStatisticsAsync(string userId)
    {
        var totalContacts = await _context.Contacts.CountAsync(c => c.UserId == userId && !c.IsDeleted);
        var favoriteContacts = await _context.Contacts.CountAsync(c => c.UserId == userId && c.IsFavorite && !c.IsDeleted);
        var blockedContacts = await _context.Contacts.CountAsync(c => c.UserId == userId && c.IsBlocked && !c.IsDeleted);
        var totalGroups = await _context.ContactGroups.CountAsync(g => g.UserId == userId && !g.IsDeleted);
        var contactsWithBirthdays = await _context.Contacts.CountAsync(c => c.UserId == userId && c.Birthday.HasValue && !c.IsDeleted);

        var topContacted = await GetFrequentlyContactedAsync(userId, 5);

        return new ContactStatisticsDto
        {
            TotalContacts = totalContacts,
            FavoriteContacts = favoriteContacts,
            BlockedContacts = blockedContacts,
            TotalGroups = totalGroups,
            ContactsWithBirthdays = contactsWithBirthdays,
            TopContacted = topContacted
        };
    }

    // ===== Helper Methods =====
    private ContactDto MapToDto(Contact contact)
    {
        return new ContactDto
        {
            Id = contact.Id,
            ContactId = contact.ContactId,
            Title = contact.Title,
            FirstName = contact.FirstName,
            MiddleName = contact.MiddleName,
            LastName = contact.LastName,
            Suffix = contact.Suffix,
            Nickname = contact.Nickname,
            DisplayName = contact.DisplayName,
            FileAs = contact.FileAs,
            EmailAddresses = contact.EmailAddresses?.Select(e => new ContactEmailDto
            {
                Id = e.Id,
                Type = e.Type,
                Email = e.Email,
                IsPrimary = e.IsPrimary,
                DisplayOrder = e.DisplayOrder
            }).ToList() ?? new List<ContactEmailDto>(),
            PhoneNumbers = contact.PhoneNumbers?.Select(p => new ContactPhoneDto
            {
                Id = p.Id,
                Type = p.Type,
                PhoneNumber = p.PhoneNumber,
                Extension = p.Extension,
                IsPrimary = p.IsPrimary,
                DisplayOrder = p.DisplayOrder
            }).ToList() ?? new List<ContactPhoneDto>(),
            Addresses = contact.Addresses?.Select(a => new ContactAddressDto
            {
                Id = a.Id,
                Type = a.Type,
                Street = a.Street,
                Street2 = a.Street2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                IsPrimary = a.IsPrimary,
                DisplayOrder = a.DisplayOrder
            }).ToList() ?? new List<ContactAddressDto>(),
            Websites = contact.Websites?.Select(w => new ContactWebsiteDto
            {
                Id = w.Id,
                Type = w.Type,
                Url = w.Url,
                DisplayOrder = w.DisplayOrder
            }).ToList() ?? new List<ContactWebsiteDto>(),
            JobTitle = contact.JobTitle,
            Department = contact.Department,
            Company = contact.Company,
            Manager = contact.Manager,
            Assistant = contact.Assistant,
            OfficeLocation = contact.OfficeLocation,
            Birthday = contact.Birthday,
            SpouseName = contact.SpouseName,
            Children = contact.Children,
            Gender = contact.Gender,
            Notes = contact.Notes,
            Categories = contact.Categories?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
            Tags = contact.Tags?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>(),
            LinkedInUrl = contact.LinkedInUrl,
            TwitterHandle = contact.TwitterHandle,
            FacebookUrl = contact.FacebookUrl,
            InstagramHandle = contact.InstagramHandle,
            PhotoUrl = contact.PhotoUrl,
            GroupMemberships = contact.GroupMemberships?.Select(m => new ContactGroupMembershipDto
            {
                Id = m.Id,
                ContactId = m.ContactId,
                GroupId = m.GroupId,
                GroupName = m.Group?.Name ?? "",
                AddedAt = m.AddedAt,
                Notes = m.Notes
            }).ToList() ?? new List<ContactGroupMembershipDto>(),
            Relationships = contact.Relationships?.Select(r => new ContactRelationshipDto
            {
                Id = r.Id,
                RelatedContactId = r.RelatedContactId,
                Type = r.Type,
                Description = r.Description
            }).ToList() ?? new List<ContactRelationshipDto>(),
            CustomFields = contact.CustomFields?.Select(f => new ContactCustomFieldDto
            {
                Id = f.Id,
                FieldName = f.FieldName,
                FieldValue = f.FieldValue,
                FieldType = f.FieldType
            }).ToList() ?? new List<ContactCustomFieldDto>(),
            IsFavorite = contact.IsFavorite,
            IsBlocked = contact.IsBlocked,
            CreatedAt = contact.CreatedAt,
            LastModified = contact.LastModified,
            LastContacted = contact.LastContacted,
            ContactFrequency = contact.ContactFrequency,
            Source = contact.Source,
            SourceId = contact.SourceId,
            Privacy = contact.Privacy
        };
    }

    private ContactGroupDto MapGroupToDto(ContactGroup group)
    {
        return new ContactGroupDto
        {
            Id = group.Id,
            GroupId = group.GroupId,
            Name = group.Name,
            Description = group.Description,
            Color = group.Color,
            Icon = group.Icon,
            MemberCount = group.Members?.Count ?? 0,
            IsDistributionList = group.IsDistributionList,
            IsSmartGroup = group.IsSmartGroup,
            CreatedAt = group.CreatedAt,
            LastModified = group.LastModified,
            DisplayOrder = group.DisplayOrder
        };
    }
}
