export interface CalendarEvent {
  id: number;
  eventId: string;
  calendarId: number;
  calendarName: string;
  title: string;
  body: string;
  location: string;

  // Dates
  startDateTime: Date;
  endDateTime: Date;
  isAllDay: boolean;
  timeZone: string;

  // Recurrence
  isRecurring: boolean;
  recurrenceRule?: string;
  recurrenceEnd?: Date;
  recurrenceExceptions: Date[];

  // Meeting
  isMeeting: boolean;
  isOnlineMeeting: boolean;
  onlineMeetingUrl?: string;
  onlineMeetingProvider?: string;
  attendees: EventAttendee[];

  // Organizer
  organizerId: string;
  organizerEmail: string;
  organizerName: string;

  // Status
  status: EventStatus;
  busyStatus: EventBusyStatus;
  importance: EventImportance;
  sensitivity: EventSensitivity;

  // Response
  responseStatus: ResponseStatus;
  responseTime?: Date;

  // Categories & Reminders
  categories: string[];
  reminders: EventReminder[];

  // Resources
  resources: EventResource[];

  // Attachments
  attachments: EventAttachment[];

  // Travel
  travelTimeMinutes?: number;

  // Metadata
  createdAt: Date;
  lastModified?: Date;
  isCancelled: boolean;
}

export interface CreateEventDto {
  calendarId: number;
  title: string;
  body?: string;
  location?: string;

  startDateTime: Date;
  endDateTime: Date;
  isAllDay: boolean;
  timeZone: string;

  // Recurrence
  isRecurring: boolean;
  recurrenceRule?: string;
  recurrenceEnd?: Date;

  // Meeting
  isMeeting: boolean;
  isOnlineMeeting: boolean;
  onlineMeetingProvider?: string;
  attendees?: CreateAttendeeDto[];

  // Status
  busyStatus: EventBusyStatus;
  importance: EventImportance;
  sensitivity: EventSensitivity;

  // Categories & Reminders
  categories?: string[];
  reminders?: CreateReminderDto[];

  // Resources
  resourceIds?: number[];

  // Travel
  travelTimeMinutes?: number;
}

export interface UpdateEventDto {
  title?: string;
  body?: string;
  location?: string;

  startDateTime?: Date;
  endDateTime?: Date;
  isAllDay?: boolean;
  timeZone?: string;

  // Recurrence
  isRecurring?: boolean;
  recurrenceRule?: string;
  recurrenceEnd?: Date;

  // Meeting
  isOnlineMeeting?: boolean;
  onlineMeetingProvider?: string;

  // Status
  busyStatus?: EventBusyStatus;
  importance?: EventImportance;
  sensitivity?: EventSensitivity;

  // Travel
  travelTimeMinutes?: number;
}

export interface EventAttendee {
  id: number;
  email: string;
  displayName: string;
  type: AttendeeType;
  responseStatus: ResponseStatus;
  responseTime?: Date;
  responseComment?: string;
  proposedStartTime?: Date;
  proposedEndTime?: Date;
}

export interface CreateAttendeeDto {
  email: string;
  displayName?: string;
  type: AttendeeType;
}

export interface UpdateAttendeeResponseDto {
  responseStatus: ResponseStatus;
  responseComment?: string;
  proposedStartTime?: Date;
  proposedEndTime?: Date;
}

export interface EventReminder {
  id: number;
  minutesBeforeStart: number;
  method: ReminderMethod;
  isTriggered: boolean;
  triggeredAt?: Date;
}

export interface CreateReminderDto {
  minutesBeforeStart: number;
  method: ReminderMethod;
}

export interface EventResource {
  id: number;
  resourceId: number;
  resourceName: string;
  resourceType: ResourceType;
  status: ResourceStatus;
}

export interface EventAttachment {
  id: number;
  fileName: string;
  contentType: string;
  fileSize: number;
  uploadedAt: Date;
}

export interface Resource {
  id: number;
  name: string;
  description?: string;
  type: ResourceType;

  // Location Details
  building?: string;
  floor?: string;
  roomNumber?: string;
  capacity?: number;

  // Equipment
  equipment: string[];

  isAvailable: boolean;
  email?: string;
}

export interface CreateResourceDto {
  name: string;
  description?: string;
  type: ResourceType;
  building?: string;
  floor?: string;
  roomNumber?: string;
  capacity?: number;
  equipment?: string[];
  email?: string;
}

export interface CalendarQueryParameters {
  startDate: Date;
  endDate: Date;
  calendarId?: number;
  categories?: string[];
  status?: EventStatus;
  includeCancelled: boolean;
}

export interface AvailabilityQueryDto {
  attendeeEmails: string[];
  resourceIds?: number[];
  startDate: Date;
  endDate: Date;
  meetingDurationMinutes: number;
}

export interface AvailabilitySlot {
  startTime: Date;
  endTime: Date;
  availableAttendees: string[];
  unavailableAttendees: string[];
  availableResources: number[];
}

export interface CalendarStatistics {
  totalEvents: number;
  totalMeetings: number;
  totalRecurringEvents: number;
  upcomingEvents: number;
  pendingResponses: number;
  acceptedMeetings: number;
  declinedMeetings: number;
  tentativeMeetings: number;
  averageEventsPerDay: number;
  eventsByCategory: { [key: string]: number };
  eventsByCalendar: { [key: string]: number };
}

// Enums
export enum EventStatus {
  Free = 0,
  Tentative = 1,
  Confirmed = 2,
  Cancelled = 3
}

export enum EventBusyStatus {
  Free = 0,
  Tentative = 1,
  Busy = 2,
  OutOfOffice = 3,
  WorkingElsewhere = 4
}

export enum EventImportance {
  Low = 0,
  Normal = 1,
  High = 2
}

export enum EventSensitivity {
  Normal = 0,
  Personal = 1,
  Private = 2,
  Confidential = 3
}

export enum AttendeeType {
  Required = 0,
  Optional = 1,
  Resource = 2,
  Organizer = 3
}

export enum ResponseStatus {
  None = 0,
  Accepted = 1,
  Declined = 2,
  Tentative = 3,
  NotResponded = 4
}

export enum ReminderMethod {
  Notification = 0,
  Email = 1,
  SMS = 2,
  Popup = 3
}

export enum ResourceType {
  Room = 0,
  Equipment = 1,
  Vehicle = 2,
  Other = 3
}

export enum ResourceStatus {
  Tentative = 0,
  Accepted = 1,
  Declined = 2
}

// View types
export enum CalendarView {
  Day = 'day',
  Week = 'week',
  Month = 'month',
  Agenda = 'agenda'
}
