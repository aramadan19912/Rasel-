import { Component, OnInit, OnDestroy, ViewChild } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CalendarOptions, EventInput, EventClickArg, DateSelectArg } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import listPlugin from '@fullcalendar/list';
import { CalendarService } from '../../services/calendar.service';
import {
  Calendar,
  CalendarEvent,
  CalendarView,
  EventStatus,
  EventBusyStatus,
  ResponseStatus,
  CreateEventDto
} from '../../models/calendar-event.model';
import { EventDialogComponent } from './event-dialog/event-dialog.component';

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.scss']
})
export class CalendarComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Calendars
  calendars: Calendar[] = [];
  selectedCalendars: number[] = [];

  // Events
  events: CalendarEvent[] = [];
  calendarEvents: EventInput[] = [];

  // View state
  currentView: CalendarView = CalendarView.Month;
  selectedDate: Date = new Date();

  // FullCalendar configuration
  calendarOptions: CalendarOptions = {
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin, listPlugin],
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
    },
    initialView: 'dayGridMonth',
    weekends: true,
    editable: true,
    selectable: true,
    selectMirror: true,
    dayMaxEvents: true,
    select: this.handleDateSelect.bind(this),
    eventClick: this.handleEventClick.bind(this),
    eventDrop: this.handleEventDrop.bind(this),
    eventResize: this.handleEventResize.bind(this),
    eventsSet: this.handleEventsSet.bind(this),
    datesSet: this.handleDatesSet.bind(this),
    events: [],
    eventTimeFormat: {
      hour: '2-digit',
      minute: '2-digit',
      meridiem: 'short'
    },
    slotMinTime: '06:00:00',
    slotMaxTime: '22:00:00',
    allDaySlot: true,
    nowIndicator: true,
    businessHours: {
      daysOfWeek: [1, 2, 3, 4, 5], // Monday - Friday
      startTime: '09:00',
      endTime: '17:00'
    }
  };

  // Mini calendar
  miniCalendarDate: Date = new Date();

  // Statistics
  statistics: any = null;

  // UI state
  isLoading = false;
  showCalendarList = true;
  showMiniCalendar = true;

  constructor(
    private calendarService: CalendarService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadCalendars();
    this.loadEvents();
    this.loadStatistics();

    // Subscribe to calendar changes
    this.calendarService.calendars$
      .pipe(takeUntil(this.destroy$))
      .subscribe(calendars => {
        this.calendars = calendars;
        if (this.selectedCalendars.length === 0 && calendars.length > 0) {
          this.selectedCalendars = calendars.map(c => c.id);
        }
        this.refreshEvents();
      });

    // Subscribe to event changes
    this.calendarService.events$
      .pipe(takeUntil(this.destroy$))
      .subscribe(events => {
        this.events = events;
        this.updateCalendarEvents();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ========== Data Loading ==========

  loadCalendars(): void {
    this.isLoading = true;
    this.calendarService.getUserCalendars().subscribe({
      next: (calendars) => {
        this.calendars = calendars;
        this.selectedCalendars = calendars.filter(c => c.isVisible).map(c => c.id);
        this.isLoading = false;
      },
      error: (error) => {
        this.showError('Failed to load calendars');
        this.isLoading = false;
      }
    });
  }

  loadEvents(): void {
    const start = new Date();
    start.setMonth(start.getMonth() - 1);
    const end = new Date();
    end.setMonth(end.getMonth() + 2);

    this.calendarService.getEvents({
      startDate: start,
      endDate: end,
      includeCancelled: false
    }).subscribe({
      next: (events) => {
        this.events = events;
        this.updateCalendarEvents();
      },
      error: (error) => {
        this.showError('Failed to load events');
      }
    });
  }

  loadStatistics(): void {
    this.calendarService.getStatistics().subscribe({
      next: (stats) => {
        this.statistics = stats;
      },
      error: (error) => {
        console.error('Failed to load statistics', error);
      }
    });
  }

  refreshEvents(): void {
    this.loadEvents();
  }

  // ========== Event Handlers ==========

  handleDateSelect(selectInfo: DateSelectArg): void {
    const calendarApi = selectInfo.view.calendar;
    calendarApi.unselect();

    this.openEventDialog(null, selectInfo.start, selectInfo.end, selectInfo.allDay);
  }

  handleEventClick(clickInfo: EventClickArg): void {
    const eventId = parseInt(clickInfo.event.id);
    const event = this.events.find(e => e.id === eventId);

    if (event) {
      this.openEventDialog(event);
    }
  }

  handleEventDrop(info: any): void {
    const eventId = parseInt(info.event.id);
    const event = this.events.find(e => e.id === eventId);

    if (event) {
      const duration = event.endDateTime.getTime() - event.startDateTime.getTime();
      const newStart = info.event.start;
      const newEnd = new Date(newStart.getTime() + duration);

      this.calendarService.updateEvent(eventId, {
        startDateTime: newStart,
        endDateTime: newEnd,
        isAllDay: info.event.allDay
      }).subscribe({
        next: () => {
          this.showSuccess('Event updated successfully');
        },
        error: () => {
          this.showError('Failed to update event');
          info.revert();
        }
      });
    }
  }

  handleEventResize(info: any): void {
    const eventId = parseInt(info.event.id);

    this.calendarService.updateEvent(eventId, {
      startDateTime: info.event.start,
      endDateTime: info.event.end
    }).subscribe({
      next: () => {
        this.showSuccess('Event updated successfully');
      },
      error: () => {
        this.showError('Failed to update event');
        info.revert();
      }
    });
  }

  handleEventsSet(events: any): void {
    // Called after events are rendered
  }

  handleDatesSet(dateInfo: any): void {
    // Called when the view date range changes
    this.selectedDate = dateInfo.start;
  }

  // ========== Event Dialog ==========

  openEventDialog(event: CalendarEvent | null, startDate?: Date, endDate?: Date, allDay?: boolean): void {
    const dialogRef = this.dialog.open(EventDialogComponent, {
      width: '800px',
      maxHeight: '90vh',
      data: {
        event: event,
        startDate: startDate || new Date(),
        endDate: endDate || new Date(new Date().getTime() + 3600000),
        allDay: allDay || false,
        calendars: this.calendars
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (result.action === 'create') {
          this.createEvent(result.event);
        } else if (result.action === 'update') {
          this.updateEvent(result.event);
        } else if (result.action === 'delete') {
          this.deleteEvent(result.eventId);
        }
      }
    });
  }

  // ========== CRUD Operations ==========

  createEvent(dto: CreateEventDto): void {
    this.calendarService.createEvent(dto).subscribe({
      next: () => {
        this.showSuccess('Event created successfully');
        this.refreshEvents();
      },
      error: () => {
        this.showError('Failed to create event');
      }
    });
  }

  updateEvent(event: { id: number, data: any }): void {
    this.calendarService.updateEvent(event.id, event.data).subscribe({
      next: () => {
        this.showSuccess('Event updated successfully');
        this.refreshEvents();
      },
      error: () => {
        this.showError('Failed to update event');
      }
    });
  }

  deleteEvent(eventId: number): void {
    if (confirm('Are you sure you want to delete this event?')) {
      this.calendarService.deleteEvent(eventId).subscribe({
        next: () => {
          this.showSuccess('Event deleted successfully');
          this.refreshEvents();
        },
        error: () => {
          this.showError('Failed to delete event');
        }
      });
    }
  }

  // ========== Calendar Management ==========

  toggleCalendar(calendarId: number): void {
    const index = this.selectedCalendars.indexOf(calendarId);
    if (index > -1) {
      this.selectedCalendars.splice(index, 1);
    } else {
      this.selectedCalendars.push(calendarId);
    }
    this.updateCalendarEvents();
  }

  isCalendarSelected(calendarId: number): boolean {
    return this.selectedCalendars.includes(calendarId);
  }

  createCalendar(): void {
    // TODO: Implement create calendar dialog
    this.showInfo('Create calendar dialog not yet implemented');
  }

  // ========== View Management ==========

  changeView(view: CalendarView): void {
    this.currentView = view;
    this.calendarService.setViewType(view);
  }

  goToToday(): void {
    this.selectedDate = new Date();
    this.calendarService.setSelectedDate(this.selectedDate);
  }

  previousPeriod(): void {
    // Handled by FullCalendar
  }

  nextPeriod(): void {
    // Handled by FullCalendar
  }

  // ========== Helper Methods ==========

  updateCalendarEvents(): void {
    const filteredEvents = this.events.filter(e =>
      this.selectedCalendars.includes(e.calendarId) && !e.isCancelled
    );

    this.calendarEvents = filteredEvents.map(event => ({
      id: event.id.toString(),
      title: event.title,
      start: new Date(event.startDateTime),
      end: new Date(event.endDateTime),
      allDay: event.isAllDay,
      backgroundColor: this.getEventColor(event),
      borderColor: this.getEventColor(event),
      textColor: '#ffffff',
      extendedProps: {
        event: event
      }
    }));

    this.calendarOptions.events = this.calendarEvents;
  }

  getEventColor(event: CalendarEvent): string {
    // Get color from calendar
    const calendar = this.calendars.find(c => c.id === event.calendarId);
    if (calendar) {
      return calendar.color;
    }

    // Fallback color based on event status
    switch (event.busyStatus) {
      case EventBusyStatus.Free:
        return '#4CAF50'; // Green
      case EventBusyStatus.Tentative:
        return '#FF9800'; // Orange
      case EventBusyStatus.Busy:
        return '#2196F3'; // Blue
      case EventBusyStatus.OutOfOffice:
        return '#9C27B0'; // Purple
      case EventBusyStatus.WorkingElsewhere:
        return '#00BCD4'; // Cyan
      default:
        return '#2196F3'; // Blue
    }
  }

  getEventStatusIcon(event: CalendarEvent): string {
    if (event.isCancelled) return 'cancel';
    if (event.isRecurring) return 'repeat';
    if (event.isMeeting) return 'group';
    if (event.isOnlineMeeting) return 'videocam';
    return 'event';
  }

  getResponseStatusText(status: ResponseStatus): string {
    switch (status) {
      case ResponseStatus.Accepted: return 'Accepted';
      case ResponseStatus.Declined: return 'Declined';
      case ResponseStatus.Tentative: return 'Tentative';
      case ResponseStatus.NotResponded: return 'Not Responded';
      default: return 'None';
    }
  }

  getResponseStatusColor(status: ResponseStatus): string {
    switch (status) {
      case ResponseStatus.Accepted: return 'primary';
      case ResponseStatus.Declined: return 'warn';
      case ResponseStatus.Tentative: return 'accent';
      default: return '';
    }
  }

  // ========== UI Helpers ==========

  toggleCalendarList(): void {
    this.showCalendarList = !this.showCalendarList;
  }

  toggleMiniCalendar(): void {
    this.showMiniCalendar = !this.showMiniCalendar;
  }

  showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['success-snackbar']
    });
  }

  showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['error-snackbar']
    });
  }

  showInfo(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['info-snackbar']
    });
  }

  // ========== Export ==========

  exportCalendar(calendarId: number): void {
    this.calendarService.exportCalendarToIcs(calendarId).subscribe({
      next: (blob) => {
        const calendar = this.calendars.find(c => c.id === calendarId);
        const filename = `${calendar?.name || 'calendar'}.ics`;
        this.calendarService.downloadFile(blob, filename);
        this.showSuccess('Calendar exported successfully');
      },
      error: () => {
        this.showError('Failed to export calendar');
      }
    });
  }

  importCalendar(calendarId: number, event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.calendarService.importEventFromIcs(calendarId, file).subscribe({
        next: () => {
          this.showSuccess('Event imported successfully');
          this.refreshEvents();
        },
        error: () => {
          this.showError('Failed to import event');
        }
      });
    }
  }
}
