using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.DTOs.Contacts;
using Backend.Application.Interfaces;
using Backend.Domain.Enums;
using Backend.Infrastructure.Authorization;
using System.Security.Claims;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly IContactsService _contactsService;

    public ContactsController(IContactsService contactsService)
    {
        _contactsService = contactsService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ===== Contact CRUD =====

    [HttpGet]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var contacts = await _contactsService.GetAllAsync(GetUserId(), pageNumber, pageSize);
        return Ok(contacts);
    }

    [HttpGet("{id}")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<ContactDto>> GetById(int id)
    {
        try
        {
            var contact = await _contactsService.GetByIdAsync(id, GetUserId());
            return Ok(contact);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("search")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> Search([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var contacts = await _contactsService.SearchAsync(query, GetUserId(), pageNumber, pageSize);
        return Ok(contacts);
    }

    [HttpGet("favorites")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetFavorites()
    {
        var contacts = await _contactsService.GetFavoritesAsync(GetUserId());
        return Ok(contacts);
    }

    [HttpGet("by-company/{company}")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetByCompany(string company)
    {
        var contacts = await _contactsService.GetByCompanyAsync(company, GetUserId());
        return Ok(contacts);
    }

    [HttpGet("by-tag/{tag}")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetByTag(string tag)
    {
        var contacts = await _contactsService.GetByTagAsync(tag, GetUserId());
        return Ok(contacts);
    }

    [HttpGet("by-category/{category}")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetByCategory(string category)
    {
        var contacts = await _contactsService.GetByCategoryAsync(category, GetUserId());
        return Ok(contacts);
    }

    [HttpGet("recently-contacted")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetRecentlyContacted([FromQuery] int count = 10)
    {
        var contacts = await _contactsService.GetRecentlyContactedAsync(GetUserId(), count);
        return Ok(contacts);
    }

    [HttpGet("frequently-contacted")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetFrequentlyContacted([FromQuery] int count = 10)
    {
        var contacts = await _contactsService.GetFrequentlyContactedAsync(GetUserId(), count);
        return Ok(contacts);
    }

    [HttpGet("birthdays")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetByBirthday([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var contacts = await _contactsService.GetByBirthdayAsync(startDate, endDate, GetUserId());
        return Ok(contacts);
    }

    [HttpGet("birthdays/upcoming")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetUpcomingBirthdays([FromQuery] int days = 30)
    {
        var contacts = await _contactsService.GetUpcomingBirthdaysAsync(GetUserId(), days);
        return Ok(contacts);
    }

    [HttpPost]
    [Permission(SystemPermission.ContactsCreate)]
    public async Task<ActionResult<ContactDto>> Create([FromBody] CreateContactDto dto)
    {
        var contact = await _contactsService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
    }

    [HttpPut("{id}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateContactDto dto)
    {
        var result = await _contactsService.UpdateAsync(id, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Permission(SystemPermission.ContactsDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _contactsService.DeleteAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("delete-multiple")]
    [Permission(SystemPermission.ContactsDelete)]
    public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
    {
        await _contactsService.DeleteMultipleAsync(ids, GetUserId());
        return NoContent();
    }

    // ===== Contact Actions =====

    [HttpPost("{id}/favorite")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> MarkAsFavorite(int id)
    {
        var result = await _contactsService.MarkAsFavoriteAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}/favorite")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> UnmarkAsFavorite(int id)
    {
        var result = await _contactsService.UnmarkAsFavoriteAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/block")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> Block(int id)
    {
        var result = await _contactsService.BlockContactAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}/block")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> Unblock(int id)
    {
        var result = await _contactsService.UnblockContactAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/update-last-contacted")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> UpdateLastContacted(int id)
    {
        var result = await _contactsService.UpdateLastContactedAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Contact Emails =====

    [HttpPost("{contactId}/emails")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> AddEmail(int contactId, [FromBody] ContactEmailDto dto)
    {
        var result = await _contactsService.AddEmailAsync(contactId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("{contactId}/emails/{emailId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> UpdateEmail(int contactId, int emailId, [FromBody] ContactEmailDto dto)
    {
        var result = await _contactsService.UpdateEmailAsync(contactId, emailId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{contactId}/emails/{emailId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> RemoveEmail(int contactId, int emailId)
    {
        var result = await _contactsService.RemoveEmailAsync(contactId, emailId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{contactId}/emails/{emailId}/set-primary")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> SetPrimaryEmail(int contactId, int emailId)
    {
        var result = await _contactsService.SetPrimaryEmailAsync(contactId, emailId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Contact Phones =====

    [HttpPost("{contactId}/phones")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> AddPhone(int contactId, [FromBody] ContactPhoneDto dto)
    {
        var result = await _contactsService.AddPhoneAsync(contactId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("{contactId}/phones/{phoneId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> UpdatePhone(int contactId, int phoneId, [FromBody] ContactPhoneDto dto)
    {
        var result = await _contactsService.UpdatePhoneAsync(contactId, phoneId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{contactId}/phones/{phoneId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> RemovePhone(int contactId, int phoneId)
    {
        var result = await _contactsService.RemovePhoneAsync(contactId, phoneId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{contactId}/phones/{phoneId}/set-primary")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> SetPrimaryPhone(int contactId, int phoneId)
    {
        var result = await _contactsService.SetPrimaryPhoneAsync(contactId, phoneId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Contact Addresses =====

    [HttpPost("{contactId}/addresses")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> AddAddress(int contactId, [FromBody] ContactAddressDto dto)
    {
        var result = await _contactsService.AddAddressAsync(contactId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("{contactId}/addresses/{addressId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> UpdateAddress(int contactId, int addressId, [FromBody] ContactAddressDto dto)
    {
        var result = await _contactsService.UpdateAddressAsync(contactId, addressId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{contactId}/addresses/{addressId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> RemoveAddress(int contactId, int addressId)
    {
        var result = await _contactsService.RemoveAddressAsync(contactId, addressId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{contactId}/addresses/{addressId}/set-primary")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> SetPrimaryAddress(int contactId, int addressId)
    {
        var result = await _contactsService.SetPrimaryAddressAsync(contactId, addressId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Contact Websites =====

    [HttpPost("{contactId}/websites")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> AddWebsite(int contactId, [FromBody] ContactWebsiteDto dto)
    {
        var result = await _contactsService.AddWebsiteAsync(contactId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{contactId}/websites/{websiteId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> RemoveWebsite(int contactId, int websiteId)
    {
        var result = await _contactsService.RemoveWebsiteAsync(contactId, websiteId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Contact Custom Fields =====

    [HttpPost("{contactId}/custom-fields")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> AddCustomField(int contactId, [FromBody] ContactCustomFieldDto dto)
    {
        var result = await _contactsService.AddCustomFieldAsync(contactId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("{contactId}/custom-fields/{fieldId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> UpdateCustomField(int contactId, int fieldId, [FromBody] ContactCustomFieldDto dto)
    {
        var result = await _contactsService.UpdateCustomFieldAsync(contactId, fieldId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{contactId}/custom-fields/{fieldId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> RemoveCustomField(int contactId, int fieldId)
    {
        var result = await _contactsService.RemoveCustomFieldAsync(contactId, fieldId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Contact Relationships =====

    [HttpGet("{contactId}/relationships")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactRelationshipDto>>> GetRelationships(int contactId)
    {
        try
        {
            var relationships = await _contactsService.GetRelationshipsAsync(contactId, GetUserId());
            return Ok(relationships);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{contactId}/relationships")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> AddRelationship(int contactId, [FromBody] AddRelationshipRequest request)
    {
        var result = await _contactsService.AddRelationshipAsync(
            contactId,
            request.RelatedContactId,
            request.Type,
            request.Description,
            GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{contactId}/relationships/{relationshipId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> RemoveRelationship(int contactId, int relationshipId)
    {
        var result = await _contactsService.RemoveRelationshipAsync(contactId, relationshipId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Contact Groups =====

    [HttpGet("groups")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactGroupDto>>> GetGroups()
    {
        var groups = await _contactsService.GetGroupsAsync(GetUserId());
        return Ok(groups);
    }

    [HttpGet("groups/{id}")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<ContactGroupDto>> GetGroupById(int id)
    {
        try
        {
            var group = await _contactsService.GetGroupByIdAsync(id, GetUserId());
            return Ok(group);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("groups")]
    [Permission(SystemPermission.ContactsCreate)]
    public async Task<ActionResult<ContactGroupDto>> CreateGroup([FromBody] CreateContactGroupDto dto)
    {
        var group = await _contactsService.CreateGroupAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetGroupById), new { id = group.Id }, group);
    }

    [HttpPut("groups/{id}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> UpdateGroup(int id, [FromBody] UpdateContactGroupDto dto)
    {
        var result = await _contactsService.UpdateGroupAsync(id, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("groups/{id}")]
    [Permission(SystemPermission.ContactsDelete)]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var result = await _contactsService.DeleteGroupAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("groups/{groupId}/members")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetGroupMembers(int groupId)
    {
        try
        {
            var contacts = await _contactsService.GetGroupMembersAsync(groupId, GetUserId());
            return Ok(contacts);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("groups/{groupId}/members")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> AddContactToGroup(int groupId, [FromBody] AddContactToGroupDto dto)
    {
        var result = await _contactsService.AddContactToGroupAsync(groupId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("groups/{groupId}/members/{contactId}")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> RemoveContactFromGroup(int groupId, int contactId)
    {
        var result = await _contactsService.RemoveContactFromGroupAsync(groupId, contactId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("groups/{groupId}/members/bulk")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> AddMultipleContactsToGroup(int groupId, [FromBody] List<int> contactIds)
    {
        var result = await _contactsService.AddMultipleContactsToGroupAsync(groupId, contactIds, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Smart Groups =====

    [HttpGet("groups/{groupId}/smart-members")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDto>>> GetSmartGroupMembers(int groupId)
    {
        var contacts = await _contactsService.GetSmartGroupMembersAsync(groupId, GetUserId());
        return Ok(contacts);
    }

    [HttpPut("groups/{groupId}/smart-rules")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> UpdateSmartGroupRules(int groupId, [FromBody] string rules)
    {
        var result = await _contactsService.UpdateSmartGroupRulesAsync(groupId, rules, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("groups/{groupId}/refresh")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<IActionResult> RefreshSmartGroup(int groupId)
    {
        var result = await _contactsService.RefreshSmartGroupAsync(groupId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Duplicate Detection =====

    [HttpGet("duplicates")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<List<ContactDuplicateDto>>> FindDuplicates()
    {
        var duplicates = await _contactsService.FindDuplicatesAsync(GetUserId());
        return Ok(duplicates);
    }

    [HttpPost("merge")]
    [Permission(SystemPermission.ContactsUpdate)]
    public async Task<ActionResult<ContactDto>> MergeContacts([FromBody] MergeContactsRequest request)
    {
        try
        {
            var contact = await _contactsService.MergeContactsAsync(request.PrimaryId, request.DuplicateId, GetUserId());
            return Ok(contact);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ===== Import/Export =====

    [HttpPost("import/csv")]
    [Permission(SystemPermission.ContactsCreate)]
    public async Task<ActionResult<List<ContactDto>>> ImportFromCsv(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var contacts = await _contactsService.ImportFromCsvAsync(stream, GetUserId());
        return Ok(contacts);
    }

    [HttpPost("export/csv")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<IActionResult> ExportToCsv([FromBody] List<int> contactIds)
    {
        var stream = await _contactsService.ExportToCsvAsync(contactIds, GetUserId());
        return File(stream, "text/csv", "contacts.csv");
    }

    [HttpPost("export/vcard")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<IActionResult> ExportToVCard([FromBody] List<int> contactIds)
    {
        var stream = await _contactsService.ExportToVCardAsync(contactIds, GetUserId());
        return File(stream, "text/vcard", "contacts.vcf");
    }

    [HttpPost("import/vcard")]
    [Permission(SystemPermission.ContactsCreate)]
    public async Task<ActionResult<List<ContactDto>>> ImportFromVCard(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var contacts = await _contactsService.ImportFromVCardAsync(stream, GetUserId());
        return Ok(contacts);
    }

    // ===== Statistics =====

    [HttpGet("statistics")]
    [Permission(SystemPermission.ContactsRead)]
    public async Task<ActionResult<ContactStatisticsDto>> GetStatistics()
    {
        var statistics = await _contactsService.GetStatisticsAsync(GetUserId());
        return Ok(statistics);
    }
}

public record AddRelationshipRequest(int RelatedContactId, RelationshipType Type, string? Description);
public record MergeContactsRequest(int PrimaryId, int DuplicateId);
