import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  Contact,
  CreateContactDto,
  UpdateContactDto,
  ContactGroup,
  CreateContactGroupDto,
  UpdateContactGroupDto,
  ContactInteraction,
  CreateInteractionDto,
  ContactQueryParameters,
  ContactStatistics,
  PaginatedList,
  ContactView
} from '../models/contact.model';

@Injectable({
  providedIn: 'root'
})
export class ContactsService {
  private apiUrl = `${environment.apiUrl}/api/Contacts`;

  // State management
  private contactsSubject = new BehaviorSubject<Contact[]>([]);
  public contacts$ = this.contactsSubject.asObservable();

  private groupsSubject = new BehaviorSubject<ContactGroup[]>([]);
  public groups$ = this.groupsSubject.asObservable();

  private selectedContactSubject = new BehaviorSubject<Contact | null>(null);
  public selectedContact$ = this.selectedContactSubject.asObservable();

  private viewTypeSubject = new BehaviorSubject<ContactView>(ContactView.List);
  public viewType$ = this.viewTypeSubject.asObservable();

  constructor(private http: HttpClient) {}

  // State setters
  setSelectedContact(contact: Contact | null): void {
    this.selectedContactSubject.next(contact);
  }

  setViewType(view: ContactView): void {
    this.viewTypeSubject.next(view);
  }

  // ========== Contact Management ==========

  /**
   * Get a specific contact
   */
  getContact(id: number): Observable<Contact> {
    return this.http.get<Contact>(`${this.apiUrl}/${id}`);
  }

  /**
   * Get contact by ContactId (GUID)
   */
  getContactByContactId(contactId: string): Observable<Contact> {
    return this.http.get<Contact>(`${this.apiUrl}/by-contact-id/${contactId}`);
  }

  /**
   * Get contacts with query parameters
   */
  getContacts(parameters: ContactQueryParameters): Observable<PaginatedList<Contact>> {
    let params = new HttpParams()
      .set('pageNumber', parameters.pageNumber.toString())
      .set('pageSize', parameters.pageSize.toString())
      .set('sortDescending', parameters.sortDescending.toString());

    if (parameters.searchTerm) {
      params = params.set('searchTerm', parameters.searchTerm);
    }

    if (parameters.sortBy) {
      params = params.set('sortBy', parameters.sortBy);
    }

    if (parameters.categories && parameters.categories.length > 0) {
      parameters.categories.forEach(cat => {
        params = params.append('categories', cat);
      });
    }

    if (parameters.groupIds && parameters.groupIds.length > 0) {
      parameters.groupIds.forEach(id => {
        params = params.append('groupIds', id.toString());
      });
    }

    if (parameters.isFavorite !== undefined) {
      params = params.set('isFavorite', parameters.isFavorite.toString());
    }

    if (parameters.company) {
      params = params.set('company', parameters.company);
    }

    return this.http.get<PaginatedList<Contact>>(`${this.apiUrl}`, { params }).pipe(
      tap(result => this.contactsSubject.next(result.items))
    );
  }

  /**
   * Get all contacts
   */
  getAllContacts(): Observable<Contact[]> {
    return this.http.get<Contact[]>(`${this.apiUrl}/all`).pipe(
      tap(contacts => this.contactsSubject.next(contacts))
    );
  }

  /**
   * Get favorite contacts
   */
  getFavoriteContacts(): Observable<Contact[]> {
    return this.http.get<Contact[]>(`${this.apiUrl}/favorites`);
  }

  /**
   * Get recent contacts
   */
  getRecentContacts(count: number = 10): Observable<Contact[]> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<Contact[]>(`${this.apiUrl}/recent`, { params });
  }

  /**
   * Get frequent contacts
   */
  getFrequentContacts(count: number = 10): Observable<Contact[]> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<Contact[]>(`${this.apiUrl}/frequent`, { params });
  }

  /**
   * Create a new contact
   */
  createContact(dto: CreateContactDto): Observable<Contact> {
    return this.http.post<Contact>(`${this.apiUrl}`, dto).pipe(
      tap(() => this.refreshContacts())
    );
  }

  /**
   * Update a contact
   */
  updateContact(id: number, dto: UpdateContactDto): Observable<Contact> {
    return this.http.put<Contact>(`${this.apiUrl}/${id}`, dto).pipe(
      tap(() => this.refreshContacts())
    );
  }

  /**
   * Delete a contact
   */
  deleteContact(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.refreshContacts())
    );
  }

  /**
   * Toggle favorite status
   */
  toggleFavorite(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/toggle-favorite`, {}).pipe(
      tap(() => this.refreshContacts())
    );
  }

  /**
   * Toggle block status
   */
  toggleBlock(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/toggle-block`, {}).pipe(
      tap(() => this.refreshContacts())
    );
  }

  // ========== Photo Management ==========

  /**
   * Upload contact photo
   */
  uploadPhoto(id: number, file: File): Observable<void> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<void>(`${this.apiUrl}/${id}/photo`, formData);
  }

  /**
   * Delete contact photo
   */
  deletePhoto(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}/photo`);
  }

  /**
   * Get contact photo URL
   */
  getPhotoUrl(id: number): string {
    return `${this.apiUrl}/${id}/photo`;
  }

  // ========== Contact Groups ==========

  /**
   * Get a specific group
   */
  getGroup(id: number): Observable<ContactGroup> {
    return this.http.get<ContactGroup>(`${this.apiUrl}/groups/${id}`);
  }

  /**
   * Get all groups
   */
  getGroups(): Observable<ContactGroup[]> {
    return this.http.get<ContactGroup[]>(`${this.apiUrl}/groups`).pipe(
      tap(groups => this.groupsSubject.next(groups))
    );
  }

  /**
   * Create a new group
   */
  createGroup(dto: CreateContactGroupDto): Observable<ContactGroup> {
    return this.http.post<ContactGroup>(`${this.apiUrl}/groups`, dto).pipe(
      tap(() => this.getGroups().subscribe())
    );
  }

  /**
   * Add contact to group
   */
  addContactToGroup(contactId: number, groupId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${contactId}/groups/${groupId}`, {});
  }

  // ========== Helper Methods ==========

  /**
   * Refresh contacts list
   */
  private refreshContacts(): void {
    this.getAllContacts().subscribe();
  }

  /**
   * Get primary email
   */
  getPrimaryEmail(contact: Contact): string {
    const primary = contact.emailAddresses.find(e => e.isPrimary);
    return primary?.email || contact.emailAddresses[0]?.email || '';
  }

  /**
   * Get primary phone
   */
  getPrimaryPhone(contact: Contact): string {
    const primary = contact.phoneNumbers.find(p => p.isPrimary);
    return primary?.phoneNumber || contact.phoneNumbers[0]?.phoneNumber || '';
  }

  /**
   * Get initials for avatar
   */
  getInitials(contact: Contact): string {
    const first = contact.firstName?.charAt(0) || '';
    const last = contact.lastName?.charAt(0) || '';
    return (first + last).toUpperCase();
  }

  /**
   * Get contact color for avatar
   */
  getContactColor(contact: Contact): string {
    const colors = [
      '#F44336', '#E91E63', '#9C27B0', '#673AB7',
      '#3F51B5', '#2196F3', '#03A9F4', '#00BCD4',
      '#009688', '#4CAF50', '#8BC34A', '#CDDC39',
      '#FFC107', '#FF9800', '#FF5722', '#795548'
    ];

    const index = (contact.firstName.charCodeAt(0) + contact.lastName.charCodeAt(0)) % colors.length;
    return colors[index];
  }

  /**
   * Format phone number
   */
  formatPhoneNumber(phoneNumber: string): string {
    // Simple US phone number formatting
    const cleaned = phoneNumber.replace(/\D/g, '');
    if (cleaned.length === 10) {
      return `(${cleaned.slice(0, 3)}) ${cleaned.slice(3, 6)}-${cleaned.slice(6)}`;
    }
    return phoneNumber;
  }

  /**
   * Get full address string
   */
  getFullAddress(address: any): string {
    const parts = [
      address.street,
      address.street2,
      address.city,
      address.state,
      address.postalCode,
      address.country
    ].filter(part => part);

    return parts.join(', ');
  }

  /**
   * Search contacts
   */
  searchContacts(searchTerm: string): Observable<Contact[]> {
    const params: ContactQueryParameters = {
      searchTerm,
      pageNumber: 1,
      pageSize: 100,
      sortDescending: false
    };
    return this.getContacts(params).pipe(
      tap(result => this.contactsSubject.next(result.items))
    ) as any;
  }
}
