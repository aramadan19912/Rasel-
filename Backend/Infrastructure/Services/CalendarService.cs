using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs.Calendar;
using Backend.Application.Interfaces;
using Backend.Domain.Entities.Calendar;
using Backend.Infrastructure.Data;

namespace Backend.Infrastructure.Services;

public class CalendarService : ICalendarService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserService _userService;
    private readonly IVideoConferenceService _videoConferenceService;

    public CalendarService(
        ApplicationDbContext context,
        IUserService userService,
        IVideoConferenceService videoConferenceService)
    {
        _context = context;
        _userService = userService;
        _videoConferenceService = videoConferenceService;
    }

    // ===== Calendar CRUD =====
    public async Task<List<CalendarDto>> GetAllCalendarsAsync(string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.read"))
            throw new UnauthorizedAccessException("Permission denied: calendar.read");

        var calendars = await _context.Calendars
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return calendars.Select(MapCalendarToDto).ToList();
    }

    public async Task<CalendarDto> GetCalendarByIdAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.read"))
            throw new UnauthorizedAccessException("Permission denied: calendar.read");

        var calendar = await _context.Calendars
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        if (calendar == null)
            throw new KeyNotFoundException($"Calendar with ID {id} not found");

        return MapCalendarToDto(calendar);
    }

    public async Task<CalendarDto> CreateCalendarAsync(CreateCalendarDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.create"))
            throw new UnauthorizedAccessException("Permission denied: calendar.create");

        var calendar = new Backend.Domain.Entities.Calendar.Calendar
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            UserId = userId,
            IsDefault = dto.IsDefault,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync();

        return MapCalendarToDto(calendar);
    }

    public async Task<bool> UpdateCalendarAsync(int id, UpdateCalendarDto dto, string userId)
    {
        var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (calendar == null) return false;

        if (dto.Name != null) calendar.Name = dto.Name;
        if (dto.Description != null) calendar.Description = dto.Description;
        if (dto.Color != null) calendar.Color = dto.Color;
        if (dto.IsVisible.HasValue) calendar.IsVisible = dto.IsVisible.Value;
        if (dto.DisplayOrder.HasValue) calendar.DisplayOrder = dto.DisplayOrder.Value;

        calendar.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteCalendarAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.delete"))
            throw new UnauthorizedAccessException("Permission denied: calendar.delete");

        var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (calendar == null || calendar.IsDefault) return false;

        _context.Calendars.Remove(calendar);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CalendarDto> GetDefaultCalendarAsync(string userId)
    {
        var calendar = await _context.Calendars
            .FirstOrDefaultAsync(c => c.UserId == userId && c.IsDefault);

        if (calendar == null)
            throw new KeyNotFoundException("Default calendar not found");

        return MapCalendarToDto(calendar);
    }

    public async Task<bool> SetDefaultCalendarAsync(int id, string userId)
    {
        var allCalendars = await _context.Calendars.Where(c => c.UserId == userId).ToListAsync();

        foreach (var cal in allCalendars)
        {
            cal.IsDefault = cal.Id == id;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Calendar Sharing =====
    public async Task<List<CalendarShareDto>> GetCalendarSharesAsync(int calendarId, string userId)
    {
        var calendar = await _context.Calendars
            .Include(c => c.Shares)
            .FirstOrDefaultAsync(c => c.Id == calendarId && c.UserId == userId);

        if (calendar == null)
            throw new KeyNotFoundException($"Calendar with ID {calendarId} not found");

        return calendar.Shares.Select(s => new CalendarShareDto
        {
            Id = s.Id,
            CalendarId = s.CalendarId,
            CalendarName = calendar.Name,
            SharedWithUserId = s.SharedWithUserId,
            Permission = s.Permission,
            SharedAt = s.SharedAt
        }).ToList();
    }

    public async Task<CalendarShareDto> ShareCalendarAsync(int calendarId, ShareCalendarDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.share"))
            throw new UnauthorizedAccessException("Permission denied: calendar.share");

        var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId && c.UserId == userId);
        if (calendar == null)
            throw new KeyNotFoundException($"Calendar with ID {calendarId} not found");

        var share = new CalendarShare
        {
            CalendarId = calendarId,
            SharedWithUserId = dto.SharedWithUserId,
            Permission = dto.Permission,
            SharedAt = DateTime.UtcNow
        };

        calendar.IsShared = true;
        _context.CalendarShares.Add(share);
        await _context.SaveChangesAsync();

        return new CalendarShareDto
        {
            Id = share.Id,
            CalendarId = share.CalendarId,
            CalendarName = calendar.Name,
            SharedWithUserId = share.SharedWithUserId,
            Permission = share.Permission,
            SharedAt = share.SharedAt
        };
    }

    public async Task<bool> UpdateCalendarShareAsync(int shareId, CalendarPermission permission, string userId)
    {
        var share = await _context.CalendarShares
            .Include(s => s.Calendar)
            .FirstOrDefaultAsync(s => s.Id == shareId && s.Calendar!.UserId == userId);

        if (share == null) return false;

        share.Permission = permission;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveCalendarShareAsync(int shareId, string userId)
    {
        var share = await _context.CalendarShares
            .Include(s => s.Calendar)
            .FirstOrDefaultAsync(s => s.Id == shareId && s.Calendar!.UserId == userId);

        if (share == null) return false;

        _context.CalendarShares.Remove(share);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<CalendarDto>> GetSharedWithMeAsync(string userId)
    {
        var shares = await _context.CalendarShares
            .Include(s => s.Calendar)
            .Where(s => s.SharedWithUserId == userId)
            .ToListAsync();

        return shares.Select(s => MapCalendarToDto(s.Calendar!)).ToList();
    }

    // ===== Event CRUD =====
    public async Task<List<CalendarEventDto>> GetEventsAsync(int calendarId, string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.read"))
            throw new UnauthorizedAccessException("Permission denied: calendar.read");

        var query = _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .Include(e => e.Reminders)
            .Where(e => e.CalendarId == calendarId && e.Calendar!.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(e => e.EndTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.StartTime <= endDate.Value);

        var events = await query.OrderBy(e => e.StartTime).ToListAsync();
        return events.Select(MapEventToDto).ToList();
    }

    public async Task<List<CalendarEventDto>> GetAllEventsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.read"))
            throw new UnauthorizedAccessException("Permission denied: calendar.read");

        var query = _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .Include(e => e.Reminders)
            .Where(e => e.Calendar!.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(e => e.EndTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.StartTime <= endDate.Value);

        var events = await query.OrderBy(e => e.StartTime).ToListAsync();
        return events.Select(MapEventToDto).ToList();
    }

    public async Task<List<CalendarEventDto>> GetUpcomingEventsAsync(string userId, int days = 7)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.read"))
            throw new UnauthorizedAccessException("Permission denied: calendar.read");

        var now = DateTime.UtcNow;
        var endDate = now.AddDays(days);

        var events = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .Include(e => e.Reminders)
            .Where(e => e.Calendar!.UserId == userId && e.StartTime >= now && e.StartTime <= endDate)
            .OrderBy(e => e.StartTime)
            .ToListAsync();

        return events.Select(MapEventToDto).ToList();
    }

    public async Task<List<CalendarEventDto>> GetEventsByDateAsync(DateTime date, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.read"))
            throw new UnauthorizedAccessException("Permission denied: calendar.read");

        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var events = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .Include(e => e.Reminders)
            .Where(e => e.Calendar!.UserId == userId &&
                       ((e.StartTime >= startOfDay && e.StartTime < endOfDay) ||
                        (e.EndTime > startOfDay && e.EndTime <= endOfDay) ||
                        (e.StartTime < startOfDay && e.EndTime > endOfDay)))
            .OrderBy(e => e.StartTime)
            .ToListAsync();

        return events.Select(MapEventToDto).ToList();
    }

    public async Task<CalendarEventDto> GetEventByIdAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.read"))
            throw new UnauthorizedAccessException("Permission denied: calendar.read");

        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .Include(e => e.Reminders)
            .FirstOrDefaultAsync(e => e.Id == id && e.Calendar!.UserId == userId);

        if (calendarEvent == null)
            throw new KeyNotFoundException($"Event with ID {id} not found");

        return MapEventToDto(calendarEvent);
    }

    public async Task<CalendarEventDto> GetEventByEventIdAsync(string eventId, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.read"))
            throw new UnauthorizedAccessException("Permission denied: calendar.read");

        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .Include(e => e.Reminders)
            .FirstOrDefaultAsync(e => e.EventId == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null)
            throw new KeyNotFoundException($"Event with EventId {eventId} not found");

        return MapEventToDto(calendarEvent);
    }

    public async Task<CalendarEventDto> CreateEventAsync(CreateCalendarEventDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.create"))
            throw new UnauthorizedAccessException("Permission denied: calendar.create");

        var calendar = await _context.Calendars.FirstOrDefaultAsync(c => c.Id == dto.CalendarId && c.UserId == userId);
        if (calendar == null)
            throw new KeyNotFoundException($"Calendar with ID {dto.CalendarId} not found");

        var calendarEvent = new CalendarEvent
        {
            EventId = Guid.NewGuid().ToString(),
            CalendarId = dto.CalendarId,
            Title = dto.Title,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Location = dto.Location,
            LocationUrl = dto.LocationUrl,
            IsAllDay = dto.IsAllDay,
            OrganizerUserId = userId,
            IsRecurring = dto.IsRecurring,
            RecurrenceRule = dto.RecurrenceRule,
            RecurrenceEndDate = dto.RecurrenceEndDate,
            Status = dto.Status,
            Priority = dto.Priority,
            Sensitivity = dto.Sensitivity,
            ShowAs = dto.ShowAs,
            OnlineMeetingUrl = dto.OnlineMeetingUrl,
            Color = dto.Color,
            CreatedAt = DateTime.UtcNow
        };

        // Add attendees
        foreach (var attendeeDto in dto.Attendees)
        {
            calendarEvent.Attendees.Add(new EventAttendee
            {
                Email = attendeeDto.Email,
                DisplayName = attendeeDto.DisplayName,
                Type = attendeeDto.Type,
                ResponseStatus = ResponseStatus.NotResponded
            });
        }

        // Add reminders
        foreach (var minutes in dto.ReminderMinutes)
        {
            calendarEvent.Reminders.Add(new EventReminder
            {
                MinutesBefore = minutes,
                Type = ReminderType.Notification,
                IsActive = true
            });
        }

        // Add categories
        if (dto.Categories != null)
        {
            calendarEvent.Categories = string.Join(",", dto.Categories);
        }

        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync();

        return MapEventToDto(calendarEvent);
    }

    public async Task<bool> UpdateEventAsync(int id, UpdateCalendarEventDto dto, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == id && e.Calendar!.UserId == userId);

        if (calendarEvent == null) return false;

        if (dto.Title != null) calendarEvent.Title = dto.Title;
        if (dto.Description != null) calendarEvent.Description = dto.Description;
        if (dto.StartTime.HasValue) calendarEvent.StartTime = dto.StartTime.Value;
        if (dto.EndTime.HasValue) calendarEvent.EndTime = dto.EndTime.Value;
        if (dto.Location != null) calendarEvent.Location = dto.Location;
        if (dto.LocationUrl != null) calendarEvent.LocationUrl = dto.LocationUrl;
        if (dto.IsAllDay.HasValue) calendarEvent.IsAllDay = dto.IsAllDay.Value;
        if (dto.Status.HasValue) calendarEvent.Status = dto.Status.Value;
        if (dto.Priority.HasValue) calendarEvent.Priority = dto.Priority.Value;
        if (dto.Sensitivity.HasValue) calendarEvent.Sensitivity = dto.Sensitivity.Value;
        if (dto.ShowAs.HasValue) calendarEvent.ShowAs = dto.ShowAs.Value;
        if (dto.Color != null) calendarEvent.Color = dto.Color;
        if (dto.Categories != null) calendarEvent.Categories = string.Join(",", dto.Categories);

        calendarEvent.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteEventAsync(int id, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.delete"))
            throw new UnauthorizedAccessException("Permission denied: calendar.delete");

        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == id && e.Calendar!.UserId == userId);

        if (calendarEvent == null) return false;

        _context.CalendarEvents.Remove(calendarEvent);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelEventAsync(int id, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == id && e.Calendar!.UserId == userId);

        if (calendarEvent == null) return false;

        calendarEvent.Status = EventStatus.Cancelled;
        calendarEvent.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<CalendarEventDto>> SearchEventsAsync(string query, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.read"))
            throw new UnauthorizedAccessException("Permission denied: calendar.read");

        var events = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .Include(e => e.Reminders)
            .Where(e => e.Calendar!.UserId == userId &&
                       (e.Title.Contains(query) ||
                        (e.Description != null && e.Description.Contains(query)) ||
                        (e.Location != null && e.Location.Contains(query))))
            .OrderBy(e => e.StartTime)
            .ToListAsync();

        return events.Select(MapEventToDto).ToList();
    }

    // ===== Recurring Events =====
    public async Task<bool> UpdateRecurringEventAsync(int id, UpdateCalendarEventDto dto, bool updateSeries, string userId)
    {
        if (updateSeries)
        {
            // Update all events in the series
            var calendarEvent = await _context.CalendarEvents
                .Include(e => e.Calendar)
                .FirstOrDefaultAsync(e => e.Id == id && e.Calendar!.UserId == userId);

            if (calendarEvent == null) return false;

            // This would require more complex logic to update all instances
            // For now, just update the single event
            return await UpdateEventAsync(id, dto, userId);
        }
        else
        {
            // Update only this instance
            return await UpdateEventAsync(id, dto, userId);
        }
    }

    public async Task<bool> DeleteRecurringEventAsync(int id, bool deleteSeries, string userId)
    {
        if (deleteSeries)
        {
            // Delete all events in the series
            // This would require more complex logic
            return await DeleteEventAsync(id, userId);
        }
        else
        {
            // Delete only this instance
            return await DeleteEventAsync(id, userId);
        }
    }

    public async Task<bool> AddRecurrenceExceptionAsync(int eventId, DateTime exceptionDate, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null || !calendarEvent.IsRecurring) return false;

        var exceptions = calendarEvent.RecurrenceExceptions?.Split(',').ToList() ?? new List<string>();
        exceptions.Add(exceptionDate.ToString("o"));
        calendarEvent.RecurrenceExceptions = string.Join(",", exceptions);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CalendarEventDto>> GetRecurringEventInstancesAsync(int eventId, DateTime startDate, DateTime endDate, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null || !calendarEvent.IsRecurring)
            return new List<CalendarEventDto>();

        // This would require parsing the recurrence rule and generating instances
        // For now, return just the original event
        return new List<CalendarEventDto> { MapEventToDto(calendarEvent) };
    }

    // ===== Event Attendees =====
    public async Task<List<EventAttendeeDto>> GetEventAttendeesAsync(int eventId, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null)
            throw new KeyNotFoundException($"Event with ID {eventId} not found");

        return calendarEvent.Attendees.Select(a => new EventAttendeeDto
        {
            Id = a.Id,
            Email = a.Email,
            DisplayName = a.DisplayName,
            Type = a.Type,
            ResponseStatus = a.ResponseStatus,
            ResponseTime = a.ResponseTime,
            ResponseComment = a.ResponseComment
        }).ToList();
    }

    public async Task<bool> AddAttendeeAsync(int eventId, CreateEventAttendeeDto dto, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null) return false;

        var attendee = new EventAttendee
        {
            EventId = eventId,
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            Type = dto.Type,
            ResponseStatus = ResponseStatus.NotResponded
        };

        _context.EventAttendees.Add(attendee);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveAttendeeAsync(int eventId, int attendeeId, string userId)
    {
        var attendee = await _context.EventAttendees
            .Include(a => a.Event)
            .ThenInclude(e => e.Calendar)
            .FirstOrDefaultAsync(a => a.Id == attendeeId && a.EventId == eventId && a.Event.Calendar!.UserId == userId);

        if (attendee == null) return false;

        _context.EventAttendees.Remove(attendee);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAttendeeResponseAsync(int eventId, int attendeeId, UpdateAttendeeResponseDto dto, string userId)
    {
        var attendee = await _context.EventAttendees
            .Include(a => a.Event)
            .ThenInclude(e => e.Calendar)
            .FirstOrDefaultAsync(a => a.Id == attendeeId && a.EventId == eventId);

        if (attendee == null) return false;

        attendee.ResponseStatus = dto.ResponseStatus;
        attendee.ResponseComment = dto.ResponseComment;
        attendee.ResponseTime = DateTime.UtcNow;

        if (dto.ProposedStartTime.HasValue)
            attendee.ProposedStartTime = dto.ProposedStartTime.Value;

        if (dto.ProposedEndTime.HasValue)
            attendee.ProposedEndTime = dto.ProposedEndTime.Value;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RespondToEventAsync(int eventId, ResponseStatus response, string? comment, string userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        var attendee = await _context.EventAttendees
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId);

        if (attendee == null) return false;

        attendee.ResponseStatus = response;
        attendee.ResponseComment = comment;
        attendee.ResponseTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    // ===== Event Reminders =====
    public async Task<List<EventReminderDto>> GetEventRemindersAsync(int eventId, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Reminders)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null)
            throw new KeyNotFoundException($"Event with ID {eventId} not found");

        return calendarEvent.Reminders.Select(r => new EventReminderDto
        {
            Id = r.Id,
            MinutesBefore = r.MinutesBefore,
            Type = r.Type,
            IsActive = r.IsActive
        }).ToList();
    }

    public async Task<bool> AddReminderAsync(int eventId, int minutesBefore, ReminderType type, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null) return false;

        var reminder = new EventReminder
        {
            EventId = eventId,
            MinutesBefore = minutesBefore,
            Type = type,
            IsActive = true
        };

        _context.EventReminders.Add(reminder);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveReminderAsync(int eventId, int reminderId, string userId)
    {
        var reminder = await _context.EventReminders
            .Include(r => r.Event)
            .ThenInclude(e => e.Calendar)
            .FirstOrDefaultAsync(r => r.Id == reminderId && r.EventId == eventId && r.Event.Calendar!.UserId == userId);

        if (reminder == null) return false;

        _context.EventReminders.Remove(reminder);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<CalendarEventDto>> GetUpcomingRemindersAsync(string userId, int minutesAhead = 60)
    {
        var now = DateTime.UtcNow;
        var targetTime = now.AddMinutes(minutesAhead);

        var events = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Reminders)
            .Where(e => e.Calendar!.UserId == userId &&
                       e.StartTime > now &&
                       e.StartTime <= targetTime &&
                       e.Reminders.Any(r => r.IsActive))
            .OrderBy(e => e.StartTime)
            .ToListAsync();

        return events.Select(MapEventToDto).ToList();
    }

    // ===== Event Categories =====
    public async Task<bool> AddEventCategoryAsync(int eventId, string category, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null) return false;

        var categories = calendarEvent.Categories?.Split(',').ToList() ?? new List<string>();
        if (!categories.Contains(category))
        {
            categories.Add(category);
            calendarEvent.Categories = string.Join(",", categories);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> RemoveEventCategoryAsync(int eventId, string category, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null) return false;

        var categories = calendarEvent.Categories?.Split(',').ToList() ?? new List<string>();
        if (categories.Contains(category))
        {
            categories.Remove(category);
            calendarEvent.Categories = string.Join(",", categories);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    // ===== Availability =====
    public async Task<List<TimeSlotDto>> GetAvailabilityAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var events = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Where(e => e.Calendar!.UserId == userId &&
                       e.StartTime < endDate &&
                       e.EndTime > startDate)
            .OrderBy(e => e.StartTime)
            .ToListAsync();

        var slots = new List<TimeSlotDto>();
        var currentTime = startDate;

        foreach (var evt in events)
        {
            if (currentTime < evt.StartTime)
            {
                slots.Add(new TimeSlotDto
                {
                    StartTime = currentTime,
                    EndTime = evt.StartTime,
                    IsAvailable = true
                });
            }

            slots.Add(new TimeSlotDto
            {
                StartTime = evt.StartTime,
                EndTime = evt.EndTime,
                IsAvailable = false,
                EventTitle = evt.Title
            });

            currentTime = evt.EndTime > currentTime ? evt.EndTime : currentTime;
        }

        if (currentTime < endDate)
        {
            slots.Add(new TimeSlotDto
            {
                StartTime = currentTime,
                EndTime = endDate,
                IsAvailable = true
            });
        }

        return slots;
    }

    public async Task<List<TimeSlotDto>> FindFreeSlotsAsync(List<string> userIds, DateTime startDate, DateTime endDate, int durationMinutes)
    {
        // This would require complex logic to find common free slots across multiple users
        // For now, return a simplified version for single user
        if (userIds.Count == 1)
        {
            var availability = await GetAvailabilityAsync(userIds[0], startDate, endDate);
            return availability.Where(s => s.IsAvailable &&
                (s.EndTime - s.StartTime).TotalMinutes >= durationMinutes).ToList();
        }

        return new List<TimeSlotDto>();
    }

    public async Task<bool> IsAvailableAsync(string userId, DateTime startTime, DateTime endTime)
    {
        var hasConflict = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .AnyAsync(e => e.Calendar!.UserId == userId &&
                          e.StartTime < endTime &&
                          e.EndTime > startTime);

        return !hasConflict;
    }

    // ===== Integration =====
    public async Task<CalendarEventDto> CreateEventWithVideoConferenceAsync(CreateCalendarEventDto dto, string userId)
    {
        if (!await _userService.HasPermissionAsync(userId, "calendar.create"))
            throw new UnauthorizedAccessException("Permission denied: calendar.create");

        // Create the calendar event first
        var calendarEvent = await CreateEventAsync(dto, userId);

        // Create video conference if requested
        if (dto.CreateVideoConference)
        {
            var conferenceDto = new Application.DTOs.VideoConference.VideoConferenceDtos.CreateConferenceDto
            {
                Title = dto.Title,
                Description = dto.Description ?? "",
                ScheduledStartTime = dto.StartTime,
                DurationMinutes = (int)(dto.EndTime - dto.StartTime).TotalMinutes,
                HostId = userId,
                CalendarEventId = calendarEvent.Id
            };

            var conference = await _videoConferenceService.CreateAsync(conferenceDto, userId);

            // Update event with conference link
            await UpdateEventAsync(calendarEvent.Id, new UpdateCalendarEventDto
            {
                OnlineMeetingUrl = $"/conference/{conference.ConferenceId}"
            }, userId);

            calendarEvent.VideoConferenceId = conference.Id;
            calendarEvent.OnlineMeetingUrl = $"/conference/{conference.ConferenceId}";
        }

        return calendarEvent;
    }

    public async Task<bool> LinkVideoConferenceAsync(int eventId, int conferenceId, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null) return false;

        calendarEvent.VideoConferenceId = conferenceId;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UnlinkVideoConferenceAsync(int eventId, string userId)
    {
        var calendarEvent = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId && e.Calendar!.UserId == userId);

        if (calendarEvent == null) return false;

        calendarEvent.VideoConferenceId = null;
        calendarEvent.OnlineMeetingUrl = null;
        await _context.SaveChangesAsync();

        return true;
    }

    // ===== Statistics =====
    public async Task<CalendarStatisticsDto> GetStatisticsAsync(string userId)
    {
        var totalCalendars = await _context.Calendars.CountAsync(c => c.UserId == userId);
        var totalEvents = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .CountAsync(e => e.Calendar!.UserId == userId);

        var now = DateTime.UtcNow;
        var upcomingEvents = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .CountAsync(e => e.Calendar!.UserId == userId && e.StartTime > now);

        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);
        var todayEvents = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .CountAsync(e => e.Calendar!.UserId == userId &&
                            e.StartTime >= todayStart && e.StartTime < todayEnd);

        var sharedCalendars = await _context.Calendars.CountAsync(c => c.UserId == userId && c.IsShared);

        return new CalendarStatisticsDto
        {
            TotalCalendars = totalCalendars,
            TotalEvents = totalEvents,
            UpcomingEvents = upcomingEvents,
            TodayEvents = todayEvents,
            SharedCalendars = sharedCalendars
        };
    }

    // ===== Helper Methods =====
    private CalendarDto MapCalendarToDto(Backend.Domain.Entities.Calendar.Calendar calendar)
    {
        return new CalendarDto
        {
            Id = calendar.Id,
            Name = calendar.Name,
            Description = calendar.Description,
            Color = calendar.Color,
            IsDefault = calendar.IsDefault,
            IsVisible = calendar.IsVisible,
            DisplayOrder = calendar.DisplayOrder,
            IsShared = calendar.IsShared,
            CreatedAt = calendar.CreatedAt,
            LastModified = calendar.LastModified
        };
    }

    private CalendarEventDto MapEventToDto(CalendarEvent calendarEvent)
    {
        return new CalendarEventDto
        {
            Id = calendarEvent.Id,
            EventId = calendarEvent.EventId,
            CalendarId = calendarEvent.CalendarId,
            CalendarName = calendarEvent.Calendar?.Name ?? "",
            Title = calendarEvent.Title,
            Description = calendarEvent.Description,
            StartTime = calendarEvent.StartTime,
            EndTime = calendarEvent.EndTime,
            Location = calendarEvent.Location,
            LocationUrl = calendarEvent.LocationUrl,
            IsAllDay = calendarEvent.IsAllDay,
            OrganizerUserId = calendarEvent.OrganizerUserId,
            OrganizerName = calendarEvent.Organizer?.FullName ?? "",
            Attendees = calendarEvent.Attendees?.Select(a => new EventAttendeeDto
            {
                Id = a.Id,
                Email = a.Email,
                DisplayName = a.DisplayName,
                Type = a.Type,
                ResponseStatus = a.ResponseStatus,
                ResponseTime = a.ResponseTime,
                ResponseComment = a.ResponseComment
            }).ToList() ?? new List<EventAttendeeDto>(),
            Reminders = calendarEvent.Reminders?.Select(r => new EventReminderDto
            {
                Id = r.Id,
                MinutesBefore = r.MinutesBefore,
                Type = r.Type,
                IsActive = r.IsActive
            }).ToList() ?? new List<EventReminderDto>(),
            IsRecurring = calendarEvent.IsRecurring,
            RecurrenceRule = calendarEvent.RecurrenceRule,
            RecurrenceEndDate = calendarEvent.RecurrenceEndDate,
            RecurrenceExceptions = calendarEvent.RecurrenceExceptions?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => DateTime.Parse(s))
                .ToList() ?? new List<DateTime>(),
            Status = calendarEvent.Status,
            Priority = calendarEvent.Priority,
            Sensitivity = calendarEvent.Sensitivity,
            ShowAs = calendarEvent.ShowAs,
            OnlineMeetingUrl = calendarEvent.OnlineMeetingUrl,
            OnlineMeetingProvider = calendarEvent.OnlineMeetingProvider,
            VideoConferenceId = calendarEvent.VideoConferenceId,
            Categories = calendarEvent.Categories?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList() ?? new List<string>(),
            Color = calendarEvent.Color,
            CreatedAt = calendarEvent.CreatedAt,
            LastModified = calendarEvent.LastModified
        };
    }
}
