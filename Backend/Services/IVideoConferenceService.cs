using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.DTOs;

namespace Backend.Services
{
    public interface IVideoConferenceService
    {
        // ========== Conference Management ==========

        /// <summary>
        /// Creates a new video conference
        /// </summary>
        Task<VideoConferenceDto> CreateConferenceAsync(CreateConferenceDto dto);

        /// <summary>
        /// Gets conference details by conference ID
        /// </summary>
        Task<VideoConferenceDto> GetConferenceAsync(string conferenceId);

        /// <summary>
        /// Updates conference settings
        /// </summary>
        Task<VideoConferenceDto> UpdateConferenceAsync(string conferenceId, UpdateConferenceDto dto);

        /// <summary>
        /// Deletes/Cancels a conference
        /// </summary>
        Task DeleteConferenceAsync(string conferenceId);

        /// <summary>
        /// Generates a shareable meeting link
        /// </summary>
        Task<string> GenerateMeetingLinkAsync(string conferenceId);

        /// <summary>
        /// Gets all conferences for a user
        /// </summary>
        Task<List<VideoConferenceDto>> GetUserConferencesAsync(string userId);

        /// <summary>
        /// Starts a scheduled conference
        /// </summary>
        Task<VideoConferenceDto> StartConferenceAsync(string conferenceId, string userId);

        /// <summary>
        /// Ends a conference for all participants
        /// </summary>
        Task EndConferenceAsync(string conferenceId, string userId);

        // ========== Participant Management ==========

        /// <summary>
        /// Joins a conference
        /// </summary>
        Task<ParticipantDto> JoinConferenceAsync(string conferenceId, JoinConferenceDto dto);

        /// <summary>
        /// Leaves a conference
        /// </summary>
        Task LeaveConferenceAsync(string conferenceId, string userId);

        /// <summary>
        /// Removes a participant from the conference
        /// </summary>
        Task RemoveParticipantAsync(string conferenceId, string participantId, string requesterId);

        /// <summary>
        /// Admits participant from waiting room
        /// </summary>
        Task AdmitFromWaitingRoomAsync(string conferenceId, string participantId, string hostId);

        /// <summary>
        /// Makes a participant a co-host
        /// </summary>
        Task MakeCoHostAsync(string conferenceId, string participantId, string hostId);

        /// <summary>
        /// Gets all participants in a conference
        /// </summary>
        Task<List<ParticipantDto>> GetParticipantsAsync(string conferenceId);

        /// <summary>
        /// Updates participant permissions
        /// </summary>
        Task UpdateParticipantPermissionsAsync(
            string conferenceId,
            string participantId,
            UpdateParticipantPermissionsDto dto,
            string requesterId);

        // ========== Media Controls ==========

        /// <summary>
        /// Mutes a specific participant's audio
        /// </summary>
        Task MuteParticipantAsync(string conferenceId, string participantId, string requesterId);

        /// <summary>
        /// Unmutes a specific participant's audio
        /// </summary>
        Task UnmuteParticipantAsync(string conferenceId, string participantId);

        /// <summary>
        /// Mutes all participants in the conference
        /// </summary>
        Task MuteAllAsync(string conferenceId, string hostId);

        /// <summary>
        /// Requests a participant to unmute
        /// </summary>
        Task RequestUnmuteAsync(string conferenceId, string participantId, string hostId);

        /// <summary>
        /// Updates participant's media status (audio/video)
        /// </summary>
        Task UpdateMediaStatusAsync(
            string conferenceId,
            string participantId,
            bool? isAudioMuted,
            bool? isVideoOff);

        /// <summary>
        /// Raises or lowers hand
        /// </summary>
        Task ToggleHandRaiseAsync(string conferenceId, string participantId);

        // ========== Recording ==========

        /// <summary>
        /// Starts recording the conference
        /// </summary>
        Task StartRecordingAsync(string conferenceId, string userId);

        /// <summary>
        /// Stops recording the conference
        /// </summary>
        Task StopRecordingAsync(string conferenceId, string userId);

        /// <summary>
        /// Gets recording data for a conference
        /// </summary>
        Task<byte[]> GetRecordingAsync(string conferenceId);

        /// <summary>
        /// Gets recording metadata
        /// </summary>
        Task<RecordingDto> GetRecordingInfoAsync(string conferenceId);

        // ========== Breakout Rooms ==========

        /// <summary>
        /// Creates breakout rooms
        /// </summary>
        Task<List<BreakoutRoomDto>> CreateBreakoutRoomsAsync(
            string conferenceId,
            int numberOfRooms,
            string hostId);

        /// <summary>
        /// Assigns participant to a breakout room
        /// </summary>
        Task AssignToBreakoutRoomAsync(
            string conferenceId,
            string participantId,
            int roomNumber,
            string hostId);

        /// <summary>
        /// Closes all breakout rooms
        /// </summary>
        Task CloseBreakoutRoomsAsync(string conferenceId, string hostId);

        /// <summary>
        /// Gets all breakout rooms for a conference
        /// </summary>
        Task<List<BreakoutRoomDto>> GetBreakoutRoomsAsync(string conferenceId);

        /// <summary>
        /// Broadcasts message to all breakout rooms
        /// </summary>
        Task BroadcastToBreakoutRoomsAsync(string conferenceId, string message, string hostId);

        // ========== Chat ==========

        /// <summary>
        /// Sends a chat message in the conference
        /// </summary>
        Task<ChatMessageDto> SendChatMessageAsync(
            string conferenceId,
            SendChatMessageDto dto,
            string senderId);

        /// <summary>
        /// Gets chat history for a conference
        /// </summary>
        Task<List<ChatMessageDto>> GetChatHistoryAsync(string conferenceId);

        /// <summary>
        /// Deletes a chat message
        /// </summary>
        Task DeleteChatMessageAsync(string conferenceId, int messageId, string requesterId);

        // ========== Screen Sharing ==========

        /// <summary>
        /// Starts screen sharing for a participant
        /// </summary>
        Task StartScreenShareAsync(string conferenceId, string userId);

        /// <summary>
        /// Stops screen sharing for a participant
        /// </summary>
        Task StopScreenShareAsync(string conferenceId, string userId);

        /// <summary>
        /// Gets current screen sharer
        /// </summary>
        Task<ParticipantDto?> GetScreenSharerAsync(string conferenceId);

        // ========== Whiteboard ==========

        /// <summary>
        /// Updates whiteboard data
        /// </summary>
        Task UpdateWhiteboardAsync(string conferenceId, WhiteboardDataDto dto, string userId);

        /// <summary>
        /// Gets whiteboard data
        /// </summary>
        Task<WhiteboardDataDto> GetWhiteboardAsync(string conferenceId);

        /// <summary>
        /// Clears whiteboard
        /// </summary>
        Task ClearWhiteboardAsync(string conferenceId, string userId);

        // ========== Security ==========

        /// <summary>
        /// Locks the meeting (prevents new participants from joining)
        /// </summary>
        Task LockMeetingAsync(string conferenceId, string hostId);

        /// <summary>
        /// Unlocks the meeting
        /// </summary>
        Task UnlockMeetingAsync(string conferenceId, string hostId);

        /// <summary>
        /// Enables waiting room
        /// </summary>
        Task EnableWaitingRoomAsync(string conferenceId, string hostId);

        /// <summary>
        /// Disables waiting room
        /// </summary>
        Task DisableWaitingRoomAsync(string conferenceId, string hostId);

        /// <summary>
        /// Updates conference password
        /// </summary>
        Task UpdatePasswordAsync(string conferenceId, string newPassword, string hostId);

        // ========== Analytics ==========

        /// <summary>
        /// Gets analytics for a conference
        /// </summary>
        Task<ConferenceAnalyticsDto> GetAnalyticsAsync(string conferenceId);

        /// <summary>
        /// Updates connection quality for a participant
        /// </summary>
        Task UpdateConnectionQualityAsync(
            string conferenceId,
            string participantId,
            Models.ConnectionQuality quality);
    }
}
