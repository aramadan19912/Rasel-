export interface Contact {
  id: number;
  contactId: string;
  userId: string;

  // Basic Information
  title?: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  suffix?: string;
  nickname?: string;
  displayName: string;
  fileAs?: string;

  // Contact Details
  emailAddresses: ContactEmail[];
  phoneNumbers: ContactPhone[];
  addresses: ContactAddress[];
  websites: ContactWebsite[];

  // Professional Information
  jobTitle?: string;
  department?: string;
  company?: string;
  manager?: string;
  assistant?: string;
  officeLocation?: string;

  // Personal Information
  birthday?: Date;
  spouseName?: string;
  children?: string;
  gender?: string;

  // Additional Details
  notes?: string;
  categories: string[];
  tags: string[];

  // Social Media
  linkedInUrl?: string;
  twitterHandle?: string;
  facebookUrl?: string;
  instagramHandle?: string;

  // Photo
  photoUrl?: string;

  // Groups
  groups: ContactGroupSummary[];

  // Relationships
  relationships: ContactRelationship[];

  // Custom Fields
  customFields: ContactCustomField[];

  // Metadata
  isFavorite: boolean;
  isBlocked: boolean;
  createdAt: Date;
  lastModified?: Date;
  lastContacted?: Date;
  contactFrequency: number;

  // Source
  source: ContactSource;
  sourceId?: string;

  // Privacy
  privacy: ContactPrivacy;
}

export interface CreateContactDto {
  title?: string;
  firstName: string;
  middleName?: string;
  lastName: string;
  suffix?: string;
  nickname?: string;

  emailAddresses?: ContactEmail[];
  phoneNumbers?: ContactPhone[];
  addresses?: ContactAddress[];
  websites?: ContactWebsite[];

  jobTitle?: string;
  department?: string;
  company?: string;
  manager?: string;
  assistant?: string;
  officeLocation?: string;

  birthday?: Date;
  spouseName?: string;
  children?: string;
  gender?: string;

  notes?: string;
  categories?: string[];
  tags?: string[];

  linkedInUrl?: string;
  twitterHandle?: string;
  facebookUrl?: string;
  instagramHandle?: string;

  customFields?: ContactCustomField[];

  privacy: ContactPrivacy;
}

export interface UpdateContactDto {
  title?: string;
  firstName?: string;
  middleName?: string;
  lastName?: string;
  suffix?: string;
  nickname?: string;

  emailAddresses?: ContactEmail[];
  phoneNumbers?: ContactPhone[];
  addresses?: ContactAddress[];
  websites?: ContactWebsite[];

  jobTitle?: string;
  department?: string;
  company?: string;
  manager?: string;
  assistant?: string;
  officeLocation?: string;

  birthday?: Date;
  spouseName?: string;
  children?: string;
  gender?: string;

  notes?: string;
  categories?: string[];
  tags?: string[];

  linkedInUrl?: string;
  twitterHandle?: string;
  facebookUrl?: string;
  instagramHandle?: string;

  customFields?: ContactCustomField[];

  isFavorite?: boolean;
  isBlocked?: boolean;
  privacy?: ContactPrivacy;
}

export interface ContactEmail {
  id: number;
  type: EmailType;
  email: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface ContactPhone {
  id: number;
  type: PhoneType;
  phoneNumber: string;
  extension?: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface ContactAddress {
  id: number;
  type: AddressType;
  street?: string;
  street2?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isPrimary: boolean;
  displayOrder: number;
}

export interface ContactWebsite {
  id: number;
  type: WebsiteType;
  url: string;
  displayOrder: number;
}

export interface ContactCustomField {
  id: number;
  fieldName: string;
  fieldValue: string;
  fieldType: CustomFieldType;
}

export interface ContactRelationship {
  id: number;
  relatedContactId: number;
  relatedContactName: string;
  type: RelationshipType;
  description?: string;
}

export interface ContactGroupSummary {
  id: number;
  groupId: string;
  name: string;
  color: string;
}

export interface ContactGroup {
  id: number;
  groupId: string;
  userId: string;
  name: string;
  description?: string;
  color: string;
  icon?: string;
  isDistributionList: boolean;
  isSmartGroup: boolean;
  memberCount: number;
  createdAt: Date;
  lastModified?: Date;
  displayOrder: number;
}

export interface CreateContactGroupDto {
  name: string;
  description?: string;
  color: string;
  icon?: string;
  isDistributionList: boolean;
}

export interface UpdateContactGroupDto {
  name?: string;
  description?: string;
  color?: string;
  icon?: string;
  isDistributionList?: boolean;
  displayOrder?: number;
}

export interface ContactInteraction {
  id: number;
  contactId: number;
  type: InteractionType;
  subject: string;
  notes?: string;
  interactionDate: Date;
  relatedMessageId?: number;
  relatedEventId?: number;
  createdAt: Date;
}

export interface CreateInteractionDto {
  type: InteractionType;
  subject: string;
  notes?: string;
  interactionDate?: Date;
  relatedMessageId?: number;
  relatedEventId?: number;
}

export interface ContactQueryParameters {
  searchTerm?: string;
  categories?: string[];
  tags?: string[];
  groupIds?: number[];
  isFavorite?: boolean;
  source?: ContactSource;
  company?: string;
  sortBy?: string;
  sortDescending: boolean;
  pageNumber: number;
  pageSize: number;
}

export interface ContactStatistics {
  totalContacts: number;
  favoriteContacts: number;
  contactsWithBirthday: number;
  totalGroups: number;
  recentlyAdded: number;
  frequentlyContacted: number;
  contactsByCompany: { [key: string]: number };
  contactsBySource: { [key: string]: number };
  contactsByCategory: { [key: string]: number };
}

export interface MergeContactsDto {
  primaryContactId: number;
  contactIdsToMerge: number[];
  keepAllEmails: boolean;
  keepAllPhones: boolean;
  keepAllAddresses: boolean;
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Enums
export enum EmailType {
  Personal = 0,
  Work = 1,
  Other = 2
}

export enum PhoneType {
  Mobile = 0,
  Home = 1,
  Work = 2,
  Main = 3,
  HomeFax = 4,
  WorkFax = 5,
  Pager = 6,
  Other = 7
}

export enum AddressType {
  Home = 0,
  Work = 1,
  Other = 2
}

export enum WebsiteType {
  Personal = 0,
  Work = 1,
  Blog = 2,
  Portfolio = 3,
  Other = 4
}

export enum CustomFieldType {
  Text = 0,
  Number = 1,
  Date = 2,
  Boolean = 3,
  Url = 4
}

export enum RelationshipType {
  Spouse = 0,
  Partner = 1,
  Child = 2,
  Parent = 3,
  Sibling = 4,
  Relative = 5,
  Friend = 6,
  Assistant = 7,
  Manager = 8,
  Colleague = 9,
  ReferredBy = 10,
  Other = 11
}

export enum ContactSource {
  Manual = 0,
  Import = 1,
  Exchange = 2,
  ActiveDirectory = 3,
  LinkedIn = 4,
  Google = 5,
  Other = 6
}

export enum ContactPrivacy {
  Private = 0,
  Public = 1,
  Shared = 2
}

export enum InteractionType {
  Email = 0,
  Call = 1,
  Meeting = 2,
  Note = 3,
  Task = 4,
  Other = 5
}

// View types
export enum ContactView {
  List = 'list',
  Grid = 'grid',
  Details = 'details'
}
