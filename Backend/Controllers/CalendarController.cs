using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutlookInboxManagement.DTOs;
using OutlookInboxManagement.Models;
using OutlookInboxManagement.Services;
using System.Security.Claims;

namespace OutlookInboxManagement.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendarService;

    public CalendarController(ICalendarService calendarService)
    {
        _calendarService = calendarService;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    #region Calendar Management

    /// <summary>
    /// Get a specific calendar by ID
    /// </summary>
    [HttpGet("calendars/{id}")]
    public async Task<ActionResult<CalendarDto>> GetCalendar(int id)
    {
        var calendar = await _calendarService.GetCalendarAsync(id, GetUserId());
        if (calendar == null)
            return NotFound();

        return Ok(calendar);
    }

    /// <summary>
    /// Get all user calendars
    /// </summary>
    [HttpGet("calendars")]
    public async Task<ActionResult<List<CalendarDto>>> GetUserCalendars()
    {
        var calendars = await _calendarService.GetUserCalendarsAsync(GetUserId());
        return Ok(calendars);
    }

    /// <summary>
    /// Get shared calendars
    /// </summary>
    [HttpGet("calendars/shared")]
    public async Task<ActionResult<List<CalendarDto>>> GetSharedCalendars()
    {
        var calendars = await _calendarService.GetSharedCalendarsAsync(GetUserId());
        return Ok(calendars);
    }

    /// <summary>
    /// Create a new calendar
    /// </summary>
    [HttpPost("calendars")]
    public async Task<ActionResult<CalendarDto>> CreateCalendar([FromBody] CreateCalendarDto dto)
    {
        var calendar = await _calendarService.CreateCalendarAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetCalendar), new { id = calendar.Id }, calendar);
    }

    /// <summary>
    /// Update a calendar
    /// </summary>
    [HttpPut("calendars/{id}")]
    public async Task<ActionResult<CalendarDto>> UpdateCalendar(int id, [FromBody] UpdateCalendarDto dto)
    {
        var calendar = await _calendarService.UpdateCalendarAsync(id, dto, GetUserId());
        if (calendar == null)
            return NotFound();

        return Ok(calendar);
    }

    /// <summary>
    /// Delete a calendar
    /// </summary>
    [HttpDelete("calendars/{id}")]
    public async Task<IActionResult> DeleteCalendar(int id)
    {
        var result = await _calendarService.DeleteCalendarAsync(id, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Set calendar as default
    /// </summary>
    [HttpPost("calendars/{id}/set-default")]
    public async Task<ActionResult<CalendarDto>> SetDefaultCalendar(int id)
    {
        var calendar = await _calendarService.SetDefaultCalendarAsync(id, GetUserId());
        if (calendar == null)
            return NotFound();

        return Ok(calendar);
    }

    #endregion

    #region Calendar Sharing

    /// <summary>
    /// Share a calendar with another user
    /// </summary>
    [HttpPost("calendars/{id}/share")]
    public async Task<ActionResult<CalendarShareDto>> ShareCalendar(int id, [FromBody] ShareCalendarDto dto)
    {
        try
        {
            var share = await _calendarService.ShareCalendarAsync(id, dto, GetUserId());
            return Ok(share);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Revoke calendar sharing
    /// </summary>
    [HttpDelete("calendars/{calendarId}/share/{shareId}")]
    public async Task<IActionResult> RevokeCalendarShare(int calendarId, int shareId)
    {
        var result = await _calendarService.RevokeCalendarShareAsync(calendarId, shareId, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    #endregion

    #region Event Management

    /// <summary>
    /// Get a specific event by ID
    /// </summary>
    [HttpGet("events/{id}")]
    public async Task<ActionResult<CalendarEventDto>> GetEvent(int id)
    {
        var evt = await _calendarService.GetEventAsync(id, GetUserId());
        if (evt == null)
            return NotFound();

        return Ok(evt);
    }

    /// <summary>
    /// Get event by EventId (GUID)
    /// </summary>
    [HttpGet("events/by-event-id/{eventId}")]
    public async Task<ActionResult<CalendarEventDto>> GetEventByEventId(string eventId)
    {
        var evt = await _calendarService.GetEventByEventIdAsync(eventId, GetUserId());
        if (evt == null)
            return NotFound();

        return Ok(evt);
    }

    /// <summary>
    /// Get events with query parameters
    /// </summary>
    [HttpGet("events")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetEvents([FromQuery] CalendarQueryParameters parameters)
    {
        var events = await _calendarService.GetEventsAsync(parameters, GetUserId());
        return Ok(events);
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    [HttpPost("events")]
    public async Task<ActionResult<CalendarEventDto>> CreateEvent([FromBody] CreateEventDto dto)
    {
        try
        {
            var evt = await _calendarService.CreateEventAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, evt);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an event
    /// </summary>
    [HttpPut("events/{id}")]
    public async Task<ActionResult<CalendarEventDto>> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
    {
        var evt = await _calendarService.UpdateEventAsync(id, dto, GetUserId());
        if (evt == null)
            return NotFound();

        return Ok(evt);
    }

    /// <summary>
    /// Delete an event
    /// </summary>
    [HttpDelete("events/{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await _calendarService.DeleteEventAsync(id, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Cancel an event
    /// </summary>
    [HttpPost("events/{id}/cancel")]
    public async Task<IActionResult> CancelEvent(int id, [FromBody] string? cancellationMessage = null)
    {
        var result = await _calendarService.CancelEventAsync(id, GetUserId(), cancellationMessage);
        if (!result)
            return NotFound();

        return Ok();
    }

    #endregion

    #region Recurring Events

    /// <summary>
    /// Get recurring event instances
    /// </summary>
    [HttpGet("events/{id}/instances")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetRecurringEventInstances(
        int id,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var instances = await _calendarService.GetRecurringEventInstancesAsync(id, startDate, endDate, GetUserId());
        return Ok(instances);
    }

    /// <summary>
    /// Update a recurring event instance
    /// </summary>
    [HttpPut("events/{id}/instances/{instanceDate}")]
    public async Task<ActionResult<CalendarEventDto>> UpdateRecurringEventInstance(
        int id,
        DateTime instanceDate,
        [FromBody] UpdateEventDto dto)
    {
        var evt = await _calendarService.UpdateRecurringEventInstanceAsync(id, instanceDate, dto, GetUserId());
        if (evt == null)
            return NotFound();

        return Ok(evt);
    }

    /// <summary>
    /// Delete a recurring event instance
    /// </summary>
    [HttpDelete("events/{id}/instances/{instanceDate}")]
    public async Task<IActionResult> DeleteRecurringEventInstance(int id, DateTime instanceDate)
    {
        var result = await _calendarService.DeleteRecurringEventInstanceAsync(id, instanceDate, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    #endregion

    #region Attendee Management

    /// <summary>
    /// Get event attendees
    /// </summary>
    [HttpGet("events/{id}/attendees")]
    public async Task<ActionResult<List<EventAttendeeDto>>> GetEventAttendees(int id)
    {
        var attendees = await _calendarService.GetEventAttendeesAsync(id, GetUserId());
        return Ok(attendees);
    }

    /// <summary>
    /// Add an attendee to an event
    /// </summary>
    [HttpPost("events/{id}/attendees")]
    public async Task<ActionResult<EventAttendeeDto>> AddAttendee(int id, [FromBody] CreateAttendeeDto dto)
    {
        try
        {
            var attendee = await _calendarService.AddAttendeeAsync(id, dto, GetUserId());
            return Ok(attendee);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove an attendee from an event
    /// </summary>
    [HttpDelete("events/{eventId}/attendees/{attendeeId}")]
    public async Task<IActionResult> RemoveAttendee(int eventId, int attendeeId)
    {
        var result = await _calendarService.RemoveAttendeeAsync(eventId, attendeeId, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Update attendee response
    /// </summary>
    [HttpPost("events/{id}/respond")]
    public async Task<IActionResult> UpdateAttendeeResponse(int id, [FromBody] UpdateAttendeeResponseDto dto)
    {
        var result = await _calendarService.UpdateAttendeeResponseAsync(id, dto, GetUserId());
        if (!result)
            return NotFound();

        return Ok();
    }

    #endregion

    #region Resource Management

    /// <summary>
    /// Get all resources
    /// </summary>
    [HttpGet("resources")]
    public async Task<ActionResult<List<ResourceDto>>> GetResources([FromQuery] ResourceType? type = null)
    {
        var resources = await _calendarService.GetResourcesAsync(type);
        return Ok(resources);
    }

    /// <summary>
    /// Get a specific resource
    /// </summary>
    [HttpGet("resources/{id}")]
    public async Task<ActionResult<ResourceDto>> GetResource(int id)
    {
        var resource = await _calendarService.GetResourceAsync(id);
        if (resource == null)
            return NotFound();

        return Ok(resource);
    }

    /// <summary>
    /// Create a new resource
    /// </summary>
    [HttpPost("resources")]
    public async Task<ActionResult<ResourceDto>> CreateResource([FromBody] CreateResourceDto dto)
    {
        var resource = await _calendarService.CreateResourceAsync(dto);
        return CreatedAtAction(nameof(GetResource), new { id = resource.Id }, resource);
    }

    /// <summary>
    /// Delete a resource
    /// </summary>
    [HttpDelete("resources/{id}")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var result = await _calendarService.DeleteResourceAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Book a resource for an event
    /// </summary>
    [HttpPost("events/{eventId}/resources/{resourceId}")]
    public async Task<IActionResult> BookResource(int eventId, int resourceId)
    {
        var result = await _calendarService.BookResourceAsync(eventId, resourceId, GetUserId());
        if (!result)
            return BadRequest("Resource is not available or already booked");

        return Ok();
    }

    /// <summary>
    /// Release a resource from an event
    /// </summary>
    [HttpDelete("events/{eventId}/resources/{resourceId}")]
    public async Task<IActionResult> ReleaseResource(int eventId, int resourceId)
    {
        var result = await _calendarService.ReleaseResourceAsync(eventId, resourceId, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get available resources for a time slot
    /// </summary>
    [HttpGet("resources/available")]
    public async Task<ActionResult<List<ResourceDto>>> GetAvailableResources(
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime,
        [FromQuery] ResourceType? type = null)
    {
        var resources = await _calendarService.GetAvailableResourcesAsync(startTime, endTime, type);
        return Ok(resources);
    }

    #endregion

    #region Reminders

    /// <summary>
    /// Add a reminder to an event
    /// </summary>
    [HttpPost("events/{id}/reminders")]
    public async Task<ActionResult<EventReminderDto>> AddReminder(int id, [FromBody] CreateReminderDto dto)
    {
        try
        {
            var reminder = await _calendarService.AddReminderAsync(id, dto, GetUserId());
            return Ok(reminder);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove a reminder from an event
    /// </summary>
    [HttpDelete("events/{eventId}/reminders/{reminderId}")]
    public async Task<IActionResult> RemoveReminder(int eventId, int reminderId)
    {
        var result = await _calendarService.RemoveReminderAsync(eventId, reminderId, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Get due reminders for the current user
    /// </summary>
    [HttpGet("reminders/due")]
    public async Task<ActionResult<List<EventReminderDto>>> GetDueReminders()
    {
        var reminders = await _calendarService.GetDueRemindersAsync(GetUserId());
        return Ok(reminders);
    }

    /// <summary>
    /// Mark a reminder as triggered
    /// </summary>
    [HttpPost("reminders/{id}/mark-triggered")]
    public async Task<IActionResult> MarkReminderTriggered(int id)
    {
        var result = await _calendarService.MarkReminderTriggeredAsync(id);
        if (!result)
            return NotFound();

        return Ok();
    }

    #endregion

    #region Attachments

    /// <summary>
    /// Add an attachment to an event
    /// </summary>
    [HttpPost("events/{id}/attachments")]
    public async Task<ActionResult<EventAttachmentDto>> AddAttachment(int id, IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var attachment = await _calendarService.AddAttachmentAsync(
                id, stream, file.FileName, file.ContentType, GetUserId());
            return Ok(attachment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Remove an attachment from an event
    /// </summary>
    [HttpDelete("events/{eventId}/attachments/{attachmentId}")]
    public async Task<IActionResult> RemoveAttachment(int eventId, int attachmentId)
    {
        var result = await _calendarService.RemoveAttachmentAsync(eventId, attachmentId, GetUserId());
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Download an event attachment
    /// </summary>
    [HttpGet("attachments/{id}")]
    public async Task<IActionResult> GetAttachment(int id)
    {
        var result = await _calendarService.GetAttachmentAsync(id, GetUserId());
        if (result == null)
            return NotFound();

        var (stream, contentType, fileName) = result.Value;
        return File(stream, contentType, fileName);
    }

    #endregion

    #region Availability & Scheduling

    /// <summary>
    /// Find available time slots
    /// </summary>
    [HttpPost("availability/find")]
    public async Task<ActionResult<List<AvailabilitySlotDto>>> FindAvailability([FromBody] AvailabilityQueryDto query)
    {
        var slots = await _calendarService.FindAvailabilityAsync(query, GetUserId());
        return Ok(slots);
    }

    /// <summary>
    /// Get user busy times
    /// </summary>
    [HttpGet("availability/busy")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetUserBusyTimes(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var busyTimes = await _calendarService.GetUserBusyTimesAsync(GetUserId(), startDate, endDate);
        return Ok(busyTimes);
    }

    /// <summary>
    /// Check for scheduling conflicts
    /// </summary>
    [HttpGet("availability/check-conflict")]
    public async Task<ActionResult<bool>> CheckConflict(
        [FromQuery] int? eventId,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        var hasConflict = await _calendarService.CheckConflictAsync(eventId, startTime, endTime, GetUserId());
        return Ok(new { hasConflict });
    }

    #endregion

    #region Calendar Views

    /// <summary>
    /// Get day view
    /// </summary>
    [HttpGet("views/day")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetDayView(
        [FromQuery] DateTime date,
        [FromQuery] int? calendarId = null)
    {
        var events = await _calendarService.GetDayViewAsync(GetUserId(), date, calendarId);
        return Ok(events);
    }

    /// <summary>
    /// Get week view
    /// </summary>
    [HttpGet("views/week")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetWeekView(
        [FromQuery] DateTime weekStart,
        [FromQuery] int? calendarId = null)
    {
        var events = await _calendarService.GetWeekViewAsync(GetUserId(), weekStart, calendarId);
        return Ok(events);
    }

    /// <summary>
    /// Get month view
    /// </summary>
    [HttpGet("views/month")]
    public async Task<ActionResult<List<CalendarEventDto>>> GetMonthView(
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int? calendarId = null)
    {
        var events = await _calendarService.GetMonthViewAsync(GetUserId(), year, month, calendarId);
        return Ok(events);
    }

    #endregion

    #region Import/Export

    /// <summary>
    /// Export event to ICS
    /// </summary>
    [HttpGet("events/{id}/export/ics")]
    public async Task<IActionResult> ExportEventToIcs(int id)
    {
        var icsContent = await _calendarService.ExportEventToIcsAsync(id, GetUserId());
        if (string.IsNullOrEmpty(icsContent))
            return NotFound();

        var bytes = System.Text.Encoding.UTF8.GetBytes(icsContent);
        return File(bytes, "text/calendar", $"event-{id}.ics");
    }

    /// <summary>
    /// Export calendar to ICS
    /// </summary>
    [HttpGet("calendars/{id}/export/ics")]
    public async Task<IActionResult> ExportCalendarToIcs(
        int id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var icsContent = await _calendarService.ExportCalendarToIcsAsync(id, startDate, endDate, GetUserId());
        if (string.IsNullOrEmpty(icsContent))
            return NotFound();

        var bytes = System.Text.Encoding.UTF8.GetBytes(icsContent);
        return File(bytes, "text/calendar", $"calendar-{id}.ics");
    }

    /// <summary>
    /// Import event from ICS
    /// </summary>
    [HttpPost("calendars/{id}/import/ics")]
    public async Task<ActionResult<CalendarEventDto>> ImportEventFromIcs(int id, IFormFile file)
    {
        using var reader = new StreamReader(file.OpenReadStream());
        var icsContent = await reader.ReadToEndAsync();

        var evt = await _calendarService.ImportEventFromIcsAsync(icsContent, id, GetUserId());
        return Ok(evt);
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get calendar statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<CalendarStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var stats = await _calendarService.GetCalendarStatisticsAsync(GetUserId(), startDate, endDate);
        return Ok(stats);
    }

    #endregion
}
