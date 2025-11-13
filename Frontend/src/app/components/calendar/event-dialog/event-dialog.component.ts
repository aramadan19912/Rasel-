import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import {
  Calendar,
  CalendarEvent,
  CreateEventDto,
  UpdateEventDto,
  EventBusyStatus,
  EventImportance,
  EventSensitivity,
  AttendeeType,
  ReminderMethod,
  CreateAttendeeDto,
  CreateReminderDto
} from '../../../models/calendar-event.model';
import { CalendarService } from '../../../services/calendar.service';

export interface EventDialogData {
  event: CalendarEvent | null;
  startDate: Date;
  endDate: Date;
  allDay: boolean;
  calendars: Calendar[];
}

@Component({
  selector: 'app-event-dialog',
  templateUrl: './event-dialog.component.html',
  styleUrls: ['./event-dialog.component.scss']
})
export class EventDialogComponent implements OnInit {
  eventForm: FormGroup;
  isEditMode = false;
  calendars: Calendar[] = [];

  // Enums for dropdowns
  busyStatusOptions = [
    { value: EventBusyStatus.Free, label: 'Free' },
    { value: EventBusyStatus.Tentative, label: 'Tentative' },
    { value: EventBusyStatus.Busy, label: 'Busy' },
    { value: EventBusyStatus.OutOfOffice, label: 'Out of Office' },
    { value: EventBusyStatus.WorkingElsewhere, label: 'Working Elsewhere' }
  ];

  importanceOptions = [
    { value: EventImportance.Low, label: 'Low' },
    { value: EventImportance.Normal, label: 'Normal' },
    { value: EventImportance.High, label: 'High' }
  ];

  sensitivityOptions = [
    { value: EventSensitivity.Normal, label: 'Normal' },
    { value: EventSensitivity.Personal, label: 'Personal' },
    { value: EventSensitivity.Private, label: 'Private' },
    { value: EventSensitivity.Confidential, label: 'Confidential' }
  ];

  attendeeTypeOptions = [
    { value: AttendeeType.Required, label: 'Required' },
    { value: AttendeeType.Optional, label: 'Optional' },
    { value: AttendeeType.Resource, label: 'Resource' }
  ];

  reminderMethodOptions = [
    { value: ReminderMethod.Notification, label: 'Notification' },
    { value: ReminderMethod.Email, label: 'Email' },
    { value: ReminderMethod.SMS, label: 'SMS' },
    { value: ReminderMethod.Popup, label: 'Popup' }
  ];

  onlineMeetingProviders = [
    { value: 'teams', label: 'Microsoft Teams' },
    { value: 'zoom', label: 'Zoom' },
    { value: 'meet', label: 'Google Meet' }
  ];

  // Available resources
  availableResources: any[] = [];

  // Tabs
  selectedTabIndex = 0;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EventDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: EventDialogData,
    private calendarService: CalendarService
  ) {
    this.calendars = data.calendars;
    this.isEditMode = !!data.event;

    this.eventForm = this.fb.group({
      // Basic Information
      calendarId: [data.event?.calendarId || (this.calendars.length > 0 ? this.calendars[0].id : null), Validators.required],
      title: [data.event?.title || '', Validators.required],
      location: [data.event?.location || ''],
      body: [data.event?.body || ''],

      // Date & Time
      startDateTime: [data.event?.startDateTime || data.startDate, Validators.required],
      endDateTime: [data.event?.endDateTime || data.endDate, Validators.required],
      isAllDay: [data.event?.isAllDay || data.allDay],
      timeZone: [data.event?.timeZone || 'UTC'],

      // Recurrence
      isRecurring: [data.event?.isRecurring || false],
      recurrenceRule: [data.event?.recurrenceRule || ''],
      recurrenceEnd: [data.event?.recurrenceEnd || null],

      // Meeting
      isMeeting: [data.event?.isMeeting || false],
      isOnlineMeeting: [data.event?.isOnlineMeeting || false],
      onlineMeetingProvider: [data.event?.onlineMeetingProvider || 'teams'],
      attendees: this.fb.array([]),

      // Status
      busyStatus: [data.event?.busyStatus || EventBusyStatus.Busy],
      importance: [data.event?.importance || EventImportance.Normal],
      sensitivity: [data.event?.sensitivity || EventSensitivity.Normal],

      // Categories
      categories: [data.event?.categories || []],

      // Reminders
      reminders: this.fb.array([]),

      // Resources
      resourceIds: [data.event?.resources?.map(r => r.resourceId) || []],

      // Travel
      travelTimeMinutes: [data.event?.travelTimeMinutes || 0]
    });

    // Add existing attendees
    if (data.event?.attendees) {
      data.event.attendees.forEach(attendee => {
        this.addAttendee(attendee.email, attendee.displayName, attendee.type);
      });
    }

    // Add existing reminders
    if (data.event?.reminders) {
      data.event.reminders.forEach(reminder => {
        this.addReminder(reminder.minutesBeforeStart, reminder.method);
      });
    } else {
      // Add default reminder
      this.addReminder(15, ReminderMethod.Notification);
    }
  }

  ngOnInit(): void {
    this.loadAvailableResources();

    // Watch for date changes to load available resources
    this.eventForm.get('startDateTime')?.valueChanges.subscribe(() => {
      this.loadAvailableResources();
    });
    this.eventForm.get('endDateTime')?.valueChanges.subscribe(() => {
      this.loadAvailableResources();
    });
  }

  // ========== Form Getters ==========

  get attendees(): FormArray {
    return this.eventForm.get('attendees') as FormArray;
  }

  get reminders(): FormArray {
    return this.eventForm.get('reminders') as FormArray;
  }

  // ========== Attendee Management ==========

  addAttendee(email: string = '', displayName: string = '', type: AttendeeType = AttendeeType.Required): void {
    const attendeeGroup = this.fb.group({
      email: [email, [Validators.required, Validators.email]],
      displayName: [displayName],
      type: [type]
    });
    this.attendees.push(attendeeGroup);
  }

  removeAttendee(index: number): void {
    this.attendees.removeAt(index);
  }

  // ========== Reminder Management ==========

  addReminder(minutes: number = 15, method: ReminderMethod = ReminderMethod.Notification): void {
    const reminderGroup = this.fb.group({
      minutesBeforeStart: [minutes, [Validators.required, Validators.min(0)]],
      method: [method]
    });
    this.reminders.push(reminderGroup);
  }

  removeReminder(index: number): void {
    this.reminders.removeAt(index);
  }

  // ========== Resource Management ==========

  loadAvailableResources(): void {
    const startTime = this.eventForm.get('startDateTime')?.value;
    const endTime = this.eventForm.get('endDateTime')?.value;

    if (startTime && endTime) {
      this.calendarService.getAvailableResources(startTime, endTime).subscribe({
        next: (resources) => {
          this.availableResources = resources;
        },
        error: (error) => {
          console.error('Failed to load resources', error);
        }
      });
    }
  }

  // ========== Form Actions ==========

  onSave(): void {
    if (this.eventForm.valid) {
      const formValue = this.eventForm.value;

      if (this.isEditMode && this.data.event) {
        // Update existing event
        const updateDto: UpdateEventDto = {
          title: formValue.title,
          body: formValue.body,
          location: formValue.location,
          startDateTime: formValue.startDateTime,
          endDateTime: formValue.endDateTime,
          isAllDay: formValue.isAllDay,
          timeZone: formValue.timeZone,
          isRecurring: formValue.isRecurring,
          recurrenceRule: formValue.recurrenceRule,
          recurrenceEnd: formValue.recurrenceEnd,
          isOnlineMeeting: formValue.isOnlineMeeting,
          onlineMeetingProvider: formValue.onlineMeetingProvider,
          busyStatus: formValue.busyStatus,
          importance: formValue.importance,
          sensitivity: formValue.sensitivity,
          travelTimeMinutes: formValue.travelTimeMinutes
        };

        this.dialogRef.close({
          action: 'update',
          event: {
            id: this.data.event.id,
            data: updateDto
          }
        });
      } else {
        // Create new event
        const createDto: CreateEventDto = {
          calendarId: formValue.calendarId,
          title: formValue.title,
          body: formValue.body,
          location: formValue.location,
          startDateTime: formValue.startDateTime,
          endDateTime: formValue.endDateTime,
          isAllDay: formValue.isAllDay,
          timeZone: formValue.timeZone,
          isRecurring: formValue.isRecurring,
          recurrenceRule: formValue.recurrenceRule,
          recurrenceEnd: formValue.recurrenceEnd,
          isMeeting: formValue.isMeeting,
          isOnlineMeeting: formValue.isOnlineMeeting,
          onlineMeetingProvider: formValue.onlineMeetingProvider,
          attendees: formValue.attendees as CreateAttendeeDto[],
          busyStatus: formValue.busyStatus,
          importance: formValue.importance,
          sensitivity: formValue.sensitivity,
          categories: formValue.categories,
          reminders: formValue.reminders as CreateReminderDto[],
          resourceIds: formValue.resourceIds,
          travelTimeMinutes: formValue.travelTimeMinutes
        };

        this.dialogRef.close({
          action: 'create',
          event: createDto
        });
      }
    }
  }

  onDelete(): void {
    if (this.isEditMode && this.data.event) {
      this.dialogRef.close({
        action: 'delete',
        eventId: this.data.event.id
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  // ========== Helper Methods ==========

  isFieldInvalid(fieldName: string): boolean {
    const field = this.eventForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  getErrorMessage(fieldName: string): string {
    const field = this.eventForm.get(fieldName);
    if (field?.hasError('required')) {
      return 'This field is required';
    }
    if (field?.hasError('email')) {
      return 'Please enter a valid email';
    }
    return '';
  }

  // ========== Quick Actions ==========

  setQuickDuration(minutes: number): void {
    const startTime = this.eventForm.get('startDateTime')?.value;
    if (startTime) {
      const endTime = new Date(startTime.getTime() + minutes * 60000);
      this.eventForm.patchValue({ endDateTime: endTime });
    }
  }

  setQuickReminder(minutes: number): void {
    this.reminders.clear();
    this.addReminder(minutes, ReminderMethod.Notification);
  }

  checkConflicts(): void {
    const startTime = this.eventForm.get('startDateTime')?.value;
    const endTime = this.eventForm.get('endDateTime')?.value;
    const eventId = this.isEditMode ? this.data.event?.id : undefined;

    if (startTime && endTime) {
      this.calendarService.checkConflict(startTime, endTime, eventId).subscribe({
        next: (result) => {
          if (result.hasConflict) {
            alert('Warning: This event conflicts with another event!');
          } else {
            alert('No conflicts found!');
          }
        },
        error: (error) => {
          console.error('Failed to check conflicts', error);
        }
      });
    }
  }

  // ========== Category Management ==========

  addCategory(event: any): void {
    const value = (event.value || '').trim();
    if (value) {
      const categories = this.eventForm.get('categories')?.value || [];
      categories.push(value);
      this.eventForm.patchValue({ categories });
    }
    event.chipInput?.clear();
  }

  removeCategory(category: string): void {
    const categories = this.eventForm.get('categories')?.value || [];
    const index = categories.indexOf(category);
    if (index >= 0) {
      categories.splice(index, 1);
      this.eventForm.patchValue({ categories });
    }
  }
}
