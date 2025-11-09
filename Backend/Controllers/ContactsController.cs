using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlookInboxManagement.DTOs;
using OutlookInboxManagement.Models;
using OutlookInboxManagement.Services;
using System.Security.Claims;

namespace OutlookInboxManagement.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ContactsController : ControllerBase
{
    private readonly IContactsService _contactsService;

    public ContactsController(IContactsService contactsService)
    {
        _contactsService = contactsService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    #region Contact Management

    /// <summary>
    /// Get a specific contact by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ContactDto>> GetContact(int id)
    {
        var contact = await _contactsService.GetContactAsync(id, GetUserId());
        if (contact == null)
            return NotFound();

        return Ok(contact);
    }

    /// <summary>
    /// Get contact by ContactId (GUID)
    /// </summary>
    [HttpGet("by-contact-id/{contactId}")]
    public async Task<ActionResult<ContactDto>> GetContactByContactId(string contactId)
    {
        var contact = await _contactsService.GetContactByContactIdAsync(contactId, GetUserId());
        if (contact == null)
            return NotFound();

        return Ok(contact);
    }

    /// <summary>
    /// Get contacts with query parameters
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedList<ContactDto>>> GetContacts([FromQuery] ContactQueryParameters parameters)
    {
        var contacts = await _contactsService.GetContactsAsync(parameters, GetUserId());
        return Ok(contacts);
    }

    /// <summary>
    /// Get all contacts
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<ContactDto>>> GetAllContacts()
    {
        var contacts = await _contactsService.GetAllContactsAsync(GetUserId());
        return Ok(contacts);
    }

    /// <summary>
    /// Get favorite contacts
    /// </summary>
    [HttpGet("favorites")]
    public async Task<ActionResult<List<ContactDto>>> GetFavoriteContacts()
    {
        var contacts = await _contactsService.GetFavoriteContactsAsync(GetUserId());
        return Ok(contacts);
    }

    /// <summary>
    /// Get recent contacts
    /// </summary>
    [HttpGet("recent")]
    public async Task<ActionResult<List<ContactDto>>> GetRecentContacts([FromQuery] int count = 10)
    {
        var contacts = await _contactsService.GetRecentContactsAsync(GetUserId(), count);
        return Ok(contacts);
    }

    /// <summary>
    /// Get frequent contacts
    /// </summary>
    [HttpGet("frequent")]
    public async Task<ActionResult<List<ContactDto>>> GetFrequentContacts([FromQuery] int count = 10)
    {
        var contacts = await _contactsService.GetFrequentContactsAsync(GetUserId(), count);
        return Ok(contacts);
    }

    /// <summary>
    /// Create a new contact
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ContactDto>> CreateContact([FromBody] CreateContactDto dto)
    {
        var contact = await _contactsService.CreateContactAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
    }

    /// <summary>
    /// Update a contact
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ContactDto>> UpdateContact(int id, [FromBody] UpdateContactDto dto)
    {
        var contact = await _contactsService.UpdateContactAsync(id, dto, GetUserId());
        if (contact == null)
            return NotFound();

        return Ok(contact);
    }

    /// <summary>
    /// Delete a contact
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var result = await _contactsService.DeleteContactAsync(id, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Toggle favorite status
    /// </summary>
    [HttpPost("{id}/toggle-favorite")]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var result = await _contactsService.ToggleFavoriteAsync(id, GetUserId());
        if (!result)
            return NotFound();

        return Ok();
    }

    /// <summary>
    /// Toggle block status
    /// </summary>
    [HttpPost("{id}/toggle-block")]
    public async Task<IActionResult> ToggleBlock(int id)
    {
        var result = await _contactsService.ToggleBlockAsync(id, GetUserId());
        if (!result)
            return NotFound();

        return Ok();
    }

    #endregion

    #region Photo Management

    /// <summary>
    /// Upload contact photo
    /// </summary>
    [HttpPost("{id}/photo")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        using var stream = file.OpenReadStream();
        var result = await _contactsService.UploadPhotoAsync(id, stream, file.FileName, GetUserId());
        if (!result)
            return NotFound();

        return Ok();
    }

    /// <summary>
    /// Delete contact photo
    /// </summary>
    [HttpDelete("{id}/photo")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        var result = await _contactsService.DeletePhotoAsync(id, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get contact photo
    /// </summary>
    [HttpGet("{id}/photo")]
    public async Task<IActionResult> GetPhoto(int id)
    {
        var result = await _contactsService.GetPhotoAsync(id, GetUserId());
        if (result == null)
            return NotFound();

        var (stream, contentType) = result.Value;
        return File(stream!, contentType, $"contact-{id}.jpg");
    }

    #endregion

    #region Contact Groups

    /// <summary>
    /// Get a specific group
    /// </summary>
    [HttpGet("groups/{id}")]
    public async Task<ActionResult<ContactGroupDto>> GetGroup(int id)
    {
        var group = await _contactsService.GetGroupAsync(id, GetUserId());
        if (group == null)
            return NotFound();

        return Ok(group);
    }

    /// <summary>
    /// Get all groups
    /// </summary>
    [HttpGet("groups")]
    public async Task<ActionResult<List<ContactGroupDto>>> GetGroups()
    {
        var groups = await _contactsService.GetGroupsAsync(GetUserId());
        return Ok(groups);
    }

    /// <summary>
    /// Create a new group
    /// </summary>
    [HttpPost("groups")]
    public async Task<ActionResult<ContactGroupDto>> CreateGroup([FromBody] CreateContactGroupDto dto)
    {
        var group = await _contactsService.CreateGroupAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetGroup), new { id = group.Id }, group);
    }

    /// <summary>
    /// Add contact to group
    /// </summary>
    [HttpPost("{contactId}/groups/{groupId}")]
    public async Task<IActionResult> AddContactToGroup(int contactId, int groupId)
    {
        var result = await _contactsService.AddContactToGroupAsync(contactId, groupId, GetUserId());
        if (!result)
            return NotFound();

        return Ok();
    }

    #endregion
}
