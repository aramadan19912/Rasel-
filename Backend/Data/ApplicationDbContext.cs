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

    // DbSets
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
