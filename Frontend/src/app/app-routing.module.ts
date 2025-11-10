import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

// Layout
import { MainLayoutComponent } from './components/layout/main-layout/main-layout.component';

// Auth Components
import { LoginComponent } from './components/auth/login/login.component';
import { RegisterComponent } from './components/auth/register/register.component';
import { UnauthorizedComponent } from './components/auth/unauthorized/unauthorized.component';

// Feature Components
import { InboxComponent } from './components/inbox/inbox.component';
import { CalendarComponent } from './components/calendar/calendar.component';
import { ContactsComponent } from './components/contacts/contacts.component';
import { VideoConferenceComponent } from './components/video-conference/video-conference.component';

// Guards
import { AuthGuard } from './guards/auth.guard';
import { PermissionGuard } from './guards/permission.guard';

const routes: Routes = [
  // Public routes
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'register',
    component: RegisterComponent
  },
  {
    path: 'unauthorized',
    component: UnauthorizedComponent
  },

  // Protected routes with main layout
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'inbox',
        pathMatch: 'full'
      },
      {
        path: 'inbox',
        component: InboxComponent,
        canActivate: [PermissionGuard],
        data: {
          permissions: ['messages.read'],
          requireAllPermissions: false
        }
      },
      {
        path: 'calendar',
        component: CalendarComponent,
        canActivate: [PermissionGuard],
        data: {
          permissions: ['calendar.read'],
          requireAllPermissions: false
        }
      },
      {
        path: 'contacts',
        component: ContactsComponent,
        canActivate: [PermissionGuard],
        data: {
          permissions: ['contacts.read'],
          requireAllPermissions: false
        }
      },
      {
        path: 'video-conference',
        component: VideoConferenceComponent,
        canActivate: [PermissionGuard],
        data: {
          permissions: ['videoconference.read', 'videoconference.join'],
          requireAllPermissions: false
        }
      },
      {
        path: 'profile',
        loadChildren: () => import('./components/profile/profile.module').then(m => m.ProfileModule)
      },
      {
        path: 'settings',
        loadChildren: () => import('./components/settings/settings.module').then(m => m.SettingsModule)
      }
    ]
  },

  // Fallback route
  {
    path: '**',
    redirectTo: 'inbox'
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
