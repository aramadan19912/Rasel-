using Backend.Application.DTOs.VideoConference;
using static Backend.Application.DTOs.VideoConference.VideoConferenceDtos;

namespace Backend.Application.Interfaces;

public interface IVideoConferenceService
{
    // ===== Conference CRUD =====
    Task<List<VideoConferenceDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50);
    Task<List<VideoConferenceDto>> GetUpcomingAsync(string userId);
    Task<List<VideoConferenceDto>> GetScheduledAsync(string userId, DateTime startDate, DateTime endDate);
    Task<List<VideoConferenceDto>> GetPastAsync(string userId, int pageNumber = 1, int pageSize = 50);
    Task<VideoConferenceDto> GetByIdAsync(int id, string userId);
    Task<VideoConferenceDto> GetByConferenceIdAsync(string conferenceId);
    Task<VideoConferenceDto> CreateAsync(CreateConferenceDto dto, string userId);
    Task<bool> UpdateAsync(int id, UpdateConferenceDto dto, string userId);
    Task<bool> DeleteAsync(int id, string userId);
    Task<bool> CancelAsync(int id, string userId);

    // ===== Conference Control =====
    Task<VideoConferenceDto> StartConferenceAsync(int id, string userId);
    Task<bool> EndConferenceAsync(int id, string userId);
    Task<bool> LockConferenceAsync(int id, string userId);
    Task<bool> UnlockConferenceAsync(int id, string userId);

    // ===== Participants =====
    Task<List<ParticipantDto>> GetParticipantsAsync(int conferenceId, string userId);
    Task<List<ParticipantDto>> GetActiveParticipantsAsync(int conferenceId, string userId);
    Task<ParticipantDto> JoinConferenceAsync(JoinConferenceDto dto);
    Task<bool> LeaveConferenceAsync(int conferenceId, string userId);
    Task<bool> RemoveParticipantAsync(int conferenceId, int participantId, string userId);
    Task<bool> AdmitFromWaitingRoomAsync(int conferenceId, int participantId, string userId);
    Task<bool> AdmitAllFromWaitingRoomAsync(int conferenceId, string userId);

    // ===== Participant Permissions =====
    Task<bool> MakeCoHostAsync(int conferenceId, int participantId, string userId);
    Task<bool> RevokeCoHostAsync(int conferenceId, int participantId, string userId);
    Task<bool> UpdateParticipantPermissionsAsync(int conferenceId, int participantId, UpdateParticipantPermissionsDto dto, string userId);

    // ===== Media Control =====
    Task<bool> MuteParticipantAsync(int conferenceId, int participantId, string userId);
    Task<bool> UnmuteParticipantAsync(int conferenceId, int participantId, string userId);
    Task<bool> MuteAllAsync(int conferenceId, string userId);
    Task<bool> UnmuteAllAsync(int conferenceId, string userId);
    Task<bool> DisableParticipantVideoAsync(int conferenceId, int participantId, string userId);
    Task<bool> EnableParticipantVideoAsync(int conferenceId, int participantId, string userId);

    // ===== Screen Sharing =====
    Task<bool> StartScreenShareAsync(int conferenceId, string userId);
    Task<bool> StopScreenShareAsync(int conferenceId, string userId);
    Task<ParticipantDto> GetCurrentScreenSharerAsync(int conferenceId);

    // ===== Recording =====
    Task<bool> StartRecordingAsync(int conferenceId, string userId);
    Task<bool> StopRecordingAsync(int conferenceId, string userId);
    Task<bool> PauseRecordingAsync(int conferenceId, string userId);
    Task<bool> ResumeRecordingAsync(int conferenceId, string userId);
    Task<RecordingDto> GetRecordingAsync(int conferenceId, string userId);
    Task<List<RecordingDto>> GetRecordingsAsync(string userId);
    Task<bool> DeleteRecordingAsync(int conferenceId, string userId);

    // ===== Breakout Rooms =====
    Task<List<BreakoutRoomDto>> CreateBreakoutRoomsAsync(int conferenceId, CreateBreakoutRoomsDto dto, string userId);
    Task<List<BreakoutRoomDto>> GetBreakoutRoomsAsync(int conferenceId, string userId);
    Task<bool> AssignToBreakoutRoomAsync(int conferenceId, int participantId, int roomId, string userId);
    Task<bool> MoveToBreakoutRoomAsync(int conferenceId, int participantId, int roomId, string userId);
    Task<bool> BroadcastMessageToRoomsAsync(int conferenceId, string message, string userId);
    Task<bool> CloseAllBreakoutRoomsAsync(int conferenceId, string userId);
    Task<bool> CloseBreakoutRoomAsync(int conferenceId, int roomId, string userId);

    // ===== Chat =====
    Task<List<ChatMessageDto>> GetChatMessagesAsync(int conferenceId, string userId);
    Task<ChatMessageDto> SendChatMessageAsync(int conferenceId, SendChatMessageDto dto, string userId);
    Task<bool> DeleteChatMessageAsync(int conferenceId, int messageId, string userId);
    Task<bool> ClearChatHistoryAsync(int conferenceId, string userId);
    Task<bool> EnableChatAsync(int conferenceId, string userId);
    Task<bool> DisableChatAsync(int conferenceId, string userId);

    // ===== Whiteboard =====
    Task<WhiteboardDataDto> GetWhiteboardDataAsync(int conferenceId, string userId);
    Task<bool> UpdateWhiteboardDataAsync(int conferenceId, string data, string userId);
    Task<bool> ClearWhiteboardAsync(int conferenceId, string userId);
    Task<bool> EnableWhiteboardAsync(int conferenceId, string userId);
    Task<bool> DisableWhiteboardAsync(int conferenceId, string userId);

    // ===== Waiting Room =====
    Task<List<ParticipantDto>> GetWaitingRoomParticipantsAsync(int conferenceId, string userId);
    Task<bool> EnableWaitingRoomAsync(int conferenceId, string userId);
    Task<bool> DisableWaitingRoomAsync(int conferenceId, string userId);

    // ===== Hand Raise =====
    Task<bool> RaiseHandAsync(int conferenceId, string userId);
    Task<bool> LowerHandAsync(int conferenceId, string userId);
    Task<bool> LowerAllHandsAsync(int conferenceId, string userId);
    Task<List<ParticipantDto>> GetRaisedHandsAsync(int conferenceId, string userId);

    // ===== Meeting Links =====
    Task<MeetingLinkDto> GetMeetingLinkAsync(int conferenceId, string userId);
    Task<MeetingLinkDto> GenerateMeetingLinkAsync(int conferenceId, string userId);
    Task<bool> RegenerateMeetingLinkAsync(int conferenceId, string userId);

    // ===== Analytics =====
    Task<ConferenceAnalyticsDto> GetAnalyticsAsync(int conferenceId, string userId);
    Task<List<ParticipantDto>> GetParticipantHistoryAsync(int conferenceId, string userId);

    // ===== Integration =====
    Task<VideoConferenceDto> CreateFromCalendarEventAsync(int calendarEventId, string userId);
    Task<bool> LinkToCalendarEventAsync(int conferenceId, int calendarEventId, string userId);

    // ===== Statistics =====
    Task<ConferenceStatisticsDto> GetStatisticsAsync(string userId);
}

public class ConferenceStatisticsDto
{
    public int TotalConferences { get; set; }
    public int UpcomingConferences { get; set; }
    public int CompletedConferences { get; set; }
    public int CancelledConferences { get; set; }
    public int TotalParticipantsHosted { get; set; }
    public TimeSpan TotalMeetingTime { get; set; }
    public int TotalRecordings { get; set; }
    public Dictionary<string, int> ConferencesByStatus { get; set; } = new();
}
