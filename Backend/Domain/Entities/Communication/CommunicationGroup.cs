using Domain.Entities.Organization;

namespace Domain.Entities.Communication;

/// <summary>
/// Communication groups for messaging
/// مجموعات الاتصال
/// </summary>
public class CommunicationGroup
{
    public int Id { get; set; }

    /// <summary>
    /// Group code
    /// كود المجموعة
    /// </summary>
    public string GroupCode { get; set; } = string.Empty;

    /// <summary>
    /// Group name in Arabic
    /// اسم المجموعة بالعربي
    /// </summary>
    public string NameAr { get; set; } = string.Empty;

    /// <summary>
    /// Group name in English
    /// اسم المجموعة بالإنجليزي
    /// </summary>
    public string? NameEn { get; set; }

    /// <summary>
    /// Description
    /// الوصف
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Group type (Department, Project, Custom, All)
    /// نوع المجموعة
    /// </summary>
    public string GroupType { get; set; } = string.Empty;

    /// <summary>
    /// Department (if type is Department)
    /// القسم
    /// </summary>
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    /// <summary>
    /// Group owner/admin
    /// مسؤول المجموعة
    /// </summary>
    public int OwnerId { get; set; }
    public Employee Owner { get; set; } = null!;

    /// <summary>
    /// Group members
    /// أعضاء المجموعة
    /// </summary>
    public ICollection<CommunicationGroupMember> Members { get; set; } = new List<CommunicationGroupMember>();

    /// <summary>
    /// Icon
    /// أيقونة
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Color
    /// لون
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Is public (visible to all)
    /// عامة
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// Allow members to add others
    /// السماح للأعضاء بإضافة آخرين
    /// </summary>
    public bool AllowMembersToAdd { get; set; } = false;

    /// <summary>
    /// Is active
    /// نشطة
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}

/// <summary>
/// Communication group member
/// عضو المجموعة
/// </summary>
public class CommunicationGroupMember
{
    public int Id { get; set; }

    public int GroupId { get; set; }
    public CommunicationGroup Group { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    /// <summary>
    /// Member role (Admin, Moderator, Member)
    /// دور العضو
    /// </summary>
    public string Role { get; set; } = "Member";

    /// <summary>
    /// Can send messages
    /// يمكنه الإرسال
    /// </summary>
    public bool CanSend { get; set; } = true;

    /// <summary>
    /// Can add members
    /// يمكنه إضافة أعضاء
    /// </summary>
    public bool CanAddMembers { get; set; } = false;

    /// <summary>
    /// Joined date
    /// تاريخ الانضمام
    /// </summary>
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Is active
    /// نشط
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}
