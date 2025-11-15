using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.Data.Seeds;

public static class PermissionSeeder
{
    public static List<Permission> GetPermissions()
    {
        var permissions = new List<Permission>();
        int id = 1;

        // Messages Module
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "messages.read", DisplayName = "View Messages", Description = "Can view and read messages", Module = "Messages", Category = "Read", IsActive = true },
            new Permission { Id = id++, Name = "messages.write", DisplayName = "Send Messages", Description = "Can send and compose messages", Module = "Messages", Category = "Write", IsActive = true },
            new Permission { Id = id++, Name = "messages.delete", DisplayName = "Delete Messages", Description = "Can delete messages", Module = "Messages", Category = "Delete", IsActive = true },
            new Permission { Id = id++, Name = "messages.manage.folders", DisplayName = "Manage Folders", Description = "Can create and manage message folders", Module = "Messages", Category = "Manage", IsActive = true },
            new Permission { Id = id++, Name = "messages.manage.categories", DisplayName = "Manage Categories", Description = "Can create and manage message categories", Module = "Messages", Category = "Manage", IsActive = true },
            new Permission { Id = id++, Name = "messages.manage.rules", DisplayName = "Manage Rules", Description = "Can create and manage message rules", Module = "Messages", Category = "Manage", IsActive = true },
        });

        // Calendar Module
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "calendar.read", DisplayName = "View Calendar", Description = "Can view calendar and events", Module = "Calendar", Category = "Read", IsActive = true },
            new Permission { Id = id++, Name = "calendar.write", DisplayName = "Create Events", Description = "Can create and edit calendar events", Module = "Calendar", Category = "Write", IsActive = true },
            new Permission { Id = id++, Name = "calendar.delete", DisplayName = "Delete Events", Description = "Can delete calendar events", Module = "Calendar", Category = "Delete", IsActive = true },
            new Permission { Id = id++, Name = "calendar.manage.shared", DisplayName = "Manage Shared Calendars", Description = "Can manage shared calendars", Module = "Calendar", Category = "Manage", IsActive = true },
        });

        // Contacts Module
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "contacts.read", DisplayName = "View Contacts", Description = "Can view contacts", Module = "Contacts", Category = "Read", IsActive = true },
            new Permission { Id = id++, Name = "contacts.write", DisplayName = "Manage Contacts", Description = "Can create and edit contacts", Module = "Contacts", Category = "Write", IsActive = true },
            new Permission { Id = id++, Name = "contacts.delete", DisplayName = "Delete Contacts", Description = "Can delete contacts", Module = "Contacts", Category = "Delete", IsActive = true },
            new Permission { Id = id++, Name = "contacts.manage.groups", DisplayName = "Manage Contact Groups", Description = "Can create and manage contact groups", Module = "Contacts", Category = "Manage", IsActive = true },
        });

        // Video Conference Module
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "videoconference.read", DisplayName = "View Conferences", Description = "Can view video conferences", Module = "VideoConference", Category = "Read", IsActive = true },
            new Permission { Id = id++, Name = "videoconference.join", DisplayName = "Join Conferences", Description = "Can join video conferences", Module = "VideoConference", Category = "Participate", IsActive = true },
            new Permission { Id = id++, Name = "videoconference.create", DisplayName = "Create Conferences", Description = "Can create video conferences", Module = "VideoConference", Category = "Write", IsActive = true },
            new Permission { Id = id++, Name = "videoconference.manage", DisplayName = "Manage Conferences", Description = "Can manage and control video conferences", Module = "VideoConference", Category = "Manage", IsActive = true },
        });

        // Correspondence Module
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "correspondence.read", DisplayName = "View Correspondence", Description = "Can view correspondence documents", Module = "Correspondence", Category = "Read", IsActive = true },
            new Permission { Id = id++, Name = "correspondence.create", DisplayName = "Create Correspondence", Description = "Can create new correspondence documents", Module = "Correspondence", Category = "Write", IsActive = true },
            new Permission { Id = id++, Name = "correspondence.update", DisplayName = "Edit Correspondence", Description = "Can edit correspondence documents", Module = "Correspondence", Category = "Write", IsActive = true },
            new Permission { Id = id++, Name = "correspondence.delete", DisplayName = "Delete Correspondence", Description = "Can delete correspondence documents", Module = "Correspondence", Category = "Delete", IsActive = true },
            new Permission { Id = id++, Name = "correspondence.route", DisplayName = "Route Correspondence", Description = "Can route correspondence to other employees", Module = "Correspondence", Category = "Workflow", IsActive = true },
            new Permission { Id = id++, Name = "correspondence.approve", DisplayName = "Approve Correspondence", Description = "Can approve correspondence documents", Module = "Correspondence", Category = "Workflow", IsActive = true },
            new Permission { Id = id++, Name = "correspondence.respond", DisplayName = "Respond to Routing", Description = "Can respond to routed correspondence", Module = "Correspondence", Category = "Workflow", IsActive = true },
        });

        // Archive Module
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "archive.read", DisplayName = "View Archives", Description = "Can view archived documents", Module = "Archive", Category = "Read", IsActive = true },
            new Permission { Id = id++, Name = "archive.manage", DisplayName = "Manage Archives", Description = "Can manage archive categories and documents", Module = "Archive", Category = "Manage", IsActive = true },
            new Permission { Id = id++, Name = "archive.delete", DisplayName = "Delete Archives", Description = "Can delete archived documents", Module = "Archive", Category = "Delete", IsActive = true },
        });

        // Admin Module - Users
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "admin.access", DisplayName = "Access Admin Panel", Description = "Can access the admin panel", Module = "Admin", Category = "Access", IsActive = true },
            new Permission { Id = id++, Name = "admin.users.view", DisplayName = "View Users", Description = "Can view user list", Module = "Admin", Category = "Users", IsActive = true },
            new Permission { Id = id++, Name = "admin.users.create", DisplayName = "Create Users", Description = "Can create new users", Module = "Admin", Category = "Users", IsActive = true },
            new Permission { Id = id++, Name = "admin.users.edit", DisplayName = "Edit Users", Description = "Can edit user information", Module = "Admin", Category = "Users", IsActive = true },
            new Permission { Id = id++, Name = "admin.users.delete", DisplayName = "Delete Users", Description = "Can delete users", Module = "Admin", Category = "Users", IsActive = true },
            new Permission { Id = id++, Name = "admin.users.manage", DisplayName = "Manage Users", Description = "Full user management access", Module = "Admin", Category = "Users", IsActive = true },
            new Permission { Id = id++, Name = "admin.users.lock", DisplayName = "Lock/Unlock Users", Description = "Can lock and unlock user accounts", Module = "Admin", Category = "Users", IsActive = true },
            new Permission { Id = id++, Name = "admin.users.resetpassword", DisplayName = "Reset User Passwords", Description = "Can reset user passwords", Module = "Admin", Category = "Users", IsActive = true },
        });

        // Admin Module - Roles
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "admin.roles.view", DisplayName = "View Roles", Description = "Can view role list", Module = "Admin", Category = "Roles", IsActive = true },
            new Permission { Id = id++, Name = "admin.roles.create", DisplayName = "Create Roles", Description = "Can create new roles", Module = "Admin", Category = "Roles", IsActive = true },
            new Permission { Id = id++, Name = "admin.roles.edit", DisplayName = "Edit Roles", Description = "Can edit role information", Module = "Admin", Category = "Roles", IsActive = true },
            new Permission { Id = id++, Name = "admin.roles.delete", DisplayName = "Delete Roles", Description = "Can delete roles", Module = "Admin", Category = "Roles", IsActive = true },
            new Permission { Id = id++, Name = "admin.roles.manage", DisplayName = "Manage Roles", Description = "Full role management access", Module = "Admin", Category = "Roles", IsActive = true },
            new Permission { Id = id++, Name = "admin.roles.permissions", DisplayName = "Assign Role Permissions", Description = "Can assign permissions to roles", Module = "Admin", Category = "Roles", IsActive = true },
        });

        // Admin Module - Permissions
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "admin.permissions.view", DisplayName = "View Permissions", Description = "Can view permission list", Module = "Admin", Category = "Permissions", IsActive = true },
            new Permission { Id = id++, Name = "admin.permissions.create", DisplayName = "Create Permissions", Description = "Can create new permissions", Module = "Admin", Category = "Permissions", IsActive = true },
            new Permission { Id = id++, Name = "admin.permissions.edit", DisplayName = "Edit Permissions", Description = "Can edit permission information", Module = "Admin", Category = "Permissions", IsActive = true },
            new Permission { Id = id++, Name = "admin.permissions.delete", DisplayName = "Delete Permissions", Description = "Can delete permissions", Module = "Admin", Category = "Permissions", IsActive = true },
            new Permission { Id = id++, Name = "admin.permissions.manage", DisplayName = "Manage Permissions", Description = "Full permission management access", Module = "Admin", Category = "Permissions", IsActive = true },
        });

        // Admin Module - Departments
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "admin.departments.view", DisplayName = "View Departments", Description = "Can view department list", Module = "Admin", Category = "Departments", IsActive = true },
            new Permission { Id = id++, Name = "admin.departments.create", DisplayName = "Create Departments", Description = "Can create new departments", Module = "Admin", Category = "Departments", IsActive = true },
            new Permission { Id = id++, Name = "admin.departments.edit", DisplayName = "Edit Departments", Description = "Can edit department information", Module = "Admin", Category = "Departments", IsActive = true },
            new Permission { Id = id++, Name = "admin.departments.delete", DisplayName = "Delete Departments", Description = "Can delete departments", Module = "Admin", Category = "Departments", IsActive = true },
            new Permission { Id = id++, Name = "admin.departments.manage", DisplayName = "Manage Departments", Description = "Full department management access", Module = "Admin", Category = "Departments", IsActive = true },
        });

        // Admin Module - Employees
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "admin.employees.view", DisplayName = "View Employees", Description = "Can view employee list", Module = "Admin", Category = "Employees", IsActive = true },
            new Permission { Id = id++, Name = "admin.employees.create", DisplayName = "Create Employees", Description = "Can create new employees", Module = "Admin", Category = "Employees", IsActive = true },
            new Permission { Id = id++, Name = "admin.employees.edit", DisplayName = "Edit Employees", Description = "Can edit employee information", Module = "Admin", Category = "Employees", IsActive = true },
            new Permission { Id = id++, Name = "admin.employees.delete", DisplayName = "Delete Employees", Description = "Can delete employees", Module = "Admin", Category = "Employees", IsActive = true },
            new Permission { Id = id++, Name = "admin.employees.manage", DisplayName = "Manage Employees", Description = "Full employee management access", Module = "Admin", Category = "Employees", IsActive = true },
        });

        // Admin Module - Audit Logs
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "admin.auditlogs.view", DisplayName = "View Audit Logs", Description = "Can view audit logs", Module = "Admin", Category = "AuditLogs", IsActive = true },
            new Permission { Id = id++, Name = "admin.auditlogs.export", DisplayName = "Export Audit Logs", Description = "Can export audit logs", Module = "Admin", Category = "AuditLogs", IsActive = true },
        });

        // Admin Module - System
        permissions.AddRange(new[]
        {
            new Permission { Id = id++, Name = "admin.system.settings", DisplayName = "Manage System Settings", Description = "Can manage system settings", Module = "Admin", Category = "System", IsActive = true },
            new Permission { Id = id++, Name = "admin.system.backup", DisplayName = "System Backup", Description = "Can perform system backups", Module = "Admin", Category = "System", IsActive = true },
            new Permission { Id = id++, Name = "admin.system.maintenance", DisplayName = "System Maintenance", Description = "Can perform system maintenance", Module = "Admin", Category = "System", IsActive = true },
        });

        return permissions;
    }

    public static List<(string RoleName, string[] PermissionNames)> GetDefaultRolePermissions()
    {
        return new List<(string RoleName, string[] PermissionNames)>
        {
            // Super Admin - All permissions
            ("SuperAdmin", new[] { "*" }), // Special case for all permissions

            // Admin - All admin permissions
            ("Admin", new[]
            {
                "admin.access",
                "admin.users.manage",
                "admin.roles.manage",
                "admin.permissions.view",
                "admin.departments.manage",
                "admin.employees.manage",
                "admin.auditlogs.view",
                "admin.system.settings"
            }),

            // Manager - Department and employee management
            ("Manager", new[]
            {
                "messages.read",
                "messages.write",
                "calendar.read",
                "calendar.write",
                "contacts.read",
                "contacts.write",
                "videoconference.read",
                "videoconference.join",
                "videoconference.create",
                "correspondence.read",
                "correspondence.create",
                "correspondence.update",
                "correspondence.route",
                "correspondence.approve",
                "admin.employees.view",
                "admin.departments.view"
            }),

            // Employee - Basic user permissions
            ("Employee", new[]
            {
                "messages.read",
                "messages.write",
                "calendar.read",
                "calendar.write",
                "contacts.read",
                "contacts.write",
                "videoconference.read",
                "videoconference.join",
                "correspondence.read",
                "correspondence.create",
                "correspondence.update",
                "correspondence.respond"
            }),

            // Guest - Read-only access
            ("Guest", new[]
            {
                "messages.read",
                "calendar.read",
                "contacts.read",
                "correspondence.read"
            })
        };
    }
}
