using OutlookInboxManagement.DTOs;
using OutlookInboxManagement.Models;

namespace OutlookInboxManagement.Services;

public interface ICalendarService
{
    // Calendar Management
    Task<CalendarDto?> GetCalendarAsync(int calendarId, string userId);
    Task<List<CalendarDto>> GetUserCalendarsAsync(string userId);
    Task<CalendarDto> CreateCalendarAsync(CreateCalendarDto dto, string userId);
    Task<CalendarDto?> UpdateCalendarAsync(int calendarId, UpdateCalendarDto dto, string userId);
    Task<bool> DeleteCalendarAsync(int calendarId, string userId);
    Task<CalendarDto?> SetDefaultCalendarAsync(int calendarId, string userId);

    // Calendar Sharing
    Task<CalendarShareDto> ShareCalendarAsync(int calendarId, ShareCalendarDto dto, string userId);
    Task<bool> RevokeCalendarShareAsync(int calendarId, int shareId, string userId);
    Task<List<CalendarDto>> GetSharedCalendarsAsync(string userId);

    // Event Management
    Task<CalendarEventDto?> GetEventAsync(int eventId, string userId);
    Task<CalendarEventDto?> GetEventByEventIdAsync(string eventId, string userId);
    Task<List<CalendarEventDto>> GetEventsAsync(CalendarQueryParameters parameters, string userId);
    Task<CalendarEventDto> CreateEventAsync(CreateEventDto dto, string userId);
    Task<CalendarEventDto?> UpdateEventAsync(int eventId, UpdateEventDto dto, string userId);
    Task<bool> DeleteEventAsync(int eventId, string userId);
    Task<bool> CancelEventAsync(int eventId, string userId, string? cancellationMessage = null);

    // Recurring Events
    Task<List<CalendarEventDto>> GetRecurringEventInstancesAsync(int eventId, DateTime startDate, DateTime endDate, string userId);
    Task<CalendarEventDto?> UpdateRecurringEventInstanceAsync(int eventId, DateTime instanceDate, UpdateEventDto dto, string userId);
    Task<bool> DeleteRecurringEventInstanceAsync(int eventId, DateTime instanceDate, string userId);

    // Attendee Management
    Task<EventAttendeeDto> AddAttendeeAsync(int eventId, CreateAttendeeDto dto, string userId);
    Task<bool> RemoveAttendeeAsync(int eventId, int attendeeId, string userId);
    Task<bool> UpdateAttendeeResponseAsync(int eventId, UpdateAttendeeResponseDto dto, string userId);
    Task<List<EventAttendeeDto>> GetEventAttendeesAsync(int eventId, string userId);

    // Resource Management
    Task<ResourceDto?> GetResourceAsync(int resourceId);
    Task<List<ResourceDto>> GetResourcesAsync(ResourceType? type = null);
    Task<ResourceDto> CreateResourceAsync(CreateResourceDto dto);
    Task<bool> DeleteResourceAsync(int resourceId);
    Task<bool> BookResourceAsync(int eventId, int resourceId, string userId);
    Task<bool> ReleaseResourceAsync(int eventId, int resourceId, string userId);
    Task<List<ResourceDto>> GetAvailableResourcesAsync(DateTime startTime, DateTime endTime, ResourceType? type = null);

    // Reminders
    Task<EventReminderDto> AddReminderAsync(int eventId, CreateReminderDto dto, string userId);
    Task<bool> RemoveReminderAsync(int eventId, int reminderId, string userId);
    Task<List<EventReminderDto>> GetDueRemindersAsync(string userId);
    Task<bool> MarkReminderTriggeredAsync(int reminderId);

    // Attachments
    Task<EventAttachmentDto> AddAttachmentAsync(int eventId, Stream fileStream, string fileName, string contentType, string userId);
    Task<bool> RemoveAttachmentAsync(int eventId, int attachmentId, string userId);
    Task<(Stream stream, string contentType, string fileName)?> GetAttachmentAsync(int attachmentId, string userId);

    // Availability & Scheduling
    Task<List<AvailabilitySlotDto>> FindAvailabilityAsync(AvailabilityQueryDto query, string userId);
    Task<List<CalendarEventDto>> GetUserBusyTimesAsync(string userId, DateTime startDate, DateTime endDate);
    Task<bool> CheckConflictAsync(int? eventId, DateTime startTime, DateTime endTime, string userId);

    // Calendar Views
    Task<List<CalendarEventDto>> GetDayViewAsync(string userId, DateTime date, int? calendarId = null);
    Task<List<CalendarEventDto>> GetWeekViewAsync(string userId, DateTime weekStart, int? calendarId = null);
    Task<List<CalendarEventDto>> GetMonthViewAsync(string userId, int year, int month, int? calendarId = null);

    // Import/Export
    Task<string> ExportEventToIcsAsync(int eventId, string userId);
    Task<string> ExportCalendarToIcsAsync(int calendarId, DateTime? startDate, DateTime? endDate, string userId);
    Task<CalendarEventDto> ImportEventFromIcsAsync(string icsContent, int calendarId, string userId);

    // Statistics
    Task<CalendarStatisticsDto> GetCalendarStatisticsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
}

public class CalendarStatisticsDto
{
    public int TotalEvents { get; set; }
    public int TotalMeetings { get; set; }
    public int TotalRecurringEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int PendingResponses { get; set; }
    public int AcceptedMeetings { get; set; }
    public int DeclinedMeetings { get; set; }
    public int TentativeMeetings { get; set; }
    public double AverageEventsPerDay { get; set; }
    public Dictionary<string, int> EventsByCategory { get; set; } = new();
    public Dictionary<string, int> EventsByCalendar { get; set; } = new();
}
