using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Backend.Domain.Entities.Identity;
using Backend.Domain.Entities.Messages;
using Backend.Domain.Entities.Calendar;
using Backend.Domain.Entities.Contacts;
using Backend.Domain.Entities.VideoConference;
using Domain.Entities.Organization;
using Domain.Entities.Archive;
using Domain.Entities.Communication;
using Domain.Entities.Settings;

namespace Backend.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, Role, string, UserClaim, UserRole, IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Identity
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Messages
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageFolder> MessageFolders { get; set; }
    public DbSet<MessageCategory> MessageCategories { get; set; }
    public DbSet<MessageRule> MessageRules { get; set; }
    public DbSet<MessageAttachment> MessageAttachments { get; set; }
    public DbSet<MessageRecipient> MessageRecipients { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }
    public DbSet<MessageMention> MessageMentions { get; set; }
    public DbSet<MessageTracking> MessageTrackings { get; set; }
    public DbSet<ConversationThread> ConversationThreads { get; set; }

    // Calendar
    public DbSet<Backend.Domain.Entities.Calendar.Calendar> Calendars { get; set; }
    public DbSet<CalendarEvent> CalendarEvents { get; set; }
    public DbSet<EventAttendee> EventAttendees { get; set; }
    public DbSet<EventReminder> EventReminders { get; set; }
    public DbSet<EventAttachment> EventAttachments { get; set; }
    public DbSet<EventResource> EventResources { get; set; }
    public DbSet<CalendarShare> CalendarShares { get; set; }

    // Contacts
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<ContactEmail> ContactEmails { get; set; }
    public DbSet<ContactPhone> ContactPhones { get; set; }
    public DbSet<ContactAddress> ContactAddresses { get; set; }
    public DbSet<ContactWebsite> ContactWebsites { get; set; }
    public DbSet<ContactCustomField> ContactCustomFields { get; set; }
    public DbSet<ContactRelationship> ContactRelationships { get; set; }
    public DbSet<ContactGroup> ContactGroups { get; set; }
    public DbSet<ContactGroupMembership> ContactGroupMemberships { get; set; }
    public DbSet<ContactInteraction> ContactInteractions { get; set; }

    // Video Conference
    public DbSet<VideoConference> VideoConferences { get; set; }
    public DbSet<ConferenceParticipant> ConferenceParticipants { get; set; }
    public DbSet<BreakoutRoom> BreakoutRooms { get; set; }
    public DbSet<ConferenceChatMessage> ConferenceChatMessages { get; set; }
    public DbSet<ChatAttachment> ChatAttachments { get; set; }
    public DbSet<WhiteboardData> WhiteboardData { get; set; }

    // Organization
    public DbSet<Department> Departments { get; set; }
    public DbSet<Position> Positions { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeeSkill> EmployeeSkills { get; set; }
    public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
    public DbSet<PerformanceReview> PerformanceReviews { get; set; }
    public DbSet<OrganizationInfo> OrganizationInfos { get; set; }
    public DbSet<LicenseInfo> LicenseInfos { get; set; }
    public DbSet<DigitalSignature> DigitalSignatures { get; set; }

    // Archive & Correspondence
    public DbSet<ArchiveCategory> ArchiveCategories { get; set; }
    public DbSet<Correspondence> Correspondences { get; set; }
    public DbSet<CorrespondenceAttachment> CorrespondenceAttachments { get; set; }
    public DbSet<CorrespondenceRouting> CorrespondenceRoutings { get; set; }
    public DbSet<CorrespondenceForm> CorrespondenceForms { get; set; }
    public DbSet<FormField> FormFields { get; set; }
    public DbSet<FormSubmission> FormSubmissions { get; set; }
    public DbSet<ArchiveDocument> ArchiveDocuments { get; set; }
    public DbSet<CorrespondenceTracking> CorrespondenceTrackings { get; set; }
    public DbSet<CorrespondenceCirculation> CorrespondenceCirculations { get; set; }
    public DbSet<CirculationRecipient> CirculationRecipients { get; set; }

    // Communication
    public DbSet<CommunicationGroup> CommunicationGroups { get; set; }
    public DbSet<CommunicationGroupMember> CommunicationGroupMembers { get; set; }
    public DbSet<CommunicationRule> CommunicationRules { get; set; }
    public DbSet<ExternalEntity> ExternalEntities { get; set; }

    // Settings
    public DbSet<SystemLookup> SystemLookups { get; set; }
    public DbSet<PrintTemplate> PrintTemplates { get; set; }
    public DbSet<FolderPermission> FolderPermissions { get; set; }
    public DbSet<ClassificationPermission> ClassificationPermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity entities
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Bio).HasMaxLength(500);
        });

        builder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasIndex(e => e.Name).IsUnique();
        });

        builder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserClaim>(entity =>
        {
            entity.ToTable("UserClaims");
            entity.HasOne(uc => uc.User)
                .WithMany(u => u.UserClaims)
                .HasForeignKey(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Permission configuration
        builder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Module).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Resource).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<UserPermission>(entity =>
        {
            entity.ToTable("UserPermissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.PermissionId }).IsUnique();

            entity.HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.ExpiresAt });

            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreatedByIp).HasMaxLength(50).IsRequired();
        });

        // ===== Messages Configuration =====
        builder.Entity<Message>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasIndex(e => e.MessageId).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => new { e.UserId, e.IsRead });
            entity.HasIndex(e => new { e.UserId, e.IsFlagged });

            entity.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Folder)
                .WithMany(f => f.Messages)
                .HasForeignKey(m => m.FolderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(m => m.Attachments)
                .WithOne(a => a.Message)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MessageFolder>(entity =>
        {
            entity.ToTable("MessageFolders");
            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();

            entity.HasOne(f => f.ParentFolder)
                .WithMany(f => f.SubFolders)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MessageCategory>(entity =>
        {
            entity.ToTable("MessageCategories");
            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
        });

        // ===== Calendar Configuration =====
        builder.Entity<Backend.Domain.Entities.Calendar.Calendar>(entity =>
        {
            entity.ToTable("Calendars");
            entity.HasIndex(e => new { e.UserId, e.Name });

            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Events)
                .WithOne(e => e.Calendar)
                .HasForeignKey(e => e.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CalendarEvent>(entity =>
        {
            entity.ToTable("CalendarEvents");
            entity.HasIndex(e => e.EventId).IsUnique();
            entity.HasIndex(e => new { e.OrganizerUserId, e.StartTime });
            entity.HasIndex(e => new { e.CalendarId, e.StartTime });

            entity.HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e => e.OrganizerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Attendees)
                .WithOne(a => a.Event)
                .HasForeignKey(a => a.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Reminders)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EventAttendee>(entity =>
        {
            entity.ToTable("EventAttendees");
            entity.HasIndex(e => new { e.EventId, e.Email });

            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== Contacts Configuration =====
        builder.Entity<Contact>(entity =>
        {
            entity.ToTable("Contacts");
            entity.HasIndex(e => e.ContactId).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.LastName, e.FirstName });
            entity.HasIndex(e => new { e.UserId, e.IsFavorite });

            entity.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.EmailAddresses)
                .WithOne(e => e.Contact)
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.PhoneNumbers)
                .WithOne(p => p.Contact)
                .HasForeignKey(p => p.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Addresses)
                .WithOne(a => a.Contact)
                .HasForeignKey(a => a.ContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ContactGroup>(entity =>
        {
            entity.ToTable("ContactGroups");
            entity.HasIndex(e => e.GroupId).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();

            entity.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ContactGroupMembership>(entity =>
        {
            entity.ToTable("ContactGroupMemberships");
            entity.HasIndex(e => new { e.GroupId, e.ContactId }).IsUnique();

            entity.HasOne(m => m.Contact)
                .WithMany(c => c.GroupMemberships)
                .HasForeignKey(m => m.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Group)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== Video Conference Configuration =====
        builder.Entity<VideoConference>(entity =>
        {
            entity.ToTable("VideoConferences");
            entity.HasIndex(e => e.ConferenceId).IsUnique();
            entity.HasIndex(e => new { e.HostId, e.ScheduledStartTime });
            entity.HasIndex(e => new { e.Status, e.ScheduledStartTime });

            entity.HasOne(v => v.Host)
                .WithMany()
                .HasForeignKey(v => v.HostId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.CalendarEvent)
                .WithMany()
                .HasForeignKey(v => v.CalendarEventId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(v => v.Participants)
                .WithOne(p => p.Conference)
                .HasForeignKey(p => p.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(v => v.BreakoutRooms)
                .WithOne(b => b.Conference)
                .HasForeignKey(b => b.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(v => v.ChatMessages)
                .WithOne(c => c.Conference)
                .HasForeignKey(c => c.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Whiteboard)
                .WithOne(w => w.Conference)
                .HasForeignKey<WhiteboardData>(w => w.ConferenceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ConferenceParticipant>(entity =>
        {
            entity.ToTable("ConferenceParticipants");
            entity.HasIndex(e => new { e.ConferenceId, e.Email });
            entity.HasIndex(e => e.PeerId).IsUnique();

            entity.HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(p => p.BreakoutRoom)
                .WithMany(b => b.Participants)
                .HasForeignKey(p => p.BreakoutRoomId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<ConferenceChatMessage>(entity =>
        {
            entity.ToTable("ConferenceChatMessages");
            entity.HasIndex(e => new { e.ConferenceId, e.SentAt });

            entity.HasOne(c => c.Sender)
                .WithMany()
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(c => c.Attachments)
                .WithOne(a => a.ChatMessage)
                .HasForeignKey(a => a.ChatMessageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== Organization Configuration =====
        builder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.DepartmentCode).IsUnique();
            entity.HasIndex(e => new { e.Name, e.IsDeleted });

            entity.Property(e => e.DepartmentCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Mission).HasMaxLength(1000);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.OfficeNumber).HasMaxLength(20);
            entity.Property(e => e.CostCenter).HasMaxLength(50);
            entity.Property(e => e.AnnualBudget).HasPrecision(18, 2);

            // Self-referencing relationship
            entity.HasOne(d => d.ParentDepartment)
                .WithMany(d => d.SubDepartments)
                .HasForeignKey(d => d.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Head of Department relationship
            entity.HasOne(d => d.HeadOfDepartment)
                .WithMany()
                .HasForeignKey(d => d.HeadOfDepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Employees relationship
            entity.HasMany(d => d.Employees)
                .WithOne(e => e.Department)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Position>(entity =>
        {
            entity.ToTable("Positions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PositionCode).IsUnique();
            entity.HasIndex(e => new { e.DepartmentId, e.Title });
            entity.HasIndex(e => e.Level);

            entity.Property(e => e.PositionCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.MinSalary).HasPrecision(18, 2);
            entity.Property(e => e.MaxSalary).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasMaxLength(3);

            // Department relationship
            entity.HasOne(p => p.Department)
                .WithMany()
                .HasForeignKey(p => p.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing relationship
            entity.HasOne(p => p.ReportsToPosition)
                .WithMany(p => p.SubordinatePositions)
                .HasForeignKey(p => p.ReportsToPositionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Employees relationship
            entity.HasMany(p => p.Employees)
                .WithOne(e => e.Position)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EmployeeNumber).IsUnique();
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => new { e.DepartmentId, e.IsDeleted });
            entity.HasIndex(e => new { e.ManagerId, e.IsDeleted });
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => new { e.LastName, e.FirstName });

            entity.Property(e => e.EmployeeNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.MiddleName).HasMaxLength(50);
            entity.Property(e => e.PreferredName).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.WorkPhone).HasMaxLength(20);
            entity.Property(e => e.MobilePhone).HasMaxLength(20);
            entity.Property(e => e.EmploymentStatus).HasMaxLength(20).IsRequired();
            entity.Property(e => e.EmploymentType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.OfficeLocation).HasMaxLength(100);
            entity.Property(e => e.WorkSite).HasMaxLength(100);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
            entity.Property(e => e.EmergencyContactRelation).HasMaxLength(50);
            entity.Property(e => e.Nationality).HasMaxLength(50);
            entity.Property(e => e.NationalId).HasMaxLength(50);
            entity.Property(e => e.PassportNumber).HasMaxLength(50);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.CurrentSalary).HasPrecision(18, 2);
            entity.Property(e => e.SalaryCurrency).HasMaxLength(3);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            // User relationship
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing Manager relationship
            entity.HasOne(e => e.Manager)
                .WithMany(e => e.DirectReports)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Skills relationship
            entity.HasMany(e => e.Skills)
                .WithOne(s => s.Employee)
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Documents relationship
            entity.HasMany(e => e.Documents)
                .WithOne(d => d.Employee)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Performance Reviews relationship
            entity.HasMany(e => e.PerformanceReviews)
                .WithOne(pr => pr.Employee)
                .HasForeignKey(pr => pr.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<EmployeeSkill>(entity =>
        {
            entity.ToTable("EmployeeSkills");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.SkillName });

            entity.Property(e => e.SkillName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SkillCategory).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProficiencyLevel).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
        });

        builder.Entity<EmployeeDocument>(entity =>
        {
            entity.ToTable("EmployeeDocuments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.DocumentType });
            entity.HasIndex(e => e.ExpiryDate);

            entity.Property(e => e.DocumentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DocumentName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.DocumentUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FileSize).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.UploadedBy).HasMaxLength(450).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        builder.Entity<PerformanceReview>(entity =>
        {
            entity.ToTable("PerformanceReviews");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EmployeeId, e.ReviewDate });
            entity.HasIndex(e => new { e.ReviewerId, e.ReviewDate });

            entity.Property(e => e.ReviewPeriodStart).IsRequired();
            entity.Property(e => e.ReviewPeriodEnd).IsRequired();
            entity.Property(e => e.ReviewDate).IsRequired();
            entity.Property(e => e.ReviewerId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.OverallRating).IsRequired();
            entity.Property(e => e.Strengths).HasMaxLength(2000);
            entity.Property(e => e.AreasForImprovement).HasMaxLength(2000);
            entity.Property(e => e.Goals).HasMaxLength(2000);
            entity.Property(e => e.ReviewerComments).HasMaxLength(2000);
            entity.Property(e => e.EmployeeComments).HasMaxLength(2000);

            // Reviewer relationship
            entity.HasOne(pr => pr.Reviewer)
                .WithMany()
                .HasForeignKey(pr => pr.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===== Archive & Correspondence Configuration =====
        builder.Entity<ArchiveCategory>(entity =>
        {
            entity.ToTable("ArchiveCategories");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CategoryCode).IsUnique();
            entity.HasIndex(e => new { e.Classification, e.IsActive });
            entity.HasIndex(e => new { e.NameAr, e.NameEn });

            entity.Property(e => e.CategoryCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Classification).HasMaxLength(50).IsRequired();
            entity.Property(e => e.RetentionPeriod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasMaxLength(450);
            entity.Property(e => e.UpdatedBy).HasMaxLength(450);

            // Self-referencing relationship
            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Correspondence>(entity =>
        {
            entity.ToTable("Correspondences");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReferenceNumber).IsUnique();
            entity.HasIndex(e => new { e.CategoryId, e.Status });
            entity.HasIndex(e => new { e.CorrespondenceDate, e.IsArchived });
            entity.HasIndex(e => e.FromEmployeeId);
            entity.HasIndex(e => e.ToEmployeeId);
            entity.HasIndex(e => e.ToDepartmentId);
            entity.HasIndex(e => e.FormSubmissionId);
            entity.HasIndex(e => e.IsDeleted);

            entity.Property(e => e.ReferenceNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SubjectAr).HasMaxLength(500).IsRequired();
            entity.Property(e => e.SubjectEn).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ConfidentialityLevel).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ExternalSenderName).HasMaxLength(200);
            entity.Property(e => e.ExternalSenderOrganization).HasMaxLength(200);
            entity.Property(e => e.Keywords).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(450);
            entity.Property(e => e.ArchivedBy).HasMaxLength(450);

            // Relationships
            entity.HasOne(c => c.Category)
                .WithMany(cat => cat.Correspondences)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.FromEmployee)
                .WithMany()
                .HasForeignKey(c => c.FromEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.ToEmployee)
                .WithMany()
                .HasForeignKey(c => c.ToEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.ToDepartment)
                .WithMany()
                .HasForeignKey(c => c.ToDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Form)
                .WithMany()
                .HasForeignKey(c => c.FormId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.FormSubmission)
                .WithOne(fs => fs.Correspondence)
                .HasForeignKey<Correspondence>(c => c.FormSubmissionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.RelatedCorrespondence)
                .WithMany()
                .HasForeignKey(c => c.RelatedCorrespondenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CorrespondenceAttachment>(entity =>
        {
            entity.ToTable("CorrespondenceAttachments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CorrespondenceId);
            entity.HasIndex(e => new { e.CorrespondenceId, e.IsMainDocument });

            entity.Property(e => e.FileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.OriginalFileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.MimeType).HasMaxLength(200).IsRequired();
            entity.Property(e => e.FileExtension).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();

            entity.HasOne(a => a.Correspondence)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.CorrespondenceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CorrespondenceRouting>(entity =>
        {
            entity.ToTable("CorrespondenceRoutings");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CorrespondenceId);
            entity.HasIndex(e => new { e.ToEmployeeId, e.Status });
            entity.HasIndex(e => new { e.RoutedDate, e.DueDate });
            entity.HasIndex(e => e.ParentRoutingId);

            entity.Property(e => e.Action).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Priority).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();

            entity.HasOne(r => r.Correspondence)
                .WithMany(c => c.Routings)
                .HasForeignKey(r => r.CorrespondenceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.FromEmployee)
                .WithMany()
                .HasForeignKey(r => r.FromEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ToEmployee)
                .WithMany()
                .HasForeignKey(r => r.ToEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ToDepartment)
                .WithMany()
                .HasForeignKey(r => r.ToDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.ParentRouting)
                .WithMany()
                .HasForeignKey(r => r.ParentRoutingId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CorrespondenceForm>(entity =>
        {
            entity.ToTable("CorrespondenceForms");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FormCode).IsUnique();
            entity.HasIndex(e => new { e.CategoryId, e.IsActive });
            entity.HasIndex(e => new { e.IsPublished, e.IsActive });

            entity.Property(e => e.FormCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.NameAr).HasMaxLength(200).IsRequired();
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.DescriptionAr).HasMaxLength(1000);
            entity.Property(e => e.DescriptionEn).HasMaxLength(1000);
            entity.Property(e => e.DefaultClassification).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();
            entity.Property(e => e.UpdatedBy).HasMaxLength(450);

            entity.HasOne(f => f.Category)
                .WithMany()
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<FormField>(entity =>
        {
            entity.ToTable("FormFields");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.FormId, e.SortOrder });
            entity.HasIndex(e => new { e.FormId, e.FieldName });

            entity.Property(e => e.FieldName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LabelAr).HasMaxLength(200).IsRequired();
            entity.Property(e => e.LabelEn).HasMaxLength(200);
            entity.Property(e => e.FieldType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Placeholder).HasMaxLength(500);
            entity.Property(e => e.HelpText).HasMaxLength(1000);
            entity.Property(e => e.CssClass).HasMaxLength(200);

            entity.HasOne(f => f.Form)
                .WithMany(form => form.Fields)
                .HasForeignKey(f => f.FormId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<FormSubmission>(entity =>
        {
            entity.ToTable("FormSubmissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ReferenceNumber).IsUnique();
            entity.HasIndex(e => new { e.FormId, e.SubmissionDate });
            entity.HasIndex(e => new { e.Status, e.IsApproved });
            entity.HasIndex(e => e.SubmittedByEmployeeId);

            entity.Property(e => e.ReferenceNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.SubmittedByUserId).HasMaxLength(450);
            entity.Property(e => e.SubmitterName).HasMaxLength(200);
            entity.Property(e => e.SubmitterEmail).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50).IsRequired();
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            entity.HasOne(s => s.Form)
                .WithMany(f => f.Submissions)
                .HasForeignKey(s => s.FormId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.SubmittedByEmployee)
                .WithMany()
                .HasForeignKey(s => s.SubmittedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.ApprovedByEmployee)
                .WithMany()
                .HasForeignKey(s => s.ApprovedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ArchiveDocument>(entity =>
        {
            entity.ToTable("ArchiveDocuments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ArchiveNumber).IsUnique();
            entity.HasIndex(e => e.CorrespondenceId).IsUnique();
            entity.HasIndex(e => new { e.IsOnLegalHold, e.IsDestroyed });
            entity.HasIndex(e => e.DestructionDate);

            entity.Property(e => e.ArchiveNumber).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PdfFilePath).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.PdfFileName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Checksum).HasMaxLength(128).IsRequired();
            entity.Property(e => e.RetentionPeriod).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LegalHoldReason).HasMaxLength(1000);
            entity.Property(e => e.StorageLocation).HasMaxLength(500);
            entity.Property(e => e.BackupLocation).HasMaxLength(500);
            entity.Property(e => e.DestroyedBy).HasMaxLength(450);
            entity.Property(e => e.CreatedBy).HasMaxLength(450).IsRequired();

            entity.HasOne(d => d.Correspondence)
                .WithOne(c => c.ArchivedDocument)
                .HasForeignKey<ArchiveDocument>(d => d.CorrespondenceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
