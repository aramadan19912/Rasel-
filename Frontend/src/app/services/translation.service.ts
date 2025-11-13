import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Language {
  code: string;
  name: string;
  nativeName: string;
  dir: 'ltr' | 'rtl';
  flag: string;
}

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private currentLanguageSubject = new BehaviorSubject<string>('en');
  public currentLanguage$ = this.currentLanguageSubject.asObservable();

  private currentDirectionSubject = new BehaviorSubject<'ltr' | 'rtl'>('ltr');
  public currentDirection$ = this.currentDirectionSubject.asObservable();

  // Available languages
  public readonly languages: Language[] = [
    {
      code: 'en',
      name: 'English',
      nativeName: 'English',
      dir: 'ltr',
      flag: '🇺🇸'
    },
    {
      code: 'ar',
      name: 'Arabic',
      nativeName: 'العربية',
      dir: 'rtl',
      flag: '🇸🇦'
    }
  ];

  // RTL languages list
  private readonly rtlLanguages = ['ar', 'he', 'fa', 'ur'];

  constructor(private translate: TranslateService) {
    this.initializeLanguage();
  }

  /**
   * Initialize the translation service
   */
  private initializeLanguage(): void {
    // Set available languages
    const availableLanguages = this.languages.map(lang => lang.code);
    this.translate.addLangs(availableLanguages);

    // Get saved language from localStorage or use browser language
    const savedLanguage = this.getSavedLanguage();
    const browserLanguage = this.getBrowserLanguage();
    const defaultLanguage = savedLanguage || browserLanguage || 'en';

    // Set default language
    this.translate.setDefaultLang('en');

    // Use the determined language
    this.setLanguage(defaultLanguage);
  }

  /**
   * Get saved language from localStorage
   */
  private getSavedLanguage(): string | null {
    return localStorage.getItem('preferredLanguage');
  }

  /**
   * Get browser language
   */
  private getBrowserLanguage(): string {
    const browserLang = this.translate.getBrowserLang();
    const supportedLanguages = this.languages.map(l => l.code);

    if (browserLang && supportedLanguages.includes(browserLang)) {
      return browserLang;
    }

    return 'en';
  }

  /**
   * Set the current language
   */
  public setLanguage(languageCode: string): void {
    const language = this.languages.find(lang => lang.code === languageCode);

    if (!language) {
      console.error(`Language ${languageCode} not supported`);
      return;
    }

    // Use the translation
    this.translate.use(languageCode).subscribe(() => {
      // Update current language
      this.currentLanguageSubject.next(languageCode);

      // Update direction
      this.currentDirectionSubject.next(language.dir);

      // Save to localStorage
      localStorage.setItem('preferredLanguage', languageCode);

      // Update HTML attributes
      this.updateHtmlAttributes(language);

      // Update Material Design direction
      this.updateMaterialDirection(language.dir);

      console.log(`Language changed to: ${language.nativeName} (${languageCode})`);
    });
  }

  /**
   * Update HTML element attributes for RTL/LTR
   */
  private updateHtmlAttributes(language: Language): void {
    const htmlElement = document.documentElement;

    // Set direction
    htmlElement.setAttribute('dir', language.dir);

    // Set language
    htmlElement.setAttribute('lang', language.code);

    // Add/remove RTL class
    if (language.dir === 'rtl') {
      htmlElement.classList.add('rtl');
      htmlElement.classList.remove('ltr');
    } else {
      htmlElement.classList.add('ltr');
      htmlElement.classList.remove('rtl');
    }
  }

  /**
   * Update Angular Material direction
   */
  private updateMaterialDirection(dir: 'ltr' | 'rtl'): void {
    const body = document.body;

    if (dir === 'rtl') {
      body.classList.add('mat-app-background', 'rtl');
      body.classList.remove('ltr');
    } else {
      body.classList.add('mat-app-background', 'ltr');
      body.classList.remove('rtl');
    }
  }

  /**
   * Get the current language code
   */
  public getCurrentLanguage(): string {
    return this.currentLanguageSubject.value;
  }

  /**
   * Get the current direction
   */
  public getCurrentDirection(): 'ltr' | 'rtl' {
    return this.currentDirectionSubject.value;
  }

  /**
   * Check if current language is RTL
   */
  public isRtl(): boolean {
    return this.getCurrentDirection() === 'rtl';
  }

  /**
   * Check if a specific language is RTL
   */
  public isLanguageRtl(languageCode: string): boolean {
    return this.rtlLanguages.includes(languageCode);
  }

  /**
   * Get language by code
   */
  public getLanguage(code: string): Language | undefined {
    return this.languages.find(lang => lang.code === code);
  }

  /**
   * Get current language object
   */
  public getCurrentLanguageObject(): Language {
    const currentLang = this.getCurrentLanguage();
    return this.getLanguage(currentLang) || this.languages[0];
  }

  /**
   * Translate a key instantly (synchronous)
   */
  public instant(key: string | string[], interpolateParams?: Object): string | any {
    return this.translate.instant(key, interpolateParams);
  }

  /**
   * Translate a key (Observable)
   */
  public get(key: string | string[], interpolateParams?: Object): Observable<string | any> {
    return this.translate.get(key, interpolateParams);
  }

  /**
   * Stream translation changes
   */
  public stream(key: string | string[], interpolateParams?: Object): Observable<string | any> {
    return this.translate.stream(key, interpolateParams);
  }

  /**
   * Get available languages
   */
  public getAvailableLanguages(): Language[] {
    return this.languages;
  }

  /**
   * Switch to next available language
   */
  public switchToNextLanguage(): void {
    const currentIndex = this.languages.findIndex(
      lang => lang.code === this.getCurrentLanguage()
    );
    const nextIndex = (currentIndex + 1) % this.languages.length;
    this.setLanguage(this.languages[nextIndex].code);
  }

  /**
   * Format date according to current locale
   */
  public formatDate(date: Date | string, format: string = 'medium'): string {
    const currentLang = this.getCurrentLanguage();
    // You can use Angular's DatePipe or a library like date-fns here
    return new Date(date).toLocaleDateString(
      currentLang === 'ar' ? 'ar-SA' : 'en-US'
    );
  }

  /**
   * Format number according to current locale
   */
  public formatNumber(num: number): string {
    const currentLang = this.getCurrentLanguage();
    return num.toLocaleString(currentLang === 'ar' ? 'ar-SA' : 'en-US');
  }

  /**
   * Get text alignment based on direction
   */
  public getTextAlign(): 'left' | 'right' {
    return this.isRtl() ? 'right' : 'left';
  }

  /**
   * Get flex direction for RTL
   */
  public getFlexDirection(reverse: boolean = false): 'row' | 'row-reverse' {
    const isRtl = this.isRtl();
    if (reverse) {
      return isRtl ? 'row' : 'row-reverse';
    }
    return isRtl ? 'row-reverse' : 'row';
  }

  /**
   * Get margin/padding direction
   */
  public getStartEnd(start: string, end: string): { [key: string]: string } {
    if (this.isRtl()) {
      return {
        'margin-right': start,
        'margin-left': end
      };
    } else {
      return {
        'margin-left': start,
        'margin-right': end
      };
    }
  }
}
