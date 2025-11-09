import { Component, Input, Output, EventEmitter, ViewChild, ElementRef, AfterViewChecked, OnChanges } from '@angular/core';
import { ChatMessage } from '../../../models/video-conference.model';

@Component({
  selector: 'app-chat-panel',
  templateUrl: './chat-panel.component.html',
  styleUrls: ['./chat-panel.component.scss']
})
export class ChatPanelComponent implements AfterViewChecked, OnChanges {
  @ViewChild('messagesContainer') messagesContainer!: ElementRef;

  @Input() messages: ChatMessage[] = [];
  @Input() currentUserId: string = '';

  @Output() sendMessage = new EventEmitter<string>();

  messageText: string = '';
  private shouldScroll = false;

  ngOnChanges(): void {
    // Scroll to bottom when new messages arrive
    this.shouldScroll = true;
  }

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollToBottom();
      this.shouldScroll = false;
    }
  }

  onSendMessage(): void {
    if (!this.messageText.trim()) return;

    this.sendMessage.emit(this.messageText);
    this.messageText = '';
    this.shouldScroll = true;
  }

  onKeyPress(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.onSendMessage();
    }
  }

  isOwnMessage(message: ChatMessage): boolean {
    return message.senderId === this.currentUserId;
  }

  formatTime(date: Date): string {
    return new Date(date).toLocaleTimeString('en-US', {
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  private scrollToBottom(): void {
    try {
      this.messagesContainer.nativeElement.scrollTop =
        this.messagesContainer.nativeElement.scrollHeight;
    } catch (err) {
      console.error('Scroll to bottom failed:', err);
    }
  }
}
