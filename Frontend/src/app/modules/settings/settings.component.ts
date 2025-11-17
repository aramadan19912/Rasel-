import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';

interface UserSettings {
  // General Settings
  language: string;
  timezone: string;
  dateFormat: string;
  timeFormat: string;

  // Notification Settings
  emailNotifications: boolean;
  pushNotifications: boolean;
  desktopNotifications: boolean;
  notifyOnNewMessage: boolean;
  notifyOnCalendarEvent: boolean;
  notifyOnCorrespondenceRouting: boolean;

  // Privacy Settings
  showOnlineStatus: boolean;
  allowSearchByEmail: boolean;
  showProfilePhoto: boolean;

  // Correspondence Settings
  autoSaveDrafts: boolean;
  defaultCorrespondencePriority: string;
  defaultConfidentialityLevel: string;
  requireApprovalBeforeSending: boolean;
}

@Component({
  standalone: false,
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.scss']
})
export class SettingsComponent implements OnInit {
  generalForm!: FormGroup;
  notificationForm!: FormGroup;
  privacyForm!: FormGroup;
  correspondenceForm!: FormGroup;
  loading = false;
  submitting = false;

  languages = [
    { code: 'ar', name: 'العربية' },
    { code: 'en', name: 'English' }
  ];

  timezones = [
    'UTC',
    'Asia/Riyadh',
    'Asia/Dubai',
    'Asia/Kuwait',
    'Africa/Cairo'
  ];

  dateFormats = [
    'dd/MM/yyyy',
    'MM/dd/yyyy',
    'yyyy-MM-dd'
  ];

  timeFormats = [
    '12-hour',
    '24-hour'
  ];

  priorities = [
    { value: 'Low', label: 'correspondence.priority.Low' },
    { value: 'Normal', label: 'correspondence.priority.Normal' },
    { value: 'High', label: 'correspondence.priority.High' },
    { value: 'Urgent', label: 'correspondence.priority.Urgent' }
  ];

  confidentialityLevels = [
    { value: 'Public', label: 'correspondence.confidentiality.Public' },
    { value: 'Internal', label: 'correspondence.confidentiality.Internal' },
    { value: 'Confidential', label: 'correspondence.confidentiality.Confidential' },
    { value: 'Secret', label: 'correspondence.confidentiality.Secret' }
  ];

  constructor(
    private fb: FormBuilder,
    private snackBar: MatSnackBar,
    public translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initializeForms();
    this.loadSettings();
  }

  initializeForms(): void {
    this.generalForm = this.fb.group({
      language: ['ar'],
      timezone: ['Asia/Riyadh'],
      dateFormat: ['dd/MM/yyyy'],
      timeFormat: ['24-hour']
    });

    this.notificationForm = this.fb.group({
      emailNotifications: [true],
      pushNotifications: [true],
      desktopNotifications: [false],
      notifyOnNewMessage: [true],
      notifyOnCalendarEvent: [true],
      notifyOnCorrespondenceRouting: [true]
    });

    this.privacyForm = this.fb.group({
      showOnlineStatus: [true],
      allowSearchByEmail: [true],
      showProfilePhoto: [true]
    });

    this.correspondenceForm = this.fb.group({
      autoSaveDrafts: [true],
      defaultCorrespondencePriority: ['Normal'],
      defaultConfidentialityLevel: ['Internal'],
      requireApprovalBeforeSending: [false]
    });
  }

  loadSettings(): void {
    // In a real app, load settings from backend/localStorage
    const savedLanguage = this.translate.currentLang || 'ar';
    this.generalForm.patchValue({
      language: savedLanguage
    });
  }

  saveGeneralSettings(): void {
    this.submitting = true;

    const settings = this.generalForm.value;

    // Change language if it was updated
    if (settings.language !== this.translate.currentLang) {
      this.translate.use(settings.language);
      // Set direction based on language
      document.documentElement.setAttribute('dir', settings.language === 'ar' ? 'rtl' : 'ltr');
      document.documentElement.setAttribute('lang', settings.language);
    }

    // Save to localStorage or backend
    localStorage.setItem('userSettings', JSON.stringify(settings));

    setTimeout(() => {
      this.snackBar.open(
        this.translate.instant('settings.saveSuccess'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
      this.submitting = false;
    }, 500);
  }

  saveNotificationSettings(): void {
    this.submitting = true;

    const settings = this.notificationForm.value;
    localStorage.setItem('notificationSettings', JSON.stringify(settings));

    setTimeout(() => {
      this.snackBar.open(
        this.translate.instant('settings.saveSuccess'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
      this.submitting = false;
    }, 500);
  }

  savePrivacySettings(): void {
    this.submitting = true;

    const settings = this.privacyForm.value;
    localStorage.setItem('privacySettings', JSON.stringify(settings));

    setTimeout(() => {
      this.snackBar.open(
        this.translate.instant('settings.saveSuccess'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
      this.submitting = false;
    }, 500);
  }

  saveCorrespondenceSettings(): void {
    this.submitting = true;

    const settings = this.correspondenceForm.value;
    localStorage.setItem('correspondenceSettings', JSON.stringify(settings));

    setTimeout(() => {
      this.snackBar.open(
        this.translate.instant('settings.saveSuccess'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
      this.submitting = false;
    }, 500);
  }

  resetToDefaults(): void {
    if (confirm(this.translate.instant('settings.confirmReset'))) {
      this.initializeForms();
      this.snackBar.open(
        this.translate.instant('settings.resetSuccess'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
    }
  }
}
