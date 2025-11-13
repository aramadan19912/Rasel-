using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlookInboxManagement.DTOs;
using OutlookInboxManagement.Models;
using OutlookInboxManagement.Services;
using System.Security.Claims;

namespace OutlookInboxManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InboxController : ControllerBase
{
    private readonly IInboxService _inboxService;

    public InboxController(IInboxService inboxService)
    {
        _inboxService = inboxService;
    }

    private string GetUserId() => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    #region Basic Message Operations

    [HttpGet]
    public async Task<ActionResult<PaginatedList<MessageDto>>> GetInbox([FromQuery] InboxQueryParameters parameters)
    {
        try
        {
            var messages = await _inboxService.GetInboxAsync(parameters);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MessageDto>> GetMessage(int id)
    {
        try
        {
            var message = await _inboxService.GetMessageAsync(id);
            return Ok(message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Message not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateDraft([FromBody] CreateMessageDto dto)
    {
        try
        {
            var draft = await _inboxService.CreateDraftAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetMessage), new { id = draft.Id }, draft);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/send")]
    public async Task<ActionResult<MessageDto>> SendMessage(int id)
    {
        try
        {
            var message = await _inboxService.SendMessageAsync(id);
            return Ok(message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Draft not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id, [FromQuery] bool permanent = false)
    {
        try
        {
            await _inboxService.DeleteMessageAsync(id, permanent);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Message not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/move")]
    public async Task<IActionResult> MoveToFolder(int id, [FromBody] MoveToFolderDto dto)
    {
        try
        {
            await _inboxService.MoveToFolderAsync(id, dto.FolderId);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Message not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Read/Unread Operations

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        try
        {
            await _inboxService.MarkAsReadAsync(id);
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Message not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/unread")]
    public async Task<IActionResult> MarkAsUnread(int id)
    {
        try
        {
            await _inboxService.MarkAsUnreadAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("bulk/read")]
    public async Task<IActionResult> BulkMarkAsRead([FromBody] BulkActionDto dto)
    {
        try
        {
            await _inboxService.BulkMarkAsReadAsync(dto.MessageIds);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Reply/Forward Operations

    [HttpPost("{id}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] ReplyDto dto)
    {
        try
        {
            await _inboxService.ReplyAsync(id, dto, GetUserId());
            return Ok();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Message not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/reply-all")]
    public async Task<IActionResult> ReplyAll(int id, [FromBody] ReplyDto dto)
    {
        try
        {
            await _inboxService.ReplyAllAsync(id, dto, GetUserId());
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/forward")]
    public async Task<IActionResult> Forward(int id, [FromBody] ForwardDto dto)
    {
        try
        {
            await _inboxService.ForwardAsync(id, dto.Recipients, GetUserId());
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/quick-reply")]
    public async Task<IActionResult> QuickReply(int id, [FromBody] QuickReplyDto dto)
    {
        try
        {
            await _inboxService.QuickReplyAsync(id, dto.Text, GetUserId());
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Folder Operations

    [HttpGet("folders")]
    public async Task<ActionResult<List<MessageFolderDto>>> GetFolders()
    {
        try
        {
            var folders = await _inboxService.GetFoldersAsync(GetUserId());
            return Ok(folders);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("folders")]
    public async Task<ActionResult<MessageFolderDto>> CreateFolder([FromBody] CreateFolderDto dto)
    {
        try
        {
            var folder = await _inboxService.CreateFolderAsync(dto, GetUserId());
            return Ok(folder);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPut("folders/{id}")]
    public async Task<ActionResult<MessageFolderDto>> UpdateFolder(int id, [FromBody] UpdateFolderDto dto)
    {
        try
        {
            var folder = await _inboxService.UpdateFolderAsync(id, dto);
            return Ok(folder);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Folder not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("folders/{id}")]
    public async Task<IActionResult> DeleteFolder(int id)
    {
        try
        {
            await _inboxService.DeleteFolderAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Folder not found" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("folders/{id}/unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(int id)
    {
        try
        {
            var count = await _inboxService.GetUnreadCountAsync(id);
            return Ok(count);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Category Operations

    [HttpGet("categories")]
    public async Task<ActionResult<List<MessageCategoryDto>>> GetCategories()
    {
        try
        {
            var categories = await _inboxService.GetCategoriesAsync(GetUserId());
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("categories")]
    public async Task<ActionResult<MessageCategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var category = await _inboxService.CreateCategoryAsync(dto, GetUserId());
            return Ok(category);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/categories/{categoryId}")]
    public async Task<IActionResult> AssignCategory(int id, int categoryId)
    {
        try
        {
            await _inboxService.AssignCategoryAsync(id, categoryId);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}/categories/{categoryId}")]
    public async Task<IActionResult> RemoveCategory(int id, int categoryId)
    {
        try
        {
            await _inboxService.RemoveCategoryAsync(id, categoryId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Flag Operations

    [HttpPost("{id}/flag")]
    public async Task<IActionResult> FlagMessage(int id, [FromBody] FlagMessageDto dto)
    {
        try
        {
            await _inboxService.FlagMessageAsync(id, dto.DueDate);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/unflag")]
    public async Task<IActionResult> UnflagMessage(int id)
    {
        try
        {
            await _inboxService.UnflagMessageAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/flag/complete")]
    public async Task<IActionResult> MarkFlagComplete(int id)
    {
        try
        {
            await _inboxService.MarkFlagCompleteAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/reminder")]
    public async Task<IActionResult> SetReminder(int id, [FromBody] DateTime reminderDate)
    {
        try
        {
            await _inboxService.SetReminderAsync(id, reminderDate);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Search Operations

    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<MessageDto>>> Search([FromQuery] SearchParameters parameters)
    {
        try
        {
            var results = await _inboxService.SearchAsync(parameters);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("search/content")]
    public async Task<ActionResult<List<MessageDto>>> SearchByContent([FromQuery] string query)
    {
        try
        {
            var results = await _inboxService.SearchByContentAsync(query);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("search/sender")]
    public async Task<ActionResult<List<MessageDto>>> SearchBySender([FromQuery] string email)
    {
        try
        {
            var results = await _inboxService.SearchBySenderAsync(email);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Conversation Operations

    [HttpGet("conversations/{conversationId}")]
    public async Task<ActionResult<ConversationThreadDto>> GetConversation(string conversationId)
    {
        try
        {
            var thread = await _inboxService.GetConversationAsync(conversationId);
            return Ok(thread);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = "Conversation not found" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}/related")]
    public async Task<ActionResult<List<MessageDto>>> GetRelatedMessages(int id)
    {
        try
        {
            var messages = await _inboxService.GetRelatedMessagesAsync(id);
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Attachment Operations

    [HttpPost("{id}/attachments")]
    public async Task<ActionResult<AttachmentDto>> AddAttachment(int id, IFormFile file)
    {
        try
        {
            var attachment = await _inboxService.AddAttachmentAsync(id, file);
            return Ok(attachment);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("attachments/{attachmentId}")]
    public async Task<IActionResult> RemoveAttachment(int attachmentId)
    {
        try
        {
            await _inboxService.RemoveAttachmentAsync(attachmentId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("attachments/{attachmentId}/download")]
    public async Task<IActionResult> DownloadAttachment(int attachmentId)
    {
        try
        {
            var data = await _inboxService.DownloadAttachmentAsync(attachmentId);
            return File(data, "application/octet-stream");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Rule Operations

    [HttpGet("rules")]
    public async Task<ActionResult<List<MessageRuleDto>>> GetRules()
    {
        try
        {
            var rules = await _inboxService.GetRulesAsync(GetUserId());
            return Ok(rules);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("rules")]
    public async Task<ActionResult<MessageRuleDto>> CreateRule([FromBody] CreateRuleDto dto)
    {
        try
        {
            dto.UserId = GetUserId();
            var rule = await _inboxService.CreateRuleAsync(dto);
            return Ok(rule);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/apply-rules")]
    public async Task<IActionResult> ApplyRules(int id)
    {
        try
        {
            await _inboxService.ApplyRulesAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Reaction Operations

    [HttpPost("{id}/reactions")]
    public async Task<IActionResult> AddReaction(int id, [FromBody] AddReactionDto dto)
    {
        try
        {
            await _inboxService.AddReactionAsync(id, dto.ReactionType, GetUserId());
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}/reactions")]
    public async Task<IActionResult> RemoveReaction(int id)
    {
        try
        {
            await _inboxService.RemoveReactionAsync(id, GetUserId());
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}/reactions")]
    public async Task<ActionResult<List<MessageReactionDto>>> GetReactions(int id)
    {
        try
        {
            var reactions = await _inboxService.GetReactionsAsync(id);
            return Ok(reactions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Bulk Operations

    [HttpPost("bulk/delete")]
    public async Task<IActionResult> BulkDelete([FromBody] BulkActionDto dto)
    {
        try
        {
            await _inboxService.BulkDeleteAsync(dto.MessageIds);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("bulk/move")]
    public async Task<IActionResult> BulkMove([FromBody] BulkMoveDto dto)
    {
        try
        {
            await _inboxService.BulkMoveAsync(dto.MessageIds, dto.FolderId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("bulk/categorize")]
    public async Task<IActionResult> BulkCategorize([FromBody] BulkMoveDto dto)
    {
        try
        {
            await _inboxService.BulkCategorizeAsync(dto.MessageIds, dto.FolderId);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Archive/Junk Operations

    [HttpPost("{id}/archive")]
    public async Task<IActionResult> ArchiveMessage(int id)
    {
        try
        {
            await _inboxService.ArchiveMessageAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/unarchive")]
    public async Task<IActionResult> UnarchiveMessage(int id)
    {
        try
        {
            await _inboxService.UnarchiveMessageAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/junk")]
    public async Task<IActionResult> MarkAsJunk(int id)
    {
        try
        {
            await _inboxService.MarkAsJunkAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("{id}/not-junk")]
    public async Task<IActionResult> MarkAsNotJunk(int id)
    {
        try
        {
            await _inboxService.MarkAsNotJunkAsync(id);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Statistics & Export

    [HttpGet("statistics")]
    public async Task<ActionResult<InboxStatisticsDto>> GetStatistics()
    {
        try
        {
            var stats = await _inboxService.GetStatisticsAsync(GetUserId());
            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}/export/eml")]
    public async Task<IActionResult> ExportToEml(int id)
    {
        try
        {
            var data = await _inboxService.ExportToEmlAsync(id);
            return File(data, "message/rfc822", $"message-{id}.eml");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("{id}/export/pdf")]
    public async Task<IActionResult> ExportToPdf(int id)
    {
        try
        {
            var data = await _inboxService.ExportToPdfAsync(id);
            return File(data, "application/pdf", $"message-{id}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Maintenance Operations

    [HttpPost("cleanup/old-messages")]
    public async Task<IActionResult> CleanupOldMessages([FromQuery] int olderThanDays = 90)
    {
        try
        {
            await _inboxService.CleanupOldMessagesAsync(olderThanDays);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("cleanup/deleted-items")]
    public async Task<IActionResult> EmptyDeletedItems()
    {
        try
        {
            await _inboxService.EmptyDeletedItemsAsync(GetUserId());
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpPost("cleanup/junk")]
    public async Task<IActionResult> EmptyJunkFolder()
    {
        try
        {
            await _inboxService.EmptyJunkFolderAsync(GetUserId());
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Mentions

    [HttpGet("mentions")]
    public async Task<ActionResult<List<MessageDto>>> GetMentions()
    {
        try
        {
            var mentions = await _inboxService.GetMentionsAsync(GetUserId());
            return Ok(mentions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion

    #region Tracking

    [HttpGet("{id}/tracking")]
    public async Task<ActionResult<List<MessageTrackingDto>>> GetTrackingInfo(int id)
    {
        try
        {
            var tracking = await _inboxService.GetTrackingInfoAsync(id);
            return Ok(tracking);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    #endregion
}
