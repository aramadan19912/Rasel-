import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-employees',
  template: '<div class="admin-container"><h2>{{ "admin.employees.title" | translate }}</h2><p>{{ "admin.employees.comingSoon" | translate }}</p></div>',
  styles: ['.admin-container { padding: 24px; }']
})
export class EmployeesComponent {}
