

using System.ComponentModel.DataAnnotations;


namespace Backend.Application.DTOs.VideoConference
{
    // ========== Video Conference DTOs ==========

    public class VideoConferenceDto
    {
        public int Id { get; set; }
        public string ConferenceId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool RequirePassword { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string HostId { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public int ParticipantCount { get; set; }
        public int MaxParticipants { get; set; }
        public bool AllowGuestsToJoin { get; set; }
        public ConferenceStatus Status { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public bool EnableChat { get; set; }
        public bool EnableScreenShare { get; set; }
        public bool EnableRecording { get; set; }
        public bool EnableWhiteboard { get; set; }
        public bool EnableBreakoutRooms { get; set; }
        public bool EnableWaitingRoom { get; set; }
        public bool MuteParticipantsOnEntry { get; set; }
        public bool IsRecording { get; set; }
        public string? RecordingUrl { get; set; }
        public bool LockMeeting { get; set; }
        public string MeetingLink { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateConferenceDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public bool RequirePassword { get; set; }
        public string? Password { get; set; }

        [Required]
        public DateTime ScheduledStartTime { get; set; }

        public int DurationMinutes { get; set; } = 60;

        [Required]
        public string HostId { get; set; } = string.Empty;

        public int? MaxParticipants { get; set; }
        public bool AllowGuestsToJoin { get; set; }
        public bool EnableRecording { get; set; }
        public bool RequireApproval { get; set; }
        public int? CalendarEventId { get; set; }
    }

    public class UpdateConferenceDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? ScheduledStartTime { get; set; }
        public int? DurationMinutes { get; set; }
        public int? MaxParticipants { get; set; }
        public bool? AllowGuestsToJoin { get; set; }
        public bool? EnableChat { get; set; }
        public bool? EnableScreenShare { get; set; }
        public bool? EnableRecording { get; set; }
        public bool? EnableWhiteboard { get; set; }
        public bool? EnableBreakoutRooms { get; set; }
        public bool? EnableWaitingRoom { get; set; }
        public bool? MuteParticipantsOnEntry { get; set; }
    }

    // ========== Participant DTOs ==========

    public class ParticipantDto
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsGuest { get; set; }
        public bool IsHost { get; set; }
        public bool IsCoHost { get; set; }
        public ParticipantStatus Status { get; set; }
        public DateTime JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool IsAudioMuted { get; set; }
        public bool IsVideoOff { get; set; }
        public bool IsHandRaised { get; set; }
        public bool IsScreenSharing { get; set; }
        public string PeerId { get; set; } = string.Empty;
        public ConnectionQuality Quality { get; set; }
        public bool CanShareScreen { get; set; }
        public bool CanRecord { get; set; }
        public bool CanUseWhiteboard { get; set; }
        public int? BreakoutRoomId { get; set; }
        public string? BreakoutRoomName { get; set; }
    }

    public class JoinConferenceDto
    {
        [Required]
        public string ConferenceId { get; set; } = string.Empty;

        public string? UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }
        public bool IsGuest { get; set; }
    }

    // ========== Breakout Room DTOs ==========

    public class BreakoutRoomDto
    {
        public int Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public int RoomNumber { get; set; }
        public int ParticipantCount { get; set; }
        public int MaxParticipants { get; set; }
        public List<ParticipantDto> Participants { get; set; } = new();
        public bool IsOpen { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateBreakoutRoomsDto
    {
        [Range(2, 50)]
        public int NumberOfRooms { get; set; }

        public bool AssignAutomatically { get; set; } = true;
    }

    // ========== Chat DTOs ==========

    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsPrivate { get; set; }
        public string? RecipientId { get; set; }
        public string? RecipientName { get; set; }
        public List<ChatAttachmentDto> Attachments { get; set; } = new();
    }

    public class SendChatMessageDto
    {
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public bool IsPrivate { get; set; }
        public string? RecipientId { get; set; }
    }

    public class ChatAttachmentDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
    }

    // ========== Whiteboard DTOs ==========

    public class WhiteboardDataDto
    {
        public string Data { get; set; } = string.Empty;
        public string LastModifiedById { get; set; } = string.Empty;
        public string LastModifiedByName { get; set; } = string.Empty;
        public DateTime LastModifiedAt { get; set; }
    }

    // ========== Analytics DTOs ==========

    public class ConferenceAnalyticsDto
    {
        public int TotalParticipantsJoined { get; set; }
        public int PeakParticipants { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan AverageJoinTime { get; set; }
        public int TotalChatMessages { get; set; }
        public int ScreenShareCount { get; set; }
        public Dictionary<string, int> ParticipantsByCountry { get; set; } = new();
        public Dictionary<ConnectionQuality, int> ConnectionQualityDistribution { get; set; } = new();
    }

    // ========== Recording DTOs ==========

    public class RecordingDto
    {
        public string RecordingUrl { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Format { get; set; } = "mp4";
    }

    // ========== Meeting Link DTOs ==========

    public class MeetingLinkDto
    {
        public string ConferenceId { get; set; } = string.Empty;
        public string MeetingUrl { get; set; } = string.Empty;
        public string? Password { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    // ========== Permissions DTOs ==========

    public class UpdateParticipantPermissionsDto
    {
        public bool? IsCoHost { get; set; }
        public bool? CanShareScreen { get; set; }
        public bool? CanRecord { get; set; }
        public bool? CanUseWhiteboard { get; set; }
    }
}
