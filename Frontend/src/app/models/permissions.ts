// System Permissions Constants
export class Permissions {
  // Messages Module
  static readonly MESSAGES_READ = 'messages.read';
  static readonly MESSAGES_WRITE = 'messages.write';
  static readonly MESSAGES_DELETE = 'messages.delete';
  static readonly MESSAGES_MANAGE_FOLDERS = 'messages.manage.folders';
  static readonly MESSAGES_MANAGE_CATEGORIES = 'messages.manage.categories';
  static readonly MESSAGES_MANAGE_RULES = 'messages.manage.rules';

  // Calendar Module
  static readonly CALENDAR_READ = 'calendar.read';
  static readonly CALENDAR_WRITE = 'calendar.write';
  static readonly CALENDAR_DELETE = 'calendar.delete';
  static readonly CALENDAR_MANAGE_SHARED = 'calendar.manage.shared';

  // Contacts Module
  static readonly CONTACTS_READ = 'contacts.read';
  static readonly CONTACTS_WRITE = 'contacts.write';
  static readonly CONTACTS_DELETE = 'contacts.delete';
  static readonly CONTACTS_MANAGE_GROUPS = 'contacts.manage.groups';

  // Video Conference Module
  static readonly VIDEOCONFERENCE_READ = 'videoconference.read';
  static readonly VIDEOCONFERENCE_JOIN = 'videoconference.join';
  static readonly VIDEOCONFERENCE_CREATE = 'videoconference.create';
  static readonly VIDEOCONFERENCE_MANAGE = 'videoconference.manage';

  // Correspondence Module
  static readonly CORRESPONDENCE_READ = 'correspondence.read';
  static readonly CORRESPONDENCE_CREATE = 'correspondence.create';
  static readonly CORRESPONDENCE_UPDATE = 'correspondence.update';
  static readonly CORRESPONDENCE_DELETE = 'correspondence.delete';
  static readonly CORRESPONDENCE_ROUTE = 'correspondence.route';
  static readonly CORRESPONDENCE_APPROVE = 'correspondence.approve';
  static readonly CORRESPONDENCE_RESPOND = 'correspondence.respond';

  // Archive Module
  static readonly ARCHIVE_READ = 'archive.read';
  static readonly ARCHIVE_MANAGE = 'archive.manage';
  static readonly ARCHIVE_DELETE = 'archive.delete';

  // Admin Module - General
  static readonly ADMIN_ACCESS = 'admin.access';

  // Admin Module - Users
  static readonly ADMIN_USERS_VIEW = 'admin.users.view';
  static readonly ADMIN_USERS_CREATE = 'admin.users.create';
  static readonly ADMIN_USERS_EDIT = 'admin.users.edit';
  static readonly ADMIN_USERS_DELETE = 'admin.users.delete';
  static readonly ADMIN_USERS_MANAGE = 'admin.users.manage';
  static readonly ADMIN_USERS_LOCK = 'admin.users.lock';
  static readonly ADMIN_USERS_RESET_PASSWORD = 'admin.users.resetpassword';

  // Admin Module - Roles
  static readonly ADMIN_ROLES_VIEW = 'admin.roles.view';
  static readonly ADMIN_ROLES_CREATE = 'admin.roles.create';
  static readonly ADMIN_ROLES_EDIT = 'admin.roles.edit';
  static readonly ADMIN_ROLES_DELETE = 'admin.roles.delete';
  static readonly ADMIN_ROLES_MANAGE = 'admin.roles.manage';
  static readonly ADMIN_ROLES_PERMISSIONS = 'admin.roles.permissions';

  // Admin Module - Permissions
  static readonly ADMIN_PERMISSIONS_VIEW = 'admin.permissions.view';
  static readonly ADMIN_PERMISSIONS_CREATE = 'admin.permissions.create';
  static readonly ADMIN_PERMISSIONS_EDIT = 'admin.permissions.edit';
  static readonly ADMIN_PERMISSIONS_DELETE = 'admin.permissions.delete';
  static readonly ADMIN_PERMISSIONS_MANAGE = 'admin.permissions.manage';

  // Admin Module - Departments
  static readonly ADMIN_DEPARTMENTS_VIEW = 'admin.departments.view';
  static readonly ADMIN_DEPARTMENTS_CREATE = 'admin.departments.create';
  static readonly ADMIN_DEPARTMENTS_EDIT = 'admin.departments.edit';
  static readonly ADMIN_DEPARTMENTS_DELETE = 'admin.departments.delete';
  static readonly ADMIN_DEPARTMENTS_MANAGE = 'admin.departments.manage';

  // Admin Module - Employees
  static readonly ADMIN_EMPLOYEES_VIEW = 'admin.employees.view';
  static readonly ADMIN_EMPLOYEES_CREATE = 'admin.employees.create';
  static readonly ADMIN_EMPLOYEES_EDIT = 'admin.employees.edit';
  static readonly ADMIN_EMPLOYEES_DELETE = 'admin.employees.delete';
  static readonly ADMIN_EMPLOYEES_MANAGE = 'admin.employees.manage';

  // Admin Module - Audit Logs
  static readonly ADMIN_AUDITLOGS_VIEW = 'admin.auditlogs.view';
  static readonly ADMIN_AUDITLOGS_EXPORT = 'admin.auditlogs.export';

  // Admin Module - System
  static readonly ADMIN_SYSTEM_SETTINGS = 'admin.system.settings';
  static readonly ADMIN_SYSTEM_BACKUP = 'admin.system.backup';
  static readonly ADMIN_SYSTEM_MAINTENANCE = 'admin.system.maintenance';
}

// Helper to group permissions by module
export const PermissionGroups = {
  messages: [
    Permissions.MESSAGES_READ,
    Permissions.MESSAGES_WRITE,
    Permissions.MESSAGES_DELETE,
    Permissions.MESSAGES_MANAGE_FOLDERS,
    Permissions.MESSAGES_MANAGE_CATEGORIES,
    Permissions.MESSAGES_MANAGE_RULES
  ],
  calendar: [
    Permissions.CALENDAR_READ,
    Permissions.CALENDAR_WRITE,
    Permissions.CALENDAR_DELETE,
    Permissions.CALENDAR_MANAGE_SHARED
  ],
  contacts: [
    Permissions.CONTACTS_READ,
    Permissions.CONTACTS_WRITE,
    Permissions.CONTACTS_DELETE,
    Permissions.CONTACTS_MANAGE_GROUPS
  ],
  videoconference: [
    Permissions.VIDEOCONFERENCE_READ,
    Permissions.VIDEOCONFERENCE_JOIN,
    Permissions.VIDEOCONFERENCE_CREATE,
    Permissions.VIDEOCONFERENCE_MANAGE
  ],
  correspondence: [
    Permissions.CORRESPONDENCE_READ,
    Permissions.CORRESPONDENCE_CREATE,
    Permissions.CORRESPONDENCE_UPDATE,
    Permissions.CORRESPONDENCE_DELETE,
    Permissions.CORRESPONDENCE_ROUTE,
    Permissions.CORRESPONDENCE_APPROVE,
    Permissions.CORRESPONDENCE_RESPOND
  ],
  archive: [
    Permissions.ARCHIVE_READ,
    Permissions.ARCHIVE_MANAGE,
    Permissions.ARCHIVE_DELETE
  ],
  admin: [
    Permissions.ADMIN_ACCESS,
    Permissions.ADMIN_USERS_MANAGE,
    Permissions.ADMIN_ROLES_MANAGE,
    Permissions.ADMIN_PERMISSIONS_MANAGE,
    Permissions.ADMIN_DEPARTMENTS_MANAGE,
    Permissions.ADMIN_EMPLOYEES_MANAGE,
    Permissions.ADMIN_AUDITLOGS_VIEW,
    Permissions.ADMIN_SYSTEM_SETTINGS
  ]
};
