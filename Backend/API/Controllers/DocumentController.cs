using Application.Interfaces.DMS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlookInboxManagement.DTOs.DMS;
using System.Security.Claims;

namespace OutlookInboxManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly ILogger<DocumentController> _logger;

    public DocumentController(IDocumentService documentService, ILogger<DocumentController> logger)
    {
        _documentService = documentService;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    #region Document Management

    /// <summary>
    /// Get document by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentDto>> GetDocument(int id)
    {
        try
        {
            var userId = GetUserId();
            var canAccess = await _documentService.CanAccessDocumentAsync(id, userId);

            if (!canAccess)
                return Forbid();

            var document = await _documentService.GetDocumentByIdAsync(id);
            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the document" });
        }
    }

    /// <summary>
    /// Search documents with filters
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<DocumentSearchResultDto>> SearchDocuments([FromBody] DocumentSearchDto searchDto)
    {
        try
        {
            var result = await _documentService.SearchDocumentsAsync(searchDto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching documents");
            return StatusCode(500, new { message = "An error occurred while searching documents" });
        }
    }

    /// <summary>
    /// Upload a new document
    /// </summary>
    [HttpPost]
    [RequestSizeLimit(104857600)] // 100 MB limit
    public async Task<ActionResult<DocumentDto>> CreateDocument([FromForm] CreateDocumentDto createDto, IFormFile file)
    {
        try
        {
            var userId = GetUserId();
            var document = await _documentService.CreateDocumentAsync(createDto, file, userId);
            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            return StatusCode(500, new { message = "An error occurred while creating the document" });
        }
    }

    /// <summary>
    /// Update document metadata
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<DocumentDto>> UpdateDocument(int id, [FromBody] UpdateDocumentDto updateDto)
    {
        try
        {
            var userId = GetUserId();
            var canEdit = await _documentService.CanEditDocumentAsync(id, userId);

            if (!canEdit)
                return Forbid();

            var document = await _documentService.UpdateDocumentAsync(id, updateDto, userId);
            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the document" });
        }
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        try
        {
            var userId = GetUserId();
            var canEdit = await _documentService.CanEditDocumentAsync(id, userId);

            if (!canEdit)
                return Forbid();

            await _documentService.DeleteDocumentAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the document" });
        }
    }

    /// <summary>
    /// Download document file
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadDocument(int id, [FromQuery] int? versionId = null)
    {
        try
        {
            var userId = GetUserId();
            var canAccess = await _documentService.CanAccessDocumentAsync(id, userId);

            if (!canAccess)
                return Forbid();

            var document = await _documentService.GetDocumentByIdAsync(id);
            var fileBytes = await _documentService.DownloadDocumentAsync(id, userId, versionId);

            return File(fileBytes, document.MimeType, document.OriginalFileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while downloading the document" });
        }
    }

    #endregion

    #region Versioning

    /// <summary>
    /// Get document version history
    /// </summary>
    [HttpGet("{id}/versions")]
    public async Task<ActionResult<List<DocumentVersionDto>>> GetVersionHistory(int id)
    {
        try
        {
            var userId = GetUserId();
            var canAccess = await _documentService.CanAccessDocumentAsync(id, userId);

            if (!canAccess)
                return Forbid();

            var versions = await _documentService.GetVersionHistoryAsync(id);
            return Ok(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version history for document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving version history" });
        }
    }

    /// <summary>
    /// Create a new version of the document
    /// </summary>
    [HttpPost("{id}/versions")]
    [RequestSizeLimit(104857600)] // 100 MB limit
    public async Task<ActionResult<DocumentVersionDto>> CreateVersion(int id, [FromForm] CreateVersionDto versionDto, IFormFile file)
    {
        try
        {
            var userId = GetUserId();
            var canEdit = await _documentService.CanEditDocumentAsync(id, userId);

            if (!canEdit)
                return Forbid();

            var version = await _documentService.CreateNewVersionAsync(id, versionDto, file, userId);
            return Ok(version);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating version for document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while creating the version" });
        }
    }

    /// <summary>
    /// Restore a previous version
    /// </summary>
    [HttpPost("{id}/versions/{versionId}/restore")]
    public async Task<ActionResult<DocumentDto>> RestoreVersion(int id, int versionId)
    {
        try
        {
            var userId = GetUserId();
            var canEdit = await _documentService.CanEditDocumentAsync(id, userId);

            if (!canEdit)
                return Forbid();

            var document = await _documentService.RestoreVersionAsync(id, versionId, userId);
            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring version {VersionId} for document {DocumentId}", versionId, id);
            return StatusCode(500, new { message = "An error occurred while restoring the version" });
        }
    }

    #endregion

    #region Locking

    /// <summary>
    /// Lock document for editing
    /// </summary>
    [HttpPost("{id}/lock")]
    public async Task<ActionResult<DocumentDto>> LockDocument(int id)
    {
        try
        {
            var userId = GetUserId();
            var document = await _documentService.LockDocumentAsync(id, userId);
            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error locking document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while locking the document" });
        }
    }

    /// <summary>
    /// Unlock document
    /// </summary>
    [HttpPost("{id}/unlock")]
    public async Task<ActionResult<DocumentDto>> UnlockDocument(int id)
    {
        try
        {
            var userId = GetUserId();
            var document = await _documentService.UnlockDocumentAsync(id, userId);
            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while unlocking the document" });
        }
    }

    #endregion

    #region Folders

    /// <summary>
    /// Get folder by ID
    /// </summary>
    [HttpGet("folders/{id}")]
    public async Task<ActionResult<DocumentFolderDto>> GetFolder(int id)
    {
        try
        {
            var folder = await _documentService.GetFolderByIdAsync(id);
            return Ok(folder);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder {FolderId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the folder" });
        }
    }

    /// <summary>
    /// Get root folders
    /// </summary>
    [HttpGet("folders")]
    public async Task<ActionResult<List<DocumentFolderDto>>> GetRootFolders()
    {
        try
        {
            var userId = GetUserId();
            var folders = await _documentService.GetRootFoldersAsync(userId);
            return Ok(folders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting root folders");
            return StatusCode(500, new { message = "An error occurred while retrieving folders" });
        }
    }

    /// <summary>
    /// Create a new folder
    /// </summary>
    [HttpPost("folders")]
    public async Task<ActionResult<DocumentFolderDto>> CreateFolder([FromBody] CreateFolderDto createDto)
    {
        try
        {
            var userId = GetUserId();
            var folder = await _documentService.CreateFolderAsync(createDto, userId);
            return CreatedAtAction(nameof(GetFolder), new { id = folder.Id }, folder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder");
            return StatusCode(500, new { message = "An error occurred while creating the folder" });
        }
    }

    /// <summary>
    /// Update folder
    /// </summary>
    [HttpPut("folders/{id}")]
    public async Task<ActionResult<DocumentFolderDto>> UpdateFolder(int id, [FromBody] CreateFolderDto updateDto)
    {
        try
        {
            var folder = await _documentService.UpdateFolderAsync(id, updateDto);
            return Ok(folder);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating folder {FolderId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the folder" });
        }
    }

    /// <summary>
    /// Delete folder
    /// </summary>
    [HttpDelete("folders/{id}")]
    public async Task<IActionResult> DeleteFolder(int id)
    {
        try
        {
            var userId = GetUserId();
            await _documentService.DeleteFolderAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting folder {FolderId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the folder" });
        }
    }

    /// <summary>
    /// Move document to folder
    /// </summary>
    [HttpPost("{id}/move")]
    public async Task<ActionResult<DocumentDto>> MoveDocument(int id, [FromBody] int? targetFolderId)
    {
        try
        {
            var userId = GetUserId();
            var canEdit = await _documentService.CanEditDocumentAsync(id, userId);

            if (!canEdit)
                return Forbid();

            var document = await _documentService.MoveDocumentAsync(id, targetFolderId, userId);
            return Ok(document);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while moving the document" });
        }
    }

    #endregion

    #region Annotations

    /// <summary>
    /// Get document annotations
    /// </summary>
    [HttpGet("{id}/annotations")]
    public async Task<ActionResult<List<DocumentAnnotationDto>>> GetAnnotations(int id, [FromQuery] int? versionId = null)
    {
        try
        {
            var userId = GetUserId();
            var canAccess = await _documentService.CanAccessDocumentAsync(id, userId);

            if (!canAccess)
                return Forbid();

            var annotations = await _documentService.GetAnnotationsAsync(id, versionId);
            return Ok(annotations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting annotations for document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving annotations" });
        }
    }

    /// <summary>
    /// Create annotation
    /// </summary>
    [HttpPost("{id}/annotations")]
    public async Task<ActionResult<DocumentAnnotationDto>> CreateAnnotation(int id, [FromBody] CreateAnnotationDto createDto)
    {
        try
        {
            var userId = GetUserId();
            var canAccess = await _documentService.CanAccessDocumentAsync(id, userId);

            if (!canAccess)
                return Forbid();

            var annotation = await _documentService.CreateAnnotationAsync(id, createDto, userId);
            return Ok(annotation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating annotation for document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while creating the annotation" });
        }
    }

    /// <summary>
    /// Update annotation
    /// </summary>
    [HttpPut("annotations/{annotationId}")]
    public async Task<ActionResult<DocumentAnnotationDto>> UpdateAnnotation(int annotationId, [FromBody] CreateAnnotationDto updateDto)
    {
        try
        {
            var annotation = await _documentService.UpdateAnnotationAsync(annotationId, updateDto);
            return Ok(annotation);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating annotation {AnnotationId}", annotationId);
            return StatusCode(500, new { message = "An error occurred while updating the annotation" });
        }
    }

    /// <summary>
    /// Delete annotation
    /// </summary>
    [HttpDelete("annotations/{annotationId}")]
    public async Task<IActionResult> DeleteAnnotation(int annotationId)
    {
        try
        {
            var userId = GetUserId();
            await _documentService.DeleteAnnotationAsync(annotationId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting annotation {AnnotationId}", annotationId);
            return StatusCode(500, new { message = "An error occurred while deleting the annotation" });
        }
    }

    #endregion

    #region Activity Log

    /// <summary>
    /// Get document activity history
    /// </summary>
    [HttpGet("{id}/activities")]
    public async Task<ActionResult<List<DocumentActivityDto>>> GetActivities(int id, [FromQuery] int limit = 50)
    {
        try
        {
            var userId = GetUserId();
            var canAccess = await _documentService.CanAccessDocumentAsync(id, userId);

            if (!canAccess)
                return Forbid();

            var activities = await _documentService.GetDocumentActivitiesAsync(id, limit);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting activities for document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving activities" });
        }
    }

    #endregion

    #region Metadata

    /// <summary>
    /// Get document metadata
    /// </summary>
    [HttpGet("{id}/metadata")]
    public async Task<ActionResult<Dictionary<string, string>>> GetMetadata(int id)
    {
        try
        {
            var userId = GetUserId();
            var canAccess = await _documentService.CanAccessDocumentAsync(id, userId);

            if (!canAccess)
                return Forbid();

            var metadata = await _documentService.GetMetadataAsync(id);
            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting metadata for document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving metadata" });
        }
    }

    /// <summary>
    /// Update document metadata
    /// </summary>
    [HttpPut("{id}/metadata")]
    public async Task<IActionResult> UpdateMetadata(int id, [FromBody] Dictionary<string, string> metadata)
    {
        try
        {
            var userId = GetUserId();
            var canEdit = await _documentService.CanEditDocumentAsync(id, userId);

            if (!canEdit)
                return Forbid();

            await _documentService.UpdateMetadataAsync(id, metadata, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating metadata for document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while updating metadata" });
        }
    }

    #endregion

    #region Bulk Operations

    /// <summary>
    /// Bulk upload documents
    /// </summary>
    [HttpPost("bulk-upload")]
    [RequestSizeLimit(524288000)] // 500 MB limit for bulk
    public async Task<ActionResult<List<DocumentDto>>> BulkUpload([FromForm] List<IFormFile> files, [FromForm] int? folderId = null)
    {
        try
        {
            var userId = GetUserId();
            var documents = await _documentService.BulkUploadAsync(files, folderId, userId);
            return Ok(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk upload");
            return StatusCode(500, new { message = "An error occurred during bulk upload" });
        }
    }

    /// <summary>
    /// Export multiple documents as ZIP
    /// </summary>
    [HttpPost("export-zip")]
    public async Task<IActionResult> ExportAsZip([FromBody] List<int> documentIds)
    {
        try
        {
            var userId = GetUserId();
            var zipBytes = await _documentService.ExportDocumentsAsZipAsync(documentIds, userId);
            return File(zipBytes, "application/zip", $"documents-{DateTime.UtcNow:yyyyMMdd}.zip");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting documents as ZIP");
            return StatusCode(500, new { message = "An error occurred while exporting documents" });
        }
    }

    #endregion

    #region Permissions

    /// <summary>
    /// Share document with users/roles
    /// </summary>
    [HttpPost("{id}/share")]
    public async Task<IActionResult> ShareDocument(int id, [FromBody] ShareDocumentRequest request)
    {
        try
        {
            var userId = GetUserId();
            await _documentService.ShareDocumentAsync(id, request.UserIds, request.RoleNames, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sharing document {DocumentId}", id);
            return StatusCode(500, new { message = "An error occurred while sharing the document" });
        }
    }

    #endregion
}

public class ShareDocumentRequest
{
    public List<string> UserIds { get; set; } = new();
    public List<string> RoleNames { get; set; } = new();
}
