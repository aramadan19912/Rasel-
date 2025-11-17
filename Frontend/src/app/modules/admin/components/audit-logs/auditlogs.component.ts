import { Component } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-audit-logs',
  template: '<div class="admin-container"><h2>{{ "admin.audit-logs.title" | translate }}</h2><p>{{ "admin.audit-logs.comingSoon" | translate }}</p></div>',
  styles: ['.admin-container { padding: 24px; }']
})
export class AuditLogsComponent {}
