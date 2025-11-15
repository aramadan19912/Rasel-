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
import { CorrespondenceDashboardComponent } from './components/correspondence-dashboard/correspondence-dashboard.component';
import { CorrespondenceListComponent } from './components/correspondence-list/correspondence-list.component';
import { CorrespondenceDetailComponent } from './components/correspondence-detail/correspondence-detail.component';
import { CorrespondenceFormComponent } from './components/correspondence-form/correspondence-form.component';
import { ArchiveManagementComponent } from './components/archive-management/archive-management.component';

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
        path: 'correspondence',
        children: [
          {
            path: '',
            component: CorrespondenceListComponent,
            canActivate: [PermissionGuard],
            data: {
              permissions: ['correspondence.read'],
              requireAllPermissions: false
            }
          },
          {
            path: 'dashboard',
            component: CorrespondenceDashboardComponent,
            canActivate: [PermissionGuard],
            data: {
              permissions: ['correspondence.read'],
              requireAllPermissions: false
            }
          },
          {
            path: 'new',
            component: CorrespondenceFormComponent,
            canActivate: [PermissionGuard],
            data: {
              permissions: ['correspondence.create'],
              requireAllPermissions: false
            }
          },
          {
            path: ':id',
            component: CorrespondenceDetailComponent,
            canActivate: [PermissionGuard],
            data: {
              permissions: ['correspondence.read'],
              requireAllPermissions: false
            }
          },
          {
            path: ':id/edit',
            component: CorrespondenceFormComponent,
            canActivate: [PermissionGuard],
            data: {
              permissions: ['correspondence.update'],
              requireAllPermissions: false
            }
          }
        ]
      },
      {
        path: 'archive',
        component: ArchiveManagementComponent,
        canActivate: [PermissionGuard],
        data: {
          permissions: ['archive.manage'],
          requireAllPermissions: false
        }
      },
      {
        path: 'profile',
        loadChildren: () => import('./modules/profile/profile.module').then(m => m.ProfileModule)
      },
      {
        path: 'settings',
        loadChildren: () => import('./modules/settings/settings.module').then(m => m.SettingsModule)
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
