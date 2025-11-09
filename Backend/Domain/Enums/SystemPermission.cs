namespace Backend.Domain.Enums;

public static class SystemPermission
{
    // User Management
    public const string UsersCreate = "users.create";
    public const string UsersRead = "users.read";
    public const string UsersUpdate = "users.update";
    public const string UsersDelete = "users.delete";
    public const string UsersManage = "users.manage";

    // Role Management
    public const string RolesCreate = "roles.create";
    public const string RolesRead = "roles.read";
    public const string RolesUpdate = "roles.update";
    public const string RolesDelete = "roles.delete";
    public const string RolesManage = "roles.manage";

    // Permission Management
    public const string PermissionsCreate = "permissions.create";
    public const string PermissionsRead = "permissions.read";
    public const string PermissionsUpdate = "permissions.update";
    public const string PermissionsDelete = "permissions.delete";
    public const string PermissionsManage = "permissions.manage";

    // Messages
    public const string MessagesCreate = "messages.create";
    public const string MessagesRead = "messages.read";
    public const string MessagesUpdate = "messages.update";
    public const string MessagesDelete = "messages.delete";
    public const string MessagesManage = "messages.manage";

    // Calendar
    public const string CalendarCreate = "calendar.create";
    public const string CalendarRead = "calendar.read";
    public const string CalendarUpdate = "calendar.update";
    public const string CalendarDelete = "calendar.delete";
    public const string CalendarManage = "calendar.manage";

    // Contacts
    public const string ContactsCreate = "contacts.create";
    public const string ContactsRead = "contacts.read";
    public const string ContactsUpdate = "contacts.update";
    public const string ContactsDelete = "contacts.delete";
    public const string ContactsManage = "contacts.manage";

    // Video Conference
    public const string ConferenceCreate = "conference.create";
    public const string ConferenceRead = "conference.read";
    public const string ConferenceUpdate = "conference.update";
    public const string ConferenceDelete = "conference.delete";
    public const string ConferenceManage = "conference.manage";
    public const string ConferenceHost = "conference.host";
    public const string ConferenceRecord = "conference.record";
}
