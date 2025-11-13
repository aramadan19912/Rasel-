using Application.DTOs.Archive;
using Application.Interfaces.Archive;
using Backend.Infrastructure.Data;
using Domain.Entities.Archive;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Archive;

public class ArchiveCategoryService : IArchiveCategoryService
{
    private readonly ApplicationDbContext _context;

    public ArchiveCategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ArchiveCategoryDto?> GetByIdAsync(int id, string userId)
    {
        var category = await _context.ArchiveCategories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);

        return category == null ? null : MapToDto(category);
    }

    public async Task<List<ArchiveCategoryDto>> GetAllAsync(string userId)
    {
        var categories = await _context.ArchiveCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<ArchiveCategoryHierarchyDto> GetCategoryTreeAsync(string userId)
    {
        var allCategories = await _context.ArchiveCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var rootCategories = allCategories.Where(c => c.ParentCategoryId == null).ToList();

        var result = new ArchiveCategoryHierarchyDto
        {
            Id = 0,
            NameAr = "الجذر",
            NameEn = "Root",
            CategoryCode = "ROOT",
            Classification = "Root",
            RetentionPeriod = "",
            IsActive = true,
            SortOrder = 0,
            CorrespondenceCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            SubCategories = BuildHierarchy(rootCategories, allCategories)
        };

        return result;
    }

    public async Task<ArchiveCategoryDto> CreateAsync(CreateArchiveCategoryRequest request, string userId)
    {
        var category = new ArchiveCategory
        {
            CategoryCode = request.CategoryCode,
            NameAr = request.NameAr,
            NameEn = request.NameEn,
            Description = request.Description,
            Classification = request.Classification,
            ParentCategoryId = request.ParentCategoryId,
            RetentionPeriod = request.RetentionPeriod,
            Icon = request.Icon,
            Color = request.Color,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.ArchiveCategories.Add(category);
        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<ArchiveCategoryDto?> UpdateAsync(int id, CreateArchiveCategoryRequest request, string userId)
    {
        var category = await _context.ArchiveCategories.FindAsync(id);
        if (category == null) return null;

        category.CategoryCode = request.CategoryCode;
        category.NameAr = request.NameAr;
        category.NameEn = request.NameEn;
        category.Description = request.Description;
        category.Classification = request.Classification;
        category.ParentCategoryId = request.ParentCategoryId;
        category.RetentionPeriod = request.RetentionPeriod;
        category.Icon = request.Icon;
        category.Color = request.Color;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        var category = await _context.ArchiveCategories.FindAsync(id);
        if (category == null) return false;

        // Check if category has correspondences
        var hasCorrespondences = await _context.Correspondences.AnyAsync(c => c.CategoryId == id);
        if (hasCorrespondences) return false;

        // Check if category has sub-categories
        var hasSubCategories = await _context.ArchiveCategories.AnyAsync(c => c.ParentCategoryId == id);
        if (hasSubCategories) return false;

        _context.ArchiveCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ArchiveCategoryDto>> GetSubCategoriesAsync(int parentId, string userId)
    {
        var subCategories = await _context.ArchiveCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.ParentCategoryId == parentId && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return subCategories.Select(MapToDto).ToList();
    }

    public async Task<ArchiveCategoryDto?> GetParentCategoryAsync(int categoryId, string userId)
    {
        var category = await _context.ArchiveCategories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        return category?.ParentCategory == null ? null : MapToDto(category.ParentCategory);
    }

    public async Task<List<ArchiveCategoryDto>> GetRootCategoriesAsync(string userId)
    {
        var rootCategories = await _context.ArchiveCategories
            .Where(c => c.ParentCategoryId == null && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return rootCategories.Select(MapToDto).ToList();
    }

    public async Task<List<ArchiveCategoryDto>> GetByClassificationAsync(string classification, string userId)
    {
        var categories = await _context.ArchiveCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.Classification == classification && c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<List<ArchiveCategoryDto>> SearchAsync(string searchTerm, string userId)
    {
        var categories = await _context.ArchiveCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.IsActive && (
                c.NameAr.Contains(searchTerm) ||
                (c.NameEn != null && c.NameEn.Contains(searchTerm)) ||
                c.CategoryCode.Contains(searchTerm) ||
                (c.Description != null && c.Description.Contains(searchTerm))
            ))
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<ArchiveCategoryStatsDto> GetCategoryStatsAsync(int categoryId, string userId)
    {
        var category = await _context.ArchiveCategories.FindAsync(categoryId);
        if (category == null)
            throw new KeyNotFoundException($"Category with ID {categoryId} not found");

        var correspondences = await _context.Correspondences
            .Where(c => c.CategoryId == categoryId)
            .ToListAsync();

        var archivedDocs = await _context.ArchiveDocuments
            .Where(d => correspondences.Select(c => c.Id).Contains(d.CorrespondenceId))
            .ToListAsync();

        return new ArchiveCategoryStatsDto
        {
            CategoryId = categoryId,
            CategoryName = category.NameAr,
            TotalCorrespondences = correspondences.Count,
            ArchivedCount = correspondences.Count(c => c.IsArchived),
            PendingCount = correspondences.Count(c => c.Status == "Pending" || c.Status == "Draft"),
            CompletedCount = correspondences.Count(c => c.Status == "Completed"),
            TotalSizeBytes = archivedDocs.Sum(d => d.FileSize),
            OldestCorrespondence = correspondences.Any() ? correspondences.Min(c => c.CorrespondenceDate) : null,
            NewestCorrespondence = correspondences.Any() ? correspondences.Max(c => c.CorrespondenceDate) : null
        };
    }

    public async Task<List<ArchiveCategoryStatsDto>> GetAllCategoriesStatsAsync(string userId)
    {
        var categories = await _context.ArchiveCategories
            .Where(c => c.IsActive)
            .ToListAsync();

        var stats = new List<ArchiveCategoryStatsDto>();
        foreach (var category in categories)
        {
            stats.Add(await GetCategoryStatsAsync(category.Id, userId));
        }

        return stats;
    }

    public async Task<bool> CategoryCodeExistsAsync(string code)
    {
        return await _context.ArchiveCategories.AnyAsync(c => c.CategoryCode == code);
    }

    public async Task<bool> CanDeleteCategoryAsync(int id)
    {
        var hasCorrespondences = await _context.Correspondences.AnyAsync(c => c.CategoryId == id);
        var hasSubCategories = await _context.ArchiveCategories.AnyAsync(c => c.ParentCategoryId == id);
        return !hasCorrespondences && !hasSubCategories;
    }

    // Helper methods
    private ArchiveCategoryDto MapToDto(ArchiveCategory category)
    {
        return new ArchiveCategoryDto
        {
            Id = category.Id,
            CategoryCode = category.CategoryCode,
            NameAr = category.NameAr,
            NameEn = category.NameEn,
            Description = category.Description,
            Classification = category.Classification,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.NameAr,
            RetentionPeriod = category.RetentionPeriod,
            Icon = category.Icon,
            Color = category.Color,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            CorrespondenceCount = _context.Correspondences.Count(c => c.CategoryId == category.Id),
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    private List<ArchiveCategoryHierarchyDto> BuildHierarchy(List<ArchiveCategory> categories, List<ArchiveCategory> allCategories)
    {
        var result = new List<ArchiveCategoryHierarchyDto>();

        foreach (var category in categories)
        {
            var dto = new ArchiveCategoryHierarchyDto
            {
                Id = category.Id,
                CategoryCode = category.CategoryCode,
                NameAr = category.NameAr,
                NameEn = category.NameEn,
                Description = category.Description,
                Classification = category.Classification,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = category.ParentCategory?.NameAr,
                RetentionPeriod = category.RetentionPeriod,
                Icon = category.Icon,
                Color = category.Color,
                SortOrder = category.SortOrder,
                IsActive = category.IsActive,
                CorrespondenceCount = _context.Correspondences.Count(c => c.CategoryId == category.Id),
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt,
                SubCategories = BuildHierarchy(
                    allCategories.Where(c => c.ParentCategoryId == category.Id).ToList(),
                    allCategories
                )
            };

            result.Add(dto);
        }

        return result;
    }
}
