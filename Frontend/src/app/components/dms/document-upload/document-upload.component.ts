import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DmsService } from '../../../services/dms.service';
import {
  CreateDocumentDto,
  DocumentCategory,
  DocumentAccessLevel
} from '../../../models/dms.models';

@Component({
  standalone: false,
  selector: 'app-document-upload',
  templateUrl: './document-upload.component.html',
  styleUrls: ['./document-upload.component.scss']
})
export class DocumentUploadComponent {
  @Input() folderId?: number;
  @Output() uploaded = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  files: File[] = [];
  uploading: boolean = false;
  uploadProgress: number = 0;

  // Form fields
  title: string = '';
  description: string = '';
  category: DocumentCategory = DocumentCategory.General;
  accessLevel: DocumentAccessLevel = DocumentAccessLevel.Internal;
  tags: string[] = [];
  tagInput: string = '';

  // Enums for template
  categories = Object.values(DocumentCategory);
  accessLevels = Object.values(DocumentAccessLevel);

  constructor(private dmsService: DmsService) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.files = Array.from(input.files);
      if (this.files.length === 1 && !this.title) {
        // Auto-fill title from filename
        this.title = this.files[0].name.replace(/\.[^/.]+$/, "");
      }
    }
  }

  onFilesDropped(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();

    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.files = Array.from(event.dataTransfer.files);
      if (this.files.length === 1 && !this.title) {
        this.title = this.files[0].name.replace(/\.[^/.]+$/, "");
      }
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  removeFile(index: number): void {
    this.files.splice(index, 1);
  }

  addTag(): void {
    if (this.tagInput.trim() && !this.tags.includes(this.tagInput.trim())) {
      this.tags.push(this.tagInput.trim());
      this.tagInput = '';
    }
  }

  removeTag(tag: string): void {
    this.tags = this.tags.filter(t => t !== tag);
  }

  async upload(): Promise<void> {
    if (this.files.length === 0) return;

    this.uploading = true;
    this.uploadProgress = 0;

    try {
      if (this.files.length === 1) {
        // Single file upload
        await this.uploadSingleFile(this.files[0]);
      } else {
        // Bulk upload
        await this.uploadMultipleFiles();
      }

      this.uploaded.emit();
    } catch (error) {
      console.error('Upload error:', error);
      alert('Upload failed. Please try again.');
    } finally {
      this.uploading = false;
    }
  }

  private async uploadSingleFile(file: File): Promise<void> {
    const createDto: CreateDocumentDto = {
      title: this.title || file.name,
      description: this.description || undefined,
      category: this.category,
      tags: this.tags,
      accessLevel: this.accessLevel,
      folderId: this.folderId
    };

    await this.dmsService.createDocument(createDto, file).toPromise();
    this.uploadProgress = 100;
  }

  private async uploadMultipleFiles(): Promise<void> {
    const uploaded = await this.dmsService.bulkUpload(this.files, this.folderId).toPromise();
    this.uploadProgress = 100;
  }

  cancel(): void {
    this.cancelled.emit();
  }

  formatFileSize(bytes: number): string {
    return this.dmsService.formatFileSize(bytes);
  }
}
