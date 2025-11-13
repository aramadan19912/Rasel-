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
public class ArchiveCategoryController : ControllerBase
{
    private readonly IArchiveCategoryService _categoryService;

    public ArchiveCategoryController(IArchiveCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    /// <summary>
    /// Get archive category by ID
    /// </summary>
    [HttpGet("{id}")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<ArchiveCategoryDto>> GetById(int id)
    {
        try
        {
            var category = await _categoryService.GetByIdAsync(id, GetUserId());
            if (category == null)
                return NotFound($"Category with ID {id} not found");

            return Ok(category);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get all archive categories
    /// </summary>
    [HttpGet]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<List<ArchiveCategoryDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync(GetUserId());
        return Ok(categories);
    }

    /// <summary>
    /// Get category tree (hierarchical structure)
    /// </summary>
    [HttpGet("tree")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<ArchiveCategoryHierarchyDto>> GetCategoryTree()
    {
        var tree = await _categoryService.GetCategoryTreeAsync(GetUserId());
        return Ok(tree);
    }

    /// <summary>
    /// Get root categories (categories without parent)
    /// </summary>
    [HttpGet("root")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<List<ArchiveCategoryDto>>> GetRootCategories()
    {
        var categories = await _categoryService.GetRootCategoriesAsync(GetUserId());
        return Ok(categories);
    }

    /// <summary>
    /// Get sub-categories of a parent category
    /// </summary>
    [HttpGet("{parentId}/subcategories")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<List<ArchiveCategoryDto>>> GetSubCategories(int parentId)
    {
        var subCategories = await _categoryService.GetSubCategoriesAsync(parentId, GetUserId());
        return Ok(subCategories);
    }

    /// <summary>
    /// Get parent category of a category
    /// </summary>
    [HttpGet("{categoryId}/parent")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<ArchiveCategoryDto>> GetParentCategory(int categoryId)
    {
        var parent = await _categoryService.GetParentCategoryAsync(categoryId, GetUserId());
        if (parent == null)
            return NotFound("Parent category not found");

        return Ok(parent);
    }

    /// <summary>
    /// Get categories by classification
    /// </summary>
    [HttpGet("classification/{classification}")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<List<ArchiveCategoryDto>>> GetByClassification(string classification)
    {
        var categories = await _categoryService.GetByClassificationAsync(classification, GetUserId());
        return Ok(categories);
    }

    /// <summary>
    /// Search categories
    /// </summary>
    [HttpGet("search")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<List<ArchiveCategoryDto>>> Search([FromQuery] string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return BadRequest("Search term is required");

        var categories = await _categoryService.SearchAsync(searchTerm, GetUserId());
        return Ok(categories);
    }

    /// <summary>
    /// Get category statistics
    /// </summary>
    [HttpGet("{id}/stats")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<ArchiveCategoryStatsDto>> GetCategoryStats(int id)
    {
        try
        {
            var stats = await _categoryService.GetCategoryStatsAsync(id, GetUserId());
            return Ok(stats);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Get all categories statistics
    /// </summary>
    [HttpGet("stats")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<List<ArchiveCategoryStatsDto>>> GetAllCategoriesStats()
    {
        var stats = await _categoryService.GetAllCategoriesStatsAsync(GetUserId());
        return Ok(stats);
    }

    /// <summary>
    /// Create new archive category
    /// </summary>
    [HttpPost]
    [Permission(SystemPermission.ArchiveCreate)]
    public async Task<ActionResult<ArchiveCategoryDto>> Create([FromBody] CreateArchiveCategoryRequest request)
    {
        // Check if category code already exists
        if (await _categoryService.CategoryCodeExistsAsync(request.CategoryCode))
            return Conflict($"Category with code '{request.CategoryCode}' already exists");

        var category = await _categoryService.CreateAsync(request, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    /// <summary>
    /// Update archive category
    /// </summary>
    [HttpPut("{id}")]
    [Permission(SystemPermission.ArchiveUpdate)]
    public async Task<ActionResult<ArchiveCategoryDto>> Update(int id, [FromBody] CreateArchiveCategoryRequest request)
    {
        var category = await _categoryService.UpdateAsync(id, request, GetUserId());
        if (category == null)
            return NotFound($"Category with ID {id} not found");

        return Ok(category);
    }

    /// <summary>
    /// Delete archive category
    /// </summary>
    [HttpDelete("{id}")]
    [Permission(SystemPermission.ArchiveDelete)]
    public async Task<IActionResult> Delete(int id)
    {
        // Check if category can be deleted
        if (!await _categoryService.CanDeleteCategoryAsync(id))
            return BadRequest("Category cannot be deleted because it has correspondences or sub-categories");

        var result = await _categoryService.DeleteAsync(id, GetUserId());
        if (!result)
            return NotFound($"Category with ID {id} not found");

        return NoContent();
    }

    /// <summary>
    /// Check if category code exists
    /// </summary>
    [HttpGet("check-code/{code}")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<bool>> CheckCodeExists(string code)
    {
        var exists = await _categoryService.CategoryCodeExistsAsync(code);
        return Ok(new { exists });
    }

    /// <summary>
    /// Check if category can be deleted
    /// </summary>
    [HttpGet("{id}/can-delete")]
    [Permission(SystemPermission.ArchiveRead)]
    public async Task<ActionResult<bool>> CanDelete(int id)
    {
        var canDelete = await _categoryService.CanDeleteCategoryAsync(id);
        return Ok(new { canDelete });
    }
}
