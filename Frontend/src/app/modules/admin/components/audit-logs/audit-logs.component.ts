import { Component } from '@angular/core';
import { AdminService } from '../../../../services/admin/admin.service';

@Component({
  selector: 'app-audit-logs',
  template: `<div class="admin-container"><h2>{{ 'admin.auditLogs.title' | translate }}</h2><mat-card><mat-card-content><p class="coming-soon">{{ 'admin.auditLogs.comingSoon' | translate }}</p></mat-card-content></mat-card></div>`,
  styles: [`.admin-container{padding:24px}.coming-soon{text-align:center;padding:48px;color:#666}`]
})
export class AuditLogsComponent {
  constructor(private adminService: AdminService) {}
}
