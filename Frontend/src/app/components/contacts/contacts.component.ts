import { Component, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ContactsService } from '../../services/contacts.service';
import {
  Contact,
  ContactGroup,
  ContactView,
  ContactQueryParameters,
  CreateContactDto
} from '../../models/contact.model';
import { ContactDialogComponent } from './contact-dialog/contact-dialog.component';

@Component({
  selector: 'app-contacts',
  templateUrl: './contacts.component.html',
  styleUrls: ['./contacts.component.scss']
})
export class ContactsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // Contacts
  contacts: Contact[] = [];
  filteredContacts: Contact[] = [];
  selectedContact: Contact | null = null;

  // Groups
  groups: ContactGroup[] = [];
  selectedGroups: number[] = [];

  // View state
  currentView: ContactView = ContactView.List;
  searchTerm = '';

  // Pagination
  pageNumber = 1;
  pageSize = 50;
  totalCount = 0;
  totalPages = 0;

  // Filters
  showOnlyFavorites = false;
  selectedCategory = '';
  categories: string[] = [];

  // UI state
  isLoading = false;
  showSidebar = true;

  // View enum for template
  ContactView = ContactView;

  constructor(
    private contactsService: ContactsService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadContacts();
    this.loadGroups();

    // Subscribe to contacts changes
    this.contactsService.contacts$
      .pipe(takeUntil(this.destroy$))
      .subscribe(contacts => {
        this.contacts = contacts;
        this.filteredContacts = contacts;
        this.extractCategories();
      });

    // Subscribe to groups changes
    this.contactsService.groups$
      .pipe(takeUntil(this.destroy$))
      .subscribe(groups => {
        this.groups = groups;
      });

    // Subscribe to selected contact
    this.contactsService.selectedContact$
      .pipe(takeUntil(this.destroy$))
      .subscribe(contact => {
        this.selectedContact = contact;
      });

    // Subscribe to view type
    this.contactsService.viewType$
      .pipe(takeUntil(this.destroy$))
      .subscribe(view => {
        this.currentView = view;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ========== Data Loading ==========

  loadContacts(): void {
    this.isLoading = true;
    const params: ContactQueryParameters = {
      searchTerm: this.searchTerm,
      isFavorite: this.showOnlyFavorites ? true : undefined,
      categories: this.selectedCategory ? [this.selectedCategory] : undefined,
      groupIds: this.selectedGroups.length > 0 ? this.selectedGroups : undefined,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      sortBy: 'DisplayName',
      sortDescending: false
    };

    this.contactsService.getContacts(params).subscribe({
      next: (result) => {
        this.contacts = result.items;
        this.filteredContacts = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages;
        this.isLoading = false;
        this.extractCategories();
      },
      error: (error) => {
        this.showError('Failed to load contacts');
        this.isLoading = false;
      }
    });
  }

  loadGroups(): void {
    this.contactsService.getGroups().subscribe({
      next: (groups) => {
        this.groups = groups;
      },
      error: (error) => {
        console.error('Failed to load groups', error);
      }
    });
  }

  refreshContacts(): void {
    this.loadContacts();
  }

  // ========== Search & Filter ==========

  onSearch(): void {
    this.pageNumber = 1;
    this.loadContacts();
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.onSearch();
  }

  toggleFavoritesFilter(): void {
    this.showOnlyFavorites = !this.showOnlyFavorites;
    this.pageNumber = 1;
    this.loadContacts();
  }

  filterByCategory(category: string): void {
    this.selectedCategory = category;
    this.pageNumber = 1;
    this.loadContacts();
  }

  clearCategoryFilter(): void {
    this.selectedCategory = '';
    this.loadContacts();
  }

  toggleGroupFilter(groupId: number): void {
    const index = this.selectedGroups.indexOf(groupId);
    if (index > -1) {
      this.selectedGroups.splice(index, 1);
    } else {
      this.selectedGroups.push(groupId);
    }
    this.pageNumber = 1;
    this.loadContacts();
  }

  isGroupSelected(groupId: number): boolean {
    return this.selectedGroups.includes(groupId);
  }

  extractCategories(): void {
    const categorySet = new Set<string>();
    this.contacts.forEach(contact => {
      contact.categories.forEach(cat => categorySet.add(cat));
    });
    this.categories = Array.from(categorySet).sort();
  }

  // ========== Contact Selection ==========

  selectContact(contact: Contact): void {
    this.selectedContact = contact;
    this.contactsService.setSelectedContact(contact);
  }

  deselectContact(): void {
    this.selectedContact = null;
    this.contactsService.setSelectedContact(null);
  }

  // ========== View Management ==========

  changeView(view: ContactView): void {
    this.currentView = view;
    this.contactsService.setViewType(view);
  }

  toggleSidebar(): void {
    this.showSidebar = !this.showSidebar;
  }

  // ========== Contact Actions ==========

  openContactDialog(contact: Contact | null = null): void {
    const dialogRef = this.dialog.open(ContactDialogComponent, {
      width: '800px',
      maxHeight: '90vh',
      data: { contact, groups: this.groups }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (result.action === 'create') {
          this.createContact(result.contact);
        } else if (result.action === 'update') {
          this.updateContact(result.contact);
        } else if (result.action === 'delete') {
          this.deleteContact(result.contactId);
        }
      }
    });
  }

  createContact(dto: CreateContactDto): void {
    this.contactsService.createContact(dto).subscribe({
      next: () => {
        this.showSuccess('Contact created successfully');
        this.refreshContacts();
      },
      error: () => {
        this.showError('Failed to create contact');
      }
    });
  }

  updateContact(data: { id: number, contact: any }): void {
    this.contactsService.updateContact(data.id, data.contact).subscribe({
      next: () => {
        this.showSuccess('Contact updated successfully');
        this.refreshContacts();
      },
      error: () => {
        this.showError('Failed to update contact');
      }
    });
  }

  deleteContact(contactId: number): void {
    if (confirm('Are you sure you want to delete this contact?')) {
      this.contactsService.deleteContact(contactId).subscribe({
        next: () => {
          this.showSuccess('Contact deleted successfully');
          this.deselectContact();
          this.refreshContacts();
        },
        error: () => {
          this.showError('Failed to delete contact');
        }
      });
    }
  }

  toggleFavorite(contact: Contact, event: Event): void {
    event.stopPropagation();
    this.contactsService.toggleFavorite(contact.id).subscribe({
      next: () => {
        contact.isFavorite = !contact.isFavorite;
        this.showSuccess(contact.isFavorite ? 'Added to favorites' : 'Removed from favorites');
      },
      error: () => {
        this.showError('Failed to update favorite status');
      }
    });
  }

  // ========== Photo Management ==========

  onPhotoUpload(contact: Contact, event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.contactsService.uploadPhoto(contact.id, file).subscribe({
        next: () => {
          this.showSuccess('Photo uploaded successfully');
          contact.photoUrl = this.contactsService.getPhotoUrl(contact.id);
        },
        error: () => {
          this.showError('Failed to upload photo');
        }
      });
    }
  }

  deletePhoto(contact: Contact): void {
    this.contactsService.deletePhoto(contact.id).subscribe({
      next: () => {
        this.showSuccess('Photo deleted successfully');
        contact.photoUrl = undefined;
      },
      error: () => {
        this.showError('Failed to delete photo');
      }
    });
  }

  // ========== Pagination ==========

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.loadContacts();
  }

  nextPage(): void {
    if (this.pageNumber < this.totalPages) {
      this.pageNumber++;
      this.loadContacts();
    }
  }

  previousPage(): void {
    if (this.pageNumber > 1) {
      this.pageNumber--;
      this.loadContacts();
    }
  }

  // ========== Helper Methods ==========

  getInitials(contact: Contact): string {
    return this.contactsService.getInitials(contact);
  }

  getContactColor(contact: Contact): string {
    return this.contactsService.getContactColor(contact);
  }

  getPrimaryEmail(contact: Contact): string {
    return this.contactsService.getPrimaryEmail(contact);
  }

  getPrimaryPhone(contact: Contact): string {
    return this.contactsService.getPrimaryPhone(contact);
  }

  formatPhoneNumber(phoneNumber: string): string {
    return this.contactsService.formatPhoneNumber(phoneNumber);
  }

  // ========== UI Helpers ==========

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
}
