namespace TaskTracker.Api.Services.Interfaces;

public interface IDeadlineNotifier
{
    Task NotifyAboutDeadlinesAsync(CancellationToken cancellationToken);
}