using Microsoft.EntityFrameworkCore;
using OutlookInboxManagement.Data;
using OutlookInboxManagement.DTOs;
using OutlookInboxManagement.Models;
using System.Text;

namespace OutlookInboxManagement.Services;

public class CalendarService : ICalendarService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public CalendarService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    #region Calendar Management

    public async Task<CalendarDto?> GetCalendarAsync(int calendarId, string userId)
    {
        var calendar = await _context.Calendars
            .Include(c => c.Shares)
            .ThenInclude(s => s.SharedWithUser)
            .FirstOrDefaultAsync(c => c.Id == calendarId &&
                (c.UserId == userId || c.Shares.Any(s => s.SharedWithUserId == userId)));

        return calendar == null ? null : MapToCalendarDto(calendar);
    }

    public async Task<List<CalendarDto>> GetUserCalendarsAsync(string userId)
    {
        var calendars = await _context.Calendars
            .Include(c => c.Shares)
            .ThenInclude(s => s.SharedWithUser)
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return calendars.Select(MapToCalendarDto).ToList();
    }

    public async Task<CalendarDto> CreateCalendarAsync(CreateCalendarDto dto, string userId)
    {
        var existingCount = await _context.Calendars.CountAsync(c => c.UserId == userId);

        var calendar = new Calendar
        {
            Name = dto.Name,
            Description = dto.Description,
            Color = dto.Color,
            UserId = userId,
            IsDefault = dto.IsDefault || existingCount == 0,
            DisplayOrder = existingCount,
            CreatedAt = DateTime.UtcNow
        };

        // If this is set as default, unset other defaults
        if (calendar.IsDefault)
        {
            var otherCalendars = await _context.Calendars
                .Where(c => c.UserId == userId && c.IsDefault)
                .ToListAsync();
            otherCalendars.ForEach(c => c.IsDefault = false);
        }

        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync();

        return MapToCalendarDto(calendar);
    }

    public async Task<CalendarDto?> UpdateCalendarAsync(int calendarId, UpdateCalendarDto dto, string userId)
    {
        var calendar = await _context.Calendars
            .FirstOrDefaultAsync(c => c.Id == calendarId && c.UserId == userId);

        if (calendar == null) return null;

        if (dto.Name != null) calendar.Name = dto.Name;
        if (dto.Description != null) calendar.Description = dto.Description;
        if (dto.Color != null) calendar.Color = dto.Color;
        if (dto.IsVisible.HasValue) calendar.IsVisible = dto.IsVisible.Value;
        if (dto.DisplayOrder.HasValue) calendar.DisplayOrder = dto.DisplayOrder.Value;

        calendar.LastModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToCalendarDto(calendar);
    }

    public async Task<bool> DeleteCalendarAsync(int calendarId, string userId)
    {
        var calendar = await _context.Calendars
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == calendarId && c.UserId == userId);

        if (calendar == null) return false;

        // Don't allow deleting the default calendar if it has events
        if (calendar.IsDefault && calendar.Events.Any())
            return false;

        _context.Calendars.Remove(calendar);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<CalendarDto?> SetDefaultCalendarAsync(int calendarId, string userId)
    {
        var calendar = await _context.Calendars
            .FirstOrDefaultAsync(c => c.Id == calendarId && c.UserId == userId);

        if (calendar == null) return null;

        // Unset other defaults
        var otherCalendars = await _context.Calendars
            .Where(c => c.UserId == userId && c.IsDefault && c.Id != calendarId)
            .ToListAsync();
        otherCalendars.ForEach(c => c.IsDefault = false);

        calendar.IsDefault = true;
        await _context.SaveChangesAsync();

        return MapToCalendarDto(calendar);
    }

    #endregion

    #region Calendar Sharing

    public async Task<CalendarShareDto> ShareCalendarAsync(int calendarId, ShareCalendarDto dto, string userId)
    {
        var calendar = await _context.Calendars
            .FirstOrDefaultAsync(c => c.Id == calendarId && c.UserId == userId);

        if (calendar == null)
            throw new InvalidOperationException("Calendar not found");

        var sharedWithUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.SharedWithUserEmail);

        if (sharedWithUser == null)
            throw new InvalidOperationException("User not found");

        var existingShare = await _context.Set<CalendarShare>()
            .FirstOrDefaultAsync(s => s.CalendarId == calendarId && s.SharedWithUserId == sharedWithUser.Id);

        if (existingShare != null)
        {
            existingShare.Permission = dto.Permission;
            await _context.SaveChangesAsync();
            return MapToCalendarShareDto(existingShare, sharedWithUser);
        }

        var share = new CalendarShare
        {
            CalendarId = calendarId,
            SharedWithUserId = sharedWithUser.Id,
            Permission = dto.Permission,
            SharedAt = DateTime.UtcNow
        };

        calendar.IsShared = true;
        _context.Set<CalendarShare>().Add(share);
        await _context.SaveChangesAsync();

        await _notificationService.SendNotificationAsync(
            sharedWithUser.Id,
            "Calendar Shared",
            $"{userId} shared calendar '{calendar.Name}' with you"
        );

        return MapToCalendarShareDto(share, sharedWithUser);
    }

    public async Task<bool> RevokeCalendarShareAsync(int calendarId, int shareId, string userId)
    {
        var share = await _context.Set<CalendarShare>()
            .Include(s => s.Calendar)
            .FirstOrDefaultAsync(s => s.Id == shareId && s.CalendarId == calendarId && s.Calendar!.UserId == userId);

        if (share == null) return false;

        _context.Set<CalendarShare>().Remove(share);

        // Update IsShared flag if no more shares
        var remainingShares = await _context.Set<CalendarShare>()
            .CountAsync(s => s.CalendarId == calendarId && s.Id != shareId);

        if (remainingShares == 0 && share.Calendar != null)
            share.Calendar.IsShared = false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<CalendarDto>> GetSharedCalendarsAsync(string userId)
    {
        var calendars = await _context.Set<CalendarShare>()
            .Include(s => s.Calendar)
            .ThenInclude(c => c!.User)
            .Where(s => s.SharedWithUserId == userId)
            .Select(s => s.Calendar!)
            .ToListAsync();

        return calendars.Select(MapToCalendarDto).ToList();
    }

    #endregion

    #region Event Management

    public async Task<CalendarEventDto?> GetEventAsync(int eventId, string userId)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees).ThenInclude(a => a.User)
            .Include(e => e.Reminders)
            .Include(e => e.Resources).ThenInclude(r => r.Resource)
            .Include(e => e.Attachments)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == eventId &&
                (e.Calendar!.UserId == userId || e.Attendees.Any(a => a.UserId == userId)));

        return evt == null ? null : MapToEventDto(evt);
    }

    public async Task<CalendarEventDto?> GetEventByEventIdAsync(string eventId, string userId)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees).ThenInclude(a => a.User)
            .Include(e => e.Reminders)
            .Include(e => e.Resources).ThenInclude(r => r.Resource)
            .Include(e => e.Attachments)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.EventId == eventId &&
                (e.Calendar!.UserId == userId || e.Attendees.Any(a => a.UserId == userId)));

        return evt == null ? null : MapToEventDto(evt);
    }

    public async Task<List<CalendarEventDto>> GetEventsAsync(CalendarQueryParameters parameters, string userId)
    {
        var query = _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees).ThenInclude(a => a.User)
            .Include(e => e.Reminders)
            .Include(e => e.Resources).ThenInclude(r => r.Resource)
            .Include(e => e.Attachments)
            .Include(e => e.Organizer)
            .Where(e => e.Calendar!.UserId == userId || e.Attendees.Any(a => a.UserId == userId))
            .Where(e => e.StartDateTime <= parameters.EndDate && e.EndDateTime >= parameters.StartDate)
            .AsQueryable();

        if (parameters.CalendarId.HasValue)
            query = query.Where(e => e.CalendarId == parameters.CalendarId.Value);

        if (parameters.Categories != null && parameters.Categories.Any())
            query = query.Where(e => e.Categories.Any(c => parameters.Categories.Contains(c)));

        if (parameters.Status.HasValue)
            query = query.Where(e => e.Status == parameters.Status.Value);

        if (!parameters.IncludeCancelled)
            query = query.Where(e => !e.IsCancelled);

        var events = await query.OrderBy(e => e.StartDateTime).ToListAsync();

        return events.Select(MapToEventDto).ToList();
    }

    public async Task<CalendarEventDto> CreateEventAsync(CreateEventDto dto, string userId)
    {
        var calendar = await _context.Calendars
            .FirstOrDefaultAsync(c => c.Id == dto.CalendarId && c.UserId == userId);

        if (calendar == null)
            throw new InvalidOperationException("Calendar not found");

        var evt = new CalendarEvent
        {
            EventId = Guid.NewGuid().ToString(),
            CalendarId = dto.CalendarId,
            Title = dto.Title,
            Body = dto.Body ?? string.Empty,
            Location = dto.Location ?? string.Empty,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            IsAllDay = dto.IsAllDay,
            TimeZone = dto.TimeZone,
            IsRecurring = dto.IsRecurring,
            RecurrenceRule = dto.RecurrenceRule,
            RecurrenceEnd = dto.RecurrenceEnd,
            IsMeeting = dto.IsMeeting,
            IsOnlineMeeting = dto.IsOnlineMeeting,
            OnlineMeetingProvider = dto.OnlineMeetingProvider,
            OrganizerId = userId,
            BusyStatus = dto.BusyStatus,
            Importance = dto.Importance,
            Sensitivity = dto.Sensitivity,
            TravelTimeMinutes = dto.TravelTimeMinutes,
            Status = EventStatus.Confirmed,
            Categories = dto.Categories?.ToList() ?? new List<string>(),
            CreatedAt = DateTime.UtcNow
        };

        // Generate online meeting URL if requested
        if (dto.IsOnlineMeeting)
        {
            evt.OnlineMeetingUrl = GenerateOnlineMeetingUrl(evt.EventId, dto.OnlineMeetingProvider);
        }

        _context.CalendarEvents.Add(evt);
        await _context.SaveChangesAsync();

        // Add attendees
        if (dto.Attendees != null && dto.Attendees.Any())
        {
            foreach (var attendeeDto in dto.Attendees)
            {
                var attendee = new EventAttendee
                {
                    EventId = evt.Id,
                    Email = attendeeDto.Email,
                    DisplayName = attendeeDto.DisplayName ?? attendeeDto.Email,
                    Type = attendeeDto.Type,
                    ResponseStatus = ResponseStatus.NotResponded
                };

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == attendeeDto.Email);
                if (user != null) attendee.UserId = user.Id;

                _context.Set<EventAttendee>().Add(attendee);
            }
            await _context.SaveChangesAsync();
        }

        // Add reminders
        if (dto.Reminders != null && dto.Reminders.Any())
        {
            foreach (var reminderDto in dto.Reminders)
            {
                _context.Set<EventReminder>().Add(new EventReminder
                {
                    EventId = evt.Id,
                    MinutesBeforeStart = reminderDto.MinutesBeforeStart,
                    Method = reminderDto.Method
                });
            }
            await _context.SaveChangesAsync();
        }
        else
        {
            // Add default reminder
            _context.Set<EventReminder>().Add(new EventReminder
            {
                EventId = evt.Id,
                MinutesBeforeStart = 15,
                Method = ReminderMethod.Notification
            });
            await _context.SaveChangesAsync();
        }

        // Book resources
        if (dto.ResourceIds != null && dto.ResourceIds.Any())
        {
            foreach (var resourceId in dto.ResourceIds)
            {
                await BookResourceAsync(evt.Id, resourceId, userId);
            }
        }

        // Send notifications to attendees
        if (dto.IsMeeting && dto.Attendees != null)
        {
            foreach (var attendeeDto in dto.Attendees)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == attendeeDto.Email);
                if (user != null)
                {
                    await _notificationService.SendNotificationAsync(
                        user.Id,
                        "Meeting Invitation",
                        $"You've been invited to: {evt.Title}"
                    );
                }
            }
        }

        return (await GetEventAsync(evt.Id, userId))!;
    }

    public async Task<CalendarEventDto?> UpdateEventAsync(int eventId, UpdateEventDto dto, string userId)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId &&
                (e.Calendar!.UserId == userId || e.OrganizerId == userId));

        if (evt == null) return null;

        if (dto.Title != null) evt.Title = dto.Title;
        if (dto.Body != null) evt.Body = dto.Body;
        if (dto.Location != null) evt.Location = dto.Location;
        if (dto.StartDateTime.HasValue) evt.StartDateTime = dto.StartDateTime.Value;
        if (dto.EndDateTime.HasValue) evt.EndDateTime = dto.EndDateTime.Value;
        if (dto.IsAllDay.HasValue) evt.IsAllDay = dto.IsAllDay.Value;
        if (dto.TimeZone != null) evt.TimeZone = dto.TimeZone;
        if (dto.IsRecurring.HasValue) evt.IsRecurring = dto.IsRecurring.Value;
        if (dto.RecurrenceRule != null) evt.RecurrenceRule = dto.RecurrenceRule;
        if (dto.RecurrenceEnd.HasValue) evt.RecurrenceEnd = dto.RecurrenceEnd;
        if (dto.IsOnlineMeeting.HasValue)
        {
            evt.IsOnlineMeeting = dto.IsOnlineMeeting.Value;
            if (evt.IsOnlineMeeting && string.IsNullOrEmpty(evt.OnlineMeetingUrl))
            {
                evt.OnlineMeetingUrl = GenerateOnlineMeetingUrl(evt.EventId, dto.OnlineMeetingProvider);
                evt.OnlineMeetingProvider = dto.OnlineMeetingProvider;
            }
        }
        if (dto.BusyStatus.HasValue) evt.BusyStatus = dto.BusyStatus.Value;
        if (dto.Importance.HasValue) evt.Importance = dto.Importance.Value;
        if (dto.Sensitivity.HasValue) evt.Sensitivity = dto.Sensitivity.Value;
        if (dto.TravelTimeMinutes.HasValue) evt.TravelTimeMinutes = dto.TravelTimeMinutes;

        evt.LastModified = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Notify attendees of changes
        if (evt.IsMeeting && evt.Attendees.Any())
        {
            foreach (var attendee in evt.Attendees.Where(a => a.UserId != null))
            {
                await _notificationService.SendNotificationAsync(
                    attendee.UserId!,
                    "Meeting Updated",
                    $"The meeting '{evt.Title}' has been updated"
                );
            }
        }

        return await GetEventAsync(evt.Id, userId);
    }

    public async Task<bool> DeleteEventAsync(int eventId, string userId)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId &&
                (e.Calendar!.UserId == userId || e.OrganizerId == userId));

        if (evt == null) return false;

        // Notify attendees
        if (evt.IsMeeting && evt.Attendees.Any())
        {
            foreach (var attendee in evt.Attendees.Where(a => a.UserId != null))
            {
                await _notificationService.SendNotificationAsync(
                    attendee.UserId!,
                    "Meeting Cancelled",
                    $"The meeting '{evt.Title}' has been cancelled"
                );
            }
        }

        _context.CalendarEvents.Remove(evt);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CancelEventAsync(int eventId, string userId, string? cancellationMessage = null)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == eventId &&
                (e.Calendar!.UserId == userId || e.OrganizerId == userId));

        if (evt == null) return false;

        evt.IsCancelled = true;
        evt.Status = EventStatus.Cancelled;
        evt.LastModified = DateTime.UtcNow;

        // Notify attendees
        if (evt.IsMeeting && evt.Attendees.Any())
        {
            var message = string.IsNullOrEmpty(cancellationMessage)
                ? $"The meeting '{evt.Title}' has been cancelled"
                : $"The meeting '{evt.Title}' has been cancelled: {cancellationMessage}";

            foreach (var attendee in evt.Attendees.Where(a => a.UserId != null))
            {
                await _notificationService.SendNotificationAsync(
                    attendee.UserId!,
                    "Meeting Cancelled",
                    message
                );
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Recurring Events

    public async Task<List<CalendarEventDto>> GetRecurringEventInstancesAsync(int eventId, DateTime startDate, DateTime endDate, string userId)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId &&
                e.IsRecurring &&
                (e.Calendar!.UserId == userId || e.Attendees.Any(a => a.UserId == userId)));

        if (evt == null || string.IsNullOrEmpty(evt.RecurrenceRule))
            return new List<CalendarEventDto>();

        // Here you would parse the RRULE and generate instances
        // For simplicity, returning the base event
        // In production, use a library like Ical.Net to parse RRULE
        var instances = new List<CalendarEventDto> { MapToEventDto(evt) };
        return instances;
    }

    public async Task<CalendarEventDto?> UpdateRecurringEventInstanceAsync(int eventId, DateTime instanceDate, UpdateEventDto dto, string userId)
    {
        // Create an exception for this instance
        var baseEvent = await _context.CalendarEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsRecurring);

        if (baseEvent == null) return null;

        // Add to exception list
        if (!baseEvent.RecurrenceExceptions.Contains(instanceDate))
        {
            baseEvent.RecurrenceExceptions.Add(instanceDate);
            await _context.SaveChangesAsync();
        }

        // Create a new event for this instance
        var createDto = new CreateEventDto
        {
            CalendarId = baseEvent.CalendarId,
            Title = dto.Title ?? baseEvent.Title,
            Body = dto.Body ?? baseEvent.Body,
            Location = dto.Location ?? baseEvent.Location,
            StartDateTime = dto.StartDateTime ?? instanceDate,
            EndDateTime = dto.EndDateTime ?? (instanceDate + (baseEvent.EndDateTime - baseEvent.StartDateTime)),
            IsAllDay = dto.IsAllDay ?? baseEvent.IsAllDay,
            TimeZone = dto.TimeZone ?? baseEvent.TimeZone,
            IsRecurring = false,
            IsMeeting = baseEvent.IsMeeting,
            IsOnlineMeeting = dto.IsOnlineMeeting ?? baseEvent.IsOnlineMeeting,
            BusyStatus = dto.BusyStatus ?? baseEvent.BusyStatus,
            Importance = dto.Importance ?? baseEvent.Importance,
            Sensitivity = dto.Sensitivity ?? baseEvent.Sensitivity
        };

        return await CreateEventAsync(createDto, userId);
    }

    public async Task<bool> DeleteRecurringEventInstanceAsync(int eventId, DateTime instanceDate, string userId)
    {
        var evt = await _context.CalendarEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.IsRecurring);

        if (evt == null) return false;

        if (!evt.RecurrenceExceptions.Contains(instanceDate))
        {
            evt.RecurrenceExceptions.Add(instanceDate);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    #endregion

    #region Attendee Management

    public async Task<EventAttendeeDto> AddAttendeeAsync(int eventId, CreateAttendeeDto dto, string userId)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId &&
                (e.Calendar!.UserId == userId || e.OrganizerId == userId));

        if (evt == null)
            throw new InvalidOperationException("Event not found");

        var attendee = new EventAttendee
        {
            EventId = eventId,
            Email = dto.Email,
            DisplayName = dto.DisplayName ?? dto.Email,
            Type = dto.Type,
            ResponseStatus = ResponseStatus.NotResponded
        };

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user != null)
        {
            attendee.UserId = user.Id;
            await _notificationService.SendNotificationAsync(
                user.Id,
                "Meeting Invitation",
                $"You've been invited to: {evt.Title}"
            );
        }

        _context.Set<EventAttendee>().Add(attendee);
        await _context.SaveChangesAsync();

        return MapToAttendeeDto(attendee, user);
    }

    public async Task<bool> RemoveAttendeeAsync(int eventId, int attendeeId, string userId)
    {
        var attendee = await _context.Set<EventAttendee>()
            .Include(a => a.Event)
            .ThenInclude(e => e!.Calendar)
            .FirstOrDefaultAsync(a => a.Id == attendeeId &&
                a.EventId == eventId &&
                (a.Event!.Calendar!.UserId == userId || a.Event.OrganizerId == userId));

        if (attendee == null) return false;

        _context.Set<EventAttendee>().Remove(attendee);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAttendeeResponseAsync(int eventId, UpdateAttendeeResponseDto dto, string userId)
    {
        var attendee = await _context.Set<EventAttendee>()
            .Include(a => a.Event)
            .FirstOrDefaultAsync(a => a.EventId == eventId && a.UserId == userId);

        if (attendee == null) return false;

        attendee.ResponseStatus = dto.ResponseStatus;
        attendee.ResponseTime = DateTime.UtcNow;
        attendee.ResponseComment = dto.ResponseComment;
        attendee.ProposedStartTime = dto.ProposedStartTime;
        attendee.ProposedEndTime = dto.ProposedEndTime;

        await _context.SaveChangesAsync();

        // Notify organizer
        if (attendee.Event != null && !string.IsNullOrEmpty(attendee.Event.OrganizerId))
        {
            var statusText = dto.ResponseStatus switch
            {
                ResponseStatus.Accepted => "accepted",
                ResponseStatus.Declined => "declined",
                ResponseStatus.Tentative => "tentatively accepted",
                _ => "responded to"
            };

            await _notificationService.SendNotificationAsync(
                attendee.Event.OrganizerId,
                "Meeting Response",
                $"{attendee.Email} has {statusText} your meeting invitation for '{attendee.Event.Title}'"
            );
        }

        return true;
    }

    public async Task<List<EventAttendeeDto>> GetEventAttendeesAsync(int eventId, string userId)
    {
        var attendees = await _context.Set<EventAttendee>()
            .Include(a => a.User)
            .Include(a => a.Event)
            .ThenInclude(e => e!.Calendar)
            .Where(a => a.EventId == eventId &&
                (a.Event!.Calendar!.UserId == userId || a.UserId == userId))
            .ToListAsync();

        return attendees.Select(a => MapToAttendeeDto(a, a.User)).ToList();
    }

    #endregion

    #region Resource Management

    public async Task<ResourceDto?> GetResourceAsync(int resourceId)
    {
        var resource = await _context.Set<Resource>().FindAsync(resourceId);
        return resource == null ? null : MapToResourceDto(resource);
    }

    public async Task<List<ResourceDto>> GetResourcesAsync(ResourceType? type = null)
    {
        var query = _context.Set<Resource>().AsQueryable();

        if (type.HasValue)
            query = query.Where(r => r.Type == type.Value);

        var resources = await query.OrderBy(r => r.Name).ToListAsync();
        return resources.Select(MapToResourceDto).ToList();
    }

    public async Task<ResourceDto> CreateResourceAsync(CreateResourceDto dto)
    {
        var resource = new Resource
        {
            Name = dto.Name,
            Description = dto.Description,
            Type = dto.Type,
            Building = dto.Building,
            Floor = dto.Floor,
            RoomNumber = dto.RoomNumber,
            Capacity = dto.Capacity,
            Equipment = dto.Equipment?.ToList() ?? new List<string>(),
            Email = dto.Email,
            IsAvailable = true
        };

        _context.Set<Resource>().Add(resource);
        await _context.SaveChangesAsync();

        return MapToResourceDto(resource);
    }

    public async Task<bool> DeleteResourceAsync(int resourceId)
    {
        var resource = await _context.Set<Resource>().FindAsync(resourceId);
        if (resource == null) return false;

        _context.Set<Resource>().Remove(resource);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> BookResourceAsync(int eventId, int resourceId, string userId)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId &&
                (e.Calendar!.UserId == userId || e.OrganizerId == userId));

        if (evt == null) return false;

        var resource = await _context.Set<Resource>().FindAsync(resourceId);
        if (resource == null || !resource.IsAvailable) return false;

        // Check if resource is already booked
        var existing = await _context.Set<EventResource>()
            .FirstOrDefaultAsync(er => er.EventId == eventId && er.ResourceId == resourceId);

        if (existing != null) return true;

        // Check for conflicts
        var hasConflict = await _context.Set<EventResource>()
            .Include(er => er.Event)
            .AnyAsync(er => er.ResourceId == resourceId &&
                er.Event!.StartDateTime < evt.EndDateTime &&
                er.Event.EndDateTime > evt.StartDateTime &&
                !er.Event.IsCancelled);

        if (hasConflict) return false;

        _context.Set<EventResource>().Add(new EventResource
        {
            EventId = eventId,
            ResourceId = resourceId,
            Status = ResourceStatus.Tentative
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReleaseResourceAsync(int eventId, int resourceId, string userId)
    {
        var eventResource = await _context.Set<EventResource>()
            .Include(er => er.Event)
            .ThenInclude(e => e!.Calendar)
            .FirstOrDefaultAsync(er => er.EventId == eventId &&
                er.ResourceId == resourceId &&
                (er.Event!.Calendar!.UserId == userId || er.Event.OrganizerId == userId));

        if (eventResource == null) return false;

        _context.Set<EventResource>().Remove(eventResource);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<ResourceDto>> GetAvailableResourcesAsync(DateTime startTime, DateTime endTime, ResourceType? type = null)
    {
        var bookedResourceIds = await _context.Set<EventResource>()
            .Include(er => er.Event)
            .Where(er => er.Event!.StartDateTime < endTime &&
                er.Event.EndDateTime > startTime &&
                !er.Event.IsCancelled)
            .Select(er => er.ResourceId)
            .Distinct()
            .ToListAsync();

        var query = _context.Set<Resource>()
            .Where(r => r.IsAvailable && !bookedResourceIds.Contains(r.Id));

        if (type.HasValue)
            query = query.Where(r => r.Type == type.Value);

        var resources = await query.ToListAsync();
        return resources.Select(MapToResourceDto).ToList();
    }

    #endregion

    #region Reminders

    public async Task<EventReminderDto> AddReminderAsync(int eventId, CreateReminderDto dto, string userId)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId &&
                (e.Calendar!.UserId == userId || e.Attendees.Any(a => a.UserId == userId)));

        if (evt == null)
            throw new InvalidOperationException("Event not found");

        var reminder = new EventReminder
        {
            EventId = eventId,
            MinutesBeforeStart = dto.MinutesBeforeStart,
            Method = dto.Method,
            IsTriggered = false
        };

        _context.Set<EventReminder>().Add(reminder);
        await _context.SaveChangesAsync();

        return MapToReminderDto(reminder);
    }

    public async Task<bool> RemoveReminderAsync(int eventId, int reminderId, string userId)
    {
        var reminder = await _context.Set<EventReminder>()
            .Include(r => r.Event)
            .ThenInclude(e => e!.Calendar)
            .FirstOrDefaultAsync(r => r.Id == reminderId &&
                r.EventId == eventId &&
                (r.Event!.Calendar!.UserId == userId || r.Event.Attendees.Any(a => a.UserId == userId)));

        if (reminder == null) return false;

        _context.Set<EventReminder>().Remove(reminder);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<EventReminderDto>> GetDueRemindersAsync(string userId)
    {
        var now = DateTime.UtcNow;

        var reminders = await _context.Set<EventReminder>()
            .Include(r => r.Event)
            .ThenInclude(e => e!.Calendar)
            .Where(r => !r.IsTriggered &&
                r.Event!.Calendar!.UserId == userId &&
                r.Event.StartDateTime.AddMinutes(-r.MinutesBeforeStart) <= now)
            .ToListAsync();

        return reminders.Select(MapToReminderDto).ToList();
    }

    public async Task<bool> MarkReminderTriggeredAsync(int reminderId)
    {
        var reminder = await _context.Set<EventReminder>().FindAsync(reminderId);
        if (reminder == null) return false;

        reminder.IsTriggered = true;
        reminder.TriggeredAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Attachments

    public async Task<EventAttachmentDto> AddAttachmentAsync(int eventId, Stream fileStream, string fileName, string contentType, string userId)
    {
        var evt = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .FirstOrDefaultAsync(e => e.Id == eventId &&
                (e.Calendar!.UserId == userId || e.OrganizerId == userId));

        if (evt == null)
            throw new InvalidOperationException("Event not found");

        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        var contentBytes = memoryStream.ToArray();

        var attachment = new EventAttachment
        {
            EventId = eventId,
            FileName = fileName,
            ContentType = contentType,
            FileSize = contentBytes.Length,
            ContentBytes = contentBytes,
            FilePath = $"/uploads/events/{eventId}/{fileName}",
            UploadedAt = DateTime.UtcNow
        };

        _context.Set<EventAttachment>().Add(attachment);
        await _context.SaveChangesAsync();

        return MapToAttachmentDto(attachment);
    }

    public async Task<bool> RemoveAttachmentAsync(int eventId, int attachmentId, string userId)
    {
        var attachment = await _context.Set<EventAttachment>()
            .Include(a => a.Event)
            .ThenInclude(e => e!.Calendar)
            .FirstOrDefaultAsync(a => a.Id == attachmentId &&
                a.EventId == eventId &&
                (a.Event!.Calendar!.UserId == userId || a.Event.OrganizerId == userId));

        if (attachment == null) return false;

        _context.Set<EventAttachment>().Remove(attachment);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<(Stream stream, string contentType, string fileName)?> GetAttachmentAsync(int attachmentId, string userId)
    {
        var attachment = await _context.Set<EventAttachment>()
            .Include(a => a.Event)
            .ThenInclude(e => e!.Calendar)
            .FirstOrDefaultAsync(a => a.Id == attachmentId &&
                (a.Event!.Calendar!.UserId == userId || a.Event.Attendees.Any(att => att.UserId == userId)));

        if (attachment == null || attachment.ContentBytes == null)
            return null;

        var stream = new MemoryStream(attachment.ContentBytes);
        return (stream, attachment.ContentType, attachment.FileName);
    }

    #endregion

    #region Availability & Scheduling

    public async Task<List<AvailabilitySlotDto>> FindAvailabilityAsync(AvailabilityQueryDto query, string userId)
    {
        var slots = new List<AvailabilitySlotDto>();

        // Get all events for the attendees in the time range
        var attendeeEvents = await _context.CalendarEvents
            .Include(e => e.Attendees)
            .Where(e => e.StartDateTime <= query.EndDate &&
                e.EndDateTime >= query.StartDate &&
                !e.IsCancelled &&
                e.Attendees.Any(a => query.AttendeeEmails.Contains(a.Email)))
            .ToListAsync();

        // Simple algorithm: find time slots where all attendees are free
        var current = query.StartDate;
        while (current.AddMinutes(query.MeetingDurationMinutes) <= query.EndDate)
        {
            var slotEnd = current.AddMinutes(query.MeetingDurationMinutes);

            var busyAttendees = attendeeEvents
                .Where(e => e.StartDateTime < slotEnd && e.EndDateTime > current)
                .SelectMany(e => e.Attendees.Select(a => a.Email))
                .Distinct()
                .ToList();

            var availableAttendees = query.AttendeeEmails
                .Except(busyAttendees)
                .ToList();

            // Check resource availability
            var availableResourceIds = new List<int>();
            if (query.ResourceIds != null && query.ResourceIds.Any())
            {
                availableResourceIds = (await GetAvailableResourcesAsync(current, slotEnd))
                    .Where(r => query.ResourceIds.Contains(r.Id))
                    .Select(r => r.Id)
                    .ToList();
            }

            // Only add slot if all attendees are available
            if (availableAttendees.Count == query.AttendeeEmails.Count)
            {
                slots.Add(new AvailabilitySlotDto
                {
                    StartTime = current,
                    EndTime = slotEnd,
                    AvailableAttendees = availableAttendees,
                    UnavailableAttendees = busyAttendees,
                    AvailableResources = availableResourceIds
                });
            }

            current = current.AddMinutes(30); // Check every 30 minutes
        }

        return slots;
    }

    public async Task<List<CalendarEventDto>> GetUserBusyTimesAsync(string userId, DateTime startDate, DateTime endDate)
    {
        var events = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Where(e => e.Calendar!.UserId == userId &&
                e.StartDateTime <= endDate &&
                e.EndDateTime >= startDate &&
                !e.IsCancelled &&
                e.BusyStatus == EventBusyStatus.Busy)
            .ToListAsync();

        return events.Select(MapToEventDto).ToList();
    }

    public async Task<bool> CheckConflictAsync(int? eventId, DateTime startTime, DateTime endTime, string userId)
    {
        var query = _context.CalendarEvents
            .Include(e => e.Calendar)
            .Where(e => e.Calendar!.UserId == userId &&
                e.StartDateTime < endTime &&
                e.EndDateTime > startTime &&
                !e.IsCancelled &&
                e.BusyStatus == EventBusyStatus.Busy);

        if (eventId.HasValue)
            query = query.Where(e => e.Id != eventId.Value);

        return await query.AnyAsync();
    }

    #endregion

    #region Calendar Views

    public async Task<List<CalendarEventDto>> GetDayViewAsync(string userId, DateTime date, int? calendarId = null)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        var parameters = new CalendarQueryParameters
        {
            StartDate = startOfDay,
            EndDate = endOfDay,
            CalendarId = calendarId
        };

        return await GetEventsAsync(parameters, userId);
    }

    public async Task<List<CalendarEventDto>> GetWeekViewAsync(string userId, DateTime weekStart, int? calendarId = null)
    {
        var endOfWeek = weekStart.AddDays(7);

        var parameters = new CalendarQueryParameters
        {
            StartDate = weekStart,
            EndDate = endOfWeek,
            CalendarId = calendarId
        };

        return await GetEventsAsync(parameters, userId);
    }

    public async Task<List<CalendarEventDto>> GetMonthViewAsync(string userId, int year, int month, int? calendarId = null)
    {
        var startOfMonth = new DateTime(year, month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var parameters = new CalendarQueryParameters
        {
            StartDate = startOfMonth,
            EndDate = endOfMonth,
            CalendarId = calendarId
        };

        return await GetEventsAsync(parameters, userId);
    }

    #endregion

    #region Import/Export

    public async Task<string> ExportEventToIcsAsync(int eventId, string userId)
    {
        var evt = await GetEventAsync(eventId, userId);
        if (evt == null) return string.Empty;

        return GenerateIcsContent(new List<CalendarEventDto> { evt });
    }

    public async Task<string> ExportCalendarToIcsAsync(int calendarId, DateTime? startDate, DateTime? endDate, string userId)
    {
        var parameters = new CalendarQueryParameters
        {
            CalendarId = calendarId,
            StartDate = startDate ?? DateTime.UtcNow.AddMonths(-1),
            EndDate = endDate ?? DateTime.UtcNow.AddMonths(12)
        };

        var events = await GetEventsAsync(parameters, userId);
        return GenerateIcsContent(events);
    }

    public async Task<CalendarEventDto> ImportEventFromIcsAsync(string icsContent, int calendarId, string userId)
    {
        // Parse ICS content and create event
        // This is a simplified version - use Ical.Net in production
        var createDto = new CreateEventDto
        {
            CalendarId = calendarId,
            Title = "Imported Event",
            StartDateTime = DateTime.UtcNow,
            EndDateTime = DateTime.UtcNow.AddHours(1)
        };

        return await CreateEventAsync(createDto, userId);
    }

    #endregion

    #region Statistics

    public async Task<CalendarStatisticsDto> GetCalendarStatisticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow.AddMonths(1);

        var events = await _context.CalendarEvents
            .Include(e => e.Calendar)
            .Include(e => e.Attendees)
            .Where(e => e.Calendar!.UserId == userId &&
                e.StartDateTime >= start &&
                e.EndDateTime <= end)
            .ToListAsync();

        var stats = new CalendarStatisticsDto
        {
            TotalEvents = events.Count,
            TotalMeetings = events.Count(e => e.IsMeeting),
            TotalRecurringEvents = events.Count(e => e.IsRecurring),
            UpcomingEvents = events.Count(e => e.StartDateTime > DateTime.UtcNow && !e.IsCancelled),
            PendingResponses = events
                .SelectMany(e => e.Attendees)
                .Count(a => a.UserId == userId && a.ResponseStatus == ResponseStatus.NotResponded),
            AcceptedMeetings = events
                .SelectMany(e => e.Attendees)
                .Count(a => a.UserId == userId && a.ResponseStatus == ResponseStatus.Accepted),
            DeclinedMeetings = events
                .SelectMany(e => e.Attendees)
                .Count(a => a.UserId == userId && a.ResponseStatus == ResponseStatus.Declined),
            TentativeMeetings = events
                .SelectMany(e => e.Attendees)
                .Count(a => a.UserId == userId && a.ResponseStatus == ResponseStatus.Tentative),
            AverageEventsPerDay = events.Count / Math.Max(1, (end - start).Days),
            EventsByCategory = events
                .SelectMany(e => e.Categories)
                .GroupBy(c => c)
                .ToDictionary(g => g.Key, g => g.Count()),
            EventsByCalendar = events
                .GroupBy(e => e.Calendar!.Name)
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return stats;
    }

    #endregion

    #region Helper Methods

    private CalendarDto MapToCalendarDto(Calendar calendar)
    {
        return new CalendarDto
        {
            Id = calendar.Id,
            Name = calendar.Name,
            Description = calendar.Description,
            Color = calendar.Color,
            UserId = calendar.UserId,
            IsDefault = calendar.IsDefault,
            IsVisible = calendar.IsVisible,
            DisplayOrder = calendar.DisplayOrder,
            IsShared = calendar.IsShared,
            Shares = calendar.Shares?.Select(s => MapToCalendarShareDto(s, s.SharedWithUser)).ToList() ?? new(),
            CreatedAt = calendar.CreatedAt,
            LastModified = calendar.LastModified
        };
    }

    private CalendarShareDto MapToCalendarShareDto(CalendarShare share, ApplicationUser? user)
    {
        return new CalendarShareDto
        {
            Id = share.Id,
            CalendarId = share.CalendarId,
            SharedWithUserId = share.SharedWithUserId,
            SharedWithUserEmail = user?.Email ?? string.Empty,
            SharedWithUserName = user?.UserName ?? string.Empty,
            Permission = share.Permission,
            SharedAt = share.SharedAt
        };
    }

    private CalendarEventDto MapToEventDto(CalendarEvent evt)
    {
        return new CalendarEventDto
        {
            Id = evt.Id,
            EventId = evt.EventId,
            CalendarId = evt.CalendarId,
            CalendarName = evt.Calendar?.Name ?? string.Empty,
            Title = evt.Title,
            Body = evt.Body,
            Location = evt.Location,
            StartDateTime = evt.StartDateTime,
            EndDateTime = evt.EndDateTime,
            IsAllDay = evt.IsAllDay,
            TimeZone = evt.TimeZone,
            IsRecurring = evt.IsRecurring,
            RecurrenceRule = evt.RecurrenceRule,
            RecurrenceEnd = evt.RecurrenceEnd,
            RecurrenceExceptions = evt.RecurrenceExceptions,
            IsMeeting = evt.IsMeeting,
            IsOnlineMeeting = evt.IsOnlineMeeting,
            OnlineMeetingUrl = evt.OnlineMeetingUrl,
            OnlineMeetingProvider = evt.OnlineMeetingProvider,
            OrganizerId = evt.OrganizerId,
            OrganizerEmail = evt.Organizer?.Email ?? string.Empty,
            OrganizerName = evt.Organizer?.UserName ?? string.Empty,
            Status = evt.Status,
            BusyStatus = evt.BusyStatus,
            Importance = evt.Importance,
            Sensitivity = evt.Sensitivity,
            ResponseStatus = evt.ResponseStatus,
            ResponseTime = evt.ResponseTime,
            Categories = evt.Categories,
            Reminders = evt.Reminders?.Select(MapToReminderDto).ToList() ?? new(),
            Resources = evt.Resources?.Select(MapToEventResourceDto).ToList() ?? new(),
            Attachments = evt.Attachments?.Select(MapToAttachmentDto).ToList() ?? new(),
            Attendees = evt.Attendees?.Select(a => MapToAttendeeDto(a, a.User)).ToList() ?? new(),
            TravelTimeMinutes = evt.TravelTimeMinutes,
            CreatedAt = evt.CreatedAt,
            LastModified = evt.LastModified,
            IsCancelled = evt.IsCancelled
        };
    }

    private EventAttendeeDto MapToAttendeeDto(EventAttendee attendee, ApplicationUser? user)
    {
        return new EventAttendeeDto
        {
            Id = attendee.Id,
            Email = attendee.Email,
            DisplayName = attendee.DisplayName,
            Type = attendee.Type,
            ResponseStatus = attendee.ResponseStatus,
            ResponseTime = attendee.ResponseTime,
            ResponseComment = attendee.ResponseComment,
            ProposedStartTime = attendee.ProposedStartTime,
            ProposedEndTime = attendee.ProposedEndTime
        };
    }

    private EventReminderDto MapToReminderDto(EventReminder reminder)
    {
        return new EventReminderDto
        {
            Id = reminder.Id,
            MinutesBeforeStart = reminder.MinutesBeforeStart,
            Method = reminder.Method,
            IsTriggered = reminder.IsTriggered,
            TriggeredAt = reminder.TriggeredAt
        };
    }

    private EventResourceDto MapToEventResourceDto(EventResource eventResource)
    {
        return new EventResourceDto
        {
            Id = eventResource.Id,
            ResourceId = eventResource.ResourceId,
            ResourceName = eventResource.Resource?.Name ?? string.Empty,
            ResourceType = eventResource.Resource?.Type ?? ResourceType.Other,
            Status = eventResource.Status
        };
    }

    private EventAttachmentDto MapToAttachmentDto(EventAttachment attachment)
    {
        return new EventAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            FileSize = attachment.FileSize,
            UploadedAt = attachment.UploadedAt
        };
    }

    private ResourceDto MapToResourceDto(Resource resource)
    {
        return new ResourceDto
        {
            Id = resource.Id,
            Name = resource.Name,
            Description = resource.Description,
            Type = resource.Type,
            Building = resource.Building,
            Floor = resource.Floor,
            RoomNumber = resource.RoomNumber,
            Capacity = resource.Capacity,
            Equipment = resource.Equipment,
            IsAvailable = resource.IsAvailable,
            Email = resource.Email
        };
    }

    private string GenerateOnlineMeetingUrl(string eventId, string? provider)
    {
        return provider?.ToLower() switch
        {
            "teams" => $"https://teams.microsoft.com/l/meetup-join/{eventId}",
            "zoom" => $"https://zoom.us/j/{eventId}",
            "meet" => $"https://meet.google.com/{eventId}",
            _ => $"https://meet.example.com/{eventId}"
        };
    }

    private string GenerateIcsContent(List<CalendarEventDto> events)
    {
        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//Rasel Outlook Inbox Management//EN");
        sb.AppendLine("CALSCALE:GREGORIAN");
        sb.AppendLine("METHOD:PUBLISH");

        foreach (var evt in events)
        {
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{evt.EventId}");
            sb.AppendLine($"DTSTART:{evt.StartDateTime:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"DTEND:{evt.EndDateTime:yyyyMMddTHHmmssZ}");
            sb.AppendLine($"SUMMARY:{evt.Title}");
            sb.AppendLine($"DESCRIPTION:{evt.Body}");
            sb.AppendLine($"LOCATION:{evt.Location}");
            sb.AppendLine($"STATUS:{evt.Status}");
            sb.AppendLine($"ORGANIZER:mailto:{evt.OrganizerEmail}");

            foreach (var attendee in evt.Attendees)
            {
                sb.AppendLine($"ATTENDEE;ROLE={(attendee.Type == AttendeeType.Required ? "REQ-PARTICIPANT" : "OPT-PARTICIPANT")};PARTSTAT={attendee.ResponseStatus}:mailto:{attendee.Email}");
            }

            if (evt.IsRecurring && !string.IsNullOrEmpty(evt.RecurrenceRule))
            {
                sb.AppendLine($"RRULE:{evt.RecurrenceRule}");
            }

            foreach (var reminder in evt.Reminders)
            {
                sb.AppendLine("BEGIN:VALARM");
                sb.AppendLine($"TRIGGER:-PT{reminder.MinutesBeforeStart}M");
                sb.AppendLine("ACTION:DISPLAY");
                sb.AppendLine($"DESCRIPTION:Reminder: {evt.Title}");
                sb.AppendLine("END:VALARM");
            }

            sb.AppendLine("END:VEVENT");
        }

        sb.AppendLine("END:VCALENDAR");
        return sb.ToString();
    }

    #endregion
}
