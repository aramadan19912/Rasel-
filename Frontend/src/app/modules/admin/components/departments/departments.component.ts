import { Component } from '@angular/core';

@Component({
  selector: 'app-departments',
  template: '<div class="admin-container"><h2>{{ "admin.departments.title" | translate }}</h2><p>{{ "admin.departments.comingSoon" | translate }}</p></div>',
  styles: ['.admin-container { padding: 24px; }']
})
export class DepartmentsComponent {}
