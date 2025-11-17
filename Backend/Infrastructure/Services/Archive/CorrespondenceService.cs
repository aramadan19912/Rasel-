using Application.DTOs.Archive;
using Application.Interfaces.Archive;
using Application.Interfaces.DMS;
using Backend.Infrastructure.Data;
using Domain.Entities.Archive;
using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.DTOs.DMS;
using OutlookInboxManagement.Domain.Entities.DMS;

namespace Infrastructure.Services.Archive;

public class CorrespondenceService : ICorrespondenceService
{
    private readonly ApplicationDbContext _context;
    private readonly IDocumentService _documentService;

    public CorrespondenceService(ApplicationDbContext context, IDocumentService documentService)
    {
        _context = context;
        _documentService = documentService;
    }

    // ==================== CRUD Operations ====================

    public async Task<CorrespondenceDto?> GetByIdAsync(int id, string userId)
    {
        var correspondence = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee).ThenInclude(e => e!.Position)
            .Include(c => c.ToEmployee).ThenInclude(e => e!.Position)
            .Include(c => c.ToDepartment)
            .Include(c => c.Form)
            .Include(c => c.FormSubmission)
            .Include(c => c.RelatedCorrespondence)
            .Include(c => c.ArchivedDocument)
            .Include(c => c.Attachments)
            .Include(c => c.Routings).ThenInclude(r => r.FromEmployee)
            .Include(c => c.Routings).ThenInclude(r => r.ToEmployee)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        return correspondence == null ? null : MapToDto(correspondence);
    }

    public async Task<CorrespondenceDto?> GetByReferenceNumberAsync(string referenceNumber, string userId)
    {
        var correspondence = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee).ThenInclude(e => e!.Position)
            .Include(c => c.ToEmployee).ThenInclude(e => e!.Position)
            .Include(c => c.ToDepartment)
            .Include(c => c.Form)
            .Include(c => c.Attachments)
            .Include(c => c.Routings)
            .FirstOrDefaultAsync(c => c.ReferenceNumber == referenceNumber && !c.IsDeleted);

        return correspondence == null ? null : MapToDto(correspondence);
    }

    public async Task<List<CorrespondenceSummaryDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 20)
    {
        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    public async Task<CorrespondenceDto> CreateAsync(CreateCorrespondenceRequest request, string userId)
    {
        var correspondence = new Correspondence
        {
            ReferenceNumber = await GenerateReferenceNumberAsync(request.CategoryId),
            SubjectAr = request.SubjectAr,
            SubjectEn = request.SubjectEn,
            ContentAr = request.ContentAr,
            ContentEn = request.ContentEn,
            CategoryId = request.CategoryId,
            Status = request.Status,
            Priority = request.Priority,
            ConfidentialityLevel = request.ConfidentialityLevel,
            CorrespondenceDate = request.CorrespondenceDate ?? DateTime.UtcNow,
            DueDate = request.DueDate,
            FromEmployeeId = request.FromEmployeeId,
            ExternalSenderName = request.ExternalSenderName,
            ExternalSenderOrganization = request.ExternalSenderOrganization,
            ToDepartmentId = request.ToDepartmentId,
            ToEmployeeId = request.ToEmployeeId,
            FormSubmissionId = request.FormSubmissionId,
            RelatedCorrespondenceId = request.RelatedCorrespondenceId,
            Keywords = request.Keywords,
            Tags = request.Tags,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Correspondences.Add(correspondence);
        await _context.SaveChangesAsync();

        return (await GetByIdAsync(correspondence.Id, userId))!;
    }

    public async Task<CorrespondenceDto?> UpdateAsync(int id, UpdateCorrespondenceRequest request, string userId)
    {
        var correspondence = await _context.Correspondences.FindAsync(id);
        if (correspondence == null || correspondence.IsDeleted) return null;

        correspondence.SubjectAr = request.SubjectAr;
        correspondence.SubjectEn = request.SubjectEn;
        correspondence.ContentAr = request.ContentAr;
        correspondence.ContentEn = request.ContentEn;
        correspondence.CategoryId = request.CategoryId;
        correspondence.Status = request.Status;
        correspondence.Priority = request.Priority;
        correspondence.ConfidentialityLevel = request.ConfidentialityLevel;
        correspondence.DueDate = request.DueDate;
        correspondence.ToDepartmentId = request.ToDepartmentId;
        correspondence.ToEmployeeId = request.ToEmployeeId;
        correspondence.Keywords = request.Keywords;
        correspondence.Tags = request.Tags;
        correspondence.Notes = request.Notes;
        correspondence.UpdatedAt = DateTime.UtcNow;
        correspondence.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id, userId);
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var correspondence = await _context.Correspondences.FindAsync(id);
        if (correspondence == null || correspondence.IsDeleted) return false;

        // Soft delete
        correspondence.IsDeleted = true;
        correspondence.UpdatedAt = DateTime.UtcNow;
        correspondence.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== Search & Filter ====================

    public async Task<(List<CorrespondenceSummaryDto> Items, int TotalCount)> SearchAsync(
        CorrespondenceSearchRequest request, string userId)
    {
        var query = _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        // Search term (across subject, content, reference number)
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.SubjectAr.ToLower().Contains(searchTerm) ||
                (c.SubjectEn != null && c.SubjectEn.ToLower().Contains(searchTerm)) ||
                c.ContentAr.ToLower().Contains(searchTerm) ||
                (c.ContentEn != null && c.ContentEn.ToLower().Contains(searchTerm)) ||
                c.ReferenceNumber.ToLower().Contains(searchTerm) ||
                (c.Keywords != null && c.Keywords.ToLower().Contains(searchTerm)) ||
                (c.Tags != null && c.Tags.ToLower().Contains(searchTerm))
            );
        }

        // Filters
        if (request.CategoryId.HasValue)
            query = query.Where(c => c.CategoryId == request.CategoryId.Value);

        if (!string.IsNullOrEmpty(request.Classification))
            query = query.Where(c => c.Category.Classification == request.Classification);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(c => c.Status == request.Status);

        if (!string.IsNullOrEmpty(request.Priority))
            query = query.Where(c => c.Priority == request.Priority);

        if (request.DateFrom.HasValue)
            query = query.Where(c => c.CorrespondenceDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(c => c.CorrespondenceDate <= request.DateTo.Value);

        if (request.FromEmployeeId.HasValue)
            query = query.Where(c => c.FromEmployeeId == request.FromEmployeeId.Value);

        if (request.ToEmployeeId.HasValue)
            query = query.Where(c => c.ToEmployeeId == request.ToEmployeeId.Value);

        if (request.ToDepartmentId.HasValue)
            query = query.Where(c => c.ToDepartmentId == request.ToDepartmentId.Value);

        if (request.IsArchived.HasValue)
            query = query.Where(c => c.IsArchived == request.IsArchived.Value);

        if (!string.IsNullOrEmpty(request.Tags))
        {
            var tags = request.Tags.Split(',').Select(t => t.Trim().ToLower());
            query = query.Where(c => c.Tags != null && tags.Any(t => c.Tags.ToLower().Contains(t)));
        }

        // Count before pagination
        var totalCount = await query.CountAsync();

        // Sorting
        query = request.SortBy.ToLower() switch
        {
            "referenzenumber" => request.SortOrder == "ASC"
                ? query.OrderBy(c => c.ReferenceNumber)
                : query.OrderByDescending(c => c.ReferenceNumber),
            "subject" => request.SortOrder == "ASC"
                ? query.OrderBy(c => c.SubjectAr)
                : query.OrderByDescending(c => c.SubjectAr),
            "status" => request.SortOrder == "ASC"
                ? query.OrderBy(c => c.Status)
                : query.OrderByDescending(c => c.Status),
            "priority" => request.SortOrder == "ASC"
                ? query.OrderBy(c => c.Priority)
                : query.OrderByDescending(c => c.Priority),
            "correspondencedate" => request.SortOrder == "ASC"
                ? query.OrderBy(c => c.CorrespondenceDate)
                : query.OrderByDescending(c => c.CorrespondenceDate),
            _ => request.SortOrder == "ASC"
                ? query.OrderBy(c => c.CreatedAt)
                : query.OrderByDescending(c => c.CreatedAt)
        };

        // Pagination
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return (items.Select(MapToSummaryDto).ToList(), totalCount);
    }

    // ==================== Category Operations ====================

    public async Task<List<CorrespondenceSummaryDto>> GetByCategoryAsync(int categoryId, string userId)
    {
        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.CategoryId == categoryId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    public async Task<List<CorrespondenceSummaryDto>> GetByClassificationAsync(string classification, string userId)
    {
        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.Category.Classification == classification && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    // ==================== Employee/Department Operations ====================

    public async Task<List<CorrespondenceSummaryDto>> GetByFromEmployeeAsync(int employeeId, string userId)
    {
        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.FromEmployeeId == employeeId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    public async Task<List<CorrespondenceSummaryDto>> GetByToEmployeeAsync(int employeeId, string userId)
    {
        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.ToEmployeeId == employeeId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    public async Task<List<CorrespondenceSummaryDto>> GetByDepartmentAsync(int departmentId, string userId)
    {
        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.ToDepartmentId == departmentId && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    // ==================== Status Operations ====================

    public async Task<List<CorrespondenceSummaryDto>> GetByStatusAsync(string status, string userId)
    {
        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.Status == status && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    public async Task<bool> UpdateStatusAsync(int id, string status, string userId)
    {
        var correspondence = await _context.Correspondences.FindAsync(id);
        if (correspondence == null || correspondence.IsDeleted) return false;

        correspondence.Status = status;
        correspondence.UpdatedAt = DateTime.UtcNow;
        correspondence.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== Attachment Operations ====================

    public async Task<CorrespondenceAttachmentDto> AddAttachmentAsync(
        int correspondenceId, UploadAttachmentRequest request, Stream fileStream,
        string fileName, string contentType, string userId)
    {
        var correspondence = await _context.Correspondences.FindAsync(correspondenceId);
        if (correspondence == null || correspondence.IsDeleted)
            throw new KeyNotFoundException($"Correspondence with ID {correspondenceId} not found");

        // Generate unique file name
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var uploadPath = Path.Combine("uploads", "correspondences", correspondenceId.ToString());
        var fullPath = Path.Combine(uploadPath, uniqueFileName);

        // Create directory if it doesn't exist
        Directory.CreateDirectory(uploadPath);

        // Save file - reset stream position for DMS
        var fileStreamPosition = fileStream.Position;

        using (var fileStream2 = new FileStream(fullPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStream2);
        }

        // Get file size
        var fileInfo = new FileInfo(fullPath);

        // Get next sort order
        var maxSortOrder = await _context.CorrespondenceAttachments
            .Where(a => a.CorrespondenceId == correspondenceId)
            .MaxAsync(a => (int?)a.SortOrder) ?? 0;

        var attachment = new CorrespondenceAttachment
        {
            CorrespondenceId = correspondenceId,
            FileName = uniqueFileName,
            OriginalFileName = fileName,
            FilePath = fullPath,
            FileSize = fileInfo.Length,
            MimeType = contentType,
            FileExtension = fileExtension,
            Description = request.Description,
            IsMainDocument = request.IsMainDocument,
            SortOrder = maxSortOrder + 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.CorrespondenceAttachments.Add(attachment);
        await _context.SaveChangesAsync();

        // Create DMS document for this attachment
        try
        {
            // Reset stream to beginning for DMS
            fileStream.Position = fileStreamPosition;

            var createDocumentDto = new CreateDocumentDto
            {
                Title = request.Description ?? fileName,
                Description = $"Attachment for correspondence {correspondence.ReferenceNumber}",
                Category = DocumentCategory.Correspondence,
                AccessLevel = MapConfidentialityToAccessLevel(correspondence.ConfidentialityLevel),
                CorrespondenceId = correspondenceId,
                Tags = new List<string> { "correspondence-attachment", correspondence.ReferenceNumber }
            };

            // Open a new stream from the saved file
            using (var dmsFileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                var document = await _documentService.CreateDocumentFromStreamAsync(
                    createDocumentDto,
                    dmsFileStream,
                    fileName,
                    contentType,
                    userId
                );

                // Store DMS document ID in attachment metadata (if needed later)
                // attachment.DocumentId = document.Id;  // Would need to add this field to CorrespondenceAttachment
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail the attachment creation if DMS fails
            // The attachment is still saved in the correspondence system
            Console.WriteLine($"Warning: Failed to create DMS document for attachment: {ex.Message}");
        }

        return MapAttachmentToDto(attachment);
    }

    private DocumentAccessLevel MapConfidentialityToAccessLevel(string confidentialityLevel)
    {
        return confidentialityLevel?.ToLower() switch
        {
            "public" => DocumentAccessLevel.Public,
            "internal" => DocumentAccessLevel.Internal,
            "restricted" => DocumentAccessLevel.Restricted,
            "confidential" => DocumentAccessLevel.Confidential,
            "secret" => DocumentAccessLevel.Secret,
            _ => DocumentAccessLevel.Internal
        };
    }

    public async Task<List<CorrespondenceAttachmentDto>> GetAttachmentsAsync(int correspondenceId, string userId)
    {
        var attachments = await _context.CorrespondenceAttachments
            .Where(a => a.CorrespondenceId == correspondenceId)
            .OrderBy(a => a.SortOrder)
            .ToListAsync();

        return attachments.Select(MapAttachmentToDto).ToList();
    }

    public async Task<bool> DeleteAttachmentAsync(int attachmentId, string userId)
    {
        var attachment = await _context.CorrespondenceAttachments.FindAsync(attachmentId);
        if (attachment == null) return false;

        // Delete physical file
        if (File.Exists(attachment.FilePath))
        {
            File.Delete(attachment.FilePath);
        }

        _context.CorrespondenceAttachments.Remove(attachment);
        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== Routing Operations ====================

    public async Task<CorrespondenceRoutingDto> RouteCorrespondenceAsync(RouteCorrespondenceRequest request, string userId)
    {
        var correspondence = await _context.Correspondences.FindAsync(request.CorrespondenceId);
        if (correspondence == null || correspondence.IsDeleted)
            throw new KeyNotFoundException($"Correspondence with ID {request.CorrespondenceId} not found");

        // Get current employee ID from userId
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId);
        if (currentEmployee == null)
            throw new InvalidOperationException("Employee not found for current user");

        // Get next sequence number
        var maxSequence = await _context.CorrespondenceRoutings
            .Where(r => r.CorrespondenceId == request.CorrespondenceId)
            .MaxAsync(r => (int?)r.SequenceNumber) ?? 0;

        var routing = new CorrespondenceRouting
        {
            CorrespondenceId = request.CorrespondenceId,
            FromEmployeeId = currentEmployee.Id,
            ToEmployeeId = request.ToEmployeeId,
            ToDepartmentId = request.ToDepartmentId,
            Action = request.Action,
            Priority = request.Priority,
            Instructions = request.Instructions,
            DueDate = request.DueDate,
            RoutedDate = DateTime.UtcNow,
            Status = "Pending",
            SequenceNumber = maxSequence + 1,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.CorrespondenceRoutings.Add(routing);

        // Update correspondence status if it's a draft
        if (correspondence.Status == "Draft")
        {
            correspondence.Status = "Pending";
            correspondence.UpdatedAt = DateTime.UtcNow;
            correspondence.UpdatedBy = userId;
        }

        await _context.SaveChangesAsync();

        // Load related entities
        await _context.Entry(routing).Reference(r => r.FromEmployee).LoadAsync();
        await _context.Entry(routing).Reference(r => r.ToEmployee).LoadAsync();
        await _context.Entry(routing).Reference(r => r.Correspondence).LoadAsync();

        return MapRoutingToDto(routing);
    }

    public async Task<List<CorrespondenceRoutingDto>> GetRoutingHistoryAsync(int correspondenceId, string userId)
    {
        var routings = await _context.CorrespondenceRoutings
            .Include(r => r.FromEmployee).ThenInclude(e => e.Position)
            .Include(r => r.ToEmployee).ThenInclude(e => e.Position)
            .Include(r => r.ToDepartment)
            .Include(r => r.Correspondence)
            .Where(r => r.CorrespondenceId == correspondenceId)
            .OrderBy(r => r.SequenceNumber)
            .ToListAsync();

        return routings.Select(MapRoutingToDto).ToList();
    }

    public async Task<RoutingChainDto> GetRoutingChainAsync(int correspondenceId, string userId)
    {
        var correspondence = await _context.Correspondences
            .Include(c => c.Routings).ThenInclude(r => r.ToEmployee)
            .FirstOrDefaultAsync(c => c.Id == correspondenceId && !c.IsDeleted);

        if (correspondence == null)
            throw new KeyNotFoundException($"Correspondence with ID {correspondenceId} not found");

        var routingHistory = await GetRoutingHistoryAsync(correspondenceId, userId);
        var lastRouting = routingHistory.LastOrDefault();

        return new RoutingChainDto
        {
            CorrespondenceId = correspondenceId,
            ReferenceNumber = correspondence.ReferenceNumber,
            RoutingHistory = routingHistory,
            TotalRoutings = routingHistory.Count,
            CurrentStatus = correspondence.Status,
            CurrentAssigneeId = lastRouting?.ToEmployeeId,
            CurrentAssigneeName = lastRouting?.ToEmployeeName
        };
    }

    public async Task<bool> RespondToRoutingAsync(RespondToRoutingRequest request, string userId)
    {
        var routing = await _context.CorrespondenceRoutings.FindAsync(request.RoutingId);
        if (routing == null) return false;

        routing.Response = request.Response;
        routing.ResponseDate = DateTime.UtcNow;
        routing.Status = request.Status;
        routing.IsRead = true;

        if (request.Status == "Completed")
        {
            routing.CompletedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ==================== My Correspondence (Inbox/Outbox) ====================

    public async Task<List<CorrespondenceSummaryDto>> GetMyInboxAsync(string userId, int pageNumber = 1, int pageSize = 20)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId);
        if (currentEmployee == null) return new List<CorrespondenceSummaryDto>();

        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.ToEmployeeId == currentEmployee.Id && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    public async Task<List<CorrespondenceSummaryDto>> GetMyOutboxAsync(string userId, int pageNumber = 1, int pageSize = 20)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId);
        if (currentEmployee == null) return new List<CorrespondenceSummaryDto>();

        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.FromEmployeeId == currentEmployee.Id && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    public async Task<List<CorrespondenceSummaryDto>> GetMyDraftsAsync(string userId, int pageNumber = 1, int pageSize = 20)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId);
        if (currentEmployee == null) return new List<CorrespondenceSummaryDto>();

        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.FromEmployeeId == currentEmployee.Id && c.Status == "Draft" && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    public async Task<List<CorrespondenceRoutingDto>> GetMyPendingActionsAsync(string userId)
    {
        var currentEmployee = await _context.Employees
            .FirstOrDefaultAsync(e => e.UserId == userId);
        if (currentEmployee == null) return new List<CorrespondenceRoutingDto>();

        var routings = await _context.CorrespondenceRoutings
            .Include(r => r.FromEmployee).ThenInclude(e => e.Position)
            .Include(r => r.ToEmployee).ThenInclude(e => e.Position)
            .Include(r => r.ToDepartment)
            .Include(r => r.Correspondence)
            .Where(r => r.ToEmployeeId == currentEmployee.Id && r.Status == "Pending")
            .OrderByDescending(r => r.RoutedDate)
            .ToListAsync();

        return routings.Select(MapRoutingToDto).ToList();
    }

    // ==================== Archive Operations ====================

    public async Task<bool> ArchiveCorrespondenceAsync(ArchiveCorrespondenceRequest request, string userId)
    {
        var correspondence = await _context.Correspondences.FindAsync(request.CorrespondenceId);
        if (correspondence == null || correspondence.IsDeleted) return false;

        correspondence.IsArchived = true;
        correspondence.ArchivedAt = DateTime.UtcNow;
        correspondence.ArchivedBy = userId;
        correspondence.UpdatedAt = DateTime.UtcNow;
        correspondence.UpdatedBy = userId;

        // Note: Actual PDF generation would be handled by IPdfConversionService
        // This is just marking the correspondence as archived

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CorrespondenceSummaryDto>> GetArchivedCorrespondencesAsync(string userId)
    {
        var correspondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c => c.IsArchived && !c.IsDeleted)
            .OrderByDescending(c => c.ArchivedAt)
            .ToListAsync();

        return correspondences.Select(MapToSummaryDto).ToList();
    }

    // ==================== Related Correspondence ====================

    public async Task<List<CorrespondenceSummaryDto>> GetRelatedCorrespondencesAsync(int correspondenceId, string userId)
    {
        var correspondence = await _context.Correspondences.FindAsync(correspondenceId);
        if (correspondence == null || correspondence.IsDeleted)
            return new List<CorrespondenceSummaryDto>();

        var relatedCorrespondences = await _context.Correspondences
            .Include(c => c.Category)
            .Include(c => c.FromEmployee)
            .Include(c => c.ToEmployee)
            .Include(c => c.Attachments)
            .Where(c =>
                (c.RelatedCorrespondenceId == correspondenceId || c.Id == correspondence.RelatedCorrespondenceId)
                && c.Id != correspondenceId
                && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return relatedCorrespondences.Select(MapToSummaryDto).ToList();
    }

    // ==================== Statistics ====================

    public async Task<Dictionary<string, int>> GetCorrespondenceStatsByStatusAsync(string userId)
    {
        var stats = await _context.Correspondences
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return stats.ToDictionary(s => s.Status, s => s.Count);
    }

    public async Task<Dictionary<string, int>> GetCorrespondenceStatsByClassificationAsync(string userId)
    {
        var stats = await _context.Correspondences
            .Include(c => c.Category)
            .Where(c => !c.IsDeleted)
            .GroupBy(c => c.Category.Classification)
            .Select(g => new { Classification = g.Key, Count = g.Count() })
            .ToListAsync();

        return stats.ToDictionary(s => s.Classification, s => s.Count);
    }

    // ==================== Reference Number Generation ====================

    public async Task<string> GenerateReferenceNumberAsync(int categoryId)
    {
        var category = await _context.ArchiveCategories.FindAsync(categoryId);
        if (category == null)
            throw new KeyNotFoundException($"Category with ID {categoryId} not found");

        var year = DateTime.UtcNow.Year;
        var categoryCode = category.CategoryCode;

        // Get the last reference number for this category and year
        var lastRefNumber = await _context.Correspondences
            .Where(c => c.CategoryId == categoryId &&
                        c.ReferenceNumber.StartsWith($"{categoryCode}-{year}-"))
            .OrderByDescending(c => c.ReferenceNumber)
            .Select(c => c.ReferenceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastRefNumber != null)
        {
            var parts = lastRefNumber.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        return $"{categoryCode}-{year}-{nextNumber:D4}";
    }

    // ==================== Helper Methods ====================

    private CorrespondenceDto MapToDto(Correspondence correspondence)
    {
        return new CorrespondenceDto
        {
            Id = correspondence.Id,
            ReferenceNumber = correspondence.ReferenceNumber,
            SubjectAr = correspondence.SubjectAr,
            SubjectEn = correspondence.SubjectEn,
            ContentAr = correspondence.ContentAr,
            ContentEn = correspondence.ContentEn,
            CategoryId = correspondence.CategoryId,
            CategoryName = correspondence.Category?.NameAr ?? "",
            Classification = correspondence.Category?.Classification ?? "",
            Status = correspondence.Status,
            Priority = correspondence.Priority,
            ConfidentialityLevel = correspondence.ConfidentialityLevel,
            CorrespondenceDate = correspondence.CorrespondenceDate,
            DueDate = correspondence.DueDate,
            FromEmployeeId = correspondence.FromEmployeeId,
            FromEmployeeName = correspondence.FromEmployee?.FullName,
            ExternalSenderName = correspondence.ExternalSenderName,
            ExternalSenderOrganization = correspondence.ExternalSenderOrganization,
            ToDepartmentId = correspondence.ToDepartmentId,
            ToDepartmentName = correspondence.ToDepartment?.Name,
            ToEmployeeId = correspondence.ToEmployeeId,
            ToEmployeeName = correspondence.ToEmployee?.FullName,
            FormId = correspondence.FormId,
            FormName = correspondence.Form?.NameAr,
            FormSubmissionId = correspondence.FormSubmissionId,
            RelatedCorrespondenceId = correspondence.RelatedCorrespondenceId,
            RelatedReferenceNumber = correspondence.RelatedCorrespondence?.ReferenceNumber,
            Attachments = correspondence.Attachments?.Select(MapAttachmentToDto).ToList() ?? new List<CorrespondenceAttachmentDto>(),
            Routings = correspondence.Routings?.Select(MapRoutingToDto).ToList() ?? new List<CorrespondenceRoutingDto>(),
            ArchivedDocumentId = correspondence.ArchivedDocumentId,
            ArchiveNumber = correspondence.ArchivedDocument?.ArchiveNumber,
            PdfFilePath = correspondence.ArchivedDocument?.PdfFilePath,
            Keywords = correspondence.Keywords,
            Tags = correspondence.Tags,
            Notes = correspondence.Notes,
            IsArchived = correspondence.IsArchived,
            ArchivedAt = correspondence.ArchivedAt,
            ArchivedBy = correspondence.ArchivedBy,
            CreatedAt = correspondence.CreatedAt,
            UpdatedAt = correspondence.UpdatedAt,
            CreatedBy = correspondence.CreatedBy,
            UpdatedBy = correspondence.UpdatedBy
        };
    }

    private CorrespondenceSummaryDto MapToSummaryDto(Correspondence correspondence)
    {
        return new CorrespondenceSummaryDto
        {
            Id = correspondence.Id,
            ReferenceNumber = correspondence.ReferenceNumber,
            SubjectAr = correspondence.SubjectAr,
            CategoryName = correspondence.Category?.NameAr ?? "",
            Status = correspondence.Status,
            Priority = correspondence.Priority,
            CorrespondenceDate = correspondence.CorrespondenceDate,
            FromEmployeeName = correspondence.FromEmployee?.FullName,
            ToEmployeeName = correspondence.ToEmployee?.FullName,
            AttachmentCount = correspondence.Attachments?.Count ?? 0,
            IsArchived = correspondence.IsArchived,
            CreatedAt = correspondence.CreatedAt
        };
    }

    private CorrespondenceAttachmentDto MapAttachmentToDto(CorrespondenceAttachment attachment)
    {
        return new CorrespondenceAttachmentDto
        {
            Id = attachment.Id,
            CorrespondenceId = attachment.CorrespondenceId,
            FileName = attachment.FileName,
            OriginalFileName = attachment.OriginalFileName,
            FilePath = attachment.FilePath,
            FileSize = attachment.FileSize,
            MimeType = attachment.MimeType,
            FileExtension = attachment.FileExtension,
            Description = attachment.Description,
            Version = attachment.Version,
            IsMainDocument = attachment.IsMainDocument,
            SortOrder = attachment.SortOrder,
            CreatedAt = attachment.CreatedAt,
            CreatedBy = attachment.CreatedBy
        };
    }

    private CorrespondenceRoutingDto MapRoutingToDto(CorrespondenceRouting routing)
    {
        return new CorrespondenceRoutingDto
        {
            Id = routing.Id,
            CorrespondenceId = routing.CorrespondenceId,
            CorrespondenceReferenceNumber = routing.Correspondence?.ReferenceNumber ?? "",
            FromEmployeeId = routing.FromEmployeeId,
            FromEmployeeName = routing.FromEmployee?.FullName ?? "",
            FromEmployeePosition = routing.FromEmployee?.Position?.Title,
            ToEmployeeId = routing.ToEmployeeId,
            ToEmployeeName = routing.ToEmployee?.FullName ?? "",
            ToEmployeePosition = routing.ToEmployee?.Position?.Title,
            ToDepartmentId = routing.ToDepartmentId,
            ToDepartmentName = routing.ToDepartment?.Name,
            Action = routing.Action,
            Priority = routing.Priority,
            Instructions = routing.Instructions,
            DueDate = routing.DueDate,
            RoutedDate = routing.RoutedDate,
            ReceivedDate = routing.ReceivedDate,
            IsRead = routing.IsRead,
            Response = routing.Response,
            ResponseDate = routing.ResponseDate,
            Status = routing.Status,
            CompletedDate = routing.CompletedDate,
            ParentRoutingId = routing.ParentRoutingId,
            SequenceNumber = routing.SequenceNumber,
            CreatedAt = routing.CreatedAt
        };
    }
}
