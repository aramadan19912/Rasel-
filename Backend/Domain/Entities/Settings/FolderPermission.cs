using Domain.Entities.Organization;
using OutlookInboxManagement.Models;

namespace Domain.Entities.Settings;

/// <summary>
/// Folder permissions management
/// صلاحيات المجلدات
/// </summary>
public class FolderPermission
{
    public int Id { get; set; }

    /// <summary>
    /// Folder name/path
    /// اسم/مسار المجلد
    /// </summary>
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    /// Folder type (Correspondence, Archive, Personal)
    /// نوع المجلد
    /// </summary>
    public string FolderType { get; set; } = string.Empty;

    /// <summary>
    /// Owner employee
    /// مالك المجلد
    /// </summary>
    public int? OwnerEmployeeId { get; set; }
    public Employee? OwnerEmployee { get; set; }

    /// <summary>
    /// Owner department
    /// القسم المالك
    /// </summary>
    public int? OwnerDepartmentId { get; set; }
    public Department? OwnerDepartment { get; set; }

    /// <summary>
    /// User permissions
    /// صلاحيات المستخدم
    /// </summary>
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Employee permissions
    /// صلاحيات الموظف
    /// </summary>
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    /// <summary>
    /// Department permissions (all members)
    /// صلاحيات القسم
    /// </summary>
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>
    /// Can view
    /// يمكنه العرض
    /// </summary>
    public bool CanView { get; set; } = false;

    /// <summary>
    /// Can create
    /// يمكنه الإنشاء
    /// </summary>
    public bool CanCreate { get; set; } = false;

    /// <summary>
    /// Can edit
    /// يمكنه التعديل
    /// </summary>
    public bool CanEdit { get; set; } = false;

    /// <summary>
    /// Can delete
    /// يمكنه الحذف
    /// </summary>
    public bool CanDelete { get; set; } = false;

    /// <summary>
    /// Can download
    /// يمكنه التحميل
    /// </summary>
    public bool CanDownload { get; set; } = false;

    /// <summary>
    /// Can print
    /// يمكنه الطباعة
    /// </summary>
    public bool CanPrint { get; set; } = false;

    /// <summary>
    /// Can share
    /// يمكنه المشاركة
    /// </summary>
    public bool CanShare { get; set; } = false;

    /// <summary>
    /// Can manage permissions
    /// يمكنه إدارة الصلاحيات
    /// </summary>
    public bool CanManagePermissions { get; set; } = false;

    /// <summary>
    /// Is inherited from parent
    /// موروث من الأب
    /// </summary>
    public bool IsInherited { get; set; } = false;

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Classification permissions
/// صلاحيات التصنيفات
/// </summary>
public class ClassificationPermission
{
    public int Id { get; set; }

    /// <summary>
    /// Archive category
    /// تصنيف الأرشيف
    /// </summary>
    public int CategoryId { get; set; }
    public Domain.Entities.Archive.ArchiveCategory Category { get; set; } = null!;

    /// <summary>
    /// User permissions
    /// صلاحيات المستخدم
    /// </summary>
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    /// <summary>
    /// Employee permissions
    /// صلاحيات الموظف
    /// </summary>
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    /// <summary>
    /// Department permissions
    /// صلاحيات القسم
    /// </summary>
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>
    /// Can view
    /// يمكنه العرض
    /// </summary>
    public bool CanView { get; set; } = false;

    /// <summary>
    /// Can create
    /// يمكنه الإنشاء
    /// </summary>
    public bool CanCreate { get; set; } = false;

    /// <summary>
    /// Can edit
    /// يمكنه التعديل
    /// </summary>
    public bool CanEdit { get; set; } = false;

    /// <summary>
    /// Can delete
    /// يمكنه الحذف
    /// </summary>
    public bool CanDelete { get; set; } = false;

    /// <summary>
    /// Can archive
    /// يمكنه الأرشفة
    /// </summary>
    public bool CanArchive { get; set; } = false;

    /// <summary>
    /// Can approve
    /// يمكنه الاعتماد
    /// </summary>
    public bool CanApprove { get; set; } = false;

    /// <summary>
    /// Can route
    /// يمكنه الإحالة
    /// </summary>
    public bool CanRoute { get; set; } = false;

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
