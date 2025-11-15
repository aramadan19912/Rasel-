import { Component } from '@angular/core';

@Component({
  selector: 'app-users',
  template: '<div class="admin-container"><h2>{{ "admin.users.title" | translate }}</h2><p>{{ "admin.users.comingSoon" | translate }}</p></div>',
  styles: ['.admin-container { padding: 24px; }']
})
export class UsersComponent {}
