using Application.Interfaces.DMS;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.Data;
using OutlookInboxManagement.Domain.Entities.DMS;
using OutlookInboxManagement.DTOs.DMS;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;

namespace Infrastructure.Services.DMS;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly string _uploadPath;
    private readonly ILogger<DocumentService> _logger;

    public DocumentService(ApplicationDbContext context, IConfiguration configuration, ILogger<DocumentService> logger)
    {
        _context = context;
        _logger = logger;
        _uploadPath = configuration["FileStorage:DocumentsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads", "documents");

        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    #region Document Management

    public async Task<DocumentDto> GetDocumentByIdAsync(int id)
    {
        var document = await _context.Set<Document>()
            .Include(d => d.Versions)
            .Include(d => d.Metadata)
            .Include(d => d.Folder)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {id} not found");

        return MapToDto(document);
    }

    public async Task<DocumentSearchResultDto> SearchDocumentsAsync(DocumentSearchDto searchDto)
    {
        var query = _context.Set<Document>()
            .Where(d => !d.IsDeleted)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
        {
            query = query.Where(d => d.Title.Contains(searchDto.SearchTerm) ||
                                    d.FileName.Contains(searchDto.SearchTerm) ||
                                    d.Description!.Contains(searchDto.SearchTerm));
        }

        if (searchDto.DocumentTypes?.Any() == true)
        {
            query = query.Where(d => searchDto.DocumentTypes.Contains(d.DocumentType));
        }

        if (searchDto.Categories?.Any() == true)
        {
            query = query.Where(d => searchDto.Categories.Contains(d.Category));
        }

        if (searchDto.FolderId.HasValue)
        {
            query = query.Where(d => d.FolderId == searchDto.FolderId.Value);
        }

        if (searchDto.CorrespondenceId.HasValue)
        {
            query = query.Where(d => d.CorrespondenceId == searchDto.CorrespondenceId.Value);
        }

        if (searchDto.CreatedFrom.HasValue)
        {
            query = query.Where(d => d.CreatedAt >= searchDto.CreatedFrom.Value);
        }

        if (searchDto.CreatedTo.HasValue)
        {
            query = query.Where(d => d.CreatedAt <= searchDto.CreatedTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchDto.OwnerId))
        {
            query = query.Where(d => d.OwnerId == searchDto.OwnerId);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)searchDto.PageSize);

        var documents = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .Include(d => d.Versions)
            .Include(d => d.Metadata)
            .ToListAsync();

        return new DocumentSearchResultDto
        {
            Documents = documents.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = searchDto.PageNumber,
            PageSize = searchDto.PageSize,
            TotalPages = totalPages
        };
    }

    public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto createDto, IFormFile file, string userId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required");

        // Generate unique file name
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);

        // Save file to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Calculate file hash
        var fileHash = await CalculateFileHashAsync(filePath);

        // Determine document type
        var documentType = DetermineDocumentType(fileExtension);

        // Create document entity
        var document = new Document
        {
            FileName = uniqueFileName,
            OriginalFileName = file.FileName,
            FileExtension = fileExtension,
            MimeType = file.ContentType,
            FileSize = file.Length,
            FilePath = filePath,
            Title = createDto.Title,
            Description = createDto.Description,
            DocumentType = documentType,
            Category = createDto.Category,
            Tags = string.Join(",", createDto.Tags ?? new List<string>()),
            CurrentVersion = 1,
            AccessLevel = createDto.AccessLevel,
            CorrespondenceId = createDto.CorrespondenceId,
            FolderId = createDto.FolderId,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Document>().Add(document);

        // Create initial version
        var version = new DocumentVersion
        {
            Document = document,
            VersionNumber = 1,
            FileName = uniqueFileName,
            FilePath = filePath,
            FileSize = file.Length,
            FileHash = fileHash,
            VersionComment = "Initial version",
            ChangeType = VersionChangeType.Created,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Set<DocumentVersion>().Add(version);

        // Add metadata if provided
        if (createDto.Metadata?.Any() == true)
        {
            foreach (var meta in createDto.Metadata)
            {
                var metadata = new DocumentMetadata
                {
                    Document = document,
                    Key = meta.Key,
                    Value = meta.Value,
                    Type = MetadataType.String
                };
                _context.Set<DocumentMetadata>().Add(metadata);
            }
        }

        // Log activity
        await LogActivityInternalAsync(document, DocumentActivityType.Created, "Document created", userId);

        await _context.SaveChangesAsync();

        return await GetDocumentByIdAsync(document.Id);
    }

    public async Task<DocumentDto> CreateDocumentFromStreamAsync(CreateDocumentDto createDto, Stream fileStream, string fileName, string contentType, string userId)
    {
        if (fileStream == null || fileStream.Length == 0)
            throw new ArgumentException("File stream is required");

        // Generate unique file name
        var fileExtension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);

        // Save file to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // Calculate file hash
        var fileHash = await CalculateFileHashAsync(filePath);

        // Determine document type
        var documentType = DetermineDocumentType(fileExtension);

        // Get file size
        var fileInfo = new FileInfo(filePath);

        // Create document entity
        var document = new Document
        {
            FileName = uniqueFileName,
            OriginalFileName = fileName,
            FileExtension = fileExtension,
            MimeType = contentType,
            FileSize = fileInfo.Length,
            FilePath = filePath,
            Title = createDto.Title,
            Description = createDto.Description,
            DocumentType = documentType,
            Category = createDto.Category,
            Tags = string.Join(",", createDto.Tags ?? new List<string>()),
            CurrentVersion = 1,
            AccessLevel = createDto.AccessLevel,
            CorrespondenceId = createDto.CorrespondenceId,
            FolderId = createDto.FolderId,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<Document>().Add(document);

        // Create initial version
        var version = new DocumentVersion
        {
            Document = document,
            VersionNumber = 1,
            FileName = uniqueFileName,
            FilePath = filePath,
            FileSize = fileInfo.Length,
            FileHash = fileHash,
            VersionComment = "Initial version",
            ChangeType = VersionChangeType.Created,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Set<DocumentVersion>().Add(version);

        // Add metadata if provided
        if (createDto.Metadata?.Any() == true)
        {
            foreach (var meta in createDto.Metadata)
            {
                var metadata = new DocumentMetadata
                {
                    Document = document,
                    Key = meta.Key,
                    Value = meta.Value,
                    Type = MetadataType.String
                };
                _context.Set<DocumentMetadata>().Add(metadata);
            }
        }

        // Log activity
        await LogActivityInternalAsync(document, DocumentActivityType.Created, "Document created from correspondence attachment", userId);

        await _context.SaveChangesAsync();

        return await GetDocumentByIdAsync(document.Id);
    }

    public async Task<DocumentDto> UpdateDocumentAsync(int id, UpdateDocumentDto updateDto, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {id} not found");

        // Check if locked by another user
        if (document.IsLocked && document.LockedBy != userId)
        {
            throw new InvalidOperationException($"Document is locked by another user");
        }

        // Update properties
        if (!string.IsNullOrWhiteSpace(updateDto.Title))
            document.Title = updateDto.Title;

        if (updateDto.Description != null)
            document.Description = updateDto.Description;

        if (updateDto.Category.HasValue)
            document.Category = updateDto.Category.Value;

        if (updateDto.Tags != null)
            document.Tags = string.Join(",", updateDto.Tags);

        if (updateDto.AccessLevel.HasValue)
            document.AccessLevel = updateDto.AccessLevel.Value;

        if (updateDto.FolderId.HasValue)
            document.FolderId = updateDto.FolderId.Value;

        document.UpdatedAt = DateTime.UtcNow;

        // Update metadata
        if (updateDto.Metadata != null)
        {
            await UpdateMetadataAsync(id, updateDto.Metadata, userId);
        }

        await LogActivityInternalAsync(document, DocumentActivityType.Edited, "Document updated", userId);
        await _context.SaveChangesAsync();

        return await GetDocumentByIdAsync(id);
    }

    public async Task DeleteDocumentAsync(int id, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {id} not found");

        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;

        await LogActivityInternalAsync(document, DocumentActivityType.Deleted, "Document deleted", userId);
        await _context.SaveChangesAsync();
    }

    public async Task<byte[]> DownloadDocumentAsync(int id, string userId, int? versionId = null)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {id} not found");

        string filePath;

        if (versionId.HasValue)
        {
            var version = await _context.Set<DocumentVersion>()
                .FirstOrDefaultAsync(v => v.Id == versionId.Value && v.DocumentId == id)
                ?? throw new KeyNotFoundException($"Version with ID {versionId} not found");

            filePath = version.FilePath;
        }
        else
        {
            filePath = document.FilePath;
        }

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found on disk");

        await LogActivityInternalAsync(document, DocumentActivityType.Downloaded, "Document downloaded", userId, versionId);
        await _context.SaveChangesAsync();

        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task<string> GetDocumentPreviewUrlAsync(int id, int? versionId = null)
    {
        // This would return a URL for document preview
        // Implementation depends on your preview service
        return $"/api/documents/{id}/preview" + (versionId.HasValue ? $"?versionId={versionId}" : "");
    }

    #endregion

    #region Versioning

    public async Task<List<DocumentVersionDto>> GetVersionHistoryAsync(int documentId)
    {
        var versions = await _context.Set<DocumentVersion>()
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();

        return versions.Select(v => new DocumentVersionDto
        {
            Id = v.Id,
            DocumentId = v.DocumentId,
            VersionNumber = v.VersionNumber,
            FileName = v.FileName,
            FilePath = v.FilePath,
            FileSize = v.FileSize,
            VersionComment = v.VersionComment,
            ChangeType = v.ChangeType,
            CreatedBy = v.CreatedBy,
            CreatedByName = v.CreatedByName,
            CreatedAt = v.CreatedAt,
            IsActive = v.IsActive
        }).ToList();
    }

    public async Task<DocumentVersionDto> CreateNewVersionAsync(int documentId, CreateVersionDto versionDto, IFormFile file, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found");

        if (file == null || file.Length == 0)
            throw new ArgumentException("File is required");

        // Generate unique file name
        var fileExtension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);

        // Save file to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Calculate file hash
        var fileHash = await CalculateFileHashAsync(filePath);

        // Increment version number
        var newVersionNumber = document.CurrentVersion + 1;

        // Create new version
        var version = new DocumentVersion
        {
            DocumentId = documentId,
            VersionNumber = newVersionNumber,
            FileName = uniqueFileName,
            FilePath = filePath,
            FileSize = file.Length,
            FileHash = fileHash,
            VersionComment = versionDto.VersionComment,
            ChangeType = versionDto.ChangeType,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Set<DocumentVersion>().Add(version);

        // Update document
        document.CurrentVersion = newVersionNumber;
        document.FileName = uniqueFileName;
        document.FilePath = filePath;
        document.FileSize = file.Length;
        document.UpdatedAt = DateTime.UtcNow;

        await LogActivityInternalAsync(document, DocumentActivityType.VersionCreated, $"Version {newVersionNumber} created", userId, version.Id);
        await _context.SaveChangesAsync();

        return new DocumentVersionDto
        {
            Id = version.Id,
            DocumentId = version.DocumentId,
            VersionNumber = version.VersionNumber,
            FileName = version.FileName,
            FilePath = version.FilePath,
            FileSize = version.FileSize,
            VersionComment = version.VersionComment,
            ChangeType = version.ChangeType,
            CreatedBy = version.CreatedBy,
            CreatedByName = version.CreatedByName,
            CreatedAt = version.CreatedAt,
            IsActive = version.IsActive
        };
    }

    public async Task<DocumentDto> RestoreVersionAsync(int documentId, int versionId, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found");

        var version = await _context.Set<DocumentVersion>()
            .FirstOrDefaultAsync(v => v.Id == versionId && v.DocumentId == documentId)
            ?? throw new KeyNotFoundException($"Version with ID {versionId} not found");

        // Update document to use this version
        document.FileName = version.FileName;
        document.FilePath = version.FilePath;
        document.FileSize = version.FileSize;
        document.UpdatedAt = DateTime.UtcNow;

        await LogActivityInternalAsync(document, DocumentActivityType.Restored, $"Restored to version {version.VersionNumber}", userId, versionId);
        await _context.SaveChangesAsync();

        return await GetDocumentByIdAsync(documentId);
    }

    public async Task<byte[]> DownloadVersionAsync(int documentId, int versionId, string userId)
    {
        return await DownloadDocumentAsync(documentId, userId, versionId);
    }

    #endregion

    #region Locking

    public async Task<DocumentDto> LockDocumentAsync(int documentId, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found");

        if (document.IsLocked && document.LockedBy != userId)
        {
            throw new InvalidOperationException($"Document is already locked by another user");
        }

        document.IsLocked = true;
        document.LockedBy = userId;
        document.LockedAt = DateTime.UtcNow;

        await LogActivityInternalAsync(document, DocumentActivityType.Locked, "Document locked for editing", userId);
        await _context.SaveChangesAsync();

        return await GetDocumentByIdAsync(documentId);
    }

    public async Task<DocumentDto> UnlockDocumentAsync(int documentId, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found");

        if (document.IsLocked && document.LockedBy != userId)
        {
            throw new InvalidOperationException($"Cannot unlock document locked by another user");
        }

        document.IsLocked = false;
        document.LockedBy = null;
        document.LockedAt = null;

        await LogActivityInternalAsync(document, DocumentActivityType.Unlocked, "Document unlocked", userId);
        await _context.SaveChangesAsync();

        return await GetDocumentByIdAsync(documentId);
    }

    public async Task<bool> IsDocumentLockedAsync(int documentId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        return document?.IsLocked ?? false;
    }

    #endregion

    #region Folders

    public async Task<DocumentFolderDto> GetFolderByIdAsync(int id)
    {
        var folder = await _context.Set<DocumentFolder>()
            .Include(f => f.SubFolders)
            .Include(f => f.Documents)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted)
            ?? throw new KeyNotFoundException($"Folder with ID {id} not found");

        return MapFolderToDto(folder);
    }

    public async Task<List<DocumentFolderDto>> GetRootFoldersAsync(string userId)
    {
        var folders = await _context.Set<DocumentFolder>()
            .Where(f => !f.IsDeleted && f.ParentFolderId == null)
            .Include(f => f.SubFolders)
            .ToListAsync();

        return folders.Select(MapFolderToDto).ToList();
    }

    public async Task<List<DocumentFolderDto>> GetSubFoldersAsync(int parentId)
    {
        var folders = await _context.Set<DocumentFolder>()
            .Where(f => !f.IsDeleted && f.ParentFolderId == parentId)
            .Include(f => f.SubFolders)
            .ToListAsync();

        return folders.Select(MapFolderToDto).ToList();
    }

    public async Task<DocumentFolderDto> CreateFolderAsync(CreateFolderDto createDto, string userId)
    {
        var folder = new DocumentFolder
        {
            Name = createDto.Name,
            Description = createDto.Description,
            ParentFolderId = createDto.ParentFolderId,
            Path = await BuildFolderPathAsync(createDto.ParentFolderId, createDto.Name),
            AccessLevel = createDto.AccessLevel,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<DocumentFolder>().Add(folder);
        await _context.SaveChangesAsync();

        return await GetFolderByIdAsync(folder.Id);
    }

    public async Task<DocumentFolderDto> UpdateFolderAsync(int id, CreateFolderDto updateDto)
    {
        var folder = await _context.Set<DocumentFolder>()
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted)
            ?? throw new KeyNotFoundException($"Folder with ID {id} not found");

        folder.Name = updateDto.Name;
        folder.Description = updateDto.Description;
        folder.AccessLevel = updateDto.AccessLevel;
        folder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetFolderByIdAsync(id);
    }

    public async Task DeleteFolderAsync(int id, string userId)
    {
        var folder = await _context.Set<DocumentFolder>()
            .Include(f => f.Documents)
            .Include(f => f.SubFolders)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted)
            ?? throw new KeyNotFoundException($"Folder with ID {id} not found");

        if (folder.Documents.Any() || folder.SubFolders.Any())
        {
            throw new InvalidOperationException("Cannot delete folder that contains documents or subfolders");
        }

        folder.IsDeleted = true;
        folder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<DocumentDto> MoveDocumentAsync(int documentId, int? targetFolderId, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found");

        document.FolderId = targetFolderId;
        document.UpdatedAt = DateTime.UtcNow;

        await LogActivityInternalAsync(document, DocumentActivityType.Moved, $"Document moved to folder {targetFolderId}", userId);
        await _context.SaveChangesAsync();

        return await GetDocumentByIdAsync(documentId);
    }

    #endregion

    #region Annotations

    public async Task<List<DocumentAnnotationDto>> GetAnnotationsAsync(int documentId, int? versionId = null)
    {
        var query = _context.Set<DocumentAnnotation>()
            .Where(a => a.DocumentId == documentId && !a.IsDeleted);

        if (versionId.HasValue)
        {
            query = query.Where(a => a.VersionId == versionId.Value);
        }

        var annotations = await query
            .Include(a => a.Replies)
            .OrderBy(a => a.PageNumber)
            .ThenBy(a => a.CreatedAt)
            .ToListAsync();

        return annotations.Select(MapAnnotationToDto).ToList();
    }

    public async Task<DocumentAnnotationDto> CreateAnnotationAsync(int documentId, CreateAnnotationDto createDto, string userId)
    {
        var annotation = new DocumentAnnotation
        {
            DocumentId = documentId,
            VersionId = createDto.VersionId,
            Type = createDto.Type,
            PageNumber = createDto.PageNumber,
            X = createDto.X,
            Y = createDto.Y,
            Width = createDto.Width,
            Height = createDto.Height,
            Content = createDto.Content,
            Color = createDto.Color,
            Opacity = createDto.Opacity,
            FontSize = createDto.FontSize,
            CreatedBy = userId,
            ParentAnnotationId = createDto.ParentAnnotationId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<DocumentAnnotation>().Add(annotation);

        var document = await _context.Set<Document>().FindAsync(documentId);
        if (document != null)
        {
            await LogActivityInternalAsync(document, DocumentActivityType.Annotated, "Annotation added", userId, createDto.VersionId);
        }

        await _context.SaveChangesAsync();

        return MapAnnotationToDto(annotation);
    }

    public async Task<DocumentAnnotationDto> UpdateAnnotationAsync(int annotationId, CreateAnnotationDto updateDto)
    {
        var annotation = await _context.Set<DocumentAnnotation>()
            .FirstOrDefaultAsync(a => a.Id == annotationId && !a.IsDeleted)
            ?? throw new KeyNotFoundException($"Annotation with ID {annotationId} not found");

        annotation.Content = updateDto.Content;
        annotation.Color = updateDto.Color;
        annotation.Opacity = updateDto.Opacity;
        annotation.FontSize = updateDto.FontSize;
        annotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapAnnotationToDto(annotation);
    }

    public async Task DeleteAnnotationAsync(int annotationId, string userId)
    {
        var annotation = await _context.Set<DocumentAnnotation>()
            .FirstOrDefaultAsync(a => a.Id == annotationId && !a.IsDeleted)
            ?? throw new KeyNotFoundException($"Annotation with ID {annotationId} not found");

        annotation.IsDeleted = true;
        annotation.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Activity Log

    public async Task<List<DocumentActivityDto>> GetDocumentActivitiesAsync(int documentId, int limit = 50)
    {
        var activities = await _context.Set<DocumentActivity>()
            .Where(a => a.DocumentId == documentId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return activities.Select(a => new DocumentActivityDto
        {
            Id = a.Id,
            DocumentId = a.DocumentId,
            VersionId = a.VersionId,
            ActivityType = a.ActivityType,
            Description = a.Description,
            UserId = a.UserId,
            UserName = a.UserName,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task LogActivityAsync(int documentId, DocumentActivityType activityType, string description, string userId, int? versionId = null)
    {
        var document = await _context.Set<Document>().FindAsync(documentId);
        if (document != null)
        {
            await LogActivityInternalAsync(document, activityType, description, userId, versionId);
            await _context.SaveChangesAsync();
        }
    }

    private async Task LogActivityInternalAsync(Document document, DocumentActivityType activityType, string description, string userId, int? versionId = null)
    {
        var activity = new DocumentActivity
        {
            Document = document,
            VersionId = versionId,
            ActivityType = activityType,
            Description = description,
            UserId = userId,
            UserName = userId, // Would need to lookup actual user name
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<DocumentActivity>().Add(activity);
    }

    #endregion

    #region Permissions

    public async Task<bool> CanAccessDocumentAsync(int documentId, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
            return false;

        // Owner always has access
        if (document.OwnerId == userId)
            return true;

        // Public documents accessible by all
        if (document.AccessLevel == DocumentAccessLevel.Public)
            return true;

        // Check allowed users
        if (!string.IsNullOrEmpty(document.AllowedUsers))
        {
            var allowedUsers = JsonSerializer.Deserialize<List<string>>(document.AllowedUsers);
            if (allowedUsers?.Contains(userId) == true)
                return true;
        }

        // Additional role-based checks would go here

        return false;
    }

    public async Task<bool> CanEditDocumentAsync(int documentId, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted);

        if (document == null)
            return false;

        // Owner can always edit
        if (document.OwnerId == userId)
            return true;

        // Locked documents can only be edited by the lock holder
        if (document.IsLocked && document.LockedBy != userId)
            return false;

        // Additional permission checks would go here

        return false;
    }

    public async Task ShareDocumentAsync(int documentId, List<string> userIds, List<string> roleNames, string userId)
    {
        var document = await _context.Set<Document>()
            .FirstOrDefaultAsync(d => d.Id == documentId && !d.IsDeleted)
            ?? throw new KeyNotFoundException($"Document with ID {documentId} not found");

        if (document.OwnerId != userId)
        {
            throw new UnauthorizedAccessException("Only the owner can share the document");
        }

        document.AllowedUsers = JsonSerializer.Serialize(userIds);
        document.AllowedRoles = JsonSerializer.Serialize(roleNames);
        document.UpdatedAt = DateTime.UtcNow;

        await LogActivityInternalAsync(document, DocumentActivityType.Shared, "Document shared", userId);
        await _context.SaveChangesAsync();
    }

    #endregion

    #region Metadata

    public async Task<Dictionary<string, string>> GetMetadataAsync(int documentId)
    {
        var metadata = await _context.Set<DocumentMetadata>()
            .Where(m => m.DocumentId == documentId)
            .ToListAsync();

        return metadata.ToDictionary(m => m.Key, m => m.Value);
    }

    public async Task UpdateMetadataAsync(int documentId, Dictionary<string, string> metadata, string userId)
    {
        // Remove existing metadata
        var existingMetadata = await _context.Set<DocumentMetadata>()
            .Where(m => m.DocumentId == documentId)
            .ToListAsync();

        _context.Set<DocumentMetadata>().RemoveRange(existingMetadata);

        // Add new metadata
        foreach (var meta in metadata)
        {
            var metadataEntity = new DocumentMetadata
            {
                DocumentId = documentId,
                Key = meta.Key,
                Value = meta.Value,
                Type = MetadataType.String
            };
            _context.Set<DocumentMetadata>().Add(metadataEntity);
        }

        var document = await _context.Set<Document>().FindAsync(documentId);
        if (document != null)
        {
            await LogActivityInternalAsync(document, DocumentActivityType.MetadataChanged, "Metadata updated", userId);
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Bulk Operations

    public async Task<List<DocumentDto>> BulkUploadAsync(List<IFormFile> files, int? folderId, string userId)
    {
        var documents = new List<DocumentDto>();

        foreach (var file in files)
        {
            var createDto = new CreateDocumentDto
            {
                Title = Path.GetFileNameWithoutExtension(file.FileName),
                Category = DocumentCategory.General,
                FolderId = folderId,
                AccessLevel = DocumentAccessLevel.Internal
            };

            var document = await CreateDocumentAsync(createDto, file, userId);
            documents.Add(document);
        }

        return documents;
    }

    public async Task BulkDeleteAsync(List<int> documentIds, string userId)
    {
        foreach (var id in documentIds)
        {
            await DeleteDocumentAsync(id, userId);
        }
    }

    public async Task<byte[]> ExportDocumentsAsZipAsync(List<int> documentIds, string userId)
    {
        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var id in documentIds)
            {
                try
                {
                    var document = await _context.Set<Document>()
                        .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

                    if (document != null && File.Exists(document.FilePath))
                    {
                        var entry = archive.CreateEntry(document.OriginalFileName);
                        using var entryStream = entry.Open();
                        using var fileStream = File.OpenRead(document.FilePath);
                        await fileStream.CopyToAsync(entryStream);

                        await LogActivityInternalAsync(document, DocumentActivityType.Downloaded, "Document included in bulk download", userId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error adding document {id} to zip");
                }
            }
        }

        await _context.SaveChangesAsync();
        return memoryStream.ToArray();
    }

    #endregion

    #region Helper Methods

    private async Task<string> CalculateFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hash = await Task.Run(() => sha256.ComputeHash(stream));
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private DocumentType DetermineDocumentType(string extension)
    {
        return extension.ToLower() switch
        {
            ".doc" or ".docx" => DocumentType.Word,
            ".xls" or ".xlsx" => DocumentType.Excel,
            ".ppt" or ".pptx" => DocumentType.PowerPoint,
            ".pdf" => DocumentType.PDF,
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => DocumentType.Image,
            ".txt" or ".md" => DocumentType.Text,
            _ => DocumentType.Other
        };
    }

    private async Task<string> BuildFolderPathAsync(int? parentId, string folderName)
    {
        if (!parentId.HasValue)
            return $"/{folderName}";

        var parent = await _context.Set<DocumentFolder>()
            .FirstOrDefaultAsync(f => f.Id == parentId.Value);

        if (parent == null)
            return $"/{folderName}";

        return $"{parent.Path}/{folderName}";
    }

    private DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            FileName = document.FileName,
            OriginalFileName = document.OriginalFileName,
            FileExtension = document.FileExtension,
            MimeType = document.MimeType,
            FileSize = document.FileSize,
            FilePath = document.FilePath,
            Title = document.Title,
            Description = document.Description,
            DocumentType = document.DocumentType,
            Category = document.Category,
            Tags = string.IsNullOrEmpty(document.Tags) ? new List<string>() : document.Tags.Split(',').ToList(),
            CurrentVersion = document.CurrentVersion,
            IsLocked = document.IsLocked,
            LockedBy = document.LockedBy,
            LockedAt = document.LockedAt,
            AccessLevel = document.AccessLevel,
            CorrespondenceId = document.CorrespondenceId,
            FolderId = document.FolderId,
            FolderPath = document.Folder?.Path,
            OwnerId = document.OwnerId,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            Versions = document.Versions?.Select(v => new DocumentVersionDto
            {
                Id = v.Id,
                DocumentId = v.DocumentId,
                VersionNumber = v.VersionNumber,
                FileName = v.FileName,
                FilePath = v.FilePath,
                FileSize = v.FileSize,
                VersionComment = v.VersionComment,
                ChangeType = v.ChangeType,
                CreatedBy = v.CreatedBy,
                CreatedByName = v.CreatedByName,
                CreatedAt = v.CreatedAt,
                IsActive = v.IsActive
            }).ToList() ?? new List<DocumentVersionDto>(),
            Metadata = document.Metadata?.ToDictionary(m => m.Key, m => m.Value) ?? new Dictionary<string, string>()
        };
    }

    private DocumentFolderDto MapFolderToDto(DocumentFolder folder)
    {
        return new DocumentFolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            Description = folder.Description,
            Path = folder.Path,
            ParentFolderId = folder.ParentFolderId,
            AccessLevel = folder.AccessLevel,
            OwnerId = folder.OwnerId,
            DocumentCount = folder.Documents?.Count(d => !d.IsDeleted) ?? 0,
            SubFolderCount = folder.SubFolders?.Count(f => !f.IsDeleted) ?? 0,
            CreatedAt = folder.CreatedAt,
            SubFolders = folder.SubFolders?.Where(f => !f.IsDeleted).Select(MapFolderToDto).ToList() ?? new List<DocumentFolderDto>()
        };
    }

    private DocumentAnnotationDto MapAnnotationToDto(DocumentAnnotation annotation)
    {
        return new DocumentAnnotationDto
        {
            Id = annotation.Id,
            DocumentId = annotation.DocumentId,
            VersionId = annotation.VersionId,
            Type = annotation.Type,
            PageNumber = annotation.PageNumber,
            X = annotation.X,
            Y = annotation.Y,
            Width = annotation.Width,
            Height = annotation.Height,
            Content = annotation.Content,
            Color = annotation.Color,
            Opacity = annotation.Opacity,
            FontSize = annotation.FontSize,
            CreatedBy = annotation.CreatedBy,
            CreatedByName = annotation.CreatedByName,
            CreatedAt = annotation.CreatedAt,
            ParentAnnotationId = annotation.ParentAnnotationId,
            Replies = annotation.Replies?.Where(r => !r.IsDeleted).Select(MapAnnotationToDto).ToList() ?? new List<DocumentAnnotationDto>()
        };
    }

    #endregion
}
