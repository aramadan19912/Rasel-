import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../environments/environment';
import {
  VideoConference,
  CreateConferenceDto,
  UpdateConferenceDto,
  Participant,
  JoinConferenceDto,
  UpdateParticipantPermissionsDto,
  BreakoutRoom,
  CreateBreakoutRoomsDto,
  ChatMessage,
  SendChatMessageDto,
  WhiteboardData,
  ConferenceAnalytics,
  Recording,
  PeerConnection,
  LocalMediaState,
  ConferenceSettings,
  SignalREvents
} from '../models/video-conference.model';

@Injectable({
  providedIn: 'root'
})
export class VideoConferenceService {
  private apiUrl = `${environment.apiUrl}/api/VideoConference`;
  private hubConnection!: signalR.HubConnection;

  // State management
  private currentConferenceSubject = new BehaviorSubject<VideoConference | null>(null);
  public currentConference$ = this.currentConferenceSubject.asObservable();

  private participantsSubject = new BehaviorSubject<Participant[]>([]);
  public participants$ = this.participantsSubject.asObservable();

  private localMediaStateSubject = new BehaviorSubject<LocalMediaState>({
    isAudioMuted: false,
    isVideoOff: false,
    isScreenSharing: false,
    isHandRaised: false
  });
  public localMediaState$ = this.localMediaStateSubject.asObservable();

  private peerConnectionsSubject = new BehaviorSubject<Map<string, PeerConnection>>(new Map());
  public peerConnections$ = this.peerConnectionsSubject.asObservable();

  private chatMessagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  public chatMessages$ = this.chatMessagesSubject.asObservable();

  private whiteboardDataSubject = new BehaviorSubject<WhiteboardData | null>(null);
  public whiteboardData$ = this.whiteboardDataSubject.asObservable();

  private screenSharerSubject = new BehaviorSubject<Participant | null>(null);
  public screenSharer$ = this.screenSharerSubject.asObservable();

  private isRecordingSubject = new BehaviorSubject<boolean>(false);
  public isRecording$ = this.isRecordingSubject.asObservable();

  private connectionStateSubject = new BehaviorSubject<signalR.HubConnectionState>(signalR.HubConnectionState.Disconnected);
  public connectionState$ = this.connectionStateSubject.asObservable();

  // Event subjects for component consumption
  public signalREvents = new Subject<{ event: keyof SignalREvents; data: any }>();

  // WebRTC configuration
  private iceServers: RTCIceServer[] = [
    { urls: 'stun:stun.l.google.com:19302' },
    { urls: 'stun:stun1.l.google.com:19302' }
  ];

  // Media streams
  private localStream?: MediaStream;
  private screenShareStream?: MediaStream;

  // Conference settings
  private conferenceSettings: ConferenceSettings = {
    backgroundBlur: false,
    noiseSuppression: true,
    echoCancellation: true,
    autoGainControl: true,
    videoResolution: 'medium'
  };

  constructor(private http: HttpClient) {}

  // ========== SignalR Connection Management ==========

  async connectToHub(conferenceId: string, accessToken: string): Promise<void> {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/video-conference?conferenceId=${conferenceId}`, {
        accessTokenFactory: () => accessToken,
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.setupSignalRListeners();

    try {
      await this.hubConnection.start();
      this.connectionStateSubject.next(this.hubConnection.state);
      console.log('SignalR Connected');
    } catch (err) {
      console.error('SignalR Connection Error:', err);
      throw err;
    }
  }

  async disconnectFromHub(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.connectionStateSubject.next(signalR.HubConnectionState.Disconnected);
    }
  }

  private setupSignalRListeners(): void {
    if (!this.hubConnection) return;

    // Connection events
    this.hubConnection.on('ParticipantConnecting', (peerId: string) => {
      this.signalREvents.next({ event: 'onParticipantConnecting', data: peerId });
    });

    this.hubConnection.on('ParticipantJoined', (participant: Participant) => {
      const participants = this.participantsSubject.value;
      this.participantsSubject.next([...participants, participant]);
      this.signalREvents.next({ event: 'onParticipantJoined', data: participant });
    });

    this.hubConnection.on('ParticipantLeft', (peerId: string, displayName: string) => {
      const participants = this.participantsSubject.value.filter(p => p.peerId !== peerId);
      this.participantsSubject.next(participants);
      this.removePeerConnection(peerId);
      this.signalREvents.next({ event: 'onParticipantLeft', data: { peerId, displayName } });
    });

    this.hubConnection.on('ParticipantDisconnected', (peerId: string) => {
      this.removePeerConnection(peerId);
      this.signalREvents.next({ event: 'onParticipantDisconnected', data: peerId });
    });

    this.hubConnection.on('ParticipantRemoved', (peerId: string) => {
      const participants = this.participantsSubject.value.filter(p => p.peerId !== peerId);
      this.participantsSubject.next(participants);
      this.removePeerConnection(peerId);
      this.signalREvents.next({ event: 'onParticipantRemoved', data: peerId });
    });

    // WebRTC signaling events
    this.hubConnection.on('ReceiveOffer', (peerId: string, offer: string) => {
      this.signalREvents.next({ event: 'onReceiveOffer', data: { peerId, offer } });
    });

    this.hubConnection.on('ReceiveAnswer', (peerId: string, answer: string) => {
      this.signalREvents.next({ event: 'onReceiveAnswer', data: { peerId, answer } });
    });

    this.hubConnection.on('ReceiveIceCandidate', (peerId: string, candidate: string) => {
      this.signalREvents.next({ event: 'onReceiveIceCandidate', data: { peerId, candidate } });
    });

    // Media control events
    this.hubConnection.on('ParticipantAudioToggled', (peerId: string, isMuted: boolean) => {
      this.updateParticipantMediaState(peerId, { isAudioMuted: isMuted });
      this.signalREvents.next({ event: 'onParticipantAudioToggled', data: { peerId, isMuted } });
    });

    this.hubConnection.on('ParticipantVideoToggled', (peerId: string, isOff: boolean) => {
      this.updateParticipantMediaState(peerId, { isVideoOff: isOff });
      this.signalREvents.next({ event: 'onParticipantVideoToggled', data: { peerId, isOff } });
    });

    this.hubConnection.on('HandRaised', (peerId: string) => {
      this.updateParticipantMediaState(peerId, { isHandRaised: true });
      this.signalREvents.next({ event: 'onHandRaised', data: peerId });
    });

    this.hubConnection.on('HandLowered', (peerId: string) => {
      this.updateParticipantMediaState(peerId, { isHandRaised: false });
      this.signalREvents.next({ event: 'onHandLowered', data: peerId });
    });

    // Screen sharing events
    this.hubConnection.on('ScreenShareStarted', (peerId: string) => {
      this.updateParticipantMediaState(peerId, { isScreenSharing: true });
      const participant = this.participantsSubject.value.find(p => p.peerId === peerId);
      if (participant) this.screenSharerSubject.next(participant);
      this.signalREvents.next({ event: 'onScreenShareStarted', data: peerId });
    });

    this.hubConnection.on('ScreenShareStopped', (peerId: string) => {
      this.updateParticipantMediaState(peerId, { isScreenSharing: false });
      this.screenSharerSubject.next(null);
      this.signalREvents.next({ event: 'onScreenShareStopped', data: peerId });
    });

    // Chat events
    this.hubConnection.on('ChatMessageReceived', (peerId: string, userId: string, message: string, timestamp: Date) => {
      const chatMessage: ChatMessage = {
        id: Date.now(),
        senderId: userId,
        senderName: this.getParticipantName(peerId),
        message,
        sentAt: new Date(timestamp),
        isPrivate: false,
        attachments: []
      };
      const messages = this.chatMessagesSubject.value;
      this.chatMessagesSubject.next([...messages, chatMessage]);
      this.signalREvents.next({ event: 'onChatMessageReceived', data: { peerId, userId, message, timestamp } });
    });

    this.hubConnection.on('PrivateChatMessageReceived', (peerId: string, userId: string, message: string, timestamp: Date) => {
      const chatMessage: ChatMessage = {
        id: Date.now(),
        senderId: userId,
        senderName: this.getParticipantName(peerId),
        message,
        sentAt: new Date(timestamp),
        isPrivate: true,
        attachments: []
      };
      const messages = this.chatMessagesSubject.value;
      this.chatMessagesSubject.next([...messages, chatMessage]);
      this.signalREvents.next({ event: 'onPrivateChatMessageReceived', data: { peerId, userId, message, timestamp } });
    });

    this.hubConnection.on('ChatMessageDeleted', (messageId: number) => {
      const messages = this.chatMessagesSubject.value.filter(m => m.id !== messageId);
      this.chatMessagesSubject.next(messages);
      this.signalREvents.next({ event: 'onChatMessageDeleted', data: messageId });
    });

    // Whiteboard events
    this.hubConnection.on('WhiteboardUpdated', (data: string, peerId: string) => {
      const whiteboardData: WhiteboardData = {
        data,
        lastModifiedById: peerId,
        lastModifiedByName: this.getParticipantName(peerId),
        lastModifiedAt: new Date()
      };
      this.whiteboardDataSubject.next(whiteboardData);
      this.signalREvents.next({ event: 'onWhiteboardUpdated', data: { data, peerId } });
    });

    this.hubConnection.on('WhiteboardCleared', (peerId: string) => {
      this.whiteboardDataSubject.next(null);
      this.signalREvents.next({ event: 'onWhiteboardCleared', data: peerId });
    });

    // Recording events
    this.hubConnection.on('RecordingStarted', () => {
      this.isRecordingSubject.next(true);
      this.signalREvents.next({ event: 'onRecordingStarted', data: null });
    });

    this.hubConnection.on('RecordingStopped', () => {
      this.isRecordingSubject.next(false);
      this.signalREvents.next({ event: 'onRecordingStopped', data: null });
    });

    // Host control events
    this.hubConnection.on('MuteAllRequested', () => {
      this.signalREvents.next({ event: 'onMuteAllRequested', data: null });
    });

    this.hubConnection.on('UnmuteRequested', () => {
      this.signalREvents.next({ event: 'onUnmuteRequested', data: null });
    });

    this.hubConnection.on('MutedByHost', () => {
      const state = this.localMediaStateSubject.value;
      this.localMediaStateSubject.next({ ...state, isAudioMuted: true });
      this.signalREvents.next({ event: 'onMutedByHost', data: null });
    });

    this.hubConnection.on('RemovedFromConference', (reason: string) => {
      this.signalREvents.next({ event: 'onRemovedFromConference', data: reason });
    });

    // Meeting control events
    this.hubConnection.on('MeetingLocked', () => {
      this.signalREvents.next({ event: 'onMeetingLocked', data: null });
    });

    this.hubConnection.on('MeetingUnlocked', () => {
      this.signalREvents.next({ event: 'onMeetingUnlocked', data: null });
    });

    this.hubConnection.on('ConferenceStarted', (conferenceId: string) => {
      this.signalREvents.next({ event: 'onConferenceStarted', data: conferenceId });
    });

    this.hubConnection.on('ConferenceEnded', (conferenceId: string) => {
      this.signalREvents.next({ event: 'onConferenceEnded', data: conferenceId });
    });

    this.hubConnection.on('ConferenceCancelled', (conferenceId: string) => {
      this.signalREvents.next({ event: 'onConferenceCancelled', data: conferenceId });
    });

    // Breakout room events
    this.hubConnection.on('AssignedToBreakoutRoom', (roomNumber: number, roomName: string) => {
      this.signalREvents.next({ event: 'onAssignedToBreakoutRoom', data: { roomNumber, roomName } });
    });

    this.hubConnection.on('ReturnedToMainRoom', () => {
      this.signalREvents.next({ event: 'onReturnedToMainRoom', data: null });
    });

    this.hubConnection.on('BroadcastMessage', (message: string) => {
      this.signalREvents.next({ event: 'onBroadcastMessage', data: message });
    });

    this.hubConnection.on('BreakoutRoomClosed', () => {
      this.signalREvents.next({ event: 'onBreakoutRoomClosed', data: null });
    });

    this.hubConnection.on('AllBreakoutRoomsClosed', () => {
      this.signalREvents.next({ event: 'onAllBreakoutRoomsClosed', data: null });
    });

    // Status events
    this.hubConnection.on('ParticipantQualityUpdated', (peerId: string, quality: string) => {
      this.signalREvents.next({ event: 'onParticipantQualityUpdated', data: { peerId, quality } });
    });

    this.hubConnection.on('AdmittedToConference', () => {
      this.signalREvents.next({ event: 'onAdmittedToConference', data: null });
    });

    this.hubConnection.on('MadeCoHost', () => {
      this.signalREvents.next({ event: 'onMadeCoHost', data: null });
    });

    this.hubConnection.on('PermissionsUpdated', (participant: Participant) => {
      this.signalREvents.next({ event: 'onPermissionsUpdated', data: participant });
    });
  }

  // ========== WebRTC Signaling (via SignalR) ==========

  async sendOffer(targetPeerId: string, offer: RTCSessionDescriptionInit): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendOffer', targetPeerId, JSON.stringify(offer));
    }
  }

  async sendAnswer(targetPeerId: string, answer: RTCSessionDescriptionInit): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendAnswer', targetPeerId, JSON.stringify(answer));
    }
  }

  async sendIceCandidate(targetPeerId: string, candidate: RTCIceCandidateInit): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendIceCandidate', targetPeerId, JSON.stringify(candidate));
    }
  }

  // ========== REST API Methods - Conference Management ==========

  createConference(dto: CreateConferenceDto): Observable<VideoConference> {
    return this.http.post<VideoConference>(this.apiUrl, dto);
  }

  getConference(conferenceId: string): Observable<VideoConference> {
    return this.http.get<VideoConference>(`${this.apiUrl}/${conferenceId}`);
  }

  updateConference(conferenceId: string, dto: UpdateConferenceDto): Observable<VideoConference> {
    return this.http.put<VideoConference>(`${this.apiUrl}/${conferenceId}`, dto);
  }

  deleteConference(conferenceId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${conferenceId}`);
  }

  startConference(conferenceId: string): Observable<VideoConference> {
    return this.http.post<VideoConference>(`${this.apiUrl}/${conferenceId}/start`, {});
  }

  endConference(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/end`, {});
  }

  generateMeetingLink(conferenceId: string): Observable<{ link: string }> {
    return this.http.get<{ link: string }>(`${this.apiUrl}/${conferenceId}/meeting-link`);
  }

  getUserConferences(): Observable<VideoConference[]> {
    return this.http.get<VideoConference[]>(`${this.apiUrl}/my-conferences`);
  }

  // ========== REST API Methods - Participant Management ==========

  joinConference(dto: JoinConferenceDto): Observable<Participant> {
    return this.http.post<Participant>(`${this.apiUrl}/${dto.conferenceId}/join`, dto);
  }

  leaveConference(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/leave`, {});
  }

  getParticipants(conferenceId: string): Observable<Participant[]> {
    return this.http.get<Participant[]>(`${this.apiUrl}/${conferenceId}/participants`);
  }

  removeParticipant(conferenceId: string, participantId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/participants/${participantId}/remove`, {});
  }

  admitFromWaitingRoom(conferenceId: string, participantId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/participants/${participantId}/admit`, {});
  }

  makeCoHost(conferenceId: string, participantId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/participants/${participantId}/make-cohost`, {});
  }

  updateParticipantPermissions(conferenceId: string, participantId: string, dto: UpdateParticipantPermissionsDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${conferenceId}/participants/${participantId}/permissions`, dto);
  }

  // ========== REST API Methods - Media Controls ==========

  muteParticipant(conferenceId: string, participantId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/participants/${participantId}/mute`, {});
  }

  muteAll(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/mute-all`, {});
  }

  requestUnmute(conferenceId: string, participantId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/participants/${participantId}/request-unmute`, {});
  }

  async toggleAudio(conferenceId: string): Promise<void> {
    const state = this.localMediaStateSubject.value;
    const newMuteState = !state.isAudioMuted;

    if (this.localStream) {
      this.localStream.getAudioTracks().forEach(track => track.enabled = !newMuteState);
    }

    this.localMediaStateSubject.next({ ...state, isAudioMuted: newMuteState });

    if (this.hubConnection) {
      await this.hubConnection.invoke('ToggleAudio', conferenceId, newMuteState);
    }
  }

  async toggleVideo(conferenceId: string): Promise<void> {
    const state = this.localMediaStateSubject.value;
    const newVideoOffState = !state.isVideoOff;

    if (this.localStream) {
      this.localStream.getVideoTracks().forEach(track => track.enabled = !newVideoOffState);
    }

    this.localMediaStateSubject.next({ ...state, isVideoOff: newVideoOffState });

    if (this.hubConnection) {
      await this.hubConnection.invoke('ToggleVideo', conferenceId, newVideoOffState);
    }
  }

  async raiseHand(conferenceId: string): Promise<void> {
    const state = this.localMediaStateSubject.value;
    this.localMediaStateSubject.next({ ...state, isHandRaised: true });

    if (this.hubConnection) {
      await this.hubConnection.invoke('RaiseHand', conferenceId);
    }
  }

  async lowerHand(conferenceId: string): Promise<void> {
    const state = this.localMediaStateSubject.value;
    this.localMediaStateSubject.next({ ...state, isHandRaised: false });

    if (this.hubConnection) {
      await this.hubConnection.invoke('LowerHand', conferenceId);
    }
  }

  // ========== REST API Methods - Recording ==========

  startRecording(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/recording/start`, {});
  }

  stopRecording(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/recording/stop`, {});
  }

  getRecording(conferenceId: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${conferenceId}/recording`, { responseType: 'blob' });
  }

  getRecordingInfo(conferenceId: string): Observable<Recording> {
    return this.http.get<Recording>(`${this.apiUrl}/${conferenceId}/recording/info`);
  }

  // ========== REST API Methods - Breakout Rooms ==========

  createBreakoutRooms(conferenceId: string, dto: CreateBreakoutRoomsDto): Observable<BreakoutRoom[]> {
    return this.http.post<BreakoutRoom[]>(`${this.apiUrl}/${conferenceId}/breakout-rooms`, dto);
  }

  assignToBreakoutRoom(conferenceId: string, participantId: string, roomNumber: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/breakout-rooms/${roomNumber}/assign/${participantId}`, {});
  }

  closeBreakoutRooms(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/breakout-rooms/close`, {});
  }

  getBreakoutRooms(conferenceId: string): Observable<BreakoutRoom[]> {
    return this.http.get<BreakoutRoom[]>(`${this.apiUrl}/${conferenceId}/breakout-rooms`);
  }

  broadcastToBreakoutRooms(conferenceId: string, message: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/breakout-rooms/broadcast`, { message });
  }

  // ========== REST API Methods - Chat ==========

  sendChatMessage(conferenceId: string, dto: SendChatMessageDto): Observable<ChatMessage> {
    return this.http.post<ChatMessage>(`${this.apiUrl}/${conferenceId}/chat`, dto);
  }

  getChatHistory(conferenceId: string): Observable<ChatMessage[]> {
    return this.http.get<ChatMessage[]>(`${this.apiUrl}/${conferenceId}/chat`);
  }

  deleteChatMessage(conferenceId: string, messageId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${conferenceId}/chat/${messageId}`);
  }

  async sendChatViaHub(conferenceId: string, message: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendChatMessage', conferenceId, message);
    }
  }

  async sendPrivateChatViaHub(conferenceId: string, recipientId: string, message: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('SendPrivateChatMessage', conferenceId, recipientId, message);
    }
  }

  // ========== REST API Methods - Screen Sharing ==========

  startScreenShare(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/screen-share/start`, {});
  }

  stopScreenShare(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/screen-share/stop`, {});
  }

  getScreenSharer(conferenceId: string): Observable<Participant | null> {
    return this.http.get<Participant | null>(`${this.apiUrl}/${conferenceId}/screen-share/current`);
  }

  async startScreenShareViaHub(conferenceId: string): Promise<void> {
    try {
      const stream = await navigator.mediaDevices.getDisplayMedia({
        video: { cursor: 'always' } as MediaTrackConstraints,
        audio: false
      });

      this.screenShareStream = stream;
      const state = this.localMediaStateSubject.value;
      this.localMediaStateSubject.next({ ...state, isScreenSharing: true, screenShareStream: stream });

      if (this.hubConnection) {
        await this.hubConnection.invoke('StartScreenShare', conferenceId);
      }

      // Stop screen share when user stops it from browser UI
      stream.getVideoTracks()[0].addEventListener('ended', () => {
        this.stopScreenShareViaHub(conferenceId);
      });
    } catch (error) {
      console.error('Screen share error:', error);
      throw error;
    }
  }

  async stopScreenShareViaHub(conferenceId: string): Promise<void> {
    if (this.screenShareStream) {
      this.screenShareStream.getTracks().forEach(track => track.stop());
      this.screenShareStream = undefined;
    }

    const state = this.localMediaStateSubject.value;
    this.localMediaStateSubject.next({ ...state, isScreenSharing: false, screenShareStream: undefined });

    if (this.hubConnection) {
      await this.hubConnection.invoke('StopScreenShare', conferenceId);
    }
  }

  // ========== REST API Methods - Whiteboard ==========

  updateWhiteboard(conferenceId: string, data: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${conferenceId}/whiteboard`, { data });
  }

  getWhiteboard(conferenceId: string): Observable<WhiteboardData> {
    return this.http.get<WhiteboardData>(`${this.apiUrl}/${conferenceId}/whiteboard`);
  }

  clearWhiteboard(conferenceId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${conferenceId}/whiteboard`);
  }

  async updateWhiteboardViaHub(conferenceId: string, data: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('UpdateWhiteboard', conferenceId, data);
    }
  }

  async clearWhiteboardViaHub(conferenceId: string): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.invoke('ClearWhiteboard', conferenceId);
    }
  }

  // ========== REST API Methods - Security ==========

  lockMeeting(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/lock`, {});
  }

  unlockMeeting(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/unlock`, {});
  }

  enableWaitingRoom(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/waiting-room/enable`, {});
  }

  disableWaitingRoom(conferenceId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${conferenceId}/waiting-room/disable`, {});
  }

  updatePassword(conferenceId: string, newPassword: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${conferenceId}/password`, { newPassword });
  }

  // ========== REST API Methods - Analytics ==========

  getAnalytics(conferenceId: string): Observable<ConferenceAnalytics> {
    return this.http.get<ConferenceAnalytics>(`${this.apiUrl}/${conferenceId}/analytics`);
  }

  // ========== Media Stream Management ==========

  async getLocalMediaStream(constraints?: MediaStreamConstraints): Promise<MediaStream> {
    try {
      const defaultConstraints: MediaStreamConstraints = {
        audio: {
          echoCancellation: this.conferenceSettings.echoCancellation,
          noiseSuppression: this.conferenceSettings.noiseSuppression,
          autoGainControl: this.conferenceSettings.autoGainControl
        },
        video: {
          width: this.getVideoResolution().width,
          height: this.getVideoResolution().height,
          facingMode: 'user'
        }
      };

      const stream = await navigator.mediaDevices.getUserMedia(constraints || defaultConstraints);
      this.localStream = stream;

      const state = this.localMediaStateSubject.value;
      this.localMediaStateSubject.next({ ...state, localStream: stream });

      return stream;
    } catch (error) {
      console.error('Failed to get local media stream:', error);
      throw error;
    }
  }

  stopLocalMediaStream(): void {
    if (this.localStream) {
      this.localStream.getTracks().forEach(track => track.stop());
      this.localStream = undefined;

      const state = this.localMediaStateSubject.value;
      this.localMediaStateSubject.next({ ...state, localStream: undefined });
    }
  }

  async getAvailableDevices(): Promise<MediaDeviceInfo[]> {
    return await navigator.mediaDevices.enumerateDevices();
  }

  async switchAudioDevice(deviceId: string): Promise<void> {
    if (!this.localStream) return;

    const newStream = await navigator.mediaDevices.getUserMedia({
      audio: { deviceId: { exact: deviceId } },
      video: false
    });

    const audioTrack = newStream.getAudioTracks()[0];
    const oldAudioTrack = this.localStream.getAudioTracks()[0];

    this.localStream.removeTrack(oldAudioTrack);
    this.localStream.addTrack(audioTrack);
    oldAudioTrack.stop();

    this.conferenceSettings.audioDeviceId = deviceId;
  }

  async switchVideoDevice(deviceId: string): Promise<void> {
    if (!this.localStream) return;

    const newStream = await navigator.mediaDevices.getUserMedia({
      audio: false,
      video: { deviceId: { exact: deviceId } }
    });

    const videoTrack = newStream.getVideoTracks()[0];
    const oldVideoTrack = this.localStream.getVideoTracks()[0];

    this.localStream.removeTrack(oldVideoTrack);
    this.localStream.addTrack(videoTrack);
    oldVideoTrack.stop();

    this.conferenceSettings.videoDeviceId = deviceId;
  }

  // ========== WebRTC Peer Connection Management ==========

  createPeerConnection(peerId: string, displayName: string, isHost: boolean): RTCPeerConnection {
    const config: RTCConfiguration = {
      iceServers: this.iceServers
    };

    const peerConnection = new RTCPeerConnection(config);

    // Add local stream tracks
    if (this.localStream) {
      this.localStream.getTracks().forEach(track => {
        peerConnection.addTrack(track, this.localStream!);
      });
    }

    const peerConnectionObj: PeerConnection = {
      peerId,
      connection: peerConnection,
      displayName,
      isHost,
      isScreenShare: false
    };

    const connections = this.peerConnectionsSubject.value;
    connections.set(peerId, peerConnectionObj);
    this.peerConnectionsSubject.next(new Map(connections));

    return peerConnection;
  }

  removePeerConnection(peerId: string): void {
    const connections = this.peerConnectionsSubject.value;
    const peerConn = connections.get(peerId);

    if (peerConn) {
      peerConn.connection.close();
      connections.delete(peerId);
      this.peerConnectionsSubject.next(new Map(connections));
    }
  }

  getPeerConnection(peerId: string): PeerConnection | undefined {
    return this.peerConnectionsSubject.value.get(peerId);
  }

  // ========== Helper Methods ==========

  private updateParticipantMediaState(peerId: string, updates: Partial<Participant>): void {
    const participants = this.participantsSubject.value;
    const index = participants.findIndex(p => p.peerId === peerId);

    if (index !== -1) {
      participants[index] = { ...participants[index], ...updates };
      this.participantsSubject.next([...participants]);
    }
  }

  private getParticipantName(peerId: string): string {
    const participant = this.participantsSubject.value.find(p => p.peerId === peerId);
    return participant?.displayName || 'Unknown';
  }

  private getVideoResolution(): { width: number; height: number } {
    switch (this.conferenceSettings.videoResolution) {
      case 'low': return { width: 640, height: 480 };
      case 'medium': return { width: 1280, height: 720 };
      case 'high': return { width: 1920, height: 1080 };
      default: return { width: 1280, height: 720 };
    }
  }

  updateSettings(settings: Partial<ConferenceSettings>): void {
    this.conferenceSettings = { ...this.conferenceSettings, ...settings };
  }

  getSettings(): ConferenceSettings {
    return { ...this.conferenceSettings };
  }

  // Clean up resources
  cleanup(): void {
    this.stopLocalMediaStream();
    this.peerConnectionsSubject.value.forEach((_, peerId) => {
      this.removePeerConnection(peerId);
    });
    this.disconnectFromHub();

    // Reset all subjects
    this.currentConferenceSubject.next(null);
    this.participantsSubject.next([]);
    this.chatMessagesSubject.next([]);
    this.whiteboardDataSubject.next(null);
    this.screenSharerSubject.next(null);
    this.isRecordingSubject.next(false);
    this.localMediaStateSubject.next({
      isAudioMuted: false,
      isVideoOff: false,
      isScreenSharing: false,
      isHandRaised: false
    });
  }
}
