using Microsoft.Extensions.Options;
using TaskTracker.Api.Options;
using TaskTracker.Api.Services.Interfaces;

namespace TaskTracker.Api.Services.Implementations;

public class DeadlineNotifier : IDeadlineNotifier
{
    private readonly NotificationOptions _options;
    private readonly ILogger<DeadlineNotifier> _logger;

    public DeadlineNotifier(IOptions<NotificationOptions> options, ILogger<DeadlineNotifier> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task NotifyAboutDeadlinesAsync(CancellationToken cancellationToken)
    {
        if (!_options.EnableDueDateReminders)
        {
            _logger.LogInformation("Deadline reminders are disabled");
            return Task.CompletedTask;
        }

        _logger.LogInformation(
            "Sending reminders from {Sender}. Days before deadline: {Days}",
            _options.DefaultSender,
            _options.ReminderDaysBefore);

        return Task.CompletedTask;
    }
}