import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import {
  Contact,
  ContactGroup,
  CreateContactDto,
  UpdateContactDto,
  EmailType,
  PhoneType,
  AddressType,
  WebsiteType,
  ContactPrivacy
} from '../../../models/contact.model';

export interface ContactDialogData {
  contact: Contact | null;
  groups: ContactGroup[];
}

@Component({
  standalone: false,
  selector: 'app-contact-dialog',
  templateUrl: './contact-dialog.component.html',
  styleUrls: ['./contact-dialog.component.scss']
})
export class ContactDialogComponent implements OnInit {
  contactForm: FormGroup;
  isEditMode = false;
  selectedTab = 0;

  // Enum references for template
  EmailType = EmailType;
  PhoneType = PhoneType;
  AddressType = AddressType;
  WebsiteType = WebsiteType;
  ContactPrivacy = ContactPrivacy;

  // Enum options
  emailTypes = [
    { value: EmailType.Personal, label: 'Personal' },
    { value: EmailType.Work, label: 'Work' },
    { value: EmailType.Other, label: 'Other' }
  ];

  phoneTypes = [
    { value: PhoneType.Mobile, label: 'Mobile' },
    { value: PhoneType.Home, label: 'Home' },
    { value: PhoneType.Work, label: 'Work' },
    { value: PhoneType.Main, label: 'Main' },
    { value: PhoneType.HomeFax, label: 'Home Fax' },
    { value: PhoneType.WorkFax, label: 'Work Fax' },
    { value: PhoneType.Pager, label: 'Pager' },
    { value: PhoneType.Other, label: 'Other' }
  ];

  addressTypes = [
    { value: AddressType.Home, label: 'Home' },
    { value: AddressType.Work, label: 'Work' },
    { value: AddressType.Other, label: 'Other' }
  ];

  websiteTypes = [
    { value: WebsiteType.Personal, label: 'Personal' },
    { value: WebsiteType.Work, label: 'Work' },
    { value: WebsiteType.Blog, label: 'Blog' },
    { value: WebsiteType.Portfolio, label: 'Portfolio' },
    { value: WebsiteType.Other, label: 'Other' }
  ];

  privacyOptions = [
    { value: ContactPrivacy.Private, label: 'Private' },
    { value: ContactPrivacy.Public, label: 'Public' },
    { value: ContactPrivacy.Shared, label: 'Shared' }
  ];

  genderOptions = ['Male', 'Female', 'Other', 'Prefer not to say'];

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<ContactDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ContactDialogData
  ) {
    this.isEditMode = !!data.contact;
    this.contactForm = this.createForm();
  }

  ngOnInit(): void {
    if (this.isEditMode && this.data.contact) {
      this.loadContact(this.data.contact);
    }
  }

  createForm(): FormGroup {
    return this.fb.group({
      // Basic Information
      title: [''],
      firstName: ['', Validators.required],
      middleName: [''],
      lastName: ['', Validators.required],
      suffix: [''],
      nickname: [''],

      // Contact Details
      emailAddresses: this.fb.array([]),
      phoneNumbers: this.fb.array([]),
      addresses: this.fb.array([]),
      websites: this.fb.array([]),

      // Professional Information
      jobTitle: [''],
      department: [''],
      company: [''],
      manager: [''],
      assistant: [''],
      officeLocation: [''],

      // Personal Information
      birthday: [null],
      spouseName: [''],
      children: [''],
      gender: [''],

      // Additional Details
      notes: [''],
      categories: [[]],
      tags: [[]],

      // Social Media
      linkedInUrl: [''],
      twitterHandle: [''],
      facebookUrl: [''],
      instagramHandle: [''],

      // Privacy
      privacy: [ContactPrivacy.Private, Validators.required]
    });
  }

  loadContact(contact: Contact): void {
    this.contactForm.patchValue({
      title: contact.title,
      firstName: contact.firstName,
      middleName: contact.middleName,
      lastName: contact.lastName,
      suffix: contact.suffix,
      nickname: contact.nickname,
      jobTitle: contact.jobTitle,
      department: contact.department,
      company: contact.company,
      manager: contact.manager,
      assistant: contact.assistant,
      officeLocation: contact.officeLocation,
      birthday: contact.birthday,
      spouseName: contact.spouseName,
      children: contact.children,
      gender: contact.gender,
      notes: contact.notes,
      categories: contact.categories,
      tags: contact.tags,
      linkedInUrl: contact.linkedInUrl,
      twitterHandle: contact.twitterHandle,
      facebookUrl: contact.facebookUrl,
      instagramHandle: contact.instagramHandle,
      privacy: contact.privacy
    });

    // Load arrays
    contact.emailAddresses.forEach(email => this.addEmail(email));
    contact.phoneNumbers.forEach(phone => this.addPhone(phone));
    contact.addresses.forEach(address => this.addAddress(address));
    contact.websites.forEach(website => this.addWebsite(website));
  }

  // ========== Email Addresses ==========

  get emailAddresses(): FormArray {
    return this.contactForm.get('emailAddresses') as FormArray;
  }

  createEmailGroup(email?: any): FormGroup {
    return this.fb.group({
      id: [email?.id || 0],
      type: [email?.type ?? EmailType.Personal],
      email: [email?.email || '', [Validators.required, Validators.email]],
      isPrimary: [email?.isPrimary || false],
      displayOrder: [email?.displayOrder || 0]
    });
  }

  addEmail(email?: any): void {
    this.emailAddresses.push(this.createEmailGroup(email));
  }

  removeEmail(index: number): void {
    this.emailAddresses.removeAt(index);
  }

  setPrimaryEmail(index: number): void {
    this.emailAddresses.controls.forEach((control, i) => {
      control.patchValue({ isPrimary: i === index });
    });
  }

  // ========== Phone Numbers ==========

  get phoneNumbers(): FormArray {
    return this.contactForm.get('phoneNumbers') as FormArray;
  }

  createPhoneGroup(phone?: any): FormGroup {
    return this.fb.group({
      id: [phone?.id || 0],
      type: [phone?.type ?? PhoneType.Mobile],
      phoneNumber: [phone?.phoneNumber || '', Validators.required],
      extension: [phone?.extension || ''],
      isPrimary: [phone?.isPrimary || false],
      displayOrder: [phone?.displayOrder || 0]
    });
  }

  addPhone(phone?: any): void {
    this.phoneNumbers.push(this.createPhoneGroup(phone));
  }

  removePhone(index: number): void {
    this.phoneNumbers.removeAt(index);
  }

  setPrimaryPhone(index: number): void {
    this.phoneNumbers.controls.forEach((control, i) => {
      control.patchValue({ isPrimary: i === index });
    });
  }

  // ========== Addresses ==========

  get addresses(): FormArray {
    return this.contactForm.get('addresses') as FormArray;
  }

  createAddressGroup(address?: any): FormGroup {
    return this.fb.group({
      id: [address?.id || 0],
      type: [address?.type ?? AddressType.Home],
      street: [address?.street || ''],
      street2: [address?.street2 || ''],
      city: [address?.city || ''],
      state: [address?.state || ''],
      postalCode: [address?.postalCode || ''],
      country: [address?.country || ''],
      isPrimary: [address?.isPrimary || false],
      displayOrder: [address?.displayOrder || 0]
    });
  }

  addAddress(address?: any): void {
    this.addresses.push(this.createAddressGroup(address));
  }

  removeAddress(index: number): void {
    this.addresses.removeAt(index);
  }

  setPrimaryAddress(index: number): void {
    this.addresses.controls.forEach((control, i) => {
      control.patchValue({ isPrimary: i === index });
    });
  }

  // ========== Websites ==========

  get websites(): FormArray {
    return this.contactForm.get('websites') as FormArray;
  }

  createWebsiteGroup(website?: any): FormGroup {
    return this.fb.group({
      id: [website?.id || 0],
      type: [website?.type ?? WebsiteType.Personal],
      url: [website?.url || '', [Validators.required, Validators.pattern(/^https?:\/\/.+/)]],
      displayOrder: [website?.displayOrder || 0]
    });
  }

  addWebsite(website?: any): void {
    this.websites.push(this.createWebsiteGroup(website));
  }

  removeWebsite(index: number): void {
    this.websites.removeAt(index);
  }

  // ========== Categories & Tags ==========

  categoryInput = '';
  tagInput = '';

  addCategory(): void {
    const category = this.categoryInput.trim();
    if (category) {
      const categories = this.contactForm.get('categories')?.value || [];
      if (!categories.includes(category)) {
        categories.push(category);
        this.contactForm.patchValue({ categories });
      }
      this.categoryInput = '';
    }
  }

  removeCategory(category: string): void {
    const categories = (this.contactForm.get('categories')?.value || []).filter(
      (c: string) => c !== category
    );
    this.contactForm.patchValue({ categories });
  }

  addTag(): void {
    const tag = this.tagInput.trim();
    if (tag) {
      const tags = this.contactForm.get('tags')?.value || [];
      if (!tags.includes(tag)) {
        tags.push(tag);
        this.contactForm.patchValue({ tags });
      }
      this.tagInput = '';
    }
  }

  removeTag(tag: string): void {
    const tags = (this.contactForm.get('tags')?.value || []).filter(
      (t: string) => t !== tag
    );
    this.contactForm.patchValue({ tags });
  }

  // ========== Form Submission ==========

  onSave(): void {
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      this.selectedTab = 0; // Go to basic info tab
      return;
    }

    const formValue = this.contactForm.value;

    if (this.isEditMode) {
      const updateDto: UpdateContactDto = {
        ...formValue,
        emailAddresses: formValue.emailAddresses.map((e: any, i: number) => ({
          ...e,
          displayOrder: i
        })),
        phoneNumbers: formValue.phoneNumbers.map((p: any, i: number) => ({
          ...p,
          displayOrder: i
        })),
        addresses: formValue.addresses.map((a: any, i: number) => ({
          ...a,
          displayOrder: i
        })),
        websites: formValue.websites.map((w: any, i: number) => ({
          ...w,
          displayOrder: i
        }))
      };

      this.dialogRef.close({
        action: 'update',
        contact: {
          id: this.data.contact!.id,
          contact: updateDto
        }
      });
    } else {
      const createDto: CreateContactDto = {
        ...formValue,
        emailAddresses: formValue.emailAddresses.map((e: any, i: number) => ({
          ...e,
          displayOrder: i
        })),
        phoneNumbers: formValue.phoneNumbers.map((p: any, i: number) => ({
          ...p,
          displayOrder: i
        })),
        addresses: formValue.addresses.map((a: any, i: number) => ({
          ...a,
          displayOrder: i
        })),
        websites: formValue.websites.map((w: any, i: number) => ({
          ...w,
          displayOrder: i
        }))
      };

      this.dialogRef.close({
        action: 'create',
        contact: createDto
      });
    }
  }

  onDelete(): void {
    if (this.isEditMode && this.data.contact) {
      this.dialogRef.close({
        action: 'delete',
        contactId: this.data.contact.id
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  // ========== Helpers ==========

  getErrorMessage(controlName: string): string {
    const control = this.contactForm.get(controlName);
    if (control?.hasError('required')) {
      return 'This field is required';
    }
    if (control?.hasError('email')) {
      return 'Invalid email address';
    }
    if (control?.hasError('pattern')) {
      return 'Invalid format';
    }
    return '';
  }

  getEmailError(index: number): string {
    const control = this.emailAddresses.at(index).get('email');
    if (control?.hasError('required')) {
      return 'Email is required';
    }
    if (control?.hasError('email')) {
      return 'Invalid email address';
    }
    return '';
  }

  getPhoneError(index: number): string {
    const control = this.phoneNumbers.at(index).get('phoneNumber');
    if (control?.hasError('required')) {
      return 'Phone number is required';
    }
    return '';
  }

  getWebsiteError(index: number): string {
    const control = this.websites.at(index).get('url');
    if (control?.hasError('required')) {
      return 'URL is required';
    }
    if (control?.hasError('pattern')) {
      return 'Invalid URL format (must start with http:// or https://)';
    }
    return '';
  }
}
