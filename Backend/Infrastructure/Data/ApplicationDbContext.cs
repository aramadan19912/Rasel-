using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Backend.Domain.Entities.Identity;
using Backend.Domain.Entities.Messages;
using Backend.Domain.Entities.Calendar;
using Backend.Domain.Entities.Contacts;
using Backend.Domain.Entities.VideoConference;

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
    }
}
