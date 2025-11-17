import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService, UserInfo } from '../../../services/auth.service';
import { Observable } from 'rxjs';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  permission?: string;
}

@Component({
  standalone: false,
  selector: 'app-main-layout',
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.css']
})
export class MainLayoutComponent implements OnInit {
  currentUser$: Observable<UserInfo | null>;
  sidenavOpened = true;

  navItems: NavItem[] = [
    { label: 'Inbox', icon: 'inbox', route: '/inbox', permission: 'messages.read' },
    { label: 'Calendar', icon: 'event', route: '/calendar', permission: 'calendar.read' },
    { label: 'Contacts', icon: 'contacts', route: '/contacts', permission: 'contacts.read' },
    { label: 'Video Conference', icon: 'video_call', route: '/video-conference', permission: 'videoconference.read' },
    { label: 'Correspondence', icon: 'mail_outline', route: '/correspondence', permission: 'correspondence.read' },
    { label: 'Documents', icon: 'folder', route: '/dms', permission: 'documents.read' },
    { label: 'Archive', icon: 'archive', route: '/archive', permission: 'archive.manage' },
    { label: 'Admin', icon: 'admin_panel_settings', route: '/admin', permission: 'admin.access' }
  ];

  constructor(
    public authService: AuthService,
    private router: Router
  ) {
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // Check window size for initial sidenav state
    this.sidenavOpened = window.innerWidth > 768;
  }

  hasPermission(permission?: string): boolean {
    if (!permission) return true;
    return this.authService.hasPermission(permission);
  }

  getUserDisplayName(user: UserInfo | null): string {
    if (!user) return '';
    return `${user.firstName} ${user.lastName}`;
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: () => {
        // Even if logout fails on server, clear local data
        this.router.navigate(['/login']);
      }
    });
  }

  toggleSidenav(): void {
    this.sidenavOpened = !this.sidenavOpened;
  }
}
