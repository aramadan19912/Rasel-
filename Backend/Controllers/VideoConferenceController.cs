using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/video-conference")]
    [Authorize]
    public class VideoConferenceController : ControllerBase
    {
        private readonly IVideoConferenceService _service;

        public VideoConferenceController(IVideoConferenceService service)
        {
            _service = service;
        }

        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new UnauthorizedAccessException();
        }

        // ========== Conference Management ==========

        /// <summary>
        /// Creates a new video conference
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(VideoConferenceDto), StatusCodes.Status201Created)]
        public async Task<ActionResult<VideoConferenceDto>> CreateConference([FromBody] CreateConferenceDto dto)
        {
            dto.HostId = GetUserId();
            var conference = await _service.CreateConferenceAsync(dto);
            return CreatedAtAction(nameof(GetConference), new { conferenceId = conference.ConferenceId }, conference);
        }

        /// <summary>
        /// Gets conference details
        /// </summary>
        [HttpGet("{conferenceId}")]
        [ProducesResponseType(typeof(VideoConferenceDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<VideoConferenceDto>> GetConference(string conferenceId)
        {
            var conference = await _service.GetConferenceAsync(conferenceId);
            return Ok(conference);
        }

        /// <summary>
        /// Updates conference settings
        /// </summary>
        [HttpPut("{conferenceId}")]
        [ProducesResponseType(typeof(VideoConferenceDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<VideoConferenceDto>> UpdateConference(
            string conferenceId,
            [FromBody] UpdateConferenceDto dto)
        {
            var conference = await _service.UpdateConferenceAsync(conferenceId, dto);
            return Ok(conference);
        }

        /// <summary>
        /// Deletes/Cancels a conference
        /// </summary>
        [HttpDelete("{conferenceId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteConference(string conferenceId)
        {
            await _service.DeleteConferenceAsync(conferenceId);
            return NoContent();
        }

        /// <summary>
        /// Gets all conferences for current user
        /// </summary>
        [HttpGet("my-conferences")]
        [ProducesResponseType(typeof(List<VideoConferenceDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<VideoConferenceDto>>> GetMyConferences()
        {
            var userId = GetUserId();
            var conferences = await _service.GetUserConferencesAsync(userId);
            return Ok(conferences);
        }

        /// <summary>
        /// Generates a shareable meeting link
        /// </summary>
        [HttpPost("{conferenceId}/generate-link")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<string>> GenerateMeetingLink(string conferenceId)
        {
            var link = await _service.GenerateMeetingLinkAsync(conferenceId);
            return Ok(new { meetingLink = link });
        }

        /// <summary>
        /// Starts a scheduled conference
        /// </summary>
        [HttpPost("{conferenceId}/start")]
        [ProducesResponseType(typeof(VideoConferenceDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<VideoConferenceDto>> StartConference(string conferenceId)
        {
            var userId = GetUserId();
            var conference = await _service.StartConferenceAsync(conferenceId, userId);
            return Ok(conference);
        }

        /// <summary>
        /// Ends a conference for all participants
        /// </summary>
        [HttpPost("{conferenceId}/end")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EndConference(string conferenceId)
        {
            var userId = GetUserId();
            await _service.EndConferenceAsync(conferenceId, userId);
            return Ok(new { message = "Conference ended successfully" });
        }

        // ========== Participant Management ==========

        /// <summary>
        /// Joins a conference
        /// </summary>
        [HttpPost("{conferenceId}/join")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ParticipantDto>> JoinConference(
            string conferenceId,
            [FromBody] JoinConferenceDto dto)
        {
            dto.ConferenceId = conferenceId;
            var participant = await _service.JoinConferenceAsync(conferenceId, dto);
            return Ok(participant);
        }

        /// <summary>
        /// Leaves a conference
        /// </summary>
        [HttpPost("{conferenceId}/leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LeaveConference(string conferenceId)
        {
            var userId = GetUserId();
            await _service.LeaveConferenceAsync(conferenceId, userId);
            return Ok(new { message = "Left conference successfully" });
        }

        /// <summary>
        /// Removes a participant
        /// </summary>
        [HttpDelete("{conferenceId}/participants/{participantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveParticipant(string conferenceId, string participantId)
        {
            var requesterId = GetUserId();
            await _service.RemoveParticipantAsync(conferenceId, participantId, requesterId);
            return Ok(new { message = "Participant removed successfully" });
        }

        /// <summary>
        /// Gets all participants
        /// </summary>
        [HttpGet("{conferenceId}/participants")]
        [ProducesResponseType(typeof(List<ParticipantDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ParticipantDto>>> GetParticipants(string conferenceId)
        {
            var participants = await _service.GetParticipantsAsync(conferenceId);
            return Ok(participants);
        }

        /// <summary>
        /// Admits participant from waiting room
        /// </summary>
        [HttpPost("{conferenceId}/participants/{participantId}/admit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AdmitParticipant(string conferenceId, string participantId)
        {
            var hostId = GetUserId();
            await _service.AdmitFromWaitingRoomAsync(conferenceId, participantId, hostId);
            return Ok(new { message = "Participant admitted" });
        }

        /// <summary>
        /// Makes participant a co-host
        /// </summary>
        [HttpPost("{conferenceId}/participants/{participantId}/make-cohost")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MakeCoHost(string conferenceId, string participantId)
        {
            var hostId = GetUserId();
            await _service.MakeCoHostAsync(conferenceId, participantId, hostId);
            return Ok(new { message = "Participant is now co-host" });
        }

        /// <summary>
        /// Updates participant permissions
        /// </summary>
        [HttpPatch("{conferenceId}/participants/{participantId}/permissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateParticipantPermissions(
            string conferenceId,
            string participantId,
            [FromBody] UpdateParticipantPermissionsDto dto)
        {
            var requesterId = GetUserId();
            await _service.UpdateParticipantPermissionsAsync(conferenceId, participantId, dto, requesterId);
            return Ok(new { message = "Permissions updated" });
        }

        // ========== Media Controls ==========

        /// <summary>
        /// Mutes a participant
        /// </summary>
        [HttpPost("{conferenceId}/participants/{participantId}/mute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MuteParticipant(string conferenceId, string participantId)
        {
            var requesterId = GetUserId();
            await _service.MuteParticipantAsync(conferenceId, participantId, requesterId);
            return Ok(new { message = "Participant muted" });
        }

        /// <summary>
        /// Mutes all participants
        /// </summary>
        [HttpPost("{conferenceId}/mute-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MuteAll(string conferenceId)
        {
            var hostId = GetUserId();
            await _service.MuteAllAsync(conferenceId, hostId);
            return Ok(new { message = "All participants muted" });
        }

        /// <summary>
        /// Requests participant to unmute
        /// </summary>
        [HttpPost("{conferenceId}/participants/{participantId}/request-unmute")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> RequestUnmute(string conferenceId, string participantId)
        {
            var hostId = GetUserId();
            await _service.RequestUnmuteAsync(conferenceId, participantId, hostId);
            return Ok(new { message = "Unmute request sent" });
        }

        // ========== Recording ==========

        /// <summary>
        /// Starts recording
        /// </summary>
        [HttpPost("{conferenceId}/recording/start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> StartRecording(string conferenceId)
        {
            var userId = GetUserId();
            await _service.StartRecordingAsync(conferenceId, userId);
            return Ok(new { message = "Recording started" });
        }

        /// <summary>
        /// Stops recording
        /// </summary>
        [HttpPost("{conferenceId}/recording/stop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> StopRecording(string conferenceId)
        {
            var userId = GetUserId();
            await _service.StopRecordingAsync(conferenceId, userId);
            return Ok(new { message = "Recording stopped" });
        }

        /// <summary>
        /// Gets recording info
        /// </summary>
        [HttpGet("{conferenceId}/recording")]
        [ProducesResponseType(typeof(RecordingDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<RecordingDto>> GetRecordingInfo(string conferenceId)
        {
            var recording = await _service.GetRecordingInfoAsync(conferenceId);
            return Ok(recording);
        }

        /// <summary>
        /// Downloads recording
        /// </summary>
        [HttpGet("{conferenceId}/recording/download")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> DownloadRecording(string conferenceId)
        {
            var recordingData = await _service.GetRecordingAsync(conferenceId);
            return File(recordingData, "video/mp4", $"{conferenceId}_recording.mp4");
        }

        // ========== Breakout Rooms ==========

        /// <summary>
        /// Creates breakout rooms
        /// </summary>
        [HttpPost("{conferenceId}/breakout-rooms")]
        [ProducesResponseType(typeof(List<BreakoutRoomDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BreakoutRoomDto>>> CreateBreakoutRooms(
            string conferenceId,
            [FromBody] CreateBreakoutRoomsDto dto)
        {
            var hostId = GetUserId();
            var rooms = await _service.CreateBreakoutRoomsAsync(conferenceId, dto.NumberOfRooms, hostId);
            return Ok(rooms);
        }

        /// <summary>
        /// Gets all breakout rooms
        /// </summary>
        [HttpGet("{conferenceId}/breakout-rooms")]
        [ProducesResponseType(typeof(List<BreakoutRoomDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BreakoutRoomDto>>> GetBreakoutRooms(string conferenceId)
        {
            var rooms = await _service.GetBreakoutRoomsAsync(conferenceId);
            return Ok(rooms);
        }

        /// <summary>
        /// Assigns participant to breakout room
        /// </summary>
        [HttpPost("{conferenceId}/breakout-rooms/{roomNumber}/assign/{participantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> AssignToBreakoutRoom(
            string conferenceId,
            int roomNumber,
            string participantId)
        {
            var hostId = GetUserId();
            await _service.AssignToBreakoutRoomAsync(conferenceId, participantId, roomNumber, hostId);
            return Ok(new { message = "Participant assigned to breakout room" });
        }

        /// <summary>
        /// Closes all breakout rooms
        /// </summary>
        [HttpPost("{conferenceId}/breakout-rooms/close-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CloseBreakoutRooms(string conferenceId)
        {
            var hostId = GetUserId();
            await _service.CloseBreakoutRoomsAsync(conferenceId, hostId);
            return Ok(new { message = "All breakout rooms closed" });
        }

        // ========== Chat ==========

        /// <summary>
        /// Gets chat history
        /// </summary>
        [HttpGet("{conferenceId}/chat")]
        [ProducesResponseType(typeof(List<ChatMessageDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ChatMessageDto>>> GetChatHistory(string conferenceId)
        {
            var messages = await _service.GetChatHistoryAsync(conferenceId);
            return Ok(messages);
        }

        /// <summary>
        /// Sends a chat message (via SignalR is preferred, this is for fallback)
        /// </summary>
        [HttpPost("{conferenceId}/chat")]
        [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ChatMessageDto>> SendChatMessage(
            string conferenceId,
            [FromBody] SendChatMessageDto dto)
        {
            var senderId = GetUserId();
            var message = await _service.SendChatMessageAsync(conferenceId, dto, senderId);
            return Ok(message);
        }

        // ========== Screen Sharing ==========

        /// <summary>
        /// Starts screen sharing
        /// </summary>
        [HttpPost("{conferenceId}/screen-share/start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> StartScreenShare(string conferenceId)
        {
            var userId = GetUserId();
            await _service.StartScreenShareAsync(conferenceId, userId);
            return Ok(new { message = "Screen sharing started" });
        }

        /// <summary>
        /// Stops screen sharing
        /// </summary>
        [HttpPost("{conferenceId}/screen-share/stop")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> StopScreenShare(string conferenceId)
        {
            var userId = GetUserId();
            await _service.StopScreenShareAsync(conferenceId, userId);
            return Ok(new { message = "Screen sharing stopped" });
        }

        /// <summary>
        /// Gets current screen sharer
        /// </summary>
        [HttpGet("{conferenceId}/screen-share/current")]
        [ProducesResponseType(typeof(ParticipantDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ParticipantDto?>> GetScreenSharer(string conferenceId)
        {
            var sharer = await _service.GetScreenSharerAsync(conferenceId);
            return Ok(sharer);
        }

        // ========== Whiteboard ==========

        /// <summary>
        /// Gets whiteboard data
        /// </summary>
        [HttpGet("{conferenceId}/whiteboard")]
        [ProducesResponseType(typeof(WhiteboardDataDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<WhiteboardDataDto>> GetWhiteboard(string conferenceId)
        {
            var whiteboard = await _service.GetWhiteboardAsync(conferenceId);
            return Ok(whiteboard);
        }

        /// <summary>
        /// Updates whiteboard (via SignalR is preferred)
        /// </summary>
        [HttpPut("{conferenceId}/whiteboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateWhiteboard(
            string conferenceId,
            [FromBody] WhiteboardDataDto dto)
        {
            var userId = GetUserId();
            await _service.UpdateWhiteboardAsync(conferenceId, dto, userId);
            return Ok(new { message = "Whiteboard updated" });
        }

        /// <summary>
        /// Clears whiteboard
        /// </summary>
        [HttpDelete("{conferenceId}/whiteboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ClearWhiteboard(string conferenceId)
        {
            var userId = GetUserId();
            await _service.ClearWhiteboardAsync(conferenceId, userId);
            return Ok(new { message = "Whiteboard cleared" });
        }

        // ========== Security ==========

        /// <summary>
        /// Locks the meeting
        /// </summary>
        [HttpPost("{conferenceId}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> LockMeeting(string conferenceId)
        {
            var hostId = GetUserId();
            await _service.LockMeetingAsync(conferenceId, hostId);
            return Ok(new { message = "Meeting locked" });
        }

        /// <summary>
        /// Unlocks the meeting
        /// </summary>
        [HttpPost("{conferenceId}/unlock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> UnlockMeeting(string conferenceId)
        {
            var hostId = GetUserId();
            await _service.UnlockMeetingAsync(conferenceId, hostId);
            return Ok(new { message = "Meeting unlocked" });
        }

        /// <summary>
        /// Enables waiting room
        /// </summary>
        [HttpPost("{conferenceId}/waiting-room/enable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> EnableWaitingRoom(string conferenceId)
        {
            var hostId = GetUserId();
            await _service.EnableWaitingRoomAsync(conferenceId, hostId);
            return Ok(new { message = "Waiting room enabled" });
        }

        /// <summary>
        /// Disables waiting room
        /// </summary>
        [HttpPost("{conferenceId}/waiting-room/disable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> DisableWaitingRoom(string conferenceId)
        {
            var hostId = GetUserId();
            await _service.DisableWaitingRoomAsync(conferenceId, hostId);
            return Ok(new { message = "Waiting room disabled" });
        }

        // ========== Analytics ==========

        /// <summary>
        /// Gets conference analytics
        /// </summary>
        [HttpGet("{conferenceId}/analytics")]
        [ProducesResponseType(typeof(ConferenceAnalyticsDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ConferenceAnalyticsDto>> GetAnalytics(string conferenceId)
        {
            var analytics = await _service.GetAnalyticsAsync(conferenceId);
            return Ok(analytics);
        }
    }
}
