namespace TaskTracker.Api.Options;

public class NotificationOptions
{
    public const string SectionName = "Notification";

    public string DefaultSender { get; set; } = null!;
    public bool EnableDueDateReminders { get; set; }
    public int ReminderDaysBefore { get; set; }
}