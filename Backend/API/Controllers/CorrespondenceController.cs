using Application.DTOs.Archive;
using Application.Interfaces.Archive;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CorrespondenceController : ControllerBase
{
    private readonly ICorrespondenceService _correspondenceService;

    public CorrespondenceController(ICorrespondenceService correspondenceService)
    {
        _correspondenceService = correspondenceService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ==================== CRUD Operations ====================

    /// <summary>
    /// Get correspondence by ID
    /// </summary>
    [HttpGet("{id}")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<CorrespondenceDto>> GetById(int id)
    {
        try
        {
            var correspondence = await _correspondenceService.GetByIdAsync(id, GetUserId());
            if (correspondence == null)
                return NotFound($"Correspondence with ID {id} not found");

            return Ok(correspondence);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get correspondence by reference number
    /// </summary>
    [HttpGet("reference/{referenceNumber}")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<CorrespondenceDto>> GetByReferenceNumber(string referenceNumber)
    {
        var correspondence = await _correspondenceService.GetByReferenceNumberAsync(referenceNumber, GetUserId());
        if (correspondence == null)
            return NotFound($"Correspondence with reference number '{referenceNumber}' not found");

        return Ok(correspondence);
    }

    /// <summary>
    /// Get all correspondences with pagination
    /// </summary>
    [HttpGet]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var correspondences = await _correspondenceService.GetAllAsync(GetUserId(), pageNumber, pageSize);
        return Ok(correspondences);
    }

    /// <summary>
    /// Create new correspondence
    /// </summary>
    [HttpPost]
    [Permission(SystemPermission.CorrespondenceCreate)]
    public async Task<ActionResult<CorrespondenceDto>> Create([FromBody] CreateCorrespondenceRequest request)
    {
        try
        {
            var correspondence = await _correspondenceService.CreateAsync(request, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = correspondence.Id }, correspondence);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update correspondence
    /// </summary>
    [HttpPut("{id}")]
    [Permission(SystemPermission.CorrespondenceUpdate)]
    public async Task<ActionResult<CorrespondenceDto>> Update(int id, [FromBody] UpdateCorrespondenceRequest request)
    {
        var correspondence = await _correspondenceService.UpdateAsync(id, request, GetUserId());
        if (correspondence == null)
            return NotFound($"Correspondence with ID {id} not found");

        return Ok(correspondence);
    }

    /// <summary>
    /// Delete correspondence (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [Permission(SystemPermission.CorrespondenceDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _correspondenceService.DeleteAsync(id, GetUserId());
        if (!result)
            return NotFound($"Correspondence with ID {id} not found");

        return NoContent();
    }

    // ==================== Search & Filter ====================

    /// <summary>
    /// Advanced search with multiple filters
    /// </summary>
    [HttpPost("search")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<object>> Search([FromBody] CorrespondenceSearchRequest request)
    {
        var (items, totalCount) = await _correspondenceService.SearchAsync(request, GetUserId());
        return Ok(new
        {
            items,
            totalCount,
            pageNumber = request.PageNumber,
            pageSize = request.PageSize,
            totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        });
    }

    /// <summary>
    /// Get correspondences by category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetByCategory(int categoryId)
    {
        var correspondences = await _correspondenceService.GetByCategoryAsync(categoryId, GetUserId());
        return Ok(correspondences);
    }

    /// <summary>
    /// Get correspondences by classification
    /// </summary>
    [HttpGet("classification/{classification}")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetByClassification(string classification)
    {
        var correspondences = await _correspondenceService.GetByClassificationAsync(classification, GetUserId());
        return Ok(correspondences);
    }

    /// <summary>
    /// Get correspondences by status
    /// </summary>
    [HttpGet("status/{status}")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetByStatus(string status)
    {
        var correspondences = await _correspondenceService.GetByStatusAsync(status, GetUserId());
        return Ok(correspondences);
    }

    /// <summary>
    /// Update correspondence status
    /// </summary>
    [HttpPatch("{id}/status")]
    [Permission(SystemPermission.CorrespondenceUpdate)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var result = await _correspondenceService.UpdateStatusAsync(id, request.Status, GetUserId());
        if (!result)
            return NotFound($"Correspondence with ID {id} not found");

        return NoContent();
    }

    // ==================== Employee/Department Operations ====================

    /// <summary>
    /// Get correspondences sent by an employee
    /// </summary>
    [HttpGet("from-employee/{employeeId}")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetByFromEmployee(int employeeId)
    {
        var correspondences = await _correspondenceService.GetByFromEmployeeAsync(employeeId, GetUserId());
        return Ok(correspondences);
    }

    /// <summary>
    /// Get correspondences sent to an employee
    /// </summary>
    [HttpGet("to-employee/{employeeId}")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetByToEmployee(int employeeId)
    {
        var correspondences = await _correspondenceService.GetByToEmployeeAsync(employeeId, GetUserId());
        return Ok(correspondences);
    }

    /// <summary>
    /// Get correspondences by department
    /// </summary>
    [HttpGet("department/{departmentId}")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetByDepartment(int departmentId)
    {
        var correspondences = await _correspondenceService.GetByDepartmentAsync(departmentId, GetUserId());
        return Ok(correspondences);
    }

    // ==================== My Correspondence (Inbox/Outbox) ====================

    /// <summary>
    /// Get my inbox (correspondences addressed to me)
    /// </summary>
    [HttpGet("my-inbox")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetMyInbox(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var correspondences = await _correspondenceService.GetMyInboxAsync(GetUserId(), pageNumber, pageSize);
        return Ok(correspondences);
    }

    /// <summary>
    /// Get my outbox (correspondences sent by me)
    /// </summary>
    [HttpGet("my-outbox")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetMyOutbox(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var correspondences = await _correspondenceService.GetMyOutboxAsync(GetUserId(), pageNumber, pageSize);
        return Ok(correspondences);
    }

    /// <summary>
    /// Get my draft correspondences
    /// </summary>
    [HttpGet("my-drafts")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetMyDrafts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var correspondences = await _correspondenceService.GetMyDraftsAsync(GetUserId(), pageNumber, pageSize);
        return Ok(correspondences);
    }

    /// <summary>
    /// Get my pending actions (routings assigned to me)
    /// </summary>
    [HttpGet("my-pending-actions")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceRoutingDto>>> GetMyPendingActions()
    {
        var routings = await _correspondenceService.GetMyPendingActionsAsync(GetUserId());
        return Ok(routings);
    }

    // ==================== Attachment Operations ====================

    /// <summary>
    /// Upload attachment to correspondence
    /// </summary>
    [HttpPost("{correspondenceId}/attachments")]
    [Permission(SystemPermission.CorrespondenceUpdate)]
    public async Task<ActionResult<CorrespondenceAttachmentDto>> UploadAttachment(
        int correspondenceId,
        [FromForm] UploadAttachmentRequest request,
        IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("File is required");

        try
        {
            using var stream = file.OpenReadStream();
            var attachment = await _correspondenceService.AddAttachmentAsync(
                correspondenceId,
                request,
                stream,
                file.FileName,
                file.ContentType,
                GetUserId()
            );

            return CreatedAtAction(nameof(GetAttachments), new { correspondenceId }, attachment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get attachments of a correspondence
    /// </summary>
    [HttpGet("{correspondenceId}/attachments")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceAttachmentDto>>> GetAttachments(int correspondenceId)
    {
        var attachments = await _correspondenceService.GetAttachmentsAsync(correspondenceId, GetUserId());
        return Ok(attachments);
    }

    /// <summary>
    /// Delete attachment
    /// </summary>
    [HttpDelete("attachments/{attachmentId}")]
    [Permission(SystemPermission.CorrespondenceUpdate)]
    public async Task<IActionResult> DeleteAttachment(int attachmentId)
    {
        var result = await _correspondenceService.DeleteAttachmentAsync(attachmentId, GetUserId());
        if (!result)
            return NotFound($"Attachment with ID {attachmentId} not found");

        return NoContent();
    }

    // ==================== Routing Operations ====================

    /// <summary>
    /// Route correspondence to another employee (إحالة)
    /// </summary>
    [HttpPost("{correspondenceId}/route")]
    [Permission(SystemPermission.CorrespondenceRoute)]
    public async Task<ActionResult<CorrespondenceRoutingDto>> RouteCorrespondence(
        int correspondenceId,
        [FromBody] RouteCorrespondenceRequest request)
    {
        try
        {
            request.CorrespondenceId = correspondenceId;
            var routing = await _correspondenceService.RouteCorrespondenceAsync(request, GetUserId());
            return CreatedAtAction(nameof(GetRoutingHistory), new { correspondenceId }, routing);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get routing history of a correspondence
    /// </summary>
    [HttpGet("{correspondenceId}/routing-history")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceRoutingDto>>> GetRoutingHistory(int correspondenceId)
    {
        var routings = await _correspondenceService.GetRoutingHistoryAsync(correspondenceId, GetUserId());
        return Ok(routings);
    }

    /// <summary>
    /// Get routing chain of a correspondence
    /// </summary>
    [HttpGet("{correspondenceId}/routing-chain")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<RoutingChainDto>> GetRoutingChain(int correspondenceId)
    {
        try
        {
            var chain = await _correspondenceService.GetRoutingChainAsync(correspondenceId, GetUserId());
            return Ok(chain);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Respond to a routing
    /// </summary>
    [HttpPost("routing/{routingId}/respond")]
    [Permission(SystemPermission.CorrespondenceUpdate)]
    public async Task<IActionResult> RespondToRouting(int routingId, [FromBody] RespondToRoutingRequest request)
    {
        request.RoutingId = routingId;
        var result = await _correspondenceService.RespondToRoutingAsync(request, GetUserId());
        if (!result)
            return NotFound($"Routing with ID {routingId} not found");

        return NoContent();
    }

    // ==================== Archive Operations ====================

    /// <summary>
    /// Archive a correspondence
    /// </summary>
    [HttpPost("{correspondenceId}/archive")]
    [Permission(SystemPermission.CorrespondenceArchive)]
    public async Task<IActionResult> ArchiveCorrespondence(
        int correspondenceId,
        [FromBody] ArchiveCorrespondenceRequest request)
    {
        request.CorrespondenceId = correspondenceId;
        var result = await _correspondenceService.ArchiveCorrespondenceAsync(request, GetUserId());
        if (!result)
            return NotFound($"Correspondence with ID {correspondenceId} not found");

        return NoContent();
    }

    /// <summary>
    /// Get archived correspondences
    /// </summary>
    [HttpGet("archived")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetArchivedCorrespondences()
    {
        var correspondences = await _correspondenceService.GetArchivedCorrespondencesAsync(GetUserId());
        return Ok(correspondences);
    }

    // ==================== Related Correspondence ====================

    /// <summary>
    /// Get related correspondences (replies, follow-ups)
    /// </summary>
    [HttpGet("{correspondenceId}/related")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<List<CorrespondenceSummaryDto>>> GetRelatedCorrespondences(int correspondenceId)
    {
        var correspondences = await _correspondenceService.GetRelatedCorrespondencesAsync(correspondenceId, GetUserId());
        return Ok(correspondences);
    }

    // ==================== Statistics ====================

    /// <summary>
    /// Get correspondence statistics by status
    /// </summary>
    [HttpGet("stats/by-status")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStatsByStatus()
    {
        var stats = await _correspondenceService.GetCorrespondenceStatsByStatusAsync(GetUserId());
        return Ok(stats);
    }

    /// <summary>
    /// Get correspondence statistics by classification
    /// </summary>
    [HttpGet("stats/by-classification")]
    [Permission(SystemPermission.CorrespondenceRead)]
    public async Task<ActionResult<Dictionary<string, int>>> GetStatsByClassification()
    {
        var stats = await _correspondenceService.GetCorrespondenceStatsByClassificationAsync(GetUserId());
        return Ok(stats);
    }

    // ==================== Reference Number Generation ====================

    /// <summary>
    /// Generate reference number for a category
    /// </summary>
    [HttpGet("generate-reference/{categoryId}")]
    [Permission(SystemPermission.CorrespondenceCreate)]
    public async Task<ActionResult<string>> GenerateReferenceNumber(int categoryId)
    {
        try
        {
            var referenceNumber = await _correspondenceService.GenerateReferenceNumberAsync(categoryId);
            return Ok(new { referenceNumber });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}

/// <summary>
/// Update status request DTO
/// </summary>
public class UpdateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
