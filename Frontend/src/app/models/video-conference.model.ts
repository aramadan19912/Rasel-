// ========== Enums ==========

export enum ConferenceStatus {
  Scheduled = 'Scheduled',
  InProgress = 'InProgress',
  Ended = 'Ended',
  Cancelled = 'Cancelled'
}

export enum ParticipantStatus {
  InWaitingRoom = 'InWaitingRoom',
  Joined = 'Joined',
  Left = 'Left',
  Removed = 'Removed'
}

export enum ConnectionQuality {
  Excellent = 'Excellent',
  Good = 'Good',
  Fair = 'Fair',
  Poor = 'Poor',
  VeryPoor = 'VeryPoor'
}

// ========== Video Conference Models ==========

export interface VideoConference {
  id: number;
  conferenceId: string;
  title: string;
  description: string;
  requirePassword: boolean;
  scheduledStartTime: Date;
  scheduledEndTime?: Date;
  durationMinutes: number;
  hostId: string;
  hostName: string;
  participantCount: number;
  maxParticipants: number;
  allowGuestsToJoin: boolean;
  status: ConferenceStatus;
  actualStartTime?: Date;
  actualEndTime?: Date;
  enableChat: boolean;
  enableScreenShare: boolean;
  enableRecording: boolean;
  enableWhiteboard: boolean;
  enableBreakoutRooms: boolean;
  enableWaitingRoom: boolean;
  muteParticipantsOnEntry: boolean;
  isRecording: boolean;
  recordingUrl?: string;
  lockMeeting: boolean;
  meetingLink: string;
  createdAt: Date;
}

export interface CreateConferenceDto {
  title: string;
  description: string;
  requirePassword: boolean;
  password?: string;
  scheduledStartTime: Date;
  durationMinutes: number;
  hostId: string;
  maxParticipants?: number;
  allowGuestsToJoin: boolean;
  enableRecording: boolean;
  requireApproval: boolean;
  calendarEventId?: number;
}

export interface UpdateConferenceDto {
  title?: string;
  description?: string;
  scheduledStartTime?: Date;
  durationMinutes?: number;
  maxParticipants?: number;
  allowGuestsToJoin?: boolean;
  enableChat?: boolean;
  enableScreenShare?: boolean;
  enableRecording?: boolean;
  enableWhiteboard?: boolean;
  enableBreakoutRooms?: boolean;
  enableWaitingRoom?: boolean;
  muteParticipantsOnEntry?: boolean;
}

// ========== Participant Models ==========

export interface Participant {
  id: number;
  displayName: string;
  email: string;
  isGuest: boolean;
  isHost: boolean;
  isCoHost: boolean;
  status: ParticipantStatus;
  joinedAt: Date;
  leftAt?: Date;
  isAudioMuted: boolean;
  isVideoOff: boolean;
  isHandRaised: boolean;
  isScreenSharing: boolean;
  peerId: string;
  quality: ConnectionQuality;
  canShareScreen: boolean;
  canRecord: boolean;
  canUseWhiteboard: boolean;
  breakoutRoomId?: number;
  breakoutRoomName?: string;
}

export interface JoinConferenceDto {
  conferenceId: string;
  userId?: string;
  displayName: string;
  email: string;
  password?: string;
  isGuest: boolean;
}

export interface UpdateParticipantPermissionsDto {
  isCoHost?: boolean;
  canShareScreen?: boolean;
  canRecord?: boolean;
  canUseWhiteboard?: boolean;
}

// ========== Breakout Room Models ==========

export interface BreakoutRoom {
  id: number;
  roomName: string;
  roomNumber: number;
  participantCount: number;
  maxParticipants: number;
  participants: Participant[];
  isOpen: boolean;
  createdAt: Date;
}

export interface CreateBreakoutRoomsDto {
  numberOfRooms: number;
  assignAutomatically: boolean;
}

// ========== Chat Models ==========

export interface ChatMessage {
  id: number;
  senderId: string;
  senderName: string;
  message: string;
  sentAt: Date;
  isPrivate: boolean;
  recipientId?: string;
  recipientName?: string;
  attachments: ChatAttachment[];
}

export interface SendChatMessageDto {
  message: string;
  isPrivate: boolean;
  recipientId?: string;
}

export interface ChatAttachment {
  fileName: string;
  fileUrl: string;
  fileSize: number;
  contentType: string;
}

// ========== Whiteboard Models ==========

export interface WhiteboardData {
  data: string;
  lastModifiedById: string;
  lastModifiedByName: string;
  lastModifiedAt: Date;
}

// ========== Analytics Models ==========

export interface ConferenceAnalytics {
  totalParticipantsJoined: number;
  peakParticipants: number;
  totalDuration: string; // TimeSpan as string "HH:mm:ss"
  averageJoinTime: string; // TimeSpan as string "HH:mm:ss"
  totalChatMessages: number;
  screenShareCount: number;
  participantsByCountry: { [country: string]: number };
  connectionQualityDistribution: { [quality: string]: number };
}

// ========== Recording Models ==========

export interface Recording {
  recordingUrl: string;
  fileSize: number;
  duration: string; // TimeSpan as string "HH:mm:ss"
  startedAt: Date;
  endedAt?: Date;
  format: string;
}

// ========== Meeting Link Models ==========

export interface MeetingLink {
  conferenceId: string;
  meetingUrl: string;
  password?: string;
  expiresAt: Date;
}

// ========== WebRTC Models ==========

export interface RTCOfferMessage {
  targetPeerId: string;
  offer: RTCSessionDescriptionInit;
}

export interface RTCAnswerMessage {
  targetPeerId: string;
  answer: RTCSessionDescriptionInit;
}

export interface RTCIceCandidateMessage {
  targetPeerId: string;
  candidate: RTCIceCandidateInit;
}

export interface MediaConstraints {
  audio: boolean | MediaTrackConstraints;
  video: boolean | MediaTrackConstraints;
}

export interface PeerConnection {
  peerId: string;
  connection: RTCPeerConnection;
  remoteStream?: MediaStream;
  displayName: string;
  isHost: boolean;
  isScreenShare: boolean;
}

// ========== Local State Models ==========

export interface LocalMediaState {
  isAudioMuted: boolean;
  isVideoOff: boolean;
  isScreenSharing: boolean;
  isHandRaised: boolean;
  localStream?: MediaStream;
  screenShareStream?: MediaStream;
}

export interface ConferenceSettings {
  audioDeviceId?: string;
  videoDeviceId?: string;
  speakerDeviceId?: string;
  backgroundBlur: boolean;
  noiseSuppression: boolean;
  echoCancellation: boolean;
  autoGainControl: boolean;
  videoResolution: 'low' | 'medium' | 'high';
}

// ========== SignalR Event Models ==========

export interface SignalREvents {
  // Connection events
  onParticipantConnecting: (peerId: string) => void;
  onParticipantJoined: (participant: Participant) => void;
  onParticipantLeft: (peerId: string, displayName: string) => void;
  onParticipantDisconnected: (peerId: string) => void;
  onParticipantRemoved: (peerId: string) => void;

  // WebRTC signaling events
  onReceiveOffer: (peerId: string, offer: string) => void;
  onReceiveAnswer: (peerId: string, answer: string) => void;
  onReceiveIceCandidate: (peerId: string, candidate: string) => void;

  // Media control events
  onParticipantAudioToggled: (peerId: string, isMuted: boolean) => void;
  onParticipantVideoToggled: (peerId: string, isOff: boolean) => void;
  onHandRaised: (peerId: string) => void;
  onHandLowered: (peerId: string) => void;

  // Screen sharing events
  onScreenShareStarted: (peerId: string) => void;
  onScreenShareStopped: (peerId: string) => void;

  // Chat events
  onChatMessageReceived: (peerId: string, userId: string, message: string, timestamp: Date) => void;
  onPrivateChatMessageReceived: (peerId: string, userId: string, message: string, timestamp: Date) => void;
  onPrivateChatMessageSent: (recipientId: string, message: string, timestamp: Date) => void;
  onChatMessageDeleted: (messageId: number) => void;

  // Whiteboard events
  onWhiteboardUpdated: (data: string, peerId: string) => void;
  onWhiteboardCleared: (peerId: string) => void;

  // Recording events
  onRecordingStarted: () => void;
  onRecordingStopped: () => void;

  // Host control events
  onMuteAllRequested: () => void;
  onUnmuteRequested: () => void;
  onRemovedFromConference: (reason: string) => void;
  onMutedByHost: () => void;

  // Meeting control events
  onMeetingLocked: () => void;
  onMeetingUnlocked: () => void;
  onConferenceStarted: (conferenceId: string) => void;
  onConferenceEnded: (conferenceId: string) => void;
  onConferenceCancelled: (conferenceId: string) => void;

  // Breakout room events
  onAssignedToBreakoutRoom: (roomNumber: number, roomName: string) => void;
  onReturnedToMainRoom: () => void;
  onBroadcastMessage: (message: string) => void;
  onBreakoutRoomClosed: () => void;
  onAllBreakoutRoomsClosed: () => void;

  // Status events
  onParticipantQualityUpdated: (peerId: string, quality: string) => void;
  onAdmittedToConference: () => void;
  onMadeCoHost: () => void;
  onPermissionsUpdated: (participant: Participant) => void;
}
