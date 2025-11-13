import { Component, OnInit } from '@angular/core';
import { TranslationService } from './services/translation.service';

@Component({
  selector: 'app-root',
  template: `<router-outlet></router-outlet>`,
  styles: []
})
export class AppComponent implements OnInit {
  title = 'Rasel - Outlook Management System';

  constructor(private translationService: TranslationService) {}

  ngOnInit(): void {
    // TranslationService automatically initializes language on construction
    // The service will:
    // 1. Check localStorage for saved language preference
    // 2. Fall back to browser language if supported
    // 3. Default to English if browser language not supported
    // 4. Set HTML direction (RTL/LTR) automatically
  }
}
