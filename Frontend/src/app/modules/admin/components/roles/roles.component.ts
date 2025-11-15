import { Component } from '@angular/core';

@Component({
  selector: 'app-roles',
  template: '<div class="admin-container"><h2>{{ "admin.roles.title" | translate }}</h2><p>{{ "admin.roles.comingSoon" | translate }}</p></div>',
  styles: ['.admin-container { padding: 24px; }']
})
export class RolesComponent {}
