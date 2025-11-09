using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Backend.Services;

namespace Backend.Hubs
{
    [Authorize]
    public class VideoConferenceHub : Hub
    {
        private readonly IVideoConferenceService _conferenceService;

        public VideoConferenceHub(IVideoConferenceService conferenceService)
        {
            _conferenceService = conferenceService;
        }

        public override async Task OnConnectedAsync()
        {
            var conferenceId = Context.GetHttpContext()?.Request.Query["conferenceId"].ToString();

            if (!string.IsNullOrEmpty(conferenceId))
            {
                // Add connection to conference group
                await Groups.AddToGroupAsync(Context.ConnectionId, conferenceId);

                // Notify others that a new participant is connecting
                await Clients.OthersInGroup(conferenceId).SendAsync("ParticipantConnecting", Context.ConnectionId);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var conferenceId = Context.GetHttpContext()?.Request.Query["conferenceId"].ToString();

            if (!string.IsNullOrEmpty(conferenceId))
            {
                // Notify others that participant disconnected
                await Clients.OthersInGroup(conferenceId).SendAsync("ParticipantDisconnected", Context.ConnectionId);

                // Remove from group
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, conferenceId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ========== WebRTC Signaling ==========

        /// <summary>
        /// Sends an WebRTC offer to a specific peer
        /// </summary>
        public async Task SendOffer(string targetPeerId, string offer)
        {
            await Clients.Client(targetPeerId).SendAsync("ReceiveOffer", Context.ConnectionId, offer);
        }

        /// <summary>
        /// Sends a WebRTC answer to a specific peer
        /// </summary>
        public async Task SendAnswer(string targetPeerId, string answer)
        {
            await Clients.Client(targetPeerId).SendAsync("ReceiveAnswer", Context.ConnectionId, answer);
        }

        /// <summary>
        /// Sends an ICE candidate to a specific peer
        /// </summary>
        public async Task SendIceCandidate(string targetPeerId, string candidate)
        {
            await Clients.Client(targetPeerId).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
        }

        // ========== Media Controls ==========

        /// <summary>
        /// Toggles audio for current user
        /// </summary>
        public async Task ToggleAudio(string conferenceId, bool isMuted)
        {
            await Clients.OthersInGroup(conferenceId).SendAsync(
                "ParticipantAudioToggled",
                Context.ConnectionId,
                isMuted
            );
        }

        /// <summary>
        /// Toggles video for current user
        /// </summary>
        public async Task ToggleVideo(string conferenceId, bool isOff)
        {
            await Clients.OthersInGroup(conferenceId).SendAsync(
                "ParticipantVideoToggled",
                Context.ConnectionId,
                isOff
            );
        }

        /// <summary>
        /// Raises hand
        /// </summary>
        public async Task RaiseHand(string conferenceId)
        {
            await Clients.Group(conferenceId).SendAsync("HandRaised", Context.ConnectionId);
        }

        /// <summary>
        /// Lowers hand
        /// </summary>
        public async Task LowerHand(string conferenceId)
        {
            await Clients.Group(conferenceId).SendAsync("HandLowered", Context.ConnectionId);
        }

        // ========== Screen Sharing ==========

        /// <summary>
        /// Starts screen sharing
        /// </summary>
        public async Task StartScreenShare(string conferenceId)
        {
            await Clients.OthersInGroup(conferenceId).SendAsync("ScreenShareStarted", Context.ConnectionId);
        }

        /// <summary>
        /// Stops screen sharing
        /// </summary>
        public async Task StopScreenShare(string conferenceId)
        {
            await Clients.OthersInGroup(conferenceId).SendAsync("ScreenShareStopped", Context.ConnectionId);
        }

        // ========== Chat ==========

        /// <summary>
        /// Sends a chat message to all participants
        /// </summary>
        public async Task SendChatMessage(string conferenceId, string message)
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.Identity?.Name ?? "Unknown";

            await Clients.Group(conferenceId).SendAsync(
                "ChatMessageReceived",
                Context.ConnectionId,
                userId,
                message,
                DateTime.UtcNow
            );
        }

        /// <summary>
        /// Sends a private chat message to a specific participant
        /// </summary>
        public async Task SendPrivateChatMessage(string conferenceId, string recipientId, string message)
        {
            var userId = Context.User?.FindFirst("sub")?.Value ?? Context.User?.Identity?.Name ?? "Unknown";

            // Send to recipient
            await Clients.Client(recipientId).SendAsync(
                "PrivateChatMessageReceived",
                Context.ConnectionId,
                userId,
                message,
                DateTime.UtcNow
            );

            // Send back to sender for confirmation
            await Clients.Caller.SendAsync(
                "PrivateChatMessageSent",
                recipientId,
                message,
                DateTime.UtcNow
            );
        }

        // ========== Whiteboard ==========

        /// <summary>
        /// Updates whiteboard data
        /// </summary>
        public async Task UpdateWhiteboard(string conferenceId, string data)
        {
            // Broadcast to all except sender
            await Clients.OthersInGroup(conferenceId).SendAsync("WhiteboardUpdated", data, Context.ConnectionId);
        }

        /// <summary>
        /// Clears whiteboard
        /// </summary>
        public async Task ClearWhiteboard(string conferenceId)
        {
            await Clients.Group(conferenceId).SendAsync("WhiteboardCleared", Context.ConnectionId);
        }

        // ========== Recording ==========

        /// <summary>
        /// Notifies participants that recording has started
        /// </summary>
        public async Task NotifyRecordingStarted(string conferenceId)
        {
            await Clients.Group(conferenceId).SendAsync("RecordingStarted");
        }

        /// <summary>
        /// Notifies participants that recording has stopped
        /// </summary>
        public async Task NotifyRecordingStopped(string conferenceId)
        {
            await Clients.Group(conferenceId).SendAsync("RecordingStopped");
        }

        // ========== Host Controls ==========

        /// <summary>
        /// Mutes all participants
        /// </summary>
        public async Task MuteAll(string conferenceId)
        {
            await Clients.Group(conferenceId).SendAsync("MuteAllRequested");
        }

        /// <summary>
        /// Requests a participant to unmute
        /// </summary>
        public async Task RequestUnmute(string conferenceId, string participantId)
        {
            await Clients.Client(participantId).SendAsync("UnmuteRequested");
        }

        /// <summary>
        /// Removes a participant from the conference
        /// </summary>
        public async Task RemoveParticipant(string conferenceId, string participantId, string reason)
        {
            await Clients.Client(participantId).SendAsync("RemovedFromConference", reason);
            await Clients.OthersInGroup(conferenceId).SendAsync("ParticipantRemoved", participantId);
        }

        /// <summary>
        /// Locks the meeting
        /// </summary>
        public async Task LockMeeting(string conferenceId)
        {
            await Clients.Group(conferenceId).SendAsync("MeetingLocked");
        }

        /// <summary>
        /// Unlocks the meeting
        /// </summary>
        public async Task UnlockMeeting(string conferenceId)
        {
            await Clients.Group(conferenceId).SendAsync("MeetingUnlocked");
        }

        // ========== Breakout Rooms ==========

        /// <summary>
        /// Assigns participant to breakout room
        /// </summary>
        public async Task AssignToBreakoutRoom(string conferenceId, string participantId, int roomNumber, string roomName)
        {
            var breakoutRoomGroup = $"{conferenceId}_room_{roomNumber}";

            // Remove from main conference
            await Groups.RemoveFromGroupAsync(participantId, conferenceId);

            // Add to breakout room
            await Groups.AddToGroupAsync(participantId, breakoutRoomGroup);

            // Notify participant
            await Clients.Client(participantId).SendAsync("AssignedToBreakoutRoom", roomNumber, roomName);
        }

        /// <summary>
        /// Returns participant from breakout room to main conference
        /// </summary>
        public async Task ReturnToMainRoom(string conferenceId, string participantId, int roomNumber)
        {
            var breakoutRoomGroup = $"{conferenceId}_room_{roomNumber}";

            // Remove from breakout room
            await Groups.RemoveFromGroupAsync(participantId, breakoutRoomGroup);

            // Add back to main conference
            await Groups.AddToGroupAsync(participantId, conferenceId);

            // Notify participant
            await Clients.Client(participantId).SendAsync("ReturnedToMainRoom");
        }

        /// <summary>
        /// Broadcasts message to all breakout rooms
        /// </summary>
        public async Task BroadcastToAllRooms(string conferenceId, string message)
        {
            // This will be sent to main room and all breakout rooms
            await Clients.Group(conferenceId).SendAsync("BroadcastMessage", message);
        }

        /// <summary>
        /// Closes all breakout rooms
        /// </summary>
        public async Task CloseBreakoutRooms(string conferenceId, int numberOfRooms)
        {
            for (int i = 1; i <= numberOfRooms; i++)
            {
                var breakoutRoomGroup = $"{conferenceId}_room_{i}";
                await Clients.Group(breakoutRoomGroup).SendAsync("BreakoutRoomClosed");
            }

            await Clients.Group(conferenceId).SendAsync("AllBreakoutRoomsClosed");
        }

        // ========== Participant Status Updates ==========

        /// <summary>
        /// Updates connection quality
        /// </summary>
        public async Task UpdateConnectionQuality(string conferenceId, string quality)
        {
            await Clients.OthersInGroup(conferenceId).SendAsync(
                "ParticipantQualityUpdated",
                Context.ConnectionId,
                quality
            );
        }

        /// <summary>
        /// Sends participant join notification
        /// </summary>
        public async Task ParticipantJoined(string conferenceId, string participantName)
        {
            await Clients.Group(conferenceId).SendAsync("ParticipantJoined", Context.ConnectionId, participantName);
        }

        /// <summary>
        /// Sends participant left notification
        /// </summary>
        public async Task ParticipantLeft(string conferenceId, string participantName)
        {
            await Clients.OthersInGroup(conferenceId).SendAsync("ParticipantLeft", Context.ConnectionId, participantName);
        }
    }
}
