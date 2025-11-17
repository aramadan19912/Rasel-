import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TranslateService } from '@ngx-translate/core';
import { AuthService } from '../../services/auth.service';

interface UserProfile {
  id: number;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  position?: string;
  department?: string;
  bio?: string;
  avatarUrl?: string;
  createdAt: Date;
  updatedAt: Date;
}

@Component({
  standalone: false,
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  profileForm!: FormGroup;
  passwordForm!: FormGroup;
  loading = true;
  submitting = false;
  userProfile: UserProfile | null = null;
  selectedAvatar: File | null = null;
  avatarPreview: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    public translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initializeForms();
    this.loadUserProfile();
  }

  initializeForms(): void {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: [{ value: '', disabled: true }],
      phoneNumber: ['', Validators.pattern(/^[0-9+\-\s()]*$/)],
      position: ['', Validators.maxLength(100)],
      department: ['', Validators.maxLength(100)],
      bio: ['', Validators.maxLength(500)]
    });

    this.passwordForm = this.fb.group({
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', Validators.required]
    }, { validator: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { 'mismatch': true };
  }

  loadUserProfile(): void {
    this.loading = true;

    // Get user info from AuthService
    this.authService.getCurrentUser().subscribe((currentUser: any) => {
      if (currentUser) {
        // Mock user profile - in real app, this would come from an API
        this.userProfile = {
          id: 1,
          email: currentUser.email || 'user@example.com',
          firstName: currentUser.firstName || '',
          lastName: currentUser.lastName || '',
          phoneNumber: '',
          position: '',
          department: '',
          bio: '',
          avatarUrl: '',
          createdAt: new Date(),
          updatedAt: new Date()
        };

        this.profileForm.patchValue({
          firstName: this.userProfile.firstName,
          lastName: this.userProfile.lastName,
          email: this.userProfile.email,
          phoneNumber: this.userProfile.phoneNumber,
          position: this.userProfile.position,
          department: this.userProfile.department,
          bio: this.userProfile.bio
        });

        if (this.userProfile.avatarUrl) {
          this.avatarPreview = this.userProfile.avatarUrl;
        }
      }

      this.loading = false;
    });
  }

  onAvatarSelected(event: any): void {
    const file = event.target.files[0];
    if (file && file.type.startsWith('image/')) {
      this.selectedAvatar = file;

      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.avatarPreview = e.target.result;
      };
      reader.readAsDataURL(file);
    } else {
      this.snackBar.open(
        this.translate.instant('profile.errors.invalidImageFormat'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
    }
  }

  removeAvatar(): void {
    this.selectedAvatar = null;
    this.avatarPreview = null;
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.markFormGroupTouched(this.profileForm);
      return;
    }

    this.submitting = true;

    // In a real application, this would call an API to update the profile
    const profileData = this.profileForm.getRawValue();

    console.log('Saving profile:', profileData);

    // Simulate API call
    setTimeout(() => {
      this.snackBar.open(
        this.translate.instant('profile.updateSuccess'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
      this.submitting = false;
    }, 1000);
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.markFormGroupTouched(this.passwordForm);

      if (this.passwordForm.hasError('mismatch')) {
        this.snackBar.open(
          this.translate.instant('profile.errors.passwordMismatch'),
          this.translate.instant('common.close'),
          { duration: 3000 }
        );
      }
      return;
    }

    this.submitting = true;

    const passwordData = this.passwordForm.value;

    // In a real application, this would call an API to change the password
    console.log('Changing password');

    // Simulate API call
    setTimeout(() => {
      this.snackBar.open(
        this.translate.instant('profile.passwordChangeSuccess'),
        this.translate.instant('common.close'),
        { duration: 3000 }
      );
      this.passwordForm.reset();
      this.submitting = false;
    }, 1000);
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  getErrorMessage(formGroup: FormGroup, fieldName: string): string {
    const control = formGroup.get(fieldName);
    if (control?.hasError('required')) {
      return this.translate.instant('validation.required');
    }
    if (control?.hasError('email')) {
      return this.translate.instant('validation.invalidEmail');
    }
    if (control?.hasError('minlength')) {
      const minLength = control.errors?.['minlength'].requiredLength;
      return this.translate.instant('validation.minLength', { min: minLength });
    }
    if (control?.hasError('maxlength')) {
      const maxLength = control.errors?.['maxlength'].requiredLength;
      return this.translate.instant('validation.maxLength', { max: maxLength });
    }
    if (control?.hasError('pattern')) {
      return this.translate.instant('validation.invalidFormat');
    }
    return '';
  }
}
