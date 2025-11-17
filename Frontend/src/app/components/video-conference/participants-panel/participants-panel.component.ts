import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Participant } from '../../../models/video-conference.model';

@Component({
  standalone: false,
  selector: 'app-participants-panel',
  templateUrl: './participants-panel.component.html',
  styleUrls: ['./participants-panel.component.scss']
})
export class ParticipantsPanelComponent {
  @Input() participants: Participant[] = [];
  @Input() currentParticipant?: Participant;
  @Input() isHost = false;
  @Input() isCoHost = false;

  @Output() muteParticipant = new EventEmitter<string>();
  @Output() removeParticipant = new EventEmitter<string>();
  @Output() makeCoHost = new EventEmitter<string>();

  searchText = '';

  get filteredParticipants(): Participant[] {
    if (!this.searchText.trim()) {
      return this.participants;
    }

    const search = this.searchText.toLowerCase();
    return this.participants.filter(p =>
      p.displayName.toLowerCase().includes(search) ||
      p.email?.toLowerCase().includes(search)
    );
  }

  get hostParticipants(): Participant[] {
    return this.filteredParticipants.filter(p => p.isHost);
  }

  get coHostParticipants(): Participant[] {
    return this.filteredParticipants.filter(p => p.isCoHost && !p.isHost);
  }

  get regularParticipants(): Participant[] {
    return this.filteredParticipants.filter(p => !p.isHost && !p.isCoHost);
  }

  onMuteParticipant(participant: Participant): void {
    if (!this.canModerateParticipant(participant)) return;
    this.muteParticipant.emit(participant.id.toString());
  }

  onRemoveParticipant(participant: Participant): void {
    if (!this.canModerateParticipant(participant)) return;

    if (confirm(`Remove ${participant.displayName} from the conference?`)) {
      this.removeParticipant.emit(participant.id.toString());
    }
  }

  onMakeCoHost(participant: Participant): void {
    if (!this.isHost) return;

    if (confirm(`Make ${participant.displayName} a co-host?`)) {
      this.makeCoHost.emit(participant.id.toString());
    }
  }

  canModerateParticipant(participant: Participant): boolean {
    if (participant.id === this.currentParticipant?.id) return false;
    if (participant.isHost) return false;
    return this.isHost || this.isCoHost;
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }
}
