import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  Calendar,
  CalendarShare,
  CreateCalendarDto,
  UpdateCalendarDto,
  ShareCalendarDto,
  CalendarEvent,
  CreateEventDto,
  UpdateEventDto,
  EventAttendee,
  CreateAttendeeDto,
  UpdateAttendeeResponseDto,
  EventReminder,
  CreateReminderDto,
  EventAttachment,
  Resource,
  CreateResourceDto,
  CalendarQueryParameters,
  AvailabilityQueryDto,
  AvailabilitySlot,
  CalendarStatistics,
  CalendarView
} from '../models/calendar-event.model';

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  private apiUrl = `${environment.apiUrl}/calendar`;

  // State management
  private calendarsSubject = new BehaviorSubject<Calendar[]>([]);
  public calendars$ = this.calendarsSubject.asObservable();

  private eventsSubject = new BehaviorSubject<CalendarEvent[]>([]);
  public events$ = this.eventsSubject.asObservable();

  private selectedDateSubject = new BehaviorSubject<Date>(new Date());
  public selectedDate$ = this.selectedDateSubject.asObservable();

  private viewTypeSubject = new BehaviorSubject<CalendarView>(CalendarView.Month);
  public viewType$ = this.viewTypeSubject.asObservable();

  constructor(private http: HttpClient) {}

  // State setters
  setSelectedDate(date: Date): void {
    this.selectedDateSubject.next(date);
  }

  setViewType(view: CalendarView): void {
    this.viewTypeSubject.next(view);
  }

  // ========== Calendar Management ==========

  /**
   * Get all user calendars
   */
  getUserCalendars(): Observable<Calendar[]> {
    return this.http.get<Calendar[]>(`${this.apiUrl}/calendars`).pipe(
      tap(calendars => this.calendarsSubject.next(calendars))
    );
  }

  /**
   * Get a specific calendar
   */
  getCalendar(id: number): Observable<Calendar> {
    return this.http.get<Calendar>(`${this.apiUrl}/calendars/${id}`);
  }

  /**
   * Get shared calendars
   */
  getSharedCalendars(): Observable<Calendar[]> {
    return this.http.get<Calendar[]>(`${this.apiUrl}/calendars/shared`);
  }

  /**
   * Create a new calendar
   */
  createCalendar(dto: CreateCalendarDto): Observable<Calendar> {
    return this.http.post<Calendar>(`${this.apiUrl}/calendars`, dto).pipe(
      tap(() => this.getUserCalendars().subscribe())
    );
  }

  /**
   * Update a calendar
   */
  updateCalendar(id: number, dto: UpdateCalendarDto): Observable<Calendar> {
    return this.http.put<Calendar>(`${this.apiUrl}/calendars/${id}`, dto).pipe(
      tap(() => this.getUserCalendars().subscribe())
    );
  }

  /**
   * Delete a calendar
   */
  deleteCalendar(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/calendars/${id}`).pipe(
      tap(() => this.getUserCalendars().subscribe())
    );
  }

  /**
   * Set calendar as default
   */
  setDefaultCalendar(id: number): Observable<Calendar> {
    return this.http.post<Calendar>(`${this.apiUrl}/calendars/${id}/set-default`, {}).pipe(
      tap(() => this.getUserCalendars().subscribe())
    );
  }

  // ========== Calendar Sharing ==========

  /**
   * Share a calendar
   */
  shareCalendar(calendarId: number, dto: ShareCalendarDto): Observable<CalendarShare> {
    return this.http.post<CalendarShare>(`${this.apiUrl}/calendars/${calendarId}/share`, dto);
  }

  /**
   * Revoke calendar share
   */
  revokeCalendarShare(calendarId: number, shareId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/calendars/${calendarId}/share/${shareId}`);
  }

  // ========== Event Management ==========

  /**
   * Get a specific event
   */
  getEvent(id: number): Observable<CalendarEvent> {
    return this.http.get<CalendarEvent>(`${this.apiUrl}/events/${id}`);
  }

  /**
   * Get event by EventId (GUID)
   */
  getEventByEventId(eventId: string): Observable<CalendarEvent> {
    return this.http.get<CalendarEvent>(`${this.apiUrl}/events/by-event-id/${eventId}`);
  }

  /**
   * Get events with query parameters
   */
  getEvents(parameters: CalendarQueryParameters): Observable<CalendarEvent[]> {
    let params = new HttpParams()
      .set('startDate', parameters.startDate.toISOString())
      .set('endDate', parameters.endDate.toISOString())
      .set('includeCancelled', parameters.includeCancelled.toString());

    if (parameters.calendarId) {
      params = params.set('calendarId', parameters.calendarId.toString());
    }

    if (parameters.categories && parameters.categories.length > 0) {
      parameters.categories.forEach(cat => {
        params = params.append('categories', cat);
      });
    }

    if (parameters.status !== undefined) {
      params = params.set('status', parameters.status.toString());
    }

    return this.http.get<CalendarEvent[]>(`${this.apiUrl}/events`, { params }).pipe(
      tap(events => this.eventsSubject.next(events))
    );
  }

  /**
   * Create a new event
   */
  createEvent(dto: CreateEventDto): Observable<CalendarEvent> {
    return this.http.post<CalendarEvent>(`${this.apiUrl}/events`, dto).pipe(
      tap(() => this.refreshCurrentEvents())
    );
  }

  /**
   * Update an event
   */
  updateEvent(id: number, dto: UpdateEventDto): Observable<CalendarEvent> {
    return this.http.put<CalendarEvent>(`${this.apiUrl}/events/${id}`, dto).pipe(
      tap(() => this.refreshCurrentEvents())
    );
  }

  /**
   * Delete an event
   */
  deleteEvent(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/events/${id}`).pipe(
      tap(() => this.refreshCurrentEvents())
    );
  }

  /**
   * Cancel an event
   */
  cancelEvent(id: number, cancellationMessage?: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/events/${id}/cancel`, { cancellationMessage }).pipe(
      tap(() => this.refreshCurrentEvents())
    );
  }

  // ========== Recurring Events ==========

  /**
   * Get recurring event instances
   */
  getRecurringEventInstances(id: number, startDate: Date, endDate: Date): Observable<CalendarEvent[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<CalendarEvent[]>(`${this.apiUrl}/events/${id}/instances`, { params });
  }

  /**
   * Update a recurring event instance
   */
  updateRecurringEventInstance(id: number, instanceDate: Date, dto: UpdateEventDto): Observable<CalendarEvent> {
    return this.http.put<CalendarEvent>(
      `${this.apiUrl}/events/${id}/instances/${instanceDate.toISOString()}`,
      dto
    ).pipe(
      tap(() => this.refreshCurrentEvents())
    );
  }

  /**
   * Delete a recurring event instance
   */
  deleteRecurringEventInstance(id: number, instanceDate: Date): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/events/${id}/instances/${instanceDate.toISOString()}`
    ).pipe(
      tap(() => this.refreshCurrentEvents())
    );
  }

  // ========== Attendee Management ==========

  /**
   * Get event attendees
   */
  getEventAttendees(eventId: number): Observable<EventAttendee[]> {
    return this.http.get<EventAttendee[]>(`${this.apiUrl}/events/${eventId}/attendees`);
  }

  /**
   * Add an attendee
   */
  addAttendee(eventId: number, dto: CreateAttendeeDto): Observable<EventAttendee> {
    return this.http.post<EventAttendee>(`${this.apiUrl}/events/${eventId}/attendees`, dto);
  }

  /**
   * Remove an attendee
   */
  removeAttendee(eventId: number, attendeeId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/events/${eventId}/attendees/${attendeeId}`);
  }

  /**
   * Update attendee response
   */
  updateAttendeeResponse(eventId: number, dto: UpdateAttendeeResponseDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/events/${eventId}/respond`, dto);
  }

  // ========== Resource Management ==========

  /**
   * Get all resources
   */
  getResources(type?: number): Observable<Resource[]> {
    let params = new HttpParams();
    if (type !== undefined) {
      params = params.set('type', type.toString());
    }
    return this.http.get<Resource[]>(`${this.apiUrl}/resources`, { params });
  }

  /**
   * Get a specific resource
   */
  getResource(id: number): Observable<Resource> {
    return this.http.get<Resource>(`${this.apiUrl}/resources/${id}`);
  }

  /**
   * Create a new resource
   */
  createResource(dto: CreateResourceDto): Observable<Resource> {
    return this.http.post<Resource>(`${this.apiUrl}/resources`, dto);
  }

  /**
   * Delete a resource
   */
  deleteResource(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/resources/${id}`);
  }

  /**
   * Book a resource
   */
  bookResource(eventId: number, resourceId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/events/${eventId}/resources/${resourceId}`, {});
  }

  /**
   * Release a resource
   */
  releaseResource(eventId: number, resourceId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/events/${eventId}/resources/${resourceId}`);
  }

  /**
   * Get available resources
   */
  getAvailableResources(startTime: Date, endTime: Date, type?: number): Observable<Resource[]> {
    let params = new HttpParams()
      .set('startTime', startTime.toISOString())
      .set('endTime', endTime.toISOString());

    if (type !== undefined) {
      params = params.set('type', type.toString());
    }

    return this.http.get<Resource[]>(`${this.apiUrl}/resources/available`, { params });
  }

  // ========== Reminders ==========

  /**
   * Add a reminder
   */
  addReminder(eventId: number, dto: CreateReminderDto): Observable<EventReminder> {
    return this.http.post<EventReminder>(`${this.apiUrl}/events/${eventId}/reminders`, dto);
  }

  /**
   * Remove a reminder
   */
  removeReminder(eventId: number, reminderId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/events/${eventId}/reminders/${reminderId}`);
  }

  /**
   * Get due reminders
   */
  getDueReminders(): Observable<EventReminder[]> {
    return this.http.get<EventReminder[]>(`${this.apiUrl}/reminders/due`);
  }

  /**
   * Mark reminder as triggered
   */
  markReminderTriggered(reminderId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reminders/${reminderId}/mark-triggered`, {});
  }

  // ========== Attachments ==========

  /**
   * Add an attachment
   */
  addAttachment(eventId: number, file: File): Observable<EventAttachment> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<EventAttachment>(`${this.apiUrl}/events/${eventId}/attachments`, formData);
  }

  /**
   * Remove an attachment
   */
  removeAttachment(eventId: number, attachmentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/events/${eventId}/attachments/${attachmentId}`);
  }

  /**
   * Download an attachment
   */
  downloadAttachment(attachmentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/attachments/${attachmentId}`, {
      responseType: 'blob'
    });
  }

  // ========== Availability & Scheduling ==========

  /**
   * Find available time slots
   */
  findAvailability(query: AvailabilityQueryDto): Observable<AvailabilitySlot[]> {
    return this.http.post<AvailabilitySlot[]>(`${this.apiUrl}/availability/find`, query);
  }

  /**
   * Get user busy times
   */
  getUserBusyTimes(startDate: Date, endDate: Date): Observable<CalendarEvent[]> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());

    return this.http.get<CalendarEvent[]>(`${this.apiUrl}/availability/busy`, { params });
  }

  /**
   * Check for conflicts
   */
  checkConflict(startTime: Date, endTime: Date, eventId?: number): Observable<{ hasConflict: boolean }> {
    let params = new HttpParams()
      .set('startTime', startTime.toISOString())
      .set('endTime', endTime.toISOString());

    if (eventId) {
      params = params.set('eventId', eventId.toString());
    }

    return this.http.get<{ hasConflict: boolean }>(`${this.apiUrl}/availability/check-conflict`, { params });
  }

  // ========== Calendar Views ==========

  /**
   * Get day view
   */
  getDayView(date: Date, calendarId?: number): Observable<CalendarEvent[]> {
    let params = new HttpParams().set('date', date.toISOString());
    if (calendarId) {
      params = params.set('calendarId', calendarId.toString());
    }

    return this.http.get<CalendarEvent[]>(`${this.apiUrl}/views/day`, { params }).pipe(
      tap(events => this.eventsSubject.next(events))
    );
  }

  /**
   * Get week view
   */
  getWeekView(weekStart: Date, calendarId?: number): Observable<CalendarEvent[]> {
    let params = new HttpParams().set('weekStart', weekStart.toISOString());
    if (calendarId) {
      params = params.set('calendarId', calendarId.toString());
    }

    return this.http.get<CalendarEvent[]>(`${this.apiUrl}/views/week`, { params }).pipe(
      tap(events => this.eventsSubject.next(events))
    );
  }

  /**
   * Get month view
   */
  getMonthView(year: number, month: number, calendarId?: number): Observable<CalendarEvent[]> {
    let params = new HttpParams()
      .set('year', year.toString())
      .set('month', month.toString());

    if (calendarId) {
      params = params.set('calendarId', calendarId.toString());
    }

    return this.http.get<CalendarEvent[]>(`${this.apiUrl}/views/month`, { params }).pipe(
      tap(events => this.eventsSubject.next(events))
    );
  }

  // ========== Import/Export ==========

  /**
   * Export event to ICS
   */
  exportEventToIcs(eventId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/events/${eventId}/export/ics`, {
      responseType: 'blob'
    });
  }

  /**
   * Export calendar to ICS
   */
  exportCalendarToIcs(calendarId: number, startDate?: Date, endDate?: Date): Observable<Blob> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get(`${this.apiUrl}/calendars/${calendarId}/export/ics`, {
      params,
      responseType: 'blob'
    });
  }

  /**
   * Import event from ICS
   */
  importEventFromIcs(calendarId: number, file: File): Observable<CalendarEvent> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<CalendarEvent>(`${this.apiUrl}/calendars/${calendarId}/import/ics`, formData);
  }

  // ========== Statistics ==========

  /**
   * Get calendar statistics
   */
  getStatistics(startDate?: Date, endDate?: Date): Observable<CalendarStatistics> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<CalendarStatistics>(`${this.apiUrl}/statistics`, { params });
  }

  // ========== Helper Methods ==========

  /**
   * Refresh current events based on selected date and view
   */
  private refreshCurrentEvents(): void {
    const date = this.selectedDateSubject.value;
    const view = this.viewTypeSubject.value;

    switch (view) {
      case CalendarView.Day:
        this.getDayView(date).subscribe();
        break;
      case CalendarView.Week:
        this.getWeekView(this.getWeekStart(date)).subscribe();
        break;
      case CalendarView.Month:
        this.getMonthView(date.getFullYear(), date.getMonth() + 1).subscribe();
        break;
    }
  }

  /**
   * Get week start date (Sunday)
   */
  private getWeekStart(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    const diff = d.getDate() - day;
    return new Date(d.setDate(diff));
  }

  /**
   * Download blob as file
   */
  downloadFile(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    window.URL.revokeObjectURL(url);
  }
}
