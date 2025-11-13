using Application.DTOs.VideoConference;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VideoConferenceController : ControllerBase
    {
        private readonly IVideoConferenceService _videoConferenceService;

        public VideoConferenceController(IVideoConferenceService videoConferenceService)
        {
            _videoConferenceService = videoConferenceService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ===== Conference CRUD Operations =====

        [HttpGet("{id}")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<ConferenceDto>> GetById(int id)
        {
            try
            {
                var conference = await _videoConferenceService.GetByIdAsync(id, GetUserId());
                return Ok(conference);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("conference/{conferenceId}")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<ConferenceDto>> GetByConferenceId(string conferenceId)
        {
            try
            {
                var conference = await _videoConferenceService.GetByConferenceIdAsync(conferenceId, GetUserId());
                return Ok(conference);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("user")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<ConferenceDto>>> GetUserConferences([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var conferences = await _videoConferenceService.GetUserConferencesAsync(GetUserId(), pageNumber, pageSize);
            return Ok(conferences);
        }

        [HttpGet("user/scheduled")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<ConferenceDto>>> GetScheduledConferences()
        {
            var conferences = await _videoConferenceService.GetScheduledConferencesAsync(GetUserId());
            return Ok(conferences);
        }

        [HttpGet("user/past")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<ConferenceDto>>> GetPastConferences([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
        {
            var conferences = await _videoConferenceService.GetPastConferencesAsync(GetUserId(), pageNumber, pageSize);
            return Ok(conferences);
        }

        [HttpGet("user/active")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<ConferenceDto>>> GetActiveConferences()
        {
            var conferences = await _videoConferenceService.GetActiveConferencesAsync(GetUserId());
            return Ok(conferences);
        }

        [HttpPost]
        [Permission(SystemPermission.VideoConferenceCreate)]
        public async Task<ActionResult<ConferenceDto>> Create([FromBody] CreateConferenceDto dto)
        {
            var conference = await _videoConferenceService.CreateAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = conference.Id }, conference);
        }

        [HttpPut("{id}")]
        [Permission(SystemPermission.VideoConferenceUpdate)]
        public async Task<ActionResult<ConferenceDto>> Update(int id, [FromBody] UpdateConferenceDto dto)
        {
            try
            {
                var conference = await _videoConferenceService.UpdateAsync(id, dto, GetUserId());
                return Ok(conference);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Permission(SystemPermission.VideoConferenceDelete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _videoConferenceService.DeleteAsync(id, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }

        // ===== Conference Control =====

        [HttpPost("{id}/start")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ConferenceDto>> Start(int id)
        {
            try
            {
                var conference = await _videoConferenceService.StartConferenceAsync(id, GetUserId());
                return Ok(conference);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{id}/end")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ConferenceDto>> End(int id)
        {
            try
            {
                var conference = await _videoConferenceService.EndConferenceAsync(id, GetUserId());
                return Ok(conference);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{id}/lock")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ConferenceDto>> Lock(int id)
        {
            try
            {
                var conference = await _videoConferenceService.LockConferenceAsync(id, GetUserId());
                return Ok(conference);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{id}/unlock")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ConferenceDto>> Unlock(int id)
        {
            try
            {
                var conference = await _videoConferenceService.UnlockConferenceAsync(id, GetUserId());
                return Ok(conference);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ===== Participant Management =====

        [HttpGet("{conferenceId}/participants")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<ParticipantDto>>> GetParticipants(int conferenceId)
        {
            try
            {
                var participants = await _videoConferenceService.GetParticipantsAsync(conferenceId, GetUserId());
                return Ok(participants);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{conferenceId}/participants/active")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<ParticipantDto>>> GetActiveParticipants(int conferenceId)
        {
            try
            {
                var participants = await _videoConferenceService.GetActiveParticipantsAsync(conferenceId, GetUserId());
                return Ok(participants);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/join")]
        [Permission(SystemPermission.VideoConferenceJoin)]
        public async Task<ActionResult<ParticipantDto>> Join(int conferenceId, [FromBody] JoinConferenceDto dto)
        {
            try
            {
                var participant = await _videoConferenceService.JoinConferenceAsync(conferenceId, dto, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/leave")]
        [Permission(SystemPermission.VideoConferenceJoin)]
        public async Task<IActionResult> Leave(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.LeaveConferenceAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/admit")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> AdmitParticipant(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.AdmitParticipantAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/remove")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> RemoveParticipant(int conferenceId, int participantId)
        {
            try
            {
                var result = await _videoConferenceService.RemoveParticipantAsync(conferenceId, participantId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/mute")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> MuteParticipant(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.MuteParticipantAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/unmute")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> UnmuteParticipant(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.UnmuteParticipantAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/mute-all")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> MuteAllParticipants(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.MuteAllParticipantsAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ===== Participant Permissions =====

        [HttpPost("{conferenceId}/participants/{participantId}/make-cohost")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> MakeCoHost(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.MakeCoHostAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/make-presenter")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> MakePresenter(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.MakePresenterAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/make-attendee")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> MakeAttendee(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.MakeAttendeeAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/enable-screenshare")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> EnableScreenShare(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.EnableScreenShareAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/disable-screenshare")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> DisableScreenShare(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.DisableScreenShareAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/enable-chat")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> EnableChat(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.EnableChatAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/disable-chat")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<ParticipantDto>> DisableChat(int conferenceId, int participantId)
        {
            try
            {
                var participant = await _videoConferenceService.DisableChatAsync(conferenceId, participantId, GetUserId());
                return Ok(participant);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ===== Screen Sharing =====

        [HttpPost("{conferenceId}/screenshare/start")]
        [Permission(SystemPermission.VideoConferencePresent)]
        public async Task<ActionResult<ScreenShareDto>> StartScreenShare(int conferenceId, [FromBody] StartScreenShareDto dto)
        {
            try
            {
                var screenShare = await _videoConferenceService.StartScreenShareAsync(conferenceId, dto, GetUserId());
                return Ok(screenShare);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/screenshare/stop")]
        [Permission(SystemPermission.VideoConferencePresent)]
        public async Task<IActionResult> StopScreenShare(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.StopScreenShareAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{conferenceId}/screenshare/active")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<ScreenShareDto>> GetActiveScreenShare(int conferenceId)
        {
            try
            {
                var screenShare = await _videoConferenceService.GetActiveScreenShareAsync(conferenceId, GetUserId());
                if (screenShare == null) return NotFound();
                return Ok(screenShare);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ===== Recording =====

        [HttpPost("{conferenceId}/recording/start")]
        [Permission(SystemPermission.VideoConferenceRecord)]
        public async Task<ActionResult<RecordingDto>> StartRecording(int conferenceId, [FromBody] StartRecordingDto dto)
        {
            try
            {
                var recording = await _videoConferenceService.StartRecordingAsync(conferenceId, dto, GetUserId());
                return Ok(recording);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/recording/stop")]
        [Permission(SystemPermission.VideoConferenceRecord)]
        public async Task<ActionResult<RecordingDto>> StopRecording(int conferenceId)
        {
            try
            {
                var recording = await _videoConferenceService.StopRecordingAsync(conferenceId, GetUserId());
                return Ok(recording);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/recording/pause")]
        [Permission(SystemPermission.VideoConferenceRecord)]
        public async Task<ActionResult<RecordingDto>> PauseRecording(int conferenceId)
        {
            try
            {
                var recording = await _videoConferenceService.PauseRecordingAsync(conferenceId, GetUserId());
                return Ok(recording);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/recording/resume")]
        [Permission(SystemPermission.VideoConferenceRecord)]
        public async Task<ActionResult<RecordingDto>> ResumeRecording(int conferenceId)
        {
            try
            {
                var recording = await _videoConferenceService.ResumeRecordingAsync(conferenceId, GetUserId());
                return Ok(recording);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{conferenceId}/recordings")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<RecordingDto>>> GetRecordings(int conferenceId)
        {
            try
            {
                var recordings = await _videoConferenceService.GetRecordingsAsync(conferenceId, GetUserId());
                return Ok(recordings);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("recordings/{recordingId}")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<RecordingDto>> GetRecordingById(int recordingId)
        {
            try
            {
                var recording = await _videoConferenceService.GetRecordingByIdAsync(recordingId, GetUserId());
                return Ok(recording);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("recordings/{recordingId}")]
        [Permission(SystemPermission.VideoConferenceDelete)]
        public async Task<IActionResult> DeleteRecording(int recordingId)
        {
            var result = await _videoConferenceService.DeleteRecordingAsync(recordingId, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }

        // ===== Breakout Rooms =====

        [HttpPost("{conferenceId}/breakout-rooms")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<List<BreakoutRoomDto>>> CreateBreakoutRooms(int conferenceId, [FromBody] CreateBreakoutRoomsDto dto)
        {
            try
            {
                var rooms = await _videoConferenceService.CreateBreakoutRoomsAsync(conferenceId, dto, GetUserId());
                return Ok(rooms);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{conferenceId}/breakout-rooms")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<BreakoutRoomDto>>> GetBreakoutRooms(int conferenceId)
        {
            try
            {
                var rooms = await _videoConferenceService.GetBreakoutRoomsAsync(conferenceId, GetUserId());
                return Ok(rooms);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/breakout-rooms/open")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> OpenBreakoutRooms(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.OpenBreakoutRoomsAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/breakout-rooms/close")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> CloseBreakoutRooms(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.CloseBreakoutRoomsAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/breakout-rooms/assign")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> AssignParticipantToBreakoutRoom(int conferenceId, [FromBody] AssignBreakoutRoomDto dto)
        {
            try
            {
                var result = await _videoConferenceService.AssignParticipantToBreakoutRoomAsync(conferenceId, dto, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/breakout-rooms/return-to-main")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> ReturnToMainRoom(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.ReturnToMainRoomAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ===== Chat =====

        [HttpPost("{conferenceId}/chat")]
        [Permission(SystemPermission.VideoConferenceChat)]
        public async Task<ActionResult<ConferenceChatDto>> SendChatMessage(int conferenceId, [FromBody] SendConferenceChatDto dto)
        {
            try
            {
                var chatMessage = await _videoConferenceService.SendChatMessageAsync(conferenceId, dto, GetUserId());
                return Ok(chatMessage);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{conferenceId}/chat")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<ConferenceChatDto>>> GetChatMessages(int conferenceId)
        {
            try
            {
                var messages = await _videoConferenceService.GetChatMessagesAsync(conferenceId, GetUserId());
                return Ok(messages);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{conferenceId}/chat/{chatId}")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> DeleteChatMessage(int conferenceId, int chatId)
        {
            try
            {
                var result = await _videoConferenceService.DeleteChatMessageAsync(conferenceId, chatId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ===== Whiteboard =====

        [HttpPost("{conferenceId}/whiteboard/start")]
        [Permission(SystemPermission.VideoConferencePresent)]
        public async Task<ActionResult<WhiteboardDto>> StartWhiteboard(int conferenceId, [FromBody] StartWhiteboardDto dto)
        {
            try
            {
                var whiteboard = await _videoConferenceService.StartWhiteboardAsync(conferenceId, dto, GetUserId());
                return Ok(whiteboard);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/whiteboard/stop")]
        [Permission(SystemPermission.VideoConferencePresent)]
        public async Task<IActionResult> StopWhiteboard(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.StopWhiteboardAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{conferenceId}/whiteboard/active")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<WhiteboardDto>> GetActiveWhiteboard(int conferenceId)
        {
            try
            {
                var whiteboard = await _videoConferenceService.GetActiveWhiteboardAsync(conferenceId, GetUserId());
                if (whiteboard == null) return NotFound();
                return Ok(whiteboard);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/whiteboard/save")]
        [Permission(SystemPermission.VideoConferencePresent)]
        public async Task<ActionResult<WhiteboardDto>> SaveWhiteboard(int conferenceId, [FromBody] SaveWhiteboardDto dto)
        {
            try
            {
                var whiteboard = await _videoConferenceService.SaveWhiteboardAsync(conferenceId, dto, GetUserId());
                return Ok(whiteboard);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ===== Hand Raise =====

        [HttpPost("{conferenceId}/hand/raise")]
        [Permission(SystemPermission.VideoConferenceJoin)]
        public async Task<IActionResult> RaiseHand(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.RaiseHandAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/hand/lower")]
        [Permission(SystemPermission.VideoConferenceJoin)]
        public async Task<IActionResult> LowerHand(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.LowerHandAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/participants/{participantId}/hand/lower")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> LowerParticipantHand(int conferenceId, int participantId)
        {
            try
            {
                var result = await _videoConferenceService.LowerParticipantHandAsync(conferenceId, participantId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ===== Meeting Links =====

        [HttpPost("{conferenceId}/links")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<MeetingLinkDto>> CreateMeetingLink(int conferenceId, [FromBody] CreateMeetingLinkDto dto)
        {
            try
            {
                var link = await _videoConferenceService.CreateMeetingLinkAsync(conferenceId, dto, GetUserId());
                return Ok(link);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{conferenceId}/links")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<List<MeetingLinkDto>>> GetMeetingLinks(int conferenceId)
        {
            try
            {
                var links = await _videoConferenceService.GetMeetingLinksAsync(conferenceId, GetUserId());
                return Ok(links);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("links/{linkId}")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> DeleteMeetingLink(int linkId)
        {
            var result = await _videoConferenceService.DeleteMeetingLinkAsync(linkId, GetUserId());
            if (!result) return NotFound();
            return NoContent();
        }

        // ===== Waiting Room =====

        [HttpPost("{conferenceId}/waiting-room/enable")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> EnableWaitingRoom(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.EnableWaitingRoomAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/waiting-room/disable")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> DisableWaitingRoom(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.DisableWaitingRoomAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("{conferenceId}/waiting-room/participants")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<ActionResult<List<ParticipantDto>>> GetWaitingRoomParticipants(int conferenceId)
        {
            try
            {
                var participants = await _videoConferenceService.GetWaitingRoomParticipantsAsync(conferenceId, GetUserId());
                return Ok(participants);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("{conferenceId}/waiting-room/admit-all")]
        [Permission(SystemPermission.VideoConferenceManage)]
        public async Task<IActionResult> AdmitAllFromWaitingRoom(int conferenceId)
        {
            try
            {
                var result = await _videoConferenceService.AdmitAllFromWaitingRoomAsync(conferenceId, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ===== Analytics and Statistics =====

        [HttpGet("{conferenceId}/analytics")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<ConferenceAnalyticsDto>> GetAnalytics(int conferenceId)
        {
            try
            {
                var analytics = await _videoConferenceService.GetConferenceAnalyticsAsync(conferenceId, GetUserId());
                return Ok(analytics);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("statistics")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<ConferenceStatisticsDto>> GetStatistics()
        {
            var statistics = await _videoConferenceService.GetUserStatisticsAsync(GetUserId());
            return Ok(statistics);
        }

        // ===== Calendar Integration =====

        [HttpPost("{conferenceId}/calendar/sync")]
        [Permission(SystemPermission.VideoConferenceUpdate)]
        public async Task<IActionResult> SyncWithCalendar(int conferenceId, [FromBody] SyncConferenceWithCalendarDto dto)
        {
            try
            {
                var result = await _videoConferenceService.SyncWithCalendarAsync(conferenceId, dto, GetUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("calendar/{calendarEventId}")]
        [Permission(SystemPermission.VideoConferenceRead)]
        public async Task<ActionResult<ConferenceDto>> GetByCalendarEvent(int calendarEventId)
        {
            try
            {
                var conference = await _videoConferenceService.GetByCalendarEventIdAsync(calendarEventId, GetUserId());
                if (conference == null) return NotFound();
                return Ok(conference);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
