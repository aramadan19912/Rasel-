using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Application.DTOs.Calendar;
using Backend.Application.Interfaces;
using Backend.Domain.Enums;
using Backend.Infrastructure.Authorization;
using System.Security.Claims;

namespace Backend.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ===== Calendar CRUD =====

    [HttpGet]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarDto>>> GetAllCalendars()
    {
        var calendars = await _calendarService.GetAllCalendarsAsync(GetUserId());
        return Ok(calendars);
    }

    [HttpGet("{id}")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<CalendarDto>> GetCalendarById(int id)
    {
        try
        {
            var calendar = await _calendarService.GetCalendarByIdAsync(id, GetUserId());
            return Ok(calendar);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("default")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<CalendarDto>> GetDefaultCalendar()
    {
        try
        {
            var calendar = await _calendarService.GetDefaultCalendarAsync(GetUserId());
            return Ok(calendar);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    [Permission(SystemPermission.CalendarCreate)]
    public async Task<ActionResult<CalendarDto>> CreateCalendar([FromBody] CreateCalendarDto dto)
    {
        var calendar = await _calendarService.CreateCalendarAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetCalendarById), new { id = calendar.Id }, calendar);
    }

    [HttpPut("{id}")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> UpdateCalendar(int id, [FromBody] UpdateCalendarDto dto)
    {
        var result = await _calendarService.UpdateCalendarAsync(id, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Permission(SystemPermission.CalendarDelete)]
    public async Task<IActionResult> DeleteCalendar(int id)
    {
        var result = await _calendarService.DeleteCalendarAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/set-default")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> SetDefaultCalendar(int id)
    {
        await _calendarService.SetDefaultCalendarAsync(id, GetUserId());
        return NoContent();
    }

    // ===== Calendar Sharing =====

    [HttpGet("{calendarId}/shares")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarShareDto>>> GetCalendarShares(int calendarId)
    {
        try
        {
            var shares = await _calendarService.GetCalendarSharesAsync(calendarId, GetUserId());
            return Ok(shares);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{calendarId}/share")]
    [Permission(SystemPermission.CalendarShare)]
    public async Task<ActionResult<CalendarShareDto>> ShareCalendar(int calendarId, [FromBody] ShareCalendarDto dto)
    {
        try
        {
            var share = await _calendarService.ShareCalendarAsync(calendarId, dto, GetUserId());
            return Ok(share);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("shares/{shareId}")]
    [Permission(SystemPermission.CalendarShare)]
    public async Task<IActionResult> UpdateCalendarShare(int shareId, [FromBody] CalendarPermission permission)
    {
        var result = await _calendarService.UpdateCalendarShareAsync(shareId, permission, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("shares/{shareId}")]
    [Permission(SystemPermission.CalendarShare)]
    public async Task<IActionResult> RemoveCalendarShare(int shareId)
    {
        var result = await _calendarService.RemoveCalendarShareAsync(shareId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("shared-with-me")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarDto>>> GetSharedWithMe()
    {
        var calendars = await _calendarService.GetSharedWithMeAsync(GetUserId());
        return Ok(calendars);
    }

    // ===== Event CRUD =====

    [HttpGet("{calendarId}/events")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarEventDto>>> GetEvents(int calendarId, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var events = await _calendarService.GetEventsAsync(calendarId, GetUserId(), startDate, endDate);
        return Ok(events);
    }

    [HttpGet("events")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarEventDto>>> GetAllEvents([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var events = await _calendarService.GetAllEventsAsync(GetUserId(), startDate, endDate);
        return Ok(events);
    }

    [HttpGet("events/upcoming")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarEventDto>>> GetUpcomingEvents([FromQuery] int days = 7)
    {
        var events = await _calendarService.GetUpcomingEventsAsync(GetUserId(), days);
        return Ok(events);
    }

    [HttpGet("events/by-date")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarEventDto>>> GetEventsByDate([FromQuery] DateTime date)
    {
        var events = await _calendarService.GetEventsByDateAsync(date, GetUserId());
        return Ok(events);
    }

    [HttpGet("events/{id}")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<CalendarEventDto>> GetEventById(int id)
    {
        try
        {
            var calendarEvent = await _calendarService.GetEventByIdAsync(id, GetUserId());
            return Ok(calendarEvent);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("events/search")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarEventDto>>> SearchEvents([FromQuery] string query)
    {
        var events = await _calendarService.SearchEventsAsync(query, GetUserId());
        return Ok(events);
    }

    [HttpPost("events")]
    [Permission(SystemPermission.CalendarCreate)]
    public async Task<ActionResult<CalendarEventDto>> CreateEvent([FromBody] CreateCalendarEventDto dto)
    {
        var calendarEvent = await _calendarService.CreateEventAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetEventById), new { id = calendarEvent.Id }, calendarEvent);
    }

    [HttpPost("events/with-conference")]
    [Permission(SystemPermission.CalendarCreate)]
    public async Task<ActionResult<CalendarEventDto>> CreateEventWithVideoConference([FromBody] CreateCalendarEventDto dto)
    {
        var calendarEvent = await _calendarService.CreateEventWithVideoConferenceAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetEventById), new { id = calendarEvent.Id }, calendarEvent);
    }

    [HttpPut("events/{id}")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateCalendarEventDto dto)
    {
        var result = await _calendarService.UpdateEventAsync(id, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("events/{id}")]
    [Permission(SystemPermission.CalendarDelete)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await _calendarService.DeleteEventAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("events/{id}/cancel")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> CancelEvent(int id)
    {
        var result = await _calendarService.CancelEventAsync(id, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Recurring Events =====

    [HttpPut("events/{id}/recurring")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> UpdateRecurringEvent(int id, [FromBody] UpdateRecurringEventRequest request)
    {
        var result = await _calendarService.UpdateRecurringEventAsync(id, request.Dto, request.UpdateSeries, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("events/{id}/recurring")]
    [Permission(SystemPermission.CalendarDelete)]
    public async Task<IActionResult> DeleteRecurringEvent(int id, [FromQuery] bool deleteSeries = false)
    {
        var result = await _calendarService.DeleteRecurringEventAsync(id, deleteSeries, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("events/{eventId}/recurrence-exception")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> AddRecurrenceException(int eventId, [FromBody] DateTime exceptionDate)
    {
        var result = await _calendarService.AddRecurrenceExceptionAsync(eventId, exceptionDate, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("events/{eventId}/instances")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarEventDto>>> GetRecurringEventInstances(
        int eventId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var instances = await _calendarService.GetRecurringEventInstancesAsync(eventId, startDate, endDate, GetUserId());
        return Ok(instances);
    }

    // ===== Event Attendees =====

    [HttpGet("events/{eventId}/attendees")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<EventAttendeeDto>>> GetEventAttendees(int eventId)
    {
        try
        {
            var attendees = await _calendarService.GetEventAttendeesAsync(eventId, GetUserId());
            return Ok(attendees);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("events/{eventId}/attendees")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> AddAttendee(int eventId, [FromBody] CreateEventAttendeeDto dto)
    {
        var result = await _calendarService.AddAttendeeAsync(eventId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("events/{eventId}/attendees/{attendeeId}")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> RemoveAttendee(int eventId, int attendeeId)
    {
        var result = await _calendarService.RemoveAttendeeAsync(eventId, attendeeId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPut("events/{eventId}/attendees/{attendeeId}/response")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> UpdateAttendeeResponse(int eventId, int attendeeId, [FromBody] UpdateAttendeeResponseDto dto)
    {
        var result = await _calendarService.UpdateAttendeeResponseAsync(eventId, attendeeId, dto, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("events/{eventId}/respond")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> RespondToEvent(int eventId, [FromBody] RespondToEventRequest request)
    {
        var result = await _calendarService.RespondToEventAsync(eventId, request.Response, request.Comment, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Event Reminders =====

    [HttpGet("events/{eventId}/reminders")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<EventReminderDto>>> GetEventReminders(int eventId)
    {
        try
        {
            var reminders = await _calendarService.GetEventRemindersAsync(eventId, GetUserId());
            return Ok(reminders);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("events/{eventId}/reminders")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> AddReminder(int eventId, [FromBody] AddReminderRequest request)
    {
        var result = await _calendarService.AddReminderAsync(eventId, request.MinutesBefore, request.Type, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("events/{eventId}/reminders/{reminderId}")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> RemoveReminder(int eventId, int reminderId)
    {
        var result = await _calendarService.RemoveReminderAsync(eventId, reminderId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("events/reminders/upcoming")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<CalendarEventDto>>> GetUpcomingReminders([FromQuery] int minutesAhead = 60)
    {
        var events = await _calendarService.GetUpcomingRemindersAsync(GetUserId(), minutesAhead);
        return Ok(events);
    }

    // ===== Event Categories =====

    [HttpPost("events/{eventId}/categories")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> AddEventCategory(int eventId, [FromBody] string category)
    {
        var result = await _calendarService.AddEventCategoryAsync(eventId, category, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("events/{eventId}/categories/{category}")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> RemoveEventCategory(int eventId, string category)
    {
        var result = await _calendarService.RemoveEventCategoryAsync(eventId, category, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Availability =====

    [HttpGet("availability")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<TimeSlotDto>>> GetAvailability([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var slots = await _calendarService.GetAvailabilityAsync(GetUserId(), startDate, endDate);
        return Ok(slots);
    }

    [HttpPost("find-free-slots")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<List<TimeSlotDto>>> FindFreeSlots([FromBody] FindFreeSlotsRequest request)
    {
        var slots = await _calendarService.FindFreeSlotsAsync(request.UserIds, request.StartDate, request.EndDate, request.DurationMinutes);
        return Ok(slots);
    }

    [HttpGet("is-available")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<bool>> IsAvailable([FromQuery] DateTime startTime, [FromQuery] DateTime endTime)
    {
        var isAvailable = await _calendarService.IsAvailableAsync(GetUserId(), startTime, endTime);
        return Ok(isAvailable);
    }

    // ===== Integration =====

    [HttpPost("events/{eventId}/link-conference/{conferenceId}")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> LinkVideoConference(int eventId, int conferenceId)
    {
        var result = await _calendarService.LinkVideoConferenceAsync(eventId, conferenceId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpDelete("events/{eventId}/unlink-conference")]
    [Permission(SystemPermission.CalendarUpdate)]
    public async Task<IActionResult> UnlinkVideoConference(int eventId)
    {
        var result = await _calendarService.UnlinkVideoConferenceAsync(eventId, GetUserId());
        if (!result) return NotFound();
        return NoContent();
    }

    // ===== Statistics =====

    [HttpGet("statistics")]
    [Permission(SystemPermission.CalendarRead)]
    public async Task<ActionResult<CalendarStatisticsDto>> GetStatistics()
    {
        var statistics = await _calendarService.GetStatisticsAsync(GetUserId());
        return Ok(statistics);
    }
}

public record UpdateRecurringEventRequest(UpdateCalendarEventDto Dto, bool UpdateSeries);
public record RespondToEventRequest(ResponseStatus Response, string? Comment);
public record AddReminderRequest(int MinutesBefore, ReminderType Type);
public record FindFreeSlotsRequest(List<string> UserIds, DateTime StartDate, DateTime EndDate, int DurationMinutes);
