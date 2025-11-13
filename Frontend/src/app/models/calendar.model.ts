export interface Calendar {
  id: number;
  name: string;
  description: string;
  color: string;
  userId: string;
  isDefault: boolean;
  isVisible: boolean;
  displayOrder: number;
  isShared: boolean;
  shares: CalendarShare[];
  createdAt: Date;
  lastModified?: Date;
}

export interface CalendarShare {
  id: number;
  calendarId: number;
  sharedWithUserId: string;
  sharedWithUserEmail: string;
  sharedWithUserName: string;
  permission: CalendarPermission;
  sharedAt: Date;
}

export interface CreateCalendarDto {
  name: string;
  description: string;
  color: string;
  isDefault: boolean;
}

export interface UpdateCalendarDto {
  name?: string;
  description?: string;
  color?: string;
  isVisible?: boolean;
  displayOrder?: number;
}

export interface ShareCalendarDto {
  sharedWithUserEmail: string;
  permission: CalendarPermission;
}

export enum CalendarPermission {
  ViewOnly = 0,
  ViewDetails = 1,
  Edit = 2,
  FullControl = 3
}
