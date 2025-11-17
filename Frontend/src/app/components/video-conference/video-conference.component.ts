import { Component, OnInit, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { VideoConferenceService } from '../../services/video-conference.service';
import {
  VideoConference,
  Participant,
  JoinConferenceDto,
  LocalMediaState,
  ChatMessage,
  PeerConnection
} from '../../models/video-conference.model';

@Component({
  standalone: false,
  selector: 'app-video-conference',
  templateUrl: './video-conference.component.html',
  styleUrls: ['./video-conference.component.scss']
})
export class VideoConferenceComponent implements OnInit, OnDestroy {
  @ViewChild('localVideo') localVideo!: ElementRef<HTMLVideoElement>;

  private destroy$ = new Subject<void>();

  // State
  conference: VideoConference | null = null;
  participants: Participant[] = [];
  localMediaState: LocalMediaState = {
    isAudioMuted: false,
    isVideoOff: false,
    isScreenSharing: false,
    isHandRaised: false
  };
  chatMessages: ChatMessage[] = [];
  peerConnections: Map<string, PeerConnection> = new Map();

  // Current participant
  currentParticipant?: Participant;
  conferenceId: string = '';
  displayName: string = '';

  // UI State
  isChatOpen = false;
  isParticipantsOpen = true;
  isFullScreen = false;
  showControls = true;
  connectionQuality: string = 'Good';

  // Layout
  gridLayout: 'grid' | 'speaker' | 'sidebar' = 'grid';
  screenSharer: Participant | null = null;

  // Loading state
  isJoining = false;
  isConnecting = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private videoConferenceService: VideoConferenceService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  async ngOnInit(): Promise<void> {
    this.conferenceId = this.route.snapshot.params['conferenceId'];

    if (!this.conferenceId) {
      this.showError('Invalid conference ID');
      this.router.navigate(['/']);
      return;
    }

    await this.joinConference();
    this.setupSubscriptions();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.leaveConference();
  }

  private async joinConference(): Promise<void> {
    try {
      this.isJoining = true;

      // Get local media stream first
      const stream = await this.videoConferenceService.getLocalMediaStream();

      if (this.localVideo) {
        this.localVideo.nativeElement.srcObject = stream;
        this.localVideo.nativeElement.muted = true; // Mute own audio
      }

      // Join conference via API
      const joinDto: JoinConferenceDto = {
        conferenceId: this.conferenceId,
        displayName: this.displayName || 'Guest User',
        email: '',
        isGuest: true
      };

      this.currentParticipant = await this.videoConferenceService.joinConference(joinDto).toPromise();

      // Connect to SignalR hub
      const accessToken = localStorage.getItem('accessToken') || '';
      await this.videoConferenceService.connectToHub(this.conferenceId, accessToken);

      // Get conference details
      this.videoConferenceService.getConference(this.conferenceId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(conference => {
          this.conference = conference;
        });

      // Get participants
      this.videoConferenceService.getParticipants(this.conferenceId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(participants => {
          this.participants = participants;
          this.setupPeerConnections(participants);
        });

      // Get chat history
      this.videoConferenceService.getChatHistory(this.conferenceId)
        .pipe(takeUntil(this.destroy$))
        .subscribe(messages => {
          this.chatMessages = messages;
        });

      this.isJoining = false;
      this.showSuccess('Joined conference successfully');

    } catch (error) {
      this.isJoining = false;
      console.error('Failed to join conference:', error);
      this.showError('Failed to join conference');
      this.router.navigate(['/']);
    }
  }

  private setupSubscriptions(): void {
    // Conference state
    this.videoConferenceService.currentConference$
      .pipe(takeUntil(this.destroy$))
      .subscribe(conference => {
        this.conference = conference;
      });

    // Participants
    this.videoConferenceService.participants$
      .pipe(takeUntil(this.destroy$))
      .subscribe(participants => {
        this.participants = participants;
      });

    // Local media state
    this.videoConferenceService.localMediaState$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.localMediaState = state;
      });

    // Peer connections
    this.videoConferenceService.peerConnections$
      .pipe(takeUntil(this.destroy$))
      .subscribe(connections => {
        this.peerConnections = connections;
      });

    // Chat messages
    this.videoConferenceService.chatMessages$
      .pipe(takeUntil(this.destroy$))
      .subscribe(messages => {
        this.chatMessages = messages;
      });

    // Screen sharer
    this.videoConferenceService.screenSharer$
      .pipe(takeUntil(this.destroy$))
      .subscribe(sharer => {
        this.screenSharer = sharer;
        if (sharer) {
          this.gridLayout = 'speaker';
        }
      });

    // SignalR events
    this.videoConferenceService.signalREvents
      .pipe(takeUntil(this.destroy$))
      .subscribe(event => {
        this.handleSignalREvent(event);
      });
  }

  private async setupPeerConnections(participants: Participant[]): Promise<void> {
    for (const participant of participants) {
      if (participant.peerId === this.currentParticipant?.peerId) {
        continue; // Skip self
      }

      const peerConnection = this.videoConferenceService.createPeerConnection(
        participant.peerId,
        participant.displayName,
        participant.isHost
      );

      // Handle incoming remote stream
      peerConnection.ontrack = (event) => {
        const [remoteStream] = event.streams;
        const peerConn = this.videoConferenceService.getPeerConnection(participant.peerId);
        if (peerConn) {
          peerConn.remoteStream = remoteStream;
        }
      };

      // Handle ICE candidates
      peerConnection.onicecandidate = (event) => {
        if (event.candidate) {
          this.videoConferenceService.sendIceCandidate(participant.peerId, event.candidate);
        }
      };

      // Create and send offer
      const offer = await peerConnection.createOffer();
      await peerConnection.setLocalDescription(offer);
      await this.videoConferenceService.sendOffer(participant.peerId, offer);
    }
  }

  private handleSignalREvent(event: { event: string; data: any }): void {
    switch (event.event) {
      case 'onParticipantJoined':
        this.handleParticipantJoined(event.data);
        break;
      case 'onParticipantLeft':
        this.showInfo(`${event.data.displayName} left the conference`);
        break;
      case 'onReceiveOffer':
        this.handleReceiveOffer(event.data.peerId, event.data.offer);
        break;
      case 'onReceiveAnswer':
        this.handleReceiveAnswer(event.data.peerId, event.data.answer);
        break;
      case 'onReceiveIceCandidate':
        this.handleReceiveIceCandidate(event.data.peerId, event.data.candidate);
        break;
      case 'onMutedByHost':
        this.showWarning('You have been muted by the host');
        break;
      case 'onRemovedFromConference':
        this.showError('You have been removed from the conference');
        this.router.navigate(['/']);
        break;
      case 'onConferenceEnded':
        this.showInfo('Conference has ended');
        this.router.navigate(['/']);
        break;
      case 'onScreenShareStarted':
        this.gridLayout = 'speaker';
        break;
      case 'onScreenShareStopped':
        this.gridLayout = 'grid';
        break;
      case 'onRecordingStarted':
        this.showInfo('Recording started');
        break;
      case 'onRecordingStopped':
        this.showInfo('Recording stopped');
        break;
    }
  }

  private async handleParticipantJoined(participant: Participant): Promise<void> {
    this.showInfo(`${participant.displayName} joined the conference`);

    // Create peer connection for new participant
    const peerConnection = this.videoConferenceService.createPeerConnection(
      participant.peerId,
      participant.displayName,
      participant.isHost
    );

    peerConnection.ontrack = (event) => {
      const [remoteStream] = event.streams;
      const peerConn = this.videoConferenceService.getPeerConnection(participant.peerId);
      if (peerConn) {
        peerConn.remoteStream = remoteStream;
      }
    };

    peerConnection.onicecandidate = (event) => {
      if (event.candidate) {
        this.videoConferenceService.sendIceCandidate(participant.peerId, event.candidate);
      }
    };
  }

  private async handleReceiveOffer(peerId: string, offerJson: string): Promise<void> {
    const offer = JSON.parse(offerJson) as RTCSessionDescriptionInit;
    const peerConn = this.videoConferenceService.getPeerConnection(peerId);

    if (!peerConn) return;

    await peerConn.connection.setRemoteDescription(new RTCSessionDescription(offer));
    const answer = await peerConn.connection.createAnswer();
    await peerConn.connection.setLocalDescription(answer);
    await this.videoConferenceService.sendAnswer(peerId, answer);
  }

  private async handleReceiveAnswer(peerId: string, answerJson: string): Promise<void> {
    const answer = JSON.parse(answerJson) as RTCSessionDescriptionInit;
    const peerConn = this.videoConferenceService.getPeerConnection(peerId);

    if (!peerConn) return;

    await peerConn.connection.setRemoteDescription(new RTCSessionDescription(answer));
  }

  private async handleReceiveIceCandidate(peerId: string, candidateJson: string): Promise<void> {
    const candidate = JSON.parse(candidateJson) as RTCIceCandidateInit;
    const peerConn = this.videoConferenceService.getPeerConnection(peerId);

    if (!peerConn) return;

    await peerConn.connection.addIceCandidate(new RTCIceCandidate(candidate));
  }

  // ========== Media Controls ==========

  async toggleAudio(): Promise<void> {
    await this.videoConferenceService.toggleAudio(this.conferenceId);
  }

  async toggleVideo(): Promise<void> {
    await this.videoConferenceService.toggleVideo(this.conferenceId);
  }

  async toggleScreenShare(): Promise<void> {
    if (this.localMediaState.isScreenSharing) {
      await this.videoConferenceService.stopScreenShareViaHub(this.conferenceId);
    } else {
      try {
        await this.videoConferenceService.startScreenShareViaHub(this.conferenceId);
      } catch (error) {
        this.showError('Failed to start screen sharing');
      }
    }
  }

  async toggleHandRaise(): Promise<void> {
    if (this.localMediaState.isHandRaised) {
      await this.videoConferenceService.lowerHand(this.conferenceId);
    } else {
      await this.videoConferenceService.raiseHand(this.conferenceId);
    }
  }

  async leaveConference(): Promise<void> {
    if (!this.conferenceId) return;

    try {
      await this.videoConferenceService.leaveConference(this.conferenceId).toPromise();
      this.videoConferenceService.cleanup();
      this.router.navigate(['/']);
    } catch (error) {
      console.error('Error leaving conference:', error);
      this.videoConferenceService.cleanup();
      this.router.navigate(['/']);
    }
  }

  // ========== UI Controls ==========

  toggleChat(): void {
    this.isChatOpen = !this.isChatOpen;
  }

  toggleParticipants(): void {
    this.isParticipantsOpen = !this.isParticipantsOpen;
  }

  toggleFullScreen(): void {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen();
      this.isFullScreen = true;
    } else {
      document.exitFullscreen();
      this.isFullScreen = false;
    }
  }

  changeLayout(layout: 'grid' | 'speaker' | 'sidebar'): void {
    this.gridLayout = layout;
  }

  openSettings(): void {
    // Open settings dialog
  }

  // ========== Chat ==========

  async sendMessage(message: string): Promise<void> {
    if (!message.trim()) return;

    try {
      await this.videoConferenceService.sendChatViaHub(this.conferenceId, message);
    } catch (error) {
      this.showError('Failed to send message');
    }
  }

  // ========== Host Controls ==========

  get isHost(): boolean {
    return this.currentParticipant?.isHost || false;
  }

  get isCoHost(): boolean {
    return this.currentParticipant?.isCoHost || false;
  }

  async muteAll(): Promise<void> {
    if (!this.isHost && !this.isCoHost) return;

    try {
      await this.videoConferenceService.muteAll(this.conferenceId).toPromise();
      this.showSuccess('All participants muted');
    } catch (error) {
      this.showError('Failed to mute all participants');
    }
  }

  async endConference(): Promise<void> {
    if (!this.isHost) return;

    if (confirm('Are you sure you want to end this conference for everyone?')) {
      try {
        await this.videoConferenceService.endConference(this.conferenceId).toPromise();
        this.showInfo('Conference ended');
        this.router.navigate(['/']);
      } catch (error) {
        this.showError('Failed to end conference');
      }
    }
  }

  async startRecording(): Promise<void> {
    if (!this.isHost && !this.currentParticipant?.canRecord) return;

    try {
      await this.videoConferenceService.startRecording(this.conferenceId).toPromise();
      this.showSuccess('Recording started');
    } catch (error) {
      this.showError('Failed to start recording');
    }
  }

  async stopRecording(): Promise<void> {
    if (!this.isHost && !this.currentParticipant?.canRecord) return;

    try {
      await this.videoConferenceService.stopRecording(this.conferenceId).toPromise();
      this.showSuccess('Recording stopped');
    } catch (error) {
      this.showError('Failed to stop recording');
    }
  }

  async onMuteParticipant(participantId: string): Promise<void> {
    if (!this.isHost && !this.isCoHost) return;

    try {
      await this.videoConferenceService.muteParticipant(this.conferenceId, participantId).toPromise();
      this.showSuccess('Participant muted');
    } catch (error) {
      this.showError('Failed to mute participant');
    }
  }

  async onRemoveParticipant(participantId: string): Promise<void> {
    if (!this.isHost && !this.isCoHost) return;

    if (confirm('Are you sure you want to remove this participant?')) {
      try {
        await this.videoConferenceService.removeParticipant(this.conferenceId, participantId).toPromise();
        this.showSuccess('Participant removed');
      } catch (error) {
        this.showError('Failed to remove participant');
      }
    }
  }

  async onMakeCoHost(participantId: string): Promise<void> {
    if (!this.isHost) return;

    try {
      await this.videoConferenceService.makeCoHost(this.conferenceId, participantId).toPromise();
      this.showSuccess('Participant promoted to co-host');
    } catch (error) {
      this.showError('Failed to make participant co-host');
    }
  }

  // ========== Utility Methods ==========

  getParticipantVideoElement(peerId: string): HTMLVideoElement | null {
    return document.getElementById(`video-${peerId}`) as HTMLVideoElement;
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['snackbar-success']
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 5000,
      panelClass: ['snackbar-error']
    });
  }

  private showWarning(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 4000,
      panelClass: ['snackbar-warning']
    });
  }

  private showInfo(message: string): void {
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      panelClass: ['snackbar-info']
    });
  }
}
