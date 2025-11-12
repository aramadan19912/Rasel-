using Backend.Infrastructure.Data;
using Domain.Entities.Archive;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

/// <summary>
/// Seeder for archive categories and initial data
/// </summary>
public class ArchiveSeeder
{
    private readonly ApplicationDbContext _context;

    public ArchiveSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Check if already seeded
        if (await _context.ArchiveCategories.AnyAsync())
        {
            return; // Already seeded
        }

        await SeedArchiveCategories();
        await _context.SaveChangesAsync();
    }

    private async Task SeedArchiveCategories()
    {
        var categories = new List<ArchiveCategory>
        {
            // عقود - Contracts
            new ArchiveCategory
            {
                CategoryCode = "CONT",
                NameAr = "عقود",
                NameEn = "Contracts",
                Description = "جميع أنواع العقود والاتفاقيات",
                Classification = ArchiveClassification.Contract.ToString(),
                RetentionPeriod = RetentionPeriod.SevenYears.ToString(),
                Icon = "file-contract",
                Color = "#3B82F6",
                SortOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new ArchiveCategory
            {
                CategoryCode = "CONT-EMP",
                NameAr = "عقود الموظفين",
                NameEn = "Employment Contracts",
                Description = "عقود التوظيف والعمل",
                Classification = ArchiveClassification.Contract.ToString(),
                RetentionPeriod = RetentionPeriod.TenYears.ToString(),
                Icon = "file-signature",
                Color = "#3B82F6",
                SortOrder = 101,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new ArchiveCategory
            {
                CategoryCode = "CONT-VEND",
                NameAr = "عقود الموردين",
                NameEn = "Vendor Contracts",
                Description = "عقود التوريد والخدمات",
                Classification = ArchiveClassification.Contract.ToString(),
                RetentionPeriod = RetentionPeriod.SevenYears.ToString(),
                Icon = "handshake",
                Color = "#3B82F6",
                SortOrder = 102,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // شكاوى - Complaints
            new ArchiveCategory
            {
                CategoryCode = "COMP",
                NameAr = "شكاوى",
                NameEn = "Complaints",
                Description = "الشكاوى والتظلمات",
                Classification = ArchiveClassification.Complaint.ToString(),
                RetentionPeriod = RetentionPeriod.FiveYears.ToString(),
                Icon = "exclamation-triangle",
                Color = "#EF4444",
                SortOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new ArchiveCategory
            {
                CategoryCode = "COMP-CUST",
                NameAr = "شكاوى العملاء",
                NameEn = "Customer Complaints",
                Description = "شكاوى العملاء والمراجعين",
                Classification = ArchiveClassification.Complaint.ToString(),
                RetentionPeriod = RetentionPeriod.FiveYears.ToString(),
                Icon = "user-slash",
                Color = "#EF4444",
                SortOrder = 201,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new ArchiveCategory
            {
                CategoryCode = "COMP-EMPL",
                NameAr = "شكاوى الموظفين",
                NameEn = "Employee Complaints",
                Description = "شكاوى وتظلمات الموظفين",
                Classification = ArchiveClassification.Complaint.ToString(),
                RetentionPeriod = RetentionPeriod.FiveYears.ToString(),
                Icon = "users",
                Color = "#EF4444",
                SortOrder = 202,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // اجتماعات - Meetings
            new ArchiveCategory
            {
                CategoryCode = "MEET",
                NameAr = "اجتماعات",
                NameEn = "Meetings",
                Description = "محاضر ووثائق الاجتماعات",
                Classification = ArchiveClassification.Meeting.ToString(),
                RetentionPeriod = RetentionPeriod.ThreeYears.ToString(),
                Icon = "calendar-check",
                Color = "#8B5CF6",
                SortOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new ArchiveCategory
            {
                CategoryCode = "MEET-EXEC",
                NameAr = "اجتماعات تنفيذية",
                NameEn = "Executive Meetings",
                Description = "اجتماعات مجلس الإدارة والإدارة التنفيذية",
                Classification = ArchiveClassification.Meeting.ToString(),
                RetentionPeriod = RetentionPeriod.Permanent.ToString(),
                Icon = "briefcase",
                Color = "#8B5CF6",
                SortOrder = 301,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // مذكرات - Memorandums
            new ArchiveCategory
            {
                CategoryCode = "MEMO",
                NameAr = "مذكرات",
                NameEn = "Memorandums",
                Description = "المذكرات والمراسلات الداخلية",
                Classification = ArchiveClassification.Memorandum.ToString(),
                RetentionPeriod = RetentionPeriod.ThreeYears.ToString(),
                Icon = "sticky-note",
                Color = "#F59E0B",
                SortOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // قرارات - Decisions
            new ArchiveCategory
            {
                CategoryCode = "DEC",
                NameAr = "قرارات",
                NameEn = "Decisions",
                Description = "القرارات الإدارية والتنفيذية",
                Classification = ArchiveClassification.Decision.ToString(),
                RetentionPeriod = RetentionPeriod.Permanent.ToString(),
                Icon = "gavel",
                Color = "#DC2626",
                SortOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // تقارير - Reports
            new ArchiveCategory
            {
                CategoryCode = "REP",
                NameAr = "تقارير",
                NameEn = "Reports",
                Description = "التقارير الدورية والخاصة",
                Classification = ArchiveClassification.Report.ToString(),
                RetentionPeriod = RetentionPeriod.FiveYears.ToString(),
                Icon = "chart-bar",
                Color = "#10B981",
                SortOrder = 6,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },
            new ArchiveCategory
            {
                CategoryCode = "REP-FIN",
                NameAr = "التقارير المالية",
                NameEn = "Financial Reports",
                Description = "التقارير المالية والميزانيات",
                Classification = ArchiveClassification.Report.ToString(),
                RetentionPeriod = RetentionPeriod.TenYears.ToString(),
                Icon = "file-invoice-dollar",
                Color = "#10B981",
                SortOrder = 601,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // فواتير - Invoices
            new ArchiveCategory
            {
                CategoryCode = "INV",
                NameAr = "فواتير",
                NameEn = "Invoices",
                Description = "الفواتير والمستندات المالية",
                Classification = ArchiveClassification.Invoice.ToString(),
                RetentionPeriod = RetentionPeriod.SevenYears.ToString(),
                Icon = "file-invoice",
                Color = "#06B6D4",
                SortOrder = 7,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // أوامر شراء - Purchase Orders
            new ArchiveCategory
            {
                CategoryCode = "PO",
                NameAr = "أوامر شراء",
                NameEn = "Purchase Orders",
                Description = "أوامر الشراء والتوريد",
                Classification = ArchiveClassification.PurchaseOrder.ToString(),
                RetentionPeriod = RetentionPeriod.FiveYears.ToString(),
                Icon = "shopping-cart",
                Color = "#8B5CF6",
                SortOrder = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // موارد بشرية - HR Documents
            new ArchiveCategory
            {
                CategoryCode = "HR",
                NameAr = "موارد بشرية",
                NameEn = "HR Documents",
                Description = "مستندات الموارد البشرية",
                Classification = ArchiveClassification.HR_Document.ToString(),
                RetentionPeriod = RetentionPeriod.TenYears.ToString(),
                Icon = "user-tie",
                Color = "#EC4899",
                SortOrder = 9,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // قانونية - Legal
            new ArchiveCategory
            {
                CategoryCode = "LEG",
                NameAr = "قانونية",
                NameEn = "Legal",
                Description = "المستندات والمراسلات القانونية",
                Classification = ArchiveClassification.Legal.ToString(),
                RetentionPeriod = RetentionPeriod.Permanent.ToString(),
                Icon = "balance-scale",
                Color = "#1F2937",
                SortOrder = 10,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // مالية - Financial
            new ArchiveCategory
            {
                CategoryCode = "FIN",
                NameAr = "مالية",
                NameEn = "Financial",
                Description = "المستندات والمراسلات المالية",
                Classification = ArchiveClassification.Financial.ToString(),
                RetentionPeriod = RetentionPeriod.TenYears.ToString(),
                Icon = "coins",
                Color = "#059669",
                SortOrder = 11,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // فنية - Technical
            new ArchiveCategory
            {
                CategoryCode = "TECH",
                NameAr = "فنية",
                NameEn = "Technical",
                Description = "المستندات الفنية والتقنية",
                Classification = ArchiveClassification.Technical.ToString(),
                RetentionPeriod = RetentionPeriod.FiveYears.ToString(),
                Icon = "cogs",
                Color = "#6366F1",
                SortOrder = 12,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // إدارية - Administrative
            new ArchiveCategory
            {
                CategoryCode = "ADM",
                NameAr = "إدارية",
                NameEn = "Administrative",
                Description = "المراسلات الإدارية العامة",
                Classification = ArchiveClassification.Administrative.ToString(),
                RetentionPeriod = RetentionPeriod.ThreeYears.ToString(),
                Icon = "building",
                Color = "#64748B",
                SortOrder = 13,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            },

            // مراسلات عامة - General Correspondence
            new ArchiveCategory
            {
                CategoryCode = "CORR",
                NameAr = "مراسلات عامة",
                NameEn = "General Correspondence",
                Description = "المراسلات العامة والمتنوعة",
                Classification = ArchiveClassification.Correspondence.ToString(),
                RetentionPeriod = RetentionPeriod.OneYear.ToString(),
                Icon = "envelope",
                Color = "#94A3B8",
                SortOrder = 14,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            }
        };

        // Establish parent-child relationships
        var rootCategories = categories.Where(c => c.SortOrder < 100).ToList();
        var childCategories = categories.Where(c => c.SortOrder >= 100).ToList();

        // Add root categories first
        await _context.ArchiveCategories.AddRangeAsync(rootCategories);
        await _context.SaveChangesAsync();

        // Set parent relationships and add child categories
        foreach (var child in childCategories)
        {
            var parentCode = child.CategoryCode.Contains("-")
                ? child.CategoryCode.Split('-')[0]
                : null;

            if (parentCode != null)
            {
                var parent = rootCategories.FirstOrDefault(c => c.CategoryCode == parentCode);
                if (parent != null)
                {
                    child.ParentCategoryId = parent.Id;
                }
            }
        }

        await _context.ArchiveCategories.AddRangeAsync(childCategories);
        await _context.SaveChangesAsync();
    }
}
