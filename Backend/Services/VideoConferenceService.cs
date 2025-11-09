using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Backend.Data;
using Backend.DTOs;
using Backend.Models;
using Backend.Hubs;

namespace Backend.Services
{
    public class VideoConferenceService : IVideoConferenceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<VideoConferenceHub> _hubContext;

        public VideoConferenceService(
            ApplicationDbContext context,
            IHubContext<VideoConferenceHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ========== Conference Management ==========

        public async Task<VideoConferenceDto> CreateConferenceAsync(CreateConferenceDto dto)
        {
            var conference = new VideoConference
            {
                ConferenceId = GenerateConferenceId(),
                Title = dto.Title,
                Description = dto.Description,
                HostId = dto.HostId,
                ScheduledStartTime = dto.ScheduledStartTime,
                ScheduledEndTime = dto.ScheduledStartTime.AddMinutes(dto.DurationMinutes),
                DurationMinutes = dto.DurationMinutes,
                MaxParticipants = dto.MaxParticipants ?? 100,
                AllowGuestsToJoin = dto.AllowGuestsToJoin,
                RequirePassword = dto.RequirePassword,
                PasswordHash = dto.RequirePassword && !string.IsNullOrEmpty(dto.Password)
                    ? BCrypt.Net.BCrypt.HashPassword(dto.Password)
                    : null,
                EnableRecording = dto.EnableRecording,
                EnableWaitingRoom = dto.RequireApproval,
                Status = ConferenceStatus.Scheduled,
                CreatedAt = DateTime.UtcNow,
                CalendarEventId = dto.CalendarEventId
            };

            _context.VideoConferences.Add(conference);
            await _context.SaveChangesAsync();

            return await MapToDto(conference);
        }

        public async Task<VideoConferenceDto> GetConferenceAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Host)
                .Include(c => c.Participants)
                .Include(c => c.BreakoutRooms)
                .Include(c => c.ChatMessages)
                .Include(c => c.Whiteboard)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            return await MapToDto(conference);
        }

        public async Task<VideoConferenceDto> UpdateConferenceAsync(string conferenceId, UpdateConferenceDto dto)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            // Update only provided fields
            if (dto.Title != null) conference.Title = dto.Title;
            if (dto.Description != null) conference.Description = dto.Description;
            if (dto.ScheduledStartTime.HasValue)
            {
                conference.ScheduledStartTime = dto.ScheduledStartTime.Value;
                if (dto.DurationMinutes.HasValue)
                {
                    conference.DurationMinutes = dto.DurationMinutes.Value;
                    conference.ScheduledEndTime = dto.ScheduledStartTime.Value.AddMinutes(dto.DurationMinutes.Value);
                }
            }
            if (dto.MaxParticipants.HasValue) conference.MaxParticipants = dto.MaxParticipants.Value;
            if (dto.AllowGuestsToJoin.HasValue) conference.AllowGuestsToJoin = dto.AllowGuestsToJoin.Value;
            if (dto.EnableChat.HasValue) conference.EnableChat = dto.EnableChat.Value;
            if (dto.EnableScreenShare.HasValue) conference.EnableScreenShare = dto.EnableScreenShare.Value;
            if (dto.EnableRecording.HasValue) conference.EnableRecording = dto.EnableRecording.Value;
            if (dto.EnableWhiteboard.HasValue) conference.EnableWhiteboard = dto.EnableWhiteboard.Value;
            if (dto.EnableBreakoutRooms.HasValue) conference.EnableBreakoutRooms = dto.EnableBreakoutRooms.Value;
            if (dto.EnableWaitingRoom.HasValue) conference.EnableWaitingRoom = dto.EnableWaitingRoom.Value;
            if (dto.MuteParticipantsOnEntry.HasValue) conference.MuteParticipantsOnEntry = dto.MuteParticipantsOnEntry.Value;

            await _context.SaveChangesAsync();

            return await MapToDto(conference);
        }

        public async Task DeleteConferenceAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            conference.Status = ConferenceStatus.Cancelled;
            await _context.SaveChangesAsync();

            // Notify all participants
            await _hubContext.Clients.Group(conferenceId).SendAsync("ConferenceCancelled", conferenceId);
        }

        public async Task<string> GenerateMeetingLinkAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            // Generate a shareable meeting link (customize base URL as needed)
            return $"https://yourdomain.com/meet/{conferenceId}";
        }

        public async Task<List<VideoConferenceDto>> GetUserConferencesAsync(string userId)
        {
            var conferences = await _context.VideoConferences
                .Include(c => c.Host)
                .Include(c => c.Participants)
                .Where(c => c.HostId == userId || c.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(c => c.ScheduledStartTime)
                .ToListAsync();

            var dtos = new List<VideoConferenceDto>();
            foreach (var conference in conferences)
            {
                dtos.Add(await MapToDto(conference));
            }
            return dtos;
        }

        public async Task<VideoConferenceDto> StartConferenceAsync(string conferenceId, string userId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Host)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != userId)
                throw new UnauthorizedAccessException("Only the host can start the conference");

            conference.Status = ConferenceStatus.InProgress;
            conference.ActualStartTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify participants
            await _hubContext.Clients.Group(conferenceId).SendAsync("ConferenceStarted", conferenceId);

            return await MapToDto(conference);
        }

        public async Task EndConferenceAsync(string conferenceId, string userId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != userId)
                throw new UnauthorizedAccessException("Only the host can end the conference");

            conference.Status = ConferenceStatus.Ended;
            conference.ActualEndTime = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify all participants
            await _hubContext.Clients.Group(conferenceId).SendAsync("ConferenceEnded", conferenceId);
        }

        // ========== Participant Management ==========

        public async Task<ParticipantDto> JoinConferenceAsync(string conferenceId, JoinConferenceDto dto)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.LockMeeting && !dto.IsGuest)
                throw new InvalidOperationException("Meeting is locked");

            if (conference.Participants.Count >= conference.MaxParticipants)
                throw new InvalidOperationException("Conference is full");

            if (conference.RequirePassword && !string.IsNullOrEmpty(conference.PasswordHash))
            {
                if (string.IsNullOrEmpty(dto.Password) || !BCrypt.Net.BCrypt.Verify(dto.Password, conference.PasswordHash))
                    throw new UnauthorizedAccessException("Invalid password");
            }

            var participant = new ConferenceParticipant
            {
                ConferenceId = conference.Id,
                UserId = dto.UserId,
                DisplayName = dto.DisplayName,
                Email = dto.Email,
                IsGuest = dto.IsGuest,
                IsHost = conference.HostId == dto.UserId,
                Status = conference.EnableWaitingRoom && !dto.IsGuest
                    ? ParticipantStatus.InWaitingRoom
                    : ParticipantStatus.Joined,
                JoinedAt = DateTime.UtcNow,
                IsAudioMuted = conference.MuteParticipantsOnEntry,
                PeerId = Guid.NewGuid().ToString()
            };

            _context.ConferenceParticipants.Add(participant);
            await _context.SaveChangesAsync();

            var participantDto = MapToParticipantDto(participant);

            // Notify other participants
            if (participant.Status == ParticipantStatus.Joined)
            {
                await _hubContext.Clients.Group(conferenceId).SendAsync("ParticipantJoined", participantDto);
            }

            return participantDto;
        }

        public async Task LeaveConferenceAsync(string conferenceId, string userId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var participant = conference.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            participant.Status = ParticipantStatus.Left;
            participant.LeftAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify others
            await _hubContext.Clients.Group(conferenceId).SendAsync("ParticipantLeft", participant.PeerId, participant.DisplayName);
        }

        public async Task RemoveParticipantAsync(string conferenceId, string participantId, string requesterId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var requester = conference.Participants.FirstOrDefault(p => p.UserId == requesterId);
            if (requester == null || (!requester.IsHost && !requester.IsCoHost))
                throw new UnauthorizedAccessException("Only hosts can remove participants");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            participant.Status = ParticipantStatus.Removed;
            participant.LeftAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify participant and others
            await _hubContext.Clients.Client(participant.PeerId).SendAsync("RemovedFromConference", "You have been removed from the conference");
            await _hubContext.Clients.Group(conferenceId).SendAsync("ParticipantRemoved", participant.PeerId);
        }

        public async Task AdmitFromWaitingRoomAsync(string conferenceId, string participantId, string hostId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can admit participants");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            participant.Status = ParticipantStatus.Joined;
            await _context.SaveChangesAsync();

            // Notify participant
            await _hubContext.Clients.Client(participant.PeerId).SendAsync("AdmittedToConference");
            await _hubContext.Clients.Group(conferenceId).SendAsync("ParticipantJoined", MapToParticipantDto(participant));
        }

        public async Task MakeCoHostAsync(string conferenceId, string participantId, string hostId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can make someone a co-host");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            participant.IsCoHost = true;
            participant.CanShareScreen = true;
            participant.CanRecord = true;
            participant.CanUseWhiteboard = true;

            await _context.SaveChangesAsync();

            // Notify participant
            await _hubContext.Clients.Client(participant.PeerId).SendAsync("MadeCoHost");
        }

        public async Task<List<ParticipantDto>> GetParticipantsAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                    .ThenInclude(p => p.BreakoutRoom)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            return conference.Participants
                .Where(p => p.Status == ParticipantStatus.Joined || p.Status == ParticipantStatus.InWaitingRoom)
                .Select(MapToParticipantDto)
                .ToList();
        }

        public async Task UpdateParticipantPermissionsAsync(
            string conferenceId,
            string participantId,
            UpdateParticipantPermissionsDto dto,
            string requesterId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var requester = conference.Participants.FirstOrDefault(p => p.UserId == requesterId);
            if (requester == null || (!requester.IsHost && !requester.IsCoHost))
                throw new UnauthorizedAccessException("Only hosts can update permissions");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            if (dto.IsCoHost.HasValue) participant.IsCoHost = dto.IsCoHost.Value;
            if (dto.CanShareScreen.HasValue) participant.CanShareScreen = dto.CanShareScreen.Value;
            if (dto.CanRecord.HasValue) participant.CanRecord = dto.CanRecord.Value;
            if (dto.CanUseWhiteboard.HasValue) participant.CanUseWhiteboard = dto.CanUseWhiteboard.Value;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Client(participant.PeerId).SendAsync("PermissionsUpdated", MapToParticipantDto(participant));
        }

        // ========== Media Controls ==========

        public async Task MuteParticipantAsync(string conferenceId, string participantId, string requesterId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var requester = conference.Participants.FirstOrDefault(p => p.UserId == requesterId);
            if (requester == null || (!requester.IsHost && !requester.IsCoHost))
                throw new UnauthorizedAccessException("Only hosts can mute participants");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            participant.IsAudioMuted = true;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Client(participant.PeerId).SendAsync("MutedByHost");
        }

        public async Task UnmuteParticipantAsync(string conferenceId, string participantId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            participant.IsAudioMuted = false;
            await _context.SaveChangesAsync();
        }

        public async Task MuteAllAsync(string conferenceId, string hostId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can mute all");

            foreach (var participant in conference.Participants.Where(p => p.Status == ParticipantStatus.Joined))
            {
                participant.IsAudioMuted = true;
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(conferenceId).SendAsync("MuteAllRequested");
        }

        public async Task RequestUnmuteAsync(string conferenceId, string participantId, string hostId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var host = conference.Participants.FirstOrDefault(p => p.UserId == hostId);
            if (host == null || (!host.IsHost && !host.IsCoHost))
                throw new UnauthorizedAccessException("Only hosts can request unmute");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            await _hubContext.Clients.Client(participant.PeerId).SendAsync("UnmuteRequested");
        }

        public async Task UpdateMediaStatusAsync(
            string conferenceId,
            string participantId,
            bool? isAudioMuted,
            bool? isVideoOff)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            if (isAudioMuted.HasValue) participant.IsAudioMuted = isAudioMuted.Value;
            if (isVideoOff.HasValue) participant.IsVideoOff = isVideoOff.Value;

            await _context.SaveChangesAsync();
        }

        public async Task ToggleHandRaiseAsync(string conferenceId, string participantId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            participant.IsHandRaised = !participant.IsHandRaised;
            await _context.SaveChangesAsync();

            var eventName = participant.IsHandRaised ? "HandRaised" : "HandLowered";
            await _hubContext.Clients.Group(conferenceId).SendAsync(eventName, participant.PeerId);
        }

        // ========== Recording ==========

        public async Task StartRecordingAsync(string conferenceId, string userId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var participant = conference.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null || (!participant.IsHost && !participant.CanRecord))
                throw new UnauthorizedAccessException("Not authorized to record");

            if (!conference.EnableRecording)
                throw new InvalidOperationException("Recording is not enabled for this conference");

            conference.IsRecording = true;
            conference.RecordingStartedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(conferenceId).SendAsync("RecordingStarted");
        }

        public async Task StopRecordingAsync(string conferenceId, string userId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var participant = conference.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null || (!participant.IsHost && !participant.CanRecord))
                throw new UnauthorizedAccessException("Not authorized to stop recording");

            conference.IsRecording = false;
            conference.RecordingEndedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(conferenceId).SendAsync("RecordingStopped");
        }

        public async Task<byte[]> GetRecordingAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (string.IsNullOrEmpty(conference.RecordingUrl))
                throw new InvalidOperationException("No recording available");

            // In a real implementation, you would fetch the recording from storage
            // For now, return empty byte array as placeholder
            return Array.Empty<byte>();
        }

        public async Task<RecordingDto> GetRecordingInfoAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (string.IsNullOrEmpty(conference.RecordingUrl))
                throw new InvalidOperationException("No recording available");

            return new RecordingDto
            {
                RecordingUrl = conference.RecordingUrl,
                StartedAt = conference.RecordingStartedAt ?? DateTime.MinValue,
                EndedAt = conference.RecordingEndedAt,
                Duration = conference.RecordingEndedAt.HasValue && conference.RecordingStartedAt.HasValue
                    ? conference.RecordingEndedAt.Value - conference.RecordingStartedAt.Value
                    : TimeSpan.Zero,
                Format = "mp4"
            };
        }

        // ========== Breakout Rooms ==========

        public async Task<List<BreakoutRoomDto>> CreateBreakoutRoomsAsync(
            string conferenceId,
            int numberOfRooms,
            string hostId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .Include(c => c.BreakoutRooms)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can create breakout rooms");

            if (!conference.EnableBreakoutRooms)
                throw new InvalidOperationException("Breakout rooms are not enabled");

            // Close existing rooms if any
            foreach (var room in conference.BreakoutRooms.Where(r => r.IsOpen))
            {
                room.IsOpen = false;
            }

            // Create new rooms
            var rooms = new List<BreakoutRoom>();
            for (int i = 1; i <= numberOfRooms; i++)
            {
                var room = new BreakoutRoom
                {
                    ConferenceId = conference.Id,
                    RoomName = $"Room {i}",
                    RoomNumber = i,
                    MaxParticipants = 50,
                    IsOpen = true,
                    CreatedAt = DateTime.UtcNow
                };
                rooms.Add(room);
                _context.BreakoutRooms.Add(room);
            }

            await _context.SaveChangesAsync();

            return rooms.Select(r => new BreakoutRoomDto
            {
                Id = r.Id,
                RoomName = r.RoomName,
                RoomNumber = r.RoomNumber,
                ParticipantCount = r.Participants.Count,
                MaxParticipants = r.MaxParticipants,
                IsOpen = r.IsOpen,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task AssignToBreakoutRoomAsync(
            string conferenceId,
            string participantId,
            int roomNumber,
            string hostId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .Include(c => c.BreakoutRooms)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can assign to breakout rooms");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            var room = conference.BreakoutRooms.FirstOrDefault(r => r.RoomNumber == roomNumber && r.IsOpen);
            if (room == null)
                throw new KeyNotFoundException("Breakout room not found");

            participant.BreakoutRoomId = room.Id;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Client(participant.PeerId).SendAsync(
                "AssignedToBreakoutRoom",
                roomNumber,
                room.RoomName);
        }

        public async Task CloseBreakoutRoomsAsync(string conferenceId, string hostId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.BreakoutRooms)
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can close breakout rooms");

            foreach (var room in conference.BreakoutRooms.Where(r => r.IsOpen))
            {
                room.IsOpen = false;
            }

            // Move all participants back to main room
            foreach (var participant in conference.Participants.Where(p => p.BreakoutRoomId.HasValue))
            {
                participant.BreakoutRoomId = null;
            }

            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(conferenceId).SendAsync("AllBreakoutRoomsClosed");
        }

        public async Task<List<BreakoutRoomDto>> GetBreakoutRoomsAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.BreakoutRooms)
                    .ThenInclude(r => r.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            return conference.BreakoutRooms
                .Where(r => r.IsOpen)
                .Select(r => new BreakoutRoomDto
                {
                    Id = r.Id,
                    RoomName = r.RoomName,
                    RoomNumber = r.RoomNumber,
                    ParticipantCount = r.Participants.Count,
                    MaxParticipants = r.MaxParticipants,
                    Participants = r.Participants.Select(MapToParticipantDto).ToList(),
                    IsOpen = r.IsOpen,
                    CreatedAt = r.CreatedAt
                }).ToList();
        }

        public async Task BroadcastToBreakoutRoomsAsync(string conferenceId, string message, string hostId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can broadcast to breakout rooms");

            await _hubContext.Clients.Group(conferenceId).SendAsync("BroadcastMessage", message);
        }

        // ========== Chat ==========

        public async Task<ChatMessageDto> SendChatMessageAsync(
            string conferenceId,
            SendChatMessageDto dto,
            string senderId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (!conference.EnableChat)
                throw new InvalidOperationException("Chat is not enabled");

            var sender = conference.Participants.FirstOrDefault(p => p.UserId == senderId);
            if (sender == null)
                throw new KeyNotFoundException("Sender not found");

            var chatMessage = new ConferenceChatMessage
            {
                ConferenceId = conference.Id,
                SenderId = senderId,
                SenderName = sender.DisplayName,
                Message = dto.Message,
                IsPrivate = dto.IsPrivate,
                RecipientId = dto.RecipientId,
                SentAt = DateTime.UtcNow
            };

            _context.ConferenceChatMessages.Add(chatMessage);
            await _context.SaveChangesAsync();

            var chatDto = new ChatMessageDto
            {
                Id = chatMessage.Id,
                SenderId = chatMessage.SenderId,
                SenderName = chatMessage.SenderName,
                Message = chatMessage.Message,
                SentAt = chatMessage.SentAt,
                IsPrivate = chatMessage.IsPrivate,
                RecipientId = chatMessage.RecipientId
            };

            // Broadcast via SignalR (handled by hub)
            return chatDto;
        }

        public async Task<List<ChatMessageDto>> GetChatHistoryAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.ChatMessages)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            return conference.ChatMessages
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    SenderName = m.SenderName,
                    Message = m.Message,
                    SentAt = m.SentAt,
                    IsPrivate = m.IsPrivate,
                    RecipientId = m.RecipientId
                }).ToList();
        }

        public async Task DeleteChatMessageAsync(string conferenceId, int messageId, string requesterId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.ChatMessages)
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var message = conference.ChatMessages.FirstOrDefault(m => m.Id == messageId);
            if (message == null)
                throw new KeyNotFoundException("Message not found");

            var requester = conference.Participants.FirstOrDefault(p => p.UserId == requesterId);
            if (requester == null || (!requester.IsHost && !requester.IsCoHost && message.SenderId != requesterId))
                throw new UnauthorizedAccessException("Not authorized to delete this message");

            _context.ConferenceChatMessages.Remove(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(conferenceId).SendAsync("ChatMessageDeleted", messageId);
        }

        // ========== Screen Sharing ==========

        public async Task StartScreenShareAsync(string conferenceId, string userId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (!conference.EnableScreenShare)
                throw new InvalidOperationException("Screen sharing is not enabled");

            var participant = conference.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            if (!participant.CanShareScreen && !participant.IsHost && !participant.IsCoHost)
                throw new UnauthorizedAccessException("Not authorized to share screen");

            // Stop current screen sharer if any
            var currentSharer = conference.Participants.FirstOrDefault(p => p.IsScreenSharing);
            if (currentSharer != null)
            {
                currentSharer.IsScreenSharing = false;
            }

            participant.IsScreenSharing = true;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(conferenceId).SendAsync("ScreenShareStarted", participant.PeerId);
        }

        public async Task StopScreenShareAsync(string conferenceId, string userId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var participant = conference.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            participant.IsScreenSharing = false;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(conferenceId).SendAsync("ScreenShareStopped", participant.PeerId);
        }

        public async Task<ParticipantDto?> GetScreenSharerAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var sharer = conference.Participants.FirstOrDefault(p => p.IsScreenSharing);
            return sharer != null ? MapToParticipantDto(sharer) : null;
        }

        // ========== Whiteboard ==========

        public async Task UpdateWhiteboardAsync(string conferenceId, WhiteboardDataDto dto, string userId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Whiteboard)
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (!conference.EnableWhiteboard)
                throw new InvalidOperationException("Whiteboard is not enabled");

            var participant = conference.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            if (!participant.CanUseWhiteboard && !participant.IsHost && !participant.IsCoHost)
                throw new UnauthorizedAccessException("Not authorized to use whiteboard");

            if (conference.Whiteboard == null)
            {
                conference.Whiteboard = new WhiteboardData
                {
                    ConferenceId = conference.Id
                };
            }

            conference.Whiteboard.Data = dto.Data;
            conference.Whiteboard.LastModifiedById = userId;
            conference.Whiteboard.LastModifiedByName = participant.DisplayName;
            conference.Whiteboard.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<WhiteboardDataDto> GetWhiteboardAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Whiteboard)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.Whiteboard == null)
            {
                return new WhiteboardDataDto
                {
                    Data = string.Empty,
                    LastModifiedById = string.Empty,
                    LastModifiedByName = string.Empty,
                    LastModifiedAt = DateTime.UtcNow
                };
            }

            return new WhiteboardDataDto
            {
                Data = conference.Whiteboard.Data,
                LastModifiedById = conference.Whiteboard.LastModifiedById,
                LastModifiedByName = conference.Whiteboard.LastModifiedByName,
                LastModifiedAt = conference.Whiteboard.LastModifiedAt
            };
        }

        public async Task ClearWhiteboardAsync(string conferenceId, string userId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Whiteboard)
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var participant = conference.Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null || (!participant.IsHost && !participant.IsCoHost))
                throw new UnauthorizedAccessException("Only hosts can clear the whiteboard");

            if (conference.Whiteboard != null)
            {
                conference.Whiteboard.Data = string.Empty;
                conference.Whiteboard.LastModifiedById = userId;
                conference.Whiteboard.LastModifiedByName = participant.DisplayName;
                conference.Whiteboard.LastModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }

            await _hubContext.Clients.Group(conferenceId).SendAsync("WhiteboardCleared", participant.PeerId);
        }

        // ========== Security ==========

        public async Task LockMeetingAsync(string conferenceId, string hostId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can lock the meeting");

            conference.LockMeeting = true;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(conferenceId).SendAsync("MeetingLocked");
        }

        public async Task UnlockMeetingAsync(string conferenceId, string hostId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can unlock the meeting");

            conference.LockMeeting = false;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(conferenceId).SendAsync("MeetingUnlocked");
        }

        public async Task EnableWaitingRoomAsync(string conferenceId, string hostId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can enable waiting room");

            conference.EnableWaitingRoom = true;
            await _context.SaveChangesAsync();
        }

        public async Task DisableWaitingRoomAsync(string conferenceId, string hostId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can disable waiting room");

            conference.EnableWaitingRoom = false;
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePasswordAsync(string conferenceId, string newPassword, string hostId)
        {
            var conference = await _context.VideoConferences
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            if (conference.HostId != hostId)
                throw new UnauthorizedAccessException("Only the host can update password");

            conference.PasswordHash = !string.IsNullOrEmpty(newPassword)
                ? BCrypt.Net.BCrypt.HashPassword(newPassword)
                : null;
            conference.RequirePassword = !string.IsNullOrEmpty(newPassword);

            await _context.SaveChangesAsync();
        }

        // ========== Analytics ==========

        public async Task<ConferenceAnalyticsDto> GetAnalyticsAsync(string conferenceId)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .Include(c => c.ChatMessages)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var analytics = new ConferenceAnalyticsDto
            {
                TotalParticipantsJoined = conference.Participants.Count(p => p.Status == ParticipantStatus.Joined || p.Status == ParticipantStatus.Left),
                PeakParticipants = conference.Participants.Count(p => p.Status == ParticipantStatus.Joined),
                TotalDuration = conference.ActualEndTime.HasValue && conference.ActualStartTime.HasValue
                    ? conference.ActualEndTime.Value - conference.ActualStartTime.Value
                    : TimeSpan.Zero,
                AverageJoinTime = TimeSpan.FromMinutes(2), // Placeholder
                TotalChatMessages = conference.ChatMessages.Count,
                ScreenShareCount = conference.Participants.Count(p => p.IsScreenSharing),
                ConnectionQualityDistribution = conference.Participants
                    .GroupBy(p => p.Quality)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return analytics;
        }

        public async Task UpdateConnectionQualityAsync(
            string conferenceId,
            string participantId,
            ConnectionQuality quality)
        {
            var conference = await _context.VideoConferences
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

            if (conference == null)
                throw new KeyNotFoundException($"Conference {conferenceId} not found");

            var participant = conference.Participants.FirstOrDefault(p => p.Id.ToString() == participantId);
            if (participant == null)
                throw new KeyNotFoundException("Participant not found");

            participant.Quality = quality;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.OthersInGroup(conferenceId).SendAsync(
                "ParticipantQualityUpdated",
                participant.PeerId,
                quality.ToString());
        }

        // ========== Helper Methods ==========

        private string GenerateConferenceId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        }

        private async Task<VideoConferenceDto> MapToDto(VideoConference conference)
        {
            var host = conference.Host ?? await _context.Users.FindAsync(conference.HostId);

            return new VideoConferenceDto
            {
                Id = conference.Id,
                ConferenceId = conference.ConferenceId,
                Title = conference.Title,
                Description = conference.Description,
                RequirePassword = conference.RequirePassword,
                ScheduledStartTime = conference.ScheduledStartTime,
                ScheduledEndTime = conference.ScheduledEndTime,
                DurationMinutes = conference.DurationMinutes,
                HostId = conference.HostId,
                HostName = host?.FullName ?? "Unknown",
                ParticipantCount = conference.Participants?.Count(p => p.Status == ParticipantStatus.Joined) ?? 0,
                MaxParticipants = conference.MaxParticipants,
                AllowGuestsToJoin = conference.AllowGuestsToJoin,
                Status = conference.Status,
                ActualStartTime = conference.ActualStartTime,
                ActualEndTime = conference.ActualEndTime,
                EnableChat = conference.EnableChat,
                EnableScreenShare = conference.EnableScreenShare,
                EnableRecording = conference.EnableRecording,
                EnableWhiteboard = conference.EnableWhiteboard,
                EnableBreakoutRooms = conference.EnableBreakoutRooms,
                EnableWaitingRoom = conference.EnableWaitingRoom,
                MuteParticipantsOnEntry = conference.MuteParticipantsOnEntry,
                IsRecording = conference.IsRecording,
                RecordingUrl = conference.RecordingUrl,
                LockMeeting = conference.LockMeeting,
                MeetingLink = $"https://yourdomain.com/meet/{conference.ConferenceId}",
                CreatedAt = conference.CreatedAt
            };
        }

        private ParticipantDto MapToParticipantDto(ConferenceParticipant participant)
        {
            return new ParticipantDto
            {
                Id = participant.Id,
                DisplayName = participant.DisplayName,
                Email = participant.Email,
                IsGuest = participant.IsGuest,
                IsHost = participant.IsHost,
                IsCoHost = participant.IsCoHost,
                Status = participant.Status,
                JoinedAt = participant.JoinedAt,
                LeftAt = participant.LeftAt,
                IsAudioMuted = participant.IsAudioMuted,
                IsVideoOff = participant.IsVideoOff,
                IsHandRaised = participant.IsHandRaised,
                IsScreenSharing = participant.IsScreenSharing,
                PeerId = participant.PeerId,
                Quality = participant.Quality,
                CanShareScreen = participant.CanShareScreen,
                CanRecord = participant.CanRecord,
                CanUseWhiteboard = participant.CanUseWhiteboard,
                BreakoutRoomId = participant.BreakoutRoomId,
                BreakoutRoomName = participant.BreakoutRoom?.RoomName
            };
        }
    }
}
