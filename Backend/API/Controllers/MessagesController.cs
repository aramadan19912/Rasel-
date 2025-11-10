using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.DTOs.Messages;
using Backend.Application.Interfaces;
using Backend.Domain.Enums;
using Backend.Infrastructure.Authorization;
using System.Security.Claims;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;

    public MessagesController(IMessageService messageService)
    {
        _messageService = messageService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ===== Message CRUD =====

    [HttpGet]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageDto>>> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _messageService.GetAllAsync(GetUserId(), pageNumber, pageSize);
        return Ok(messages);
    }

    [HttpGet("{id}")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<MessageDto>> GetById(int id)
    {
        try
        {
            var message = await _messageService.GetByIdAsync(id, GetUserId());
            return Ok(message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("unread")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageDto>>> GetUnread([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _messageService.GetUnreadAsync(GetUserId(), pageNumber, pageSize);
        return Ok(messages);
    }

    [HttpGet("flagged")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageDto>>> GetFlagged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _messageService.GetFlaggedAsync(GetUserId(), pageNumber, pageSize);
        return Ok(messages);
    }

    [HttpGet("drafts")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageDto>>> GetDrafts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _messageService.GetDraftsAsync(GetUserId(), pageNumber, pageSize);
        return Ok(messages);
    }

    [HttpGet("folder/{folderId}")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageDto>>> GetByFolder(int folderId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _messageService.GetByFolderAsync(folderId, GetUserId(), pageNumber, pageSize);
        return Ok(messages);
    }

    [HttpGet("search")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageDto>>> Search([FromQuery] string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        var messages = await _messageService.SearchAsync(query, GetUserId(), pageNumber, pageSize);
        return Ok(messages);
    }

    [HttpPost("draft")]
    [Permission(SystemPermission.MessagesCreate)]
    public async Task<ActionResult<MessageDto>> CreateDraft([FromBody] CreateMessageDto dto)
    {
        var message = await _messageService.CreateDraftAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
    }

    [HttpPost("send")]
    [Permission(SystemPermission.MessagesSend)]
    public async Task<ActionResult<MessageDto>> Send([FromBody] SendMessageDto dto)
    {
        var message = await _messageService.SendAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
    }

    [HttpPost("{id}/reply")]
    [Permission(SystemPermission.MessagesSend)]
    public async Task<ActionResult<MessageDto>> Reply(int id, [FromBody] ReplyMessageDto dto)
    {
        try
        {
            var message = await _messageService.ReplyAsync(id, dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id}/forward")]
    [Permission(SystemPermission.MessagesSend)]
    public async Task<ActionResult<MessageDto>> Forward(int id, [FromBody] ForwardMessageDto dto)
    {
        try
        {
            var message = await _messageService.ForwardAsync(id, dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = message.Id }, message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMessageDto dto)
    {
        var result = await _messageService.UpdateAsync(id, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Permission(SystemPermission.MessagesDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _messageService.DeleteAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}/permanent")]
    [Permission(SystemPermission.MessagesDelete)]
    public async Task<IActionResult> DeletePermanently(int id)
    {
        var result = await _messageService.DeletePermanentlyAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("delete-multiple")]
    [Permission(SystemPermission.MessagesDelete)]
    public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
    {
        await _messageService.DeleteMultipleAsync(ids, GetUserId());
        return NoContent();
    }

    // ===== Message Actions =====

    [HttpPost("{id}/mark-read")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _messageService.MarkAsReadAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/mark-unread")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> MarkAsUnread(int id)
    {
        var result = await _messageService.MarkAsUnreadAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("mark-multiple-read")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> MarkMultipleAsRead([FromBody] List<int> ids)
    {
        await _messageService.MarkMultipleAsReadAsync(ids, GetUserId());
        return NoContent();
    }

    [HttpPost("{id}/flag")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> Flag(int id)
    {
        var result = await _messageService.FlagAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/unflag")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> Unflag(int id)
    {
        var result = await _messageService.UnflagAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/move")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> MoveToFolder(int id, [FromBody] int folderId)
    {
        var result = await _messageService.MoveToFolderAsync(id, folderId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("move-multiple")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> MoveMultipleToFolder([FromBody] MoveMultipleRequest request)
    {
        await _messageService.MoveMultipleToFolderAsync(request.MessageIds, request.FolderId, GetUserId());
        return NoContent();
    }

    [HttpPost("{id}/archive")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> Archive(int id)
    {
        var result = await _messageService.ArchiveAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("archive-multiple")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> ArchiveMultiple([FromBody] List<int> ids)
    {
        await _messageService.ArchiveMultipleAsync(ids, GetUserId());
        return NoContent();
    }

    // ===== Categories =====

    [HttpGet("categories")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageCategoryDto>>> GetCategories()
    {
        var categories = await _messageService.GetCategoriesAsync(GetUserId());
        return Ok(categories);
    }

    [HttpPost("categories")]
    [Permission(SystemPermission.MessagesCreate)]
    public async Task<ActionResult<MessageCategoryDto>> CreateCategory([FromBody] CreateMessageCategoryDto dto)
    {
        var category = await _messageService.CreateCategoryAsync(dto, GetUserId());
        return Ok(category);
    }

    [HttpPut("categories/{id}")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateMessageCategoryDto dto)
    {
        var result = await _messageService.UpdateCategoryAsync(id, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("categories/{id}")]
    [Permission(SystemPermission.MessagesDelete)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _messageService.DeleteCategoryAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{messageId}/categories/{categoryId}")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> AddCategory(int messageId, int categoryId)
    {
        var result = await _messageService.AddCategoryAsync(messageId, categoryId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{messageId}/categories/{categoryId}")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> RemoveCategory(int messageId, int categoryId)
    {
        var result = await _messageService.RemoveCategoryAsync(messageId, categoryId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Folders =====

    [HttpGet("folders")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageFolderDto>>> GetFolders()
    {
        var folders = await _messageService.GetFoldersAsync(GetUserId());
        return Ok(folders);
    }

    [HttpGet("folders/{id}")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<MessageFolderDto>> GetFolderById(int id)
    {
        try
        {
            var folder = await _messageService.GetFolderByIdAsync(id, GetUserId());
            return Ok(folder);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("folders")]
    [Permission(SystemPermission.MessagesCreate)]
    public async Task<ActionResult<MessageFolderDto>> CreateFolder([FromBody] CreateMessageFolderDto dto)
    {
        var folder = await _messageService.CreateFolderAsync(dto, GetUserId());
        return Ok(folder);
    }

    [HttpPut("folders/{id}")]
    [Permission(SystemPermission.MessagesUpdate)]
    public async Task<IActionResult> UpdateFolder(int id, [FromBody] UpdateMessageFolderDto dto)
    {
        var result = await _messageService.UpdateFolderAsync(id, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("folders/{id}")]
    [Permission(SystemPermission.MessagesDelete)]
    public async Task<IActionResult> DeleteFolder(int id)
    {
        var result = await _messageService.DeleteFolderAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("folders/{folderId}/unread-count")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<int>> GetFolderUnreadCount(int folderId)
    {
        var count = await _messageService.GetFolderUnreadCountAsync(folderId, GetUserId());
        return Ok(count);
    }

    [HttpGet("unread-count")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<int>> GetTotalUnreadCount()
    {
        var count = await _messageService.GetTotalUnreadCountAsync(GetUserId());
        return Ok(count);
    }

    // ===== Attachments =====

    [HttpGet("{messageId}/attachments")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageAttachmentDto>>> GetAttachments(int messageId)
    {
        try
        {
            var attachments = await _messageService.GetAttachmentsAsync(messageId, GetUserId());
            return Ok(attachments);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("attachments/{attachmentId}")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<MessageAttachmentDto>> GetAttachmentById(int attachmentId)
    {
        try
        {
            var attachment = await _messageService.GetAttachmentByIdAsync(attachmentId, GetUserId());
            return Ok(attachment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("attachments/{attachmentId}/download")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        try
        {
            var bytes = await _messageService.DownloadAttachmentAsync(attachmentId, GetUserId());
            var attachment = await _messageService.GetAttachmentByIdAsync(attachmentId, GetUserId());
            return File(bytes, attachment.ContentType, attachment.Name);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ===== Conversation Threads =====

    [HttpGet("threads/{threadId}")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageDto>>> GetConversationThread(int threadId)
    {
        var messages = await _messageService.GetConversationThreadAsync(threadId, GetUserId());
        return Ok(messages);
    }

    [HttpGet("{messageId}/related")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<List<MessageDto>>> GetRelatedMessages(int messageId)
    {
        var messages = await _messageService.GetRelatedMessagesAsync(messageId, GetUserId());
        return Ok(messages);
    }

    // ===== Statistics =====

    [HttpGet("statistics")]
    [Permission(SystemPermission.MessagesRead)]
    public async Task<ActionResult<MessageStatisticsDto>> GetStatistics()
    {
        var statistics = await _messageService.GetStatisticsAsync(GetUserId());
        return Ok(statistics);
    }
}

public record MoveMultipleRequest(List<int> MessageIds, int FolderId);
