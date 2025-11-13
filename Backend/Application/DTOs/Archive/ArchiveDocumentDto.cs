namespace Application.DTOs.Archive;

/// <summary>
/// Archive document DTO
/// </summary>
public class ArchiveDocumentDto
{
    public int Id { get; set; }
    public string ArchiveNumber { get; set; } = string.Empty;
    public int CorrespondenceId { get; set; }
    public string CorrespondenceReferenceNumber { get; set; } = string.Empty;

    public string PdfFilePath { get; set; } = string.Empty;
    public string PdfFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int PageCount { get; set; }
    public bool IsPdfA { get; set; }

    public string Metadata { get; set; } = string.Empty;
    public string? SearchableContent { get; set; }
    public string Checksum { get; set; } = string.Empty;

    public string RetentionPeriod { get; set; } = string.Empty;
    public DateTime? DestructionDate { get; set; }
    public bool IsOnLegalHold { get; set; }
    public string? LegalHoldReason { get; set; }

    public int AccessCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }

    public bool HasWatermark { get; set; }
    public bool HasDigitalSignature { get; set; }
    public bool IsEncrypted { get; set; }

    public string? StorageLocation { get; set; }
    public string? BackupLocation { get; set; }
    public DateTime? LastBackupAt { get; set; }

    public bool IsDestroyed { get; set; }
    public DateTime? DestroyedAt { get; set; }
    public string? DestroyedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Generate PDF request
/// </summary>
public class GeneratePdfRequest
{
    public int CorrespondenceId { get; set; }
    public bool IncludeAttachments { get; set; } = true;
    public bool ConvertToPdfA { get; set; } = false;
    public bool ApplyWatermark { get; set; } = false;
    public string? WatermarkText { get; set; }
    public bool ApplyDigitalSignature { get; set; } = false;
    public bool Encrypt { get; set; } = false;
    public string? EncryptionPassword { get; set; }
}

/// <summary>
/// Archive document search request
/// </summary>
public class ArchiveSearchRequest
{
    public string? SearchTerm { get; set; }
    public string? ArchiveNumber { get; set; }
    public int? CategoryId { get; set; }
    public string? Classification { get; set; }
    public DateTime? ArchivedFrom { get; set; }
    public DateTime? ArchivedTo { get; set; }
    public bool? IsOnLegalHold { get; set; }
    public bool? IsDestroyed { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string SortBy { get; set; } = "CreatedAt";
    public string SortOrder { get; set; } = "DESC";
}

/// <summary>
/// Archive statistics
/// </summary>
public class ArchiveStatsDto
{
    public int TotalDocuments { get; set; }
    public int TotalPages { get; set; }
    public long TotalSizeBytes { get; set; }
    public string TotalSizeFormatted { get; set; } = string.Empty;

    public int DocumentsThisMonth { get; set; }
    public int DocumentsThisYear { get; set; }

    public int DocumentsByRetentionPeriod { get; set; }
    public int DocumentsOnLegalHold { get; set; }
    public int DocumentsNearingDestruction { get; set; }

    public Dictionary<string, int> DocumentsByClassification { get; set; } = new();
    public Dictionary<string, int> DocumentsByCategory { get; set; } = new();

    public DateTime? OldestDocument { get; set; }
    public DateTime? NewestDocument { get; set; }
    public DateTime? LastBackup { get; set; }
}

/// <summary>
/// Legal hold request
/// </summary>
public class LegalHoldRequest
{
    public int ArchiveDocumentId { get; set; }
    public bool IsOnLegalHold { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Document destruction request
/// </summary>
public class DestroyDocumentRequest
{
    public int ArchiveDocumentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool ConfirmDestruction { get; set; } = false;
}
