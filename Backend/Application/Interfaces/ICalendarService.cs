using Backend.Application.DTOs.Calendar;

namespace Backend.Application.Interfaces;

public interface ICalendarService
{
    // ===== Calendar CRUD =====
    Task<List<CalendarDto>> GetAllCalendarsAsync(string userId);
    Task<CalendarDto> GetCalendarByIdAsync(int id, string userId);
    Task<CalendarDto> CreateCalendarAsync(CreateCalendarDto dto, string userId);
    Task<bool> UpdateCalendarAsync(int id, UpdateCalendarDto dto, string userId);
    Task<bool> DeleteCalendarAsync(int id, string userId);
    Task<CalendarDto> GetDefaultCalendarAsync(string userId);
    Task<bool> SetDefaultCalendarAsync(int id, string userId);

    // ===== Calendar Sharing =====
    Task<List<CalendarShareDto>> GetCalendarSharesAsync(int calendarId, string userId);
    Task<CalendarShareDto> ShareCalendarAsync(int calendarId, ShareCalendarDto dto, string userId);
    Task<bool> UpdateCalendarShareAsync(int shareId, CalendarPermission permission, string userId);
    Task<bool> RemoveCalendarShareAsync(int shareId, string userId);
    Task<List<CalendarDto>> GetSharedWithMeAsync(string userId);

    // ===== Event CRUD =====
    Task<List<CalendarEventDto>> GetEventsAsync(int calendarId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<CalendarEventDto>> GetAllEventsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<CalendarEventDto>> GetUpcomingEventsAsync(string userId, int days = 7);
    Task<List<CalendarEventDto>> GetEventsByDateAsync(DateTime date, string userId);
    Task<CalendarEventDto> GetEventByIdAsync(int id, string userId);
    Task<CalendarEventDto> GetEventByEventIdAsync(string eventId, string userId);
    Task<CalendarEventDto> CreateEventAsync(CreateCalendarEventDto dto, string userId);
    Task<bool> UpdateEventAsync(int id, UpdateCalendarEventDto dto, string userId);
    Task<bool> DeleteEventAsync(int id, string userId);
    Task<bool> CancelEventAsync(int id, string userId);
    Task<List<CalendarEventDto>> SearchEventsAsync(string query, string userId);

    // ===== Recurring Events =====
    Task<bool> UpdateRecurringEventAsync(int id, UpdateCalendarEventDto dto, bool updateSeries, string userId);
    Task<bool> DeleteRecurringEventAsync(int id, bool deleteSeries, string userId);
    Task<bool> AddRecurrenceExceptionAsync(int eventId, DateTime exceptionDate, string userId);
    Task<List<CalendarEventDto>> GetRecurringEventInstancesAsync(int eventId, DateTime startDate, DateTime endDate, string userId);

    // ===== Event Attendees =====
    Task<List<EventAttendeeDto>> GetEventAttendeesAsync(int eventId, string userId);
    Task<bool> AddAttendeeAsync(int eventId, CreateEventAttendeeDto dto, string userId);
    Task<bool> RemoveAttendeeAsync(int eventId, int attendeeId, string userId);
    Task<bool> UpdateAttendeeResponseAsync(int eventId, int attendeeId, UpdateAttendeeResponseDto dto, string userId);
    Task<bool> RespondToEventAsync(int eventId, ResponseStatus response, string? comment, string userId);

    // ===== Event Reminders =====
    Task<List<EventReminderDto>> GetEventRemindersAsync(int eventId, string userId);
    Task<bool> AddReminderAsync(int eventId, int minutesBefore, ReminderType type, string userId);
    Task<bool> RemoveReminderAsync(int eventId, int reminderId, string userId);
    Task<List<CalendarEventDto>> GetUpcomingRemindersAsync(string userId, int minutesAhead = 60);

    // ===== Event Categories =====
    Task<bool> AddEventCategoryAsync(int eventId, string category, string userId);
    Task<bool> RemoveEventCategoryAsync(int eventId, string category, string userId);

    // ===== Availability =====
    Task<List<TimeSlotDto>> GetAvailabilityAsync(string userId, DateTime startDate, DateTime endDate);
    Task<List<TimeSlotDto>> FindFreeSlotsAsync(List<string> userIds, DateTime startDate, DateTime endDate, int durationMinutes);
    Task<bool> IsAvailableAsync(string userId, DateTime startTime, DateTime endTime);

    // ===== Integration =====
    Task<CalendarEventDto> CreateEventWithVideoConferenceAsync(CreateCalendarEventDto dto, string userId);
    Task<bool> LinkVideoConferenceAsync(int eventId, int conferenceId, string userId);
    Task<bool> UnlinkVideoConferenceAsync(int eventId, string userId);

    // ===== Statistics =====
    Task<CalendarStatisticsDto> GetStatisticsAsync(string userId);
}

public class TimeSlotDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsAvailable { get; set; }
    public string? EventTitle { get; set; }
}

public class CalendarStatisticsDto
{
    public int TotalCalendars { get; set; }
    public int TotalEvents { get; set; }
    public int UpcomingEvents { get; set; }
    public int TodayEvents { get; set; }
    public int SharedCalendars { get; set; }
    public Dictionary<string, int> EventsByCalendar { get; set; } = new();
    public Dictionary<string, int> EventsByStatus { get; set; } = new();
}
