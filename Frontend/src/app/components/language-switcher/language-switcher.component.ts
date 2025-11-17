import { Component, OnInit } from '@angular/core';
import { TranslationService, Language } from '../../services/translation.service';

@Component({
  standalone: false,
  selector: 'app-language-switcher',
  template: `
    <button mat-icon-button
            [matMenuTriggerFor]="languageMenu"
            matTooltip="{{ 'language.switchLanguage' | translate }}">
      <mat-icon>language</mat-icon>
    </button>

    <mat-menu #languageMenu="matMenu">
      <button mat-menu-item
              *ngFor="let lang of availableLanguages"
              (click)="switchLanguage(lang.code)"
              [class.active]="lang.code === currentLanguage">
        <span class="language-flag">{{ lang.flag }}</span>
        <span class="language-name">{{ lang.nativeName }}</span>
        <mat-icon *ngIf="lang.code === currentLanguage" class="check-icon">check</mat-icon>
      </button>
    </mat-menu>
  `,
  styles: [`
    .language-flag {
      margin-right: 8px;
      font-size: 20px;
    }

    .language-name {
      margin-right: 8px;
    }

    button.active {
      background-color: rgba(0, 0, 0, 0.04);
      font-weight: 500;
    }

    .check-icon {
      margin-left: auto;
      color: #4caf50;
    }

    ::ng-deep .mat-mdc-menu-item {
      min-width: 180px;
    }
  `]
})
export class LanguageSwitcherComponent implements OnInit {
  availableLanguages: Language[] = [];
  currentLanguage: string = 'en';

  constructor(public translationService: TranslationService) {}

  ngOnInit(): void {
    this.availableLanguages = this.translationService.getAvailableLanguages();
    this.currentLanguage = this.translationService.getCurrentLanguage();

    // Subscribe to language changes
    this.translationService.currentLanguage$.subscribe(lang => {
      this.currentLanguage = lang;
    });
  }

  switchLanguage(languageCode: string): void {
    this.translationService.setLanguage(languageCode);
  }
}
