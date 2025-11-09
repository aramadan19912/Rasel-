namespace Backend.Application.DTOs.VideoConference;

public enum ConferenceStatus
{
    Scheduled = 1,
    WaitingRoom = 2,
    InProgress = 3,
    Ended = 4,
    Cancelled = 5
}

public enum ParticipantStatus
{
    InWaitingRoom = 1,
    Joined = 2,
    Left = 3,
    Removed = 4
}

public enum ConnectionQuality
{
    Excellent = 1,
    Good = 2,
    Fair = 3,
    Poor = 4
}
