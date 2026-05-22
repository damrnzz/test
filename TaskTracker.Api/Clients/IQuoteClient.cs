namespace TaskTracker.Api.Clients;

public interface IQuoteClient
{
    Task<string> GetRandomQuoteAsync(CancellationToken cancellationToken);
}