import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { FormControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, startWith } from 'rxjs/operators';
import { ContactsService } from '../../../services/contacts.service';
import { Contact } from '../../../models/contact.model';

export interface ContactPickerData {
  title?: string;
  multiSelect?: boolean;
  selectedContacts?: Contact[];
}

export interface ContactPickerResult {
  contacts: Contact[];
}

@Component({
  standalone: false,
  selector: 'app-contact-picker',
  templateUrl: './contact-picker.component.html',
  styleUrls: ['./contact-picker.component.scss']
})
export class ContactPickerComponent implements OnInit {
  searchControl = new FormControl('');
  contacts: Contact[] = [];
  filteredContacts: Contact[] = [];
  selectedContacts: Contact[] = [];
  multiSelect = true;
  title = 'Select Contacts';
  isLoading = false;

  constructor(
    private contactsService: ContactsService,
    public dialogRef: MatDialogRef<ContactPickerComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ContactPickerData
  ) {
    if (data) {
      this.title = data.title || this.title;
      this.multiSelect = data.multiSelect !== undefined ? data.multiSelect : this.multiSelect;
      this.selectedContacts = data.selectedContacts ? [...data.selectedContacts] : [];
    }
  }

  ngOnInit(): void {
    this.loadContacts();

    // Setup search
    this.searchControl.valueChanges.pipe(
      startWith(''),
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(searchTerm => {
      this.filterContacts(searchTerm || '');
    });
  }

  loadContacts(): void {
    this.isLoading = true;
    this.contactsService.getAllContacts().subscribe({
      next: (contacts) => {
        this.contacts = contacts;
        this.filteredContacts = contacts;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load contacts', error);
        this.isLoading = false;
      }
    });
  }

  filterContacts(searchTerm: string): void {
    if (!searchTerm) {
      this.filteredContacts = this.contacts;
      return;
    }

    const term = searchTerm.toLowerCase();
    this.filteredContacts = this.contacts.filter(contact =>
      contact.displayName.toLowerCase().includes(term) ||
      this.getPrimaryEmail(contact).toLowerCase().includes(term) ||
      contact.company?.toLowerCase().includes(term)
    );
  }

  toggleContact(contact: Contact): void {
    const index = this.selectedContacts.findIndex(c => c.id === contact.id);

    if (index > -1) {
      this.selectedContacts.splice(index, 1);
    } else {
      if (this.multiSelect) {
        this.selectedContacts.push(contact);
      } else {
        this.selectedContacts = [contact];
      }
    }
  }

  isContactSelected(contact: Contact): boolean {
    return this.selectedContacts.some(c => c.id === contact.id);
  }

  onConfirm(): void {
    const result: ContactPickerResult = {
      contacts: this.selectedContacts
    };
    this.dialogRef.close(result);
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  // Helper methods
  getPrimaryEmail(contact: Contact): string {
    return this.contactsService.getPrimaryEmail(contact);
  }

  getPrimaryPhone(contact: Contact): string {
    return this.contactsService.getPrimaryPhone(contact);
  }

  getInitials(contact: Contact): string {
    return this.contactsService.getInitials(contact);
  }

  getContactColor(contact: Contact): string {
    return this.contactsService.getContactColor(contact);
  }
}
