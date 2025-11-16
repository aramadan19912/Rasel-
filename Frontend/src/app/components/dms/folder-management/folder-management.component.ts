import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { DmsService } from '../../../services/dms.service';
import {
  CreateFolderDto,
  DocumentFolder,
  FolderAccessLevel
} from '../../../models/dms.models';

@Component({
  selector: 'app-folder-management',
  templateUrl: './folder-management.component.html',
  styleUrls: ['./folder-management.component.scss']
})
export class FolderManagementComponent implements OnInit {
  @Input() parentFolderId?: number;
  @Input() folder?: DocumentFolder; // For editing existing folder
  @Output() created = new EventEmitter<DocumentFolder>();
  @Output() updated = new EventEmitter<DocumentFolder>();
  @Output() cancelled = new EventEmitter<void>();

  isEditMode: boolean = false;
  saving: boolean = false;

  // Form fields
  name: string = '';
  description: string = '';
  accessLevel: FolderAccessLevel = FolderAccessLevel.Internal;
  allowedUsers: string[] = [];
  allowedRoles: string[] = [];

  userInput: string = '';
  roleInput: string = '';

  // Enums for template
  accessLevels = Object.values(FolderAccessLevel);

  constructor(private dmsService: DmsService) {}

  ngOnInit(): void {
    if (this.folder) {
      // Edit mode
      this.isEditMode = true;
      this.name = this.folder.name;
      this.description = this.folder.description || '';
      this.accessLevel = this.folder.accessLevel;
      this.allowedUsers = this.folder.allowedUsers || [];
      this.allowedRoles = this.folder.allowedRoles || [];
    }
  }

  addUser(): void {
    if (this.userInput.trim() && !this.allowedUsers.includes(this.userInput.trim())) {
      this.allowedUsers.push(this.userInput.trim());
      this.userInput = '';
    }
  }

  removeUser(user: string): void {
    this.allowedUsers = this.allowedUsers.filter(u => u !== user);
  }

  addRole(): void {
    if (this.roleInput.trim() && !this.allowedRoles.includes(this.roleInput.trim())) {
      this.allowedRoles.push(this.roleInput.trim());
      this.roleInput = '';
    }
  }

  removeRole(role: string): void {
    this.allowedRoles = this.allowedRoles.filter(r => r !== role);
  }

  async save(): Promise<void> {
    if (!this.name.trim()) {
      alert('Folder name is required');
      return;
    }

    this.saving = true;

    try {
      const folderDto: CreateFolderDto = {
        name: this.name.trim(),
        description: this.description.trim() || undefined,
        parentFolderId: this.parentFolderId,
        accessLevel: this.accessLevel,
        allowedUsers: this.allowedUsers.length > 0 ? this.allowedUsers : undefined,
        allowedRoles: this.allowedRoles.length > 0 ? this.allowedRoles : undefined
      };

      if (this.isEditMode && this.folder) {
        const updatedFolder = await this.dmsService.updateFolder(this.folder.id, folderDto).toPromise();
        this.updated.emit(updatedFolder);
      } else {
        const newFolder = await this.dmsService.createFolder(folderDto).toPromise();
        this.created.emit(newFolder!);
      }
    } catch (error) {
      console.error('Error saving folder:', error);
      alert('Failed to save folder. Please try again.');
    } finally {
      this.saving = false;
    }
  }

  cancel(): void {
    this.cancelled.emit();
  }
}
