using Microsoft.AspNetCore.Authorization;

namespace OutlookInboxManagement.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(params string[] permissions)
    {
        Policy = string.Join(",", permissions);
    }
}

public static class PermissionPolicies
{
    // Messages permissions
    public const string MessagesRead = "messages.read";
    public const string MessagesWrite = "messages.write";
    public const string MessagesDelete = "messages.delete";

    // Calendar permissions
    public const string CalendarRead = "calendar.read";
    public const string CalendarWrite = "calendar.write";
    public const string CalendarDelete = "calendar.delete";

    // Contacts permissions
    public const string ContactsRead = "contacts.read";
    public const string ContactsWrite = "contacts.write";
    public const string ContactsDelete = "contacts.delete";

    // Video conference permissions
    public const string VideoConferenceRead = "videoconference.read";
    public const string VideoConferenceJoin = "videoconference.join";
    public const string VideoConferenceCreate = "videoconference.create";
    public const string VideoConferenceManage = "videoconference.manage";

    // Correspondence permissions
    public const string CorrespondenceRead = "correspondence.read";
    public const string CorrespondenceCreate = "correspondence.create";
    public const string CorrespondenceUpdate = "correspondence.update";
    public const string CorrespondenceDelete = "correspondence.delete";
    public const string CorrespondenceRoute = "correspondence.route";
    public const string CorrespondenceApprove = "correspondence.approve";

    // Archive permissions
    public const string ArchiveRead = "archive.read";
    public const string ArchiveManage = "archive.manage";

    // Admin permissions
    public const string AdminAccess = "admin.access";
    public const string UserManage = "admin.users.manage";
    public const string RoleManage = "admin.roles.manage";
    public const string PermissionManage = "admin.permissions.manage";
    public const string DepartmentManage = "admin.departments.manage";
    public const string EmployeeManage = "admin.employees.manage";
    public const string AuditLogView = "admin.auditlogs.view";
    public const string SystemSettings = "admin.system.settings";
}
