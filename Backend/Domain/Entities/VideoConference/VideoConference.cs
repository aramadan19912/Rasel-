using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.Domain.Entities.VideoConference
{
    public class VideoConference
    {
        public int Id { get; set; }

        [Required]
        public string ConferenceId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();

        // Meeting Details
        [Required]
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Password { get; set; }
        public bool RequirePassword { get; set; }

        // Scheduling
        public DateTime ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public int DurationMinutes { get; set; }

        // Host
        [Required]
        public string HostId { get; set; } = string.Empty;
        public ApplicationUser Host { get; set; } = null!;

        // Participants
        public List<ConferenceParticipant> Participants { get; set; } = new();
        public int MaxParticipants { get; set; } = 100;
        public bool AllowGuestsToJoin { get; set; }

        // Status
        public ConferenceStatus Status { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }

        // Features
        public bool EnableChat { get; set; } = true;
        public bool EnableScreenShare { get; set; } = true;
        public bool EnableRecording { get; set; }
        public bool EnableWhiteboard { get; set; } = true;
        public bool EnableBreakoutRooms { get; set; }
        public bool EnableWaitingRoom { get; set; }
        public bool MuteParticipantsOnEntry { get; set; }

        // Recording
        public string? RecordingUrl { get; set; }
        public long RecordingSize { get; set; }
        public bool IsRecording { get; set; }
        public DateTime? RecordingStartedAt { get; set; }

        // Analytics
        public int TotalParticipantsJoined { get; set; }
        public int PeakParticipants { get; set; }
        public TimeSpan TotalDuration { get; set; }

        // Security
        public bool EnableEndToEndEncryption { get; set; }
        public bool LockMeeting { get; set; }

        // Integration
        public int? CalendarEventId { get; set; }
        public CalendarEvent? CalendarEvent { get; set; }
        public int? CorrespondenceId { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }

        // Relationships
        public List<BreakoutRoom> BreakoutRooms { get; set; } = new();
        public List<ConferenceChatMessage> ChatMessages { get; set; } = new();
        public WhiteboardData? Whiteboard { get; set; }
    }

    public enum ConferenceStatus
    {
        Scheduled = 1,
        WaitingRoom = 2,
        InProgress = 3,
        Ended = 4,
        Cancelled = 5
    }

    public class ConferenceParticipant
    {
        public int Id { get; set; }

        public int ConferenceId { get; set; }
        public VideoConference Conference { get; set; } = null!;

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsGuest { get; set; }

        // Status
        public ParticipantStatus Status { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }
        public TimeSpan TotalTime { get; set; }

        // Permissions
        public bool IsCoHost { get; set; }
        public bool CanShareScreen { get; set; } = true;
        public bool CanRecord { get; set; }
        public bool CanUseWhiteboard { get; set; } = true;

        // Media Status
        public bool IsAudioMuted { get; set; }
        public bool IsVideoOff { get; set; }
        public bool IsHandRaised { get; set; }
        public bool IsScreenSharing { get; set; }

        // Connection
        public string PeerId { get; set; } = Guid.NewGuid().ToString();
        public string? ConnectionId { get; set; }
        public ConnectionQuality Quality { get; set; } = ConnectionQuality.Good;

        // Breakout Room
        public int? BreakoutRoomId { get; set; }
        public BreakoutRoom? BreakoutRoom { get; set; }
    }

    public enum ParticipantStatus
    {
        InWaitingRoom = 1,
        Joined = 2,
        Left = 3,
        Removed = 4
    }

    public enum ConnectionQuality
    {
        Excellent = 1,
        Good = 2,
        Fair = 3,
        Poor = 4
    }

    public class BreakoutRoom
    {
        public int Id { get; set; }

        public int ConferenceId { get; set; }
        public VideoConference Conference { get; set; } = null!;

        public string RoomName { get; set; } = string.Empty;
        public int RoomNumber { get; set; }
        public List<ConferenceParticipant> Participants { get; set; } = new();
        public int MaxParticipants { get; set; } = 50;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedAt { get; set; }
        public bool IsOpen { get; set; } = true;
    }

    public class ConferenceChatMessage
    {
        public int Id { get; set; }

        public int ConferenceId { get; set; }
        public VideoConference Conference { get; set; } = null!;

        public string SenderId { get; set; } = string.Empty;
        public ApplicationUser Sender { get; set; } = null!;

        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsPrivate { get; set; }
        public string? RecipientId { get; set; }

        public List<ChatAttachment> Attachments { get; set; } = new();
    }

    public class ChatAttachment
    {
        public int Id { get; set; }

        public int ChatMessageId { get; set; }
        public ConferenceChatMessage ChatMessage { get; set; } = null!;

        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    public class WhiteboardData
    {
        public int Id { get; set; }

        public int ConferenceId { get; set; }
        public VideoConference Conference { get; set; } = null!;

        public string Data { get; set; } = string.Empty; // JSON canvas data
        public string LastModifiedById { get; set; } = string.Empty;
        public ApplicationUser LastModifiedBy { get; set; } = null!;
        public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;
    }
}
