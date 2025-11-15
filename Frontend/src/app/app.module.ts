import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

// Routing
import { AppRoutingModule } from './app-routing.module';

// ngx-translate
import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';

// Interceptors
import { JwtInterceptor } from './interceptors/jwt.interceptor';
import { ErrorInterceptor } from './interceptors/error.interceptor';

// Translation loader factory
export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

// Angular Material Modules
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCardModule } from '@angular/material/card';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTreeModule } from '@angular/material/tree';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatDividerModule } from '@angular/material/divider';
import { MatStepperModule } from '@angular/material/stepper';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTableModule } from '@angular/material/table';
import { ScrollingModule } from '@angular/cdk/scrolling';

// FullCalendar
import { FullCalendarModule } from '@fullcalendar/angular';

// Components
import { AppComponent } from './app.component';
import { InboxComponent } from './components/inbox/inbox.component';
import { CalendarComponent } from './components/calendar/calendar.component';
import { EventDialogComponent } from './components/calendar/event-dialog/event-dialog.component';
import { ContactsComponent } from './components/contacts/contacts.component';
import { ContactDialogComponent } from './components/contacts/contact-dialog/contact-dialog.component';
import { VideoConferenceComponent } from './components/video-conference/video-conference.component';
import { LanguageSwitcherComponent } from './components/language-switcher/language-switcher.component';
import { CorrespondenceDashboardComponent } from './components/correspondence-dashboard/correspondence-dashboard.component';
import { CorrespondenceListComponent } from './components/correspondence-list/correspondence-list.component';
import { CorrespondenceDetailComponent } from './components/correspondence-detail/correspondence-detail.component';
import { CorrespondenceFormComponent } from './components/correspondence-form/correspondence-form.component';
import { CorrespondenceRoutingDialogComponent } from './components/correspondence-routing-dialog/correspondence-routing-dialog.component';
import { ArchiveManagementComponent } from './components/archive-management/archive-management.component';

// Auth Components
import { LoginComponent } from './components/auth/login/login.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { UnauthorizedComponent } from './components/auth/unauthorized/unauthorized.component';

// Layout Components
import { MainLayoutComponent } from './components/layout/main-layout/main-layout.component';

// Services
import { InboxService } from './services/inbox.service';
import { CalendarService } from './services/calendar.service';
import { ContactsService } from './services/contacts.service';
import { VideoConferenceService } from './services/video-conference.service';
import { TranslationService } from './services/translation.service';
import { AuthService } from './services/auth.service';
import { CorrespondenceService } from './services/correspondence.service';
import { ArchiveCategoryService } from './services/archive-category.service';

// Directives
import { HasPermissionDirective } from './directives/has-permission.directive';
import { HasRoleDirective } from './directives/has-role.directive';

@NgModule({
  declarations: [
    AppComponent,
    InboxComponent,
    CalendarComponent,
    EventDialogComponent,
    ContactsComponent,
    ContactDialogComponent,
    VideoConferenceComponent,
    LanguageSwitcherComponent,
    LoginComponent,
    RegisterComponent,
    UnauthorizedComponent,
    MainLayoutComponent,
    CorrespondenceDashboardComponent,
    CorrespondenceListComponent,
    CorrespondenceDetailComponent,
    CorrespondenceFormComponent,
    CorrespondenceRoutingDialogComponent,
    ArchiveManagementComponent,
    HasPermissionDirective,
    HasRoleDirective
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AppRoutingModule,

    // Angular Material
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatListModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatMenuModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCardModule,
    MatBadgeModule,
    MatTreeModule,
    MatTabsModule,
    MatButtonToggleModule,
    MatDividerModule,
    MatStepperModule,
    MatSlideToggleModule,
    MatTableModule,
    ScrollingModule,

    // FullCalendar
    FullCalendarModule,

    // Translation
    TranslateModule.forRoot({
      defaultLanguage: 'en',
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient]
      }
    })
  ],
  providers: [
    // Services
    InboxService,
    CalendarService,
    ContactsService,
    VideoConferenceService,
    TranslationService,
    AuthService,
    CorrespondenceService,
    ArchiveCategoryService,

    // HTTP Interceptors
    {
      provide: HTTP_INTERCEPTORS,
      useClass: JwtInterceptor,
      multi: true
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
