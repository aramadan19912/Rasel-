using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs.VideoConference;
using static Backend.Application.DTOs.VideoConference.VideoConferenceDtos;
using Backend.Application.Interfaces;
using Backend.Domain.Entities.VideoConference;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Services;

public class VideoConferenceService : IVideoConferenceService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;

    public VideoConferenceService(ApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    // ===== Conference CRUD =====
    public async Task<List<VideoConferenceDto>> GetAllAsync(string userId, int pageNumber = 1, int pageSize = 50)
    {
        if (!await _userService.HasPermissionAsync(userId, "conference.read"))
            throw new UnauthorizedAccessException("Permission denied: conference.read");

        var conferences = await _context.VideoConferences
            .Where(c => c.HostId == userId)
            .OrderByDescending(c => c.ScheduledStartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return conferences.Select(MapToDto).ToList();
    }

    public async Task<List<VideoConferenceDto>> GetUpcomingAsync(string userId)
    {
        var now = DateTime.UtcNow;
        var conferences = await _context.VideoConferences
            .Where(c => c.HostId == userId && c.ScheduledStartTime > now && c.Status == ConferenceStatus.Scheduled)
            .OrderBy(c => c.ScheduledStartTime)
            .ToListAsync();

        return conferences.Select(MapToDto).ToList();
    }

    public async Task<List<VideoConferenceDto>> GetScheduledAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var conferences = await _context.VideoConferences
            .Where(c => c.HostId == userId &&
                       c.ScheduledStartTime >= startDate &&
                       c.ScheduledStartTime <= endDate)
            .OrderBy(c => c.ScheduledStartTime)
            .ToListAsync();

        return conferences.Select(MapToDto).ToList();
    }

    public async Task<List<VideoConferenceDto>> GetPastAsync(string userId, int pageNumber = 1, int pageSize = 50)
    {
        var conferences = await _context.VideoConferences
            .Where(c => c.HostId == userId && c.Status == ConferenceStatus.Ended)
            .OrderByDescending(c => c.ActualStartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return conferences.Select(MapToDto).ToList();
    }

    public async Task<VideoConferenceDto> GetByIdAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "conference.read"))
            throw new UnauthorizedAccessException("Permission denied: conference.read");

        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {id} not found");

        return MapToDto(conference);
    }

    public async Task<VideoConferenceDto> GetByConferenceIdAsync(string conferenceId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.ConferenceId == conferenceId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        return MapToDto(conference);
    }

    public async Task<VideoConferenceDto> CreateAsync(CreateConferenceDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "conference.create"))
            throw new UnauthorizedAccessException("Permission denied: conference.create");

        var conference = new VideoConference
        {
            ConferenceId = GenerateConferenceId(),
            Title = dto.Title,
            Description = dto.Description,
            RequirePassword = dto.RequirePassword,
            Password = dto.Password,
            ScheduledStartTime = dto.ScheduledStartTime,
            DurationMinutes = dto.DurationMinutes,
            HostId = dto.HostId,
            MaxParticipants = dto.MaxParticipants ?? 100,
            AllowGuestsToJoin = dto.AllowGuestsToJoin,
            EnableRecording = dto.EnableRecording,
            CalendarEventId = dto.CalendarEventId,
            Status = ConferenceStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.DurationMinutes > 0)
        {
            conference.ScheduledEndTime = dto.ScheduledStartTime.AddMinutes(dto.DurationMinutes);
        }

        _context.VideoConferences.Add(conference);
        await _context.SaveChangesAsync();

        return MapToDto(conference);
    }

    public async Task<bool> UpdateAsync(int id, UpdateConferenceDto dto, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == id && c.HostId == userId);
        if (conference == null) return false;

        if (dto.Title != null) conference.Title = dto.Title;
        if (dto.Description != null) conference.Description = dto.Description;
        if (dto.ScheduledStartTime.HasValue) conference.ScheduledStartTime = dto.ScheduledStartTime.Value;
        if (dto.DurationMinutes.HasValue)
        {
            conference.DurationMinutes = dto.DurationMinutes.Value;
            conference.ScheduledEndTime = conference.ScheduledStartTime.AddMinutes(dto.DurationMinutes.Value);
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

        conference.LastModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "conference.delete"))
            throw new UnauthorizedAccessException("Permission denied: conference.delete");

        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == id && c.HostId == userId);
        if (conference == null || conference.Status == ConferenceStatus.InProgress) return false;

        _context.VideoConferences.Remove(conference);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelAsync(int id, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == id && c.HostId == userId);
        if (conference == null) return false;

        conference.Status = ConferenceStatus.Cancelled;
        await _context.SaveChangesAsync();

        return true;
    }

    // ===== Conference Control =====
    public async Task<VideoConferenceDto> StartConferenceAsync(int id, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == id && c.HostId == userId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {id} not found");

        conference.Status = ConferenceStatus.InProgress;
        conference.ActualStartTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return MapToDto(conference);
    }

    public async Task<bool> EndConferenceAsync(int id, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == id && c.HostId == userId);
        if (conference == null) return false;

        conference.Status = ConferenceStatus.Ended;
        conference.ActualEndTime = DateTime.UtcNow;

        if (conference.ActualStartTime.HasValue)
        {
            conference.TotalDuration = conference.ActualEndTime.Value - conference.ActualStartTime.Value;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LockConferenceAsync(int id, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == id && c.HostId == userId);
        if (conference == null) return false;

        conference.LockMeeting = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnlockConferenceAsync(int id, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == id && c.HostId == userId);
        if (conference == null) return false;

        conference.LockMeeting = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Participants =====
    public async Task<List<ParticipantDto>> GetParticipantsAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        return conference.Participants.Select(MapParticipantToDto).ToList();
    }

    public async Task<List<ParticipantDto>> GetActiveParticipantsAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        return conference.Participants
            .Where(p => p.Status == ParticipantStatus.Joined)
            .Select(MapParticipantToDto)
            .ToList();
    }

    public async Task<ParticipantDto> JoinConferenceAsync(JoinConferenceDto dto)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.ConferenceId == dto.ConferenceId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {dto.ConferenceId} not found");

        // Check password if required
        if (conference.RequirePassword && conference.Password != dto.Password)
            throw new UnauthorizedAccessException("Invalid password");

        // Check if locked
        if (conference.LockMeeting)
            throw new UnauthorizedAccessException("Conference is locked");

        // Check capacity
        var activeCount = conference.Participants.Count(p => p.Status == ParticipantStatus.Joined);
        if (activeCount >= conference.MaxParticipants)
            throw new InvalidOperationException("Conference is full");

        var participant = new ConferenceParticipant
        {
            ConferenceId = conference.Id,
            UserId = dto.UserId,
            DisplayName = dto.DisplayName,
            Email = dto.Email,
            IsGuest = dto.IsGuest,
            Status = conference.EnableWaitingRoom ? ParticipantStatus.InWaitingRoom : ParticipantStatus.Joined,
            JoinedAt = DateTime.UtcNow,
            IsAudioMuted = conference.MuteParticipantsOnEntry,
            PeerId = Guid.NewGuid().ToString()
        };

        conference.Participants.Add(participant);
        conference.TotalParticipantsJoined++;

        var currentActive = conference.Participants.Count(p => p.Status == ParticipantStatus.Joined);
        if (currentActive > conference.PeakParticipants)
        {
            conference.PeakParticipants = currentActive;
        }

        await _context.SaveChangesAsync();

        return MapParticipantToDto(participant);
    }

    public async Task<bool> LeaveConferenceAsync(int conferenceId, string userId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId && p.UserId == userId);

        if (participant == null) return false;

        participant.Status = ParticipantStatus.Left;
        participant.LeftAt = DateTime.UtcNow;

        if (participant.JoinedAt != null)
        {
            participant.TotalTime = (participant.LeftAt.Value - participant.JoinedAt).Value;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveParticipantAsync(int conferenceId, int participantId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        if (participant == null) return false;

        participant.Status = ParticipantStatus.Removed;
        participant.LeftAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AdmitFromWaitingRoomAsync(int conferenceId, int participantId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        if (participant == null || participant.Status != ParticipantStatus.InWaitingRoom) return false;

        participant.Status = ParticipantStatus.Joined;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AdmitAllFromWaitingRoomAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);

        if (conference == null) return false;

        foreach (var participant in conference.Participants.Where(p => p.Status == ParticipantStatus.InWaitingRoom))
        {
            participant.Status = ParticipantStatus.Joined;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Participant Permissions =====
    public async Task<bool> MakeCoHostAsync(int conferenceId, int participantId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        if (participant == null) return false;

        participant.IsCoHost = true;
        participant.CanShareScreen = true;
        participant.CanRecord = true;
        participant.CanUseWhiteboard = true;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RevokeCoHostAsync(int conferenceId, int participantId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        if (participant == null) return false;

        participant.IsCoHost = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateParticipantPermissionsAsync(int conferenceId, int participantId, UpdateParticipantPermissionsDto dto, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        if (participant == null) return false;

        if (dto.IsCoHost.HasValue) participant.IsCoHost = dto.IsCoHost.Value;
        if (dto.CanShareScreen.HasValue) participant.CanShareScreen = dto.CanShareScreen.Value;
        if (dto.CanRecord.HasValue) participant.CanRecord = dto.CanRecord.Value;
        if (dto.CanUseWhiteboard.HasValue) participant.CanUseWhiteboard = dto.CanUseWhiteboard.Value;

        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Media Control =====
    public async Task<bool> MuteParticipantAsync(int conferenceId, int participantId, string userId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        if (participant == null) return false;

        participant.IsAudioMuted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnmuteParticipantAsync(int conferenceId, int participantId, string userId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        if (participant == null) return false;

        participant.IsAudioMuted = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MuteAllAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);

        if (conference == null) return false;

        foreach (var participant in conference.Participants.Where(p => p.Status == ParticipantStatus.Joined))
        {
            participant.IsAudioMuted = true;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UnmuteAllAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);

        if (conference == null) return false;

        foreach (var participant in conference.Participants.Where(p => p.Status == ParticipantStatus.Joined))
        {
            participant.IsAudioMuted = false;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableParticipantVideoAsync(int conferenceId, int participantId, string userId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        if (participant == null) return false;

        participant.IsVideoOff = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableParticipantVideoAsync(int conferenceId, int participantId, string userId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        if (participant == null) return false;

        participant.IsVideoOff = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Screen Sharing =====
    public async Task<bool> StartScreenShareAsync(int conferenceId, string userId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId && p.UserId == userId);

        if (participant == null || !participant.CanShareScreen) return false;

        // Stop any other screen shares
        var otherShares = await _context.ConferenceParticipants
            .Where(p => p.ConferenceId == conferenceId && p.IsScreenSharing)
            .ToListAsync();

        foreach (var share in otherShares)
        {
            share.IsScreenSharing = false;
        }

        participant.IsScreenSharing = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StopScreenShareAsync(int conferenceId, string userId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId && p.UserId == userId);

        if (participant == null) return false;

        participant.IsScreenSharing = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ParticipantDto> GetCurrentScreenSharerAsync(int conferenceId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId && p.IsScreenSharing);

        if (participant == null)
            throw new KeyNotFoundException("No active screen share");

        return MapParticipantToDto(participant);
    }

    // ===== Recording =====
    public async Task<bool> StartRecordingAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .FirstOrDefaultAsync(c => c.Id == conferenceId && (c.HostId == userId || c.Participants.Any(p => p.UserId == userId && p.IsCoHost)));

        if (conference == null || !conference.EnableRecording) return false;

        conference.IsRecording = true;
        conference.RecordingStartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> StopRecordingAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .FirstOrDefaultAsync(c => c.Id == conferenceId && (c.HostId == userId || c.Participants.Any(p => p.UserId == userId && p.IsCoHost)));

        if (conference == null) return false;

        conference.IsRecording = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> PauseRecordingAsync(int conferenceId, string userId)
    {
        // Recording pause logic
        return await StopRecordingAsync(conferenceId, userId);
    }

    public async Task<bool> ResumeRecordingAsync(int conferenceId, string userId)
    {
        // Recording resume logic
        return await StartRecordingAsync(conferenceId, userId);
    }

    public async Task<RecordingDto> GetRecordingAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId);
        if (conference == null || string.IsNullOrEmpty(conference.RecordingUrl))
            throw new KeyNotFoundException("Recording not found");

        return new RecordingDto
        {
            RecordingUrl = conference.RecordingUrl,
            FileSize = conference.RecordingSize,
            Duration = conference.TotalDuration,
            StartedAt = conference.RecordingStartedAt ?? DateTime.MinValue,
            Format = "mp4"
        };
    }

    public async Task<List<RecordingDto>> GetRecordingsAsync(string userId)
    {
        var conferences = await _context.VideoConferences
            .Where(c => c.HostId == userId && c.RecordingUrl != null)
            .ToListAsync();

        return conferences.Select(c => new RecordingDto
        {
            RecordingUrl = c.RecordingUrl!,
            FileSize = c.RecordingSize,
            Duration = c.TotalDuration,
            StartedAt = c.RecordingStartedAt ?? DateTime.MinValue,
            Format = "mp4"
        }).ToList();
    }

    public async Task<bool> DeleteRecordingAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);

        if (conference == null) return false;

        conference.RecordingUrl = null;
        conference.RecordingSize = 0;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Breakout Rooms =====
    public async Task<List<BreakoutRoomDto>> CreateBreakoutRoomsAsync(int conferenceId, CreateBreakoutRoomsDto dto, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);

        if (conference == null || !conference.EnableBreakoutRooms)
            throw new InvalidOperationException("Breakout rooms not enabled");

        var rooms = new List<BreakoutRoom>();
        for (int i = 0; i < dto.NumberOfRooms; i++)
        {
            var room = new BreakoutRoom
            {
                ConferenceId = conferenceId,
                RoomName = $"Room {i + 1}",
                RoomNumber = i + 1,
                IsOpen = true,
                CreatedAt = DateTime.UtcNow
            };
            rooms.Add(room);
            _context.BreakoutRooms.Add(room);
        }

        await _context.SaveChangesAsync();

        // Assign participants if requested
        if (dto.AssignAutomatically)
        {
            var activeParticipants = conference.Participants
                .Where(p => p.Status == ParticipantStatus.Joined)
                .ToList();

            for (int i = 0; i < activeParticipants.Count; i++)
            {
                activeParticipants[i].BreakoutRoomId = rooms[i % dto.NumberOfRooms].Id;
            }

            await _context.SaveChangesAsync();
        }

        return rooms.Select(r => new BreakoutRoomDto
        {
            Id = r.Id,
            RoomName = r.RoomName,
            RoomNumber = r.RoomNumber,
            ParticipantCount = conference.Participants.Count(p => p.BreakoutRoomId == r.Id),
            IsOpen = r.IsOpen,
            CreatedAt = r.CreatedAt
        }).ToList();
    }

    public async Task<List<BreakoutRoomDto>> GetBreakoutRoomsAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.BreakoutRooms)
            .ThenInclude(r => r.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        return conference.BreakoutRooms.Select(r => new BreakoutRoomDto
        {
            Id = r.Id,
            RoomName = r.RoomName,
            RoomNumber = r.RoomNumber,
            ParticipantCount = r.Participants.Count,
            MaxParticipants = r.MaxParticipants,
            IsOpen = r.IsOpen,
            CreatedAt = r.CreatedAt,
            Participants = r.Participants.Select(MapParticipantToDto).ToList()
        }).ToList();
    }

    public async Task<bool> AssignToBreakoutRoomAsync(int conferenceId, int participantId, int roomId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.Id == participantId && p.ConferenceId == conferenceId);

        var room = await _context.BreakoutRooms
            .FirstOrDefaultAsync(r => r.Id == roomId && r.ConferenceId == conferenceId);

        if (participant == null || room == null) return false;

        participant.BreakoutRoomId = roomId;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MoveToBreakoutRoomAsync(int conferenceId, int participantId, int roomId, string userId)
    {
        return await AssignToBreakoutRoomAsync(conferenceId, participantId, roomId, userId);
    }

    public async Task<bool> BroadcastMessageToRoomsAsync(int conferenceId, string message, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        // This would send a broadcast message to all breakout rooms
        // Implementation would depend on signaling mechanism
        return true;
    }

    public async Task<bool> CloseAllBreakoutRoomsAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.BreakoutRooms)
            .ThenInclude(r => r.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);

        if (conference == null) return false;

        foreach (var room in conference.BreakoutRooms)
        {
            room.IsOpen = false;
            room.ClosedAt = DateTime.UtcNow;

            // Move participants back to main room
            foreach (var participant in room.Participants)
            {
                participant.BreakoutRoomId = null;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CloseBreakoutRoomAsync(int conferenceId, int roomId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        var room = await _context.BreakoutRooms
            .Include(r => r.Participants)
            .FirstOrDefaultAsync(r => r.Id == roomId && r.ConferenceId == conferenceId);

        if (room == null) return false;

        room.IsOpen = false;
        room.ClosedAt = DateTime.UtcNow;

        foreach (var participant in room.Participants)
        {
            participant.BreakoutRoomId = null;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Chat =====
    public async Task<List<ChatMessageDto>> GetChatMessagesAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.ChatMessages)
            .FirstOrDefaultAsync(c => c.Id == conferenceId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        return conference.ChatMessages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            SenderId = m.SenderId,
            SenderName = m.Sender?.FullName ?? "",
            Message = m.Message,
            SentAt = m.SentAt,
            IsPrivate = m.IsPrivate,
            RecipientId = m.RecipientId
        }).ToList();
    }

    public async Task<ChatMessageDto> SendChatMessageAsync(int conferenceId, SendChatMessageDto dto, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId);
        if (conference == null || !conference.EnableChat)
            throw new InvalidOperationException("Chat not enabled");

        var chatMessage = new ConferenceChatMessage
        {
            ConferenceId = conferenceId,
            SenderId = userId,
            Message = dto.Message,
            SentAt = DateTime.UtcNow,
            IsPrivate = dto.IsPrivate,
            RecipientId = dto.RecipientId
        };

        _context.ConferenceChatMessages.Add(chatMessage);
        await _context.SaveChangesAsync();

        return new ChatMessageDto
        {
            Id = chatMessage.Id,
            SenderId = chatMessage.SenderId,
            Message = chatMessage.Message,
            SentAt = chatMessage.SentAt,
            IsPrivate = chatMessage.IsPrivate,
            RecipientId = chatMessage.RecipientId
        };
    }

    public async Task<bool> DeleteChatMessageAsync(int conferenceId, int messageId, string userId)
    {
        var message = await _context.ConferenceChatMessages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ConferenceId == conferenceId && m.SenderId == userId);

        if (message == null) return false;

        _context.ConferenceChatMessages.Remove(message);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearChatHistoryAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        var messages = await _context.ConferenceChatMessages.Where(m => m.ConferenceId == conferenceId).ToListAsync();
        _context.ConferenceChatMessages.RemoveRange(messages);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableChatAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        conference.EnableChat = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableChatAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        conference.EnableChat = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Whiteboard =====
    public async Task<WhiteboardDataDto> GetWhiteboardDataAsync(int conferenceId, string userId)
    {
        var whiteboard = await _context.WhiteboardData.FirstOrDefaultAsync(w => w.ConferenceId == conferenceId);
        if (whiteboard == null)
            return new WhiteboardDataDto { Data = "{}", LastModifiedAt = DateTime.UtcNow };

        return new WhiteboardDataDto
        {
            Data = whiteboard.Data,
            LastModifiedById = whiteboard.LastModifiedById,
            LastModifiedByName = whiteboard.LastModifiedBy?.FullName ?? "",
            LastModifiedAt = whiteboard.LastModifiedAt
        };
    }

    public async Task<bool> UpdateWhiteboardDataAsync(int conferenceId, string data, string userId)
    {
        var whiteboard = await _context.WhiteboardData.FirstOrDefaultAsync(w => w.ConferenceId == conferenceId);

        if (whiteboard == null)
        {
            whiteboard = new WhiteboardData
            {
                ConferenceId = conferenceId,
                Data = data,
                LastModifiedById = userId,
                LastModifiedAt = DateTime.UtcNow
            };
            _context.WhiteboardData.Add(whiteboard);
        }
        else
        {
            whiteboard.Data = data;
            whiteboard.LastModifiedById = userId;
            whiteboard.LastModifiedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearWhiteboardAsync(int conferenceId, string userId)
    {
        var whiteboard = await _context.WhiteboardData.FirstOrDefaultAsync(w => w.ConferenceId == conferenceId);
        if (whiteboard == null) return false;

        whiteboard.Data = "{}";
        whiteboard.LastModifiedById = userId;
        whiteboard.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EnableWhiteboardAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        conference.EnableWhiteboard = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableWhiteboardAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        conference.EnableWhiteboard = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Waiting Room =====
    public async Task<List<ParticipantDto>> GetWaitingRoomParticipantsAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        return conference.Participants
            .Where(p => p.Status == ParticipantStatus.InWaitingRoom)
            .Select(MapParticipantToDto)
            .ToList();
    }

    public async Task<bool> EnableWaitingRoomAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        conference.EnableWaitingRoom = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DisableWaitingRoomAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        conference.EnableWaitingRoom = false;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Hand Raise =====
    public async Task<bool> RaiseHandAsync(int conferenceId, string userId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId && p.UserId == userId);

        if (participant == null) return false;

        participant.IsHandRaised = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LowerHandAsync(int conferenceId, string userId)
    {
        var participant = await _context.ConferenceParticipants
            .FirstOrDefaultAsync(p => p.ConferenceId == conferenceId && p.UserId == userId);

        if (participant == null) return false;

        participant.IsHandRaised = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> LowerAllHandsAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);

        if (conference == null) return false;

        foreach (var participant in conference.Participants)
        {
            participant.IsHandRaised = false;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ParticipantDto>> GetRaisedHandsAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        return conference.Participants
            .Where(p => p.IsHandRaised)
            .Select(MapParticipantToDto)
            .ToList();
    }

    // ===== Meeting Links =====
    public async Task<MeetingLinkDto> GetMeetingLinkAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId);
        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        return new MeetingLinkDto
        {
            ConferenceId = conference.ConferenceId,
            MeetingUrl = $"/conference/{conference.ConferenceId}",
            Password = conference.RequirePassword ? conference.Password : null,
            ExpiresAt = conference.ScheduledEndTime ?? DateTime.UtcNow.AddDays(1)
        };
    }

    public async Task<MeetingLinkDto> GenerateMeetingLinkAsync(int conferenceId, string userId)
    {
        return await GetMeetingLinkAsync(conferenceId, userId);
    }

    public async Task<bool> RegenerateMeetingLinkAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        conference.ConferenceId = GenerateConferenceId();
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Analytics =====
    public async Task<ConferenceAnalyticsDto> GetAnalyticsAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        var chatCount = await _context.ConferenceChatMessages.CountAsync(m => m.ConferenceId == conferenceId);

        return new ConferenceAnalyticsDto
        {
            TotalParticipantsJoined = conference.TotalParticipantsJoined,
            PeakParticipants = conference.PeakParticipants,
            TotalDuration = conference.TotalDuration,
            TotalChatMessages = chatCount,
            ConnectionQualityDistribution = conference.Participants
                .GroupBy(p => p.Quality)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<List<ParticipantDto>> GetParticipantHistoryAsync(int conferenceId, string userId)
    {
        var conference = await _context.VideoConferences
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == conferenceId);

        if (conference == null)
            throw new KeyNotFoundException($"Conference with ID {conferenceId} not found");

        return conference.Participants
            .OrderBy(p => p.JoinedAt)
            .Select(MapParticipantToDto)
            .ToList();
    }

    // ===== Integration =====
    public async Task<VideoConferenceDto> CreateFromCalendarEventAsync(int calendarEventId, string userId)
    {
        var calendarEvent = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id == calendarEventId);
        if (calendarEvent == null)
            throw new KeyNotFoundException($"Calendar event with ID {calendarEventId} not found");

        var conference = new VideoConference
        {
            ConferenceId = GenerateConferenceId(),
            Title = calendarEvent.Title,
            Description = calendarEvent.Description,
            ScheduledStartTime = calendarEvent.StartTime,
            ScheduledEndTime = calendarEvent.EndTime,
            DurationMinutes = (int)(calendarEvent.EndTime - calendarEvent.StartTime).TotalMinutes,
            HostId = userId,
            CalendarEventId = calendarEventId,
            Status = ConferenceStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _context.VideoConferences.Add(conference);
        await _context.SaveChangesAsync();

        return MapToDto(conference);
    }

    public async Task<bool> LinkToCalendarEventAsync(int conferenceId, int calendarEventId, string userId)
    {
        var conference = await _context.VideoConferences.FirstOrDefaultAsync(c => c.Id == conferenceId && c.HostId == userId);
        if (conference == null) return false;

        conference.CalendarEventId = calendarEventId;
        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Statistics =====
    public async Task<ConferenceStatisticsDto> GetStatisticsAsync(string userId)
    {
        var totalConferences = await _context.VideoConferences.CountAsync(c => c.HostId == userId);
        var upcomingConferences = await _context.VideoConferences.CountAsync(c => c.HostId == userId && c.Status == ConferenceStatus.Scheduled);
        var completedConferences = await _context.VideoConferences.CountAsync(c => c.HostId == userId && c.Status == ConferenceStatus.Ended);
        var cancelledConferences = await _context.VideoConferences.CountAsync(c => c.HostId == userId && c.Status == ConferenceStatus.Cancelled);

        var totalParticipants = await _context.VideoConferences
            .Where(c => c.HostId == userId)
            .SumAsync(c => c.TotalParticipantsJoined);

        var totalMeetingTime = await _context.VideoConferences
            .Where(c => c.HostId == userId && c.Status == ConferenceStatus.Ended)
            .SumAsync(c => c.TotalDuration.Ticks);

        var totalRecordings = await _context.VideoConferences.CountAsync(c => c.HostId == userId && c.RecordingUrl != null);

        return new ConferenceStatisticsDto
        {
            TotalConferences = totalConferences,
            UpcomingConferences = upcomingConferences,
            CompletedConferences = completedConferences,
            CancelledConferences = cancelledConferences,
            TotalParticipantsHosted = totalParticipants,
            TotalMeetingTime = TimeSpan.FromTicks(totalMeetingTime),
            TotalRecordings = totalRecordings
        };
    }

    // ===== Helper Methods =====
    private string GenerateConferenceId()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
    }

    private VideoConferenceDto MapToDto(VideoConference conference)
    {
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
            HostName = conference.Host?.FullName ?? "",
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
            MeetingLink = $"/conference/{conference.ConferenceId}",
            CreatedAt = conference.CreatedAt
        };
    }

    private ParticipantDto MapParticipantToDto(ConferenceParticipant participant)
    {
        return new ParticipantDto
        {
            Id = participant.Id,
            DisplayName = participant.DisplayName,
            Email = participant.Email,
            IsGuest = participant.IsGuest,
            IsHost = participant.Conference?.HostId == participant.UserId,
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
