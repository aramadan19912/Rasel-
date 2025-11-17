import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { DmsService } from '../../../services/dms.service';
import { DocumentActivity, DocumentActivityType } from '../../../models/dms.models';

@Component({
  standalone: false,
  selector: 'app-document-activity',
  templateUrl: './document-activity.component.html',
  styleUrls: ['./document-activity.component.scss']
})
export class DocumentActivityComponent implements OnInit, OnDestroy {
  @Input() documentId!: number;

  private destroy$ = new Subject<void>();

  activities: DocumentActivity[] = [];
  loading = false;
  error: string | null = null;

  activityTypes = DocumentActivityType;

  constructor(private dmsService: DmsService) {}

  ngOnInit(): void {
    if (this.documentId) {
      this.loadActivities();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadActivities(): void {
    this.loading = true;
    this.error = null;

    this.dmsService.getActivity(this.documentId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (activities: DocumentActivity[]) => {
          this.activities = activities;
          this.loading = false;
        },
        error: (err: any) => {
          this.error = 'Failed to load activity log';
          this.loading = false;
          console.error('Error loading activities:', err);
        }
      });
  }

  getActivityIcon(type: DocumentActivityType): string {
    const icons: { [key in DocumentActivityType]: string } = {
      [DocumentActivityType.Created]: 'add_circle',
      [DocumentActivityType.Viewed]: 'visibility',
      [DocumentActivityType.Downloaded]: 'download',
      [DocumentActivityType.Edited]: 'edit',
      [DocumentActivityType.Deleted]: 'delete',
      [DocumentActivityType.Shared]: 'share',
      [DocumentActivityType.CommentAdded]: 'comment',
      [DocumentActivityType.VersionCreated]: 'update',
      [DocumentActivityType.Restored]: 'restore',
      [DocumentActivityType.Locked]: 'lock',
      [DocumentActivityType.Unlocked]: 'lock_open',
      [DocumentActivityType.PermissionsChanged]: 'security',
      [DocumentActivityType.Moved]: 'drive_file_move',
      [DocumentActivityType.Renamed]: 'text_format',
      [DocumentActivityType.MetadataChanged]: 'info',
      [DocumentActivityType.Annotated]: 'annotation'
    };
    return icons[type] || 'circle';
  }

  getActivityColor(type: DocumentActivityType): string {
    const colors: { [key in DocumentActivityType]: string } = {
      [DocumentActivityType.Created]: '#4caf50',
      [DocumentActivityType.Viewed]: '#2196f3',
      [DocumentActivityType.Downloaded]: '#00bcd4',
      [DocumentActivityType.Edited]: '#ff9800',
      [DocumentActivityType.Deleted]: '#f44336',
      [DocumentActivityType.Shared]: '#9c27b0',
      [DocumentActivityType.CommentAdded]: '#3f51b5',
      [DocumentActivityType.VersionCreated]: '#4caf50',
      [DocumentActivityType.Restored]: '#ff9800',
      [DocumentActivityType.Locked]: '#f44336',
      [DocumentActivityType.Unlocked]: '#4caf50',
      [DocumentActivityType.PermissionsChanged]: '#ff5722',
      [DocumentActivityType.Moved]: '#795548',
      [DocumentActivityType.Renamed]: '#607d8b',
      [DocumentActivityType.MetadataChanged]: '#9e9e9e',
      [DocumentActivityType.Annotated]: '#673ab7'
    };
    return colors[type] || '#757575';
  }

  getActivityLabel(type: DocumentActivityType): string {
    const labels: { [key in DocumentActivityType]: string } = {
      [DocumentActivityType.Created]: 'Created',
      [DocumentActivityType.Viewed]: 'Viewed',
      [DocumentActivityType.Downloaded]: 'Downloaded',
      [DocumentActivityType.Edited]: 'Edited',
      [DocumentActivityType.Deleted]: 'Deleted',
      [DocumentActivityType.Shared]: 'Shared',
      [DocumentActivityType.CommentAdded]: 'Comment Added',
      [DocumentActivityType.VersionCreated]: 'New Version',
      [DocumentActivityType.Restored]: 'Restored',
      [DocumentActivityType.Locked]: 'Locked',
      [DocumentActivityType.Unlocked]: 'Unlocked',
      [DocumentActivityType.PermissionsChanged]: 'Permissions Changed',
      [DocumentActivityType.Moved]: 'Moved',
      [DocumentActivityType.Renamed]: 'Renamed',
      [DocumentActivityType.MetadataChanged]: 'Metadata Changed',
      [DocumentActivityType.Annotated]: 'Annotated'
    };
    return labels[type] || 'Activity';
  }

  formatDate(date: Date | string): string {
    const d = new Date(date);
    const now = new Date();
    const diffMs = now.getTime() - d.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    if (diffDays < 7) return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;

    return d.toLocaleString();
  }
}
