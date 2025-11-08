namespace OutlookInboxManagement.Models;

public class EventReminder
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public CalendarEvent? Event { get; set; }

    public int MinutesBeforeStart { get; set; } = 15;
    public ReminderMethod Method { get; set; } = ReminderMethod.Notification;

    public bool IsTriggered { get; set; }
    public DateTime? TriggeredAt { get; set; }
}

public enum ReminderMethod
{
    Notification = 0,
    Email = 1,
    SMS = 2,
    Popup = 3
}
