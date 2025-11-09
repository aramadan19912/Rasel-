using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.Models;
using Newtonsoft.Json;

namespace OutlookInboxManagement.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets - Messages
    public DbSet<Message> Messages { get; set; }
    public DbSet<MessageFolder> MessageFolders { get; set; }
    public DbSet<MessageCategory> MessageCategories { get; set; }
    public DbSet<MessageRecipient> MessageRecipients { get; set; }
    public DbSet<MessageAttachment> MessageAttachments { get; set; }
    public DbSet<MessageMention> MessageMentions { get; set; }
    public DbSet<MessageReaction> MessageReactions { get; set; }
    public DbSet<MessageTracking> MessageTrackings { get; set; }
    public DbSet<ConversationThread> ConversationThreads { get; set; }
    public DbSet<MessageRule> MessageRules { get; set; }

    // DbSets - Calendar
    public DbSet<Calendar> Calendars { get; set; }
    public DbSet<CalendarShare> CalendarShares { get; set; }
    public DbSet<CalendarEvent> CalendarEvents { get; set; }
    public DbSet<EventAttendee> EventAttendees { get; set; }
    public DbSet<EventReminder> EventReminders { get; set; }
    public DbSet<EventResource> EventResources { get; set; }
    public DbSet<Resource> Resources { get; set; }
    public DbSet<EventAttachment> EventAttachments { get; set; }

    // DbSets - Contacts
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Message configuration
        builder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MessageId).IsUnique();
            entity.HasIndex(e => e.ConversationId);
            entity.HasIndex(e => e.SenderId);
            entity.HasIndex(e => e.ReceivedAt);
            entity.HasIndex(e => e.IsRead);
            entity.HasIndex(e => e.IsFlagged);

            // Relationships
            entity.HasOne(e => e.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(e => e.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Folder)
                .WithMany(f => f.Messages)
                .HasForeignKey(e => e.FolderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ParentMessage)
                .WithMany(m => m.Replies)
                .HasForeignKey(e => e.ParentMessageId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.ToRecipients)
                .WithOne(r => r.Message)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Attachments)
                .WithOne(a => a.Message)
                .HasForeignKey(a => a.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Mentions)
                .WithOne(m => m.Message)
                .HasForeignKey(m => m.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Reactions)
                .WithOne(r => r.Message)
                .HasForeignKey(r => r.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TrackingInfo)
                .WithOne(t => t.Message)
                .HasForeignKey(t => t.MessageId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many with Categories
            entity.HasMany(e => e.Categories)
                .WithMany(c => c.Messages)
                .UsingEntity(j => j.ToTable("MessageCategoryMappings"));

            // JSON column conversions
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());

            entity.Property(e => e.CustomProperties)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v) ?? new Dictionary<string, string>());

            entity.Property(e => e.InternetMessageHeaders)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());

            // Text columns
            entity.Property(e => e.Subject).HasMaxLength(500);
            entity.Property(e => e.BodyPreview).HasMaxLength(1000);
        });

        // MessageFolder configuration
        builder.Entity<MessageFolder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Type });
            entity.HasIndex(e => e.DisplayOrder);

            entity.HasOne(e => e.ParentFolder)
                .WithMany(f => f.SubFolders)
                .HasForeignKey(e => e.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
        });

        // MessageCategory configuration
        builder.Entity<MessageCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(20);
        });

        // MessageRecipient configuration
        builder.Entity<MessageRecipient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => e.Email);

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.DisplayName).HasMaxLength(256);
        });

        // MessageAttachment configuration
        builder.Entity<MessageAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MessageId);

            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
        });

        // MessageMention configuration
        builder.Entity<MessageMention>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => e.MentionedUserId);

            entity.HasOne(e => e.MentionedUser)
                .WithMany()
                .HasForeignKey(e => e.MentionedUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MessageReaction configuration
        builder.Entity<MessageReaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.MessageId, e.UserId, e.ReactionType });

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // MessageTracking configuration
        builder.Entity<MessageTracking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.MessageId);
            entity.HasIndex(e => e.RecipientEmail);

            entity.Property(e => e.RecipientEmail).HasMaxLength(256);
        });

        // ConversationThread configuration
        builder.Entity<ConversationThread>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConversationId).IsUnique();

            entity.Property(e => e.Topic).HasMaxLength(500);

            entity.Property(e => e.Participants)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());
        });

        // MessageRule configuration
        builder.Entity<MessageRule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.Priority });

            entity.Property(e => e.Name).HasMaxLength(200);
        });

        // Calendar configuration
        builder.Entity<Calendar>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsDefault });

            entity.HasOne(e => e.User)
                .WithMany(u => u.Calendars)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Events)
                .WithOne(ev => ev.Calendar)
                .HasForeignKey(ev => ev.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Shares)
                .WithOne(s => s.Calendar)
                .HasForeignKey(s => s.CalendarId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Color).HasMaxLength(20);
        });

        // CalendarShare configuration
        builder.Entity<CalendarShare>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CalendarId, e.SharedWithUserId });

            entity.HasOne(e => e.SharedWithUser)
                .WithMany()
                .HasForeignKey(e => e.SharedWithUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // CalendarEvent configuration
        builder.Entity<CalendarEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EventId).IsUnique();
            entity.HasIndex(e => e.CalendarId);
            entity.HasIndex(e => e.OrganizerId);
            entity.HasIndex(e => e.StartDateTime);
            entity.HasIndex(e => e.EndDateTime);
            entity.HasIndex(e => new { e.CalendarId, e.StartDateTime, e.EndDateTime });

            entity.HasOne(e => e.Organizer)
                .WithMany()
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Attendees)
                .WithOne(a => a.Event)
                .HasForeignKey(a => a.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Reminders)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Resources)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Attachments)
                .WithOne(a => a.Event)
                .HasForeignKey(a => a.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // JSON column conversions
            entity.Property(e => e.Categories)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());

            entity.Property(e => e.RecurrenceExceptions)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<DateTime>>(v) ?? new List<DateTime>());

            entity.Property(e => e.Title).HasMaxLength(500);
            entity.Property(e => e.Location).HasMaxLength(500);
        });

        // EventAttendee configuration
        builder.Entity<EventAttendee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EventId);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.DisplayName).HasMaxLength(256);
        });

        // EventReminder configuration
        builder.Entity<EventReminder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EventId);
            entity.HasIndex(e => new { e.IsTriggered, e.TriggeredAt });
        });

        // Resource configuration
        builder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.IsAvailable);

            entity.HasMany(e => e.EventResources)
                .WithOne(er => er.Resource)
                .HasForeignKey(er => er.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Equipment)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());

            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        // EventResource configuration
        builder.Entity<EventResource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.EventId, e.ResourceId });
            entity.HasIndex(e => e.ResourceId);
        });

        // EventAttachment configuration
        builder.Entity<EventAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EventId);

            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.ContentType).HasMaxLength(200);
            entity.Property(e => e.FilePath).HasMaxLength(1000);
        });

        // Contact configuration
        builder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContactId).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.IsFavorite });
            entity.HasIndex(e => new { e.UserId, e.LastName, e.FirstName });

            entity.HasOne(e => e.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.EmailAddresses)
                .WithOne(ea => ea.Contact)
                .HasForeignKey(ea => ea.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.PhoneNumbers)
                .WithOne(p => p.Contact)
                .HasForeignKey(p => p.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Addresses)
                .WithOne(a => a.Contact)
                .HasForeignKey(a => a.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Websites)
                .WithOne(w => w.Contact)
                .HasForeignKey(w => w.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.CustomFields)
                .WithOne(cf => cf.Contact)
                .HasForeignKey(cf => cf.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.GroupMemberships)
                .WithOne(gm => gm.Contact)
                .HasForeignKey(gm => gm.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            // JSON conversions
            entity.Property(e => e.Categories)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());

            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<List<string>>(v) ?? new List<string>());

            entity.Property(e => e.DisplayName).HasMaxLength(500);
            entity.Property(e => e.Company).HasMaxLength(200);
        });

        // ContactEmail configuration
        builder.Entity<ContactEmail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContactId);
            entity.HasIndex(e => e.Email);
            entity.Property(e => e.Email).HasMaxLength(256);
        });

        // ContactPhone configuration
        builder.Entity<ContactPhone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContactId);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
        });

        // ContactAddress configuration
        builder.Entity<ContactAddress>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContactId);
        });

        // ContactGroup configuration
        builder.Entity<ContactGroup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.GroupId).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.ContactGroups)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Name).HasMaxLength(200);
        });

        // ContactGroupMembership configuration
        builder.Entity<ContactGroupMembership>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ContactId, e.GroupId }).IsUnique();
        });

        // ContactInteraction configuration
        builder.Entity<ContactInteraction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContactId);
            entity.HasIndex(e => e.InteractionDate);

            entity.HasOne(e => e.Contact)
                .WithMany()
                .HasForeignKey(e => e.ContactId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed default folders
        SeedDefaultData(builder);
    }

    private void SeedDefaultData(ModelBuilder builder)
    {
        // Seed default categories colors
        var defaultCategories = new[]
        {
            new { Id = 1, Name = "Red", Color = "#FF0000", UserId = "", IsDefault = true },
            new { Id = 2, Name = "Orange", Color = "#FFA500", UserId = "", IsDefault = true },
            new { Id = 3, Name = "Yellow", Color = "#FFFF00", UserId = "", IsDefault = true },
            new { Id = 4, Name = "Green", Color = "#00FF00", UserId = "", IsDefault = true },
            new { Id = 5, Name = "Blue", Color = "#0000FF", UserId = "", IsDefault = true },
            new { Id = 6, Name = "Purple", Color = "#800080", UserId = "", IsDefault = true }
        };

        // Note: Actual user-specific folders will be created during user registration
    }
}
