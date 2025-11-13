import { Component, Input, OnInit, OnDestroy, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { VideoConferenceService } from '../../../services/video-conference.service';
import { ConnectionQuality } from '../../../models/video-conference.model';

@Component({
  selector: 'app-video-tile',
  templateUrl: './video-tile.component.html',
  styleUrls: ['./video-tile.component.scss']
})
export class VideoTileComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild('videoElement') videoElement!: ElementRef<HTMLVideoElement>;

  @Input() peerId!: string;
  @Input() displayName!: string;
  @Input() isHost = false;
  @Input() isCoHost = false;
  @Input() isScreenShare = false;
  @Input() isAudioMuted = false;
  @Input() isVideoOff = false;
  @Input() isHandRaised = false;
  @Input() connectionQuality: ConnectionQuality = ConnectionQuality.Good;
  @Input() avatarUrl?: string;

  // State
  isSpeaking = false;
  audioLevel = 0;

  constructor(private videoConferenceService: VideoConferenceService) {}

  ngOnInit(): void {
    // Subscribe to peer connection updates
  }

  ngAfterViewInit(): void {
    this.attachMediaStream();
  }

  ngOnDestroy(): void {
    this.detachMediaStream();
  }

  private attachMediaStream(): void {
    const peerConnection = this.videoConferenceService.getPeerConnection(this.peerId);

    if (peerConnection?.remoteStream && this.videoElement) {
      this.videoElement.nativeElement.srcObject = peerConnection.remoteStream;

      // Set up audio level detection
      this.setupAudioLevelDetection(peerConnection.remoteStream);
    }
  }

  private detachMediaStream(): void {
    if (this.videoElement) {
      this.videoElement.nativeElement.srcObject = null;
    }
  }

  private setupAudioLevelDetection(stream: MediaStream): void {
    const audioContext = new AudioContext();
    const audioSource = audioContext.createMediaStreamSource(stream);
    const analyzer = audioContext.createAnalyser();
    analyzer.fftSize = 512;
    analyzer.smoothingTimeConstant = 0.3;

    audioSource.connect(analyzer);

    const dataArray = new Uint8Array(analyzer.frequencyBinCount);

    const detectAudioLevel = () => {
      analyzer.getByteFrequencyData(dataArray);

      // Calculate average audio level
      const sum = dataArray.reduce((a, b) => a + b, 0);
      const average = sum / dataArray.length;

      this.audioLevel = average;
      this.isSpeaking = average > 20; // Threshold for speaking detection

      // Continue detection
      requestAnimationFrame(detectAudioLevel);
    };

    detectAudioLevel();
  }

  get connectionQualityIcon(): string {
    switch (this.connectionQuality) {
      case ConnectionQuality.Excellent:
        return 'signal_cellular_alt';
      case ConnectionQuality.Good:
        return 'signal_cellular_alt';
      case ConnectionQuality.Fair:
        return 'signal_cellular_alt_2_bar';
      case ConnectionQuality.Poor:
        return 'signal_cellular_alt_1_bar';
      case ConnectionQuality.VeryPoor:
        return 'signal_cellular_connected_no_internet_0_bar';
      default:
        return 'signal_cellular_alt';
    }
  }

  get connectionQualityClass(): string {
    return this.connectionQuality.toLowerCase();
  }

  get roleLabel(): string {
    if (this.isHost) return 'HOST';
    if (this.isCoHost) return 'CO-HOST';
    return '';
  }

  get initials(): string {
    return this.displayName
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  }
}
