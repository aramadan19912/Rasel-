using Application.DTOs.Archive;

namespace Application.Interfaces.Archive;

/// <summary>
/// Archive category service interface
/// </summary>
public interface IArchiveCategoryService
{
    // CRUD Operations
    Task<ArchiveCategoryDto?> GetByIdAsync(int id, string userId);
    Task<List<ArchiveCategoryDto>> GetAllAsync(string userId);
    Task<ArchiveCategoryHierarchyDto> GetCategoryTreeAsync(string userId);
    Task<ArchiveCategoryDto> CreateAsync(CreateArchiveCategoryRequest request, string userId);
    Task<ArchiveCategoryDto?> UpdateAsync(int id, CreateArchiveCategoryRequest request, string userId);
    Task<bool> DeleteAsync(int id, string userId);

    // Hierarchy Operations
    Task<List<ArchiveCategoryDto>> GetSubCategoriesAsync(int parentId, string userId);
    Task<ArchiveCategoryDto?> GetParentCategoryAsync(int categoryId, string userId);
    Task<List<ArchiveCategoryDto>> GetRootCategoriesAsync(string userId);

    // Filter & Search
    Task<List<ArchiveCategoryDto>> GetByClassificationAsync(string classification, string userId);
    Task<List<ArchiveCategoryDto>> SearchAsync(string searchTerm, string userId);

    // Statistics
    Task<ArchiveCategoryStatsDto> GetCategoryStatsAsync(int categoryId, string userId);
    Task<List<ArchiveCategoryStatsDto>> GetAllCategoriesStatsAsync(string userId);

    // Validation
    Task<bool> CategoryCodeExistsAsync(string code);
    Task<bool> CanDeleteCategoryAsync(int id);
}
