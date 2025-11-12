namespace Application.DTOs.Archive;

/// <summary>
/// Archive category DTO
/// </summary>
public class ArchiveCategoryDto
{
    public int Id { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string Classification { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string RetentionPeriod { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int CorrespondenceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Hierarchical category with children
/// </summary>
public class ArchiveCategoryHierarchyDto : ArchiveCategoryDto
{
    public List<ArchiveCategoryHierarchyDto> SubCategories { get; set; } = new();
}

/// <summary>
/// Create/Update category request
/// </summary>
public class CreateArchiveCategoryRequest
{
    public string CategoryCode { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string Classification { get; set; } = string.Empty;
    public int? ParentCategoryId { get; set; }
    public string RetentionPeriod { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Category statistics
/// </summary>
public class ArchiveCategoryStatsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int TotalCorrespondences { get; set; }
    public int ArchivedCount { get; set; }
    public int PendingCount { get; set; }
    public int CompletedCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime? OldestCorrespondence { get; set; }
    public DateTime? NewestCorrespondence { get; set; }
}
