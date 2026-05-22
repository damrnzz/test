namespace TaskTracker.Api.Clients;

public class QuoteClient: IQuoteClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<QuoteClient> _logger;

    public QuoteClient(HttpClient httpClient, ILogger<QuoteClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GetRandomQuoteAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/random", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get random quote");
            return "No quote available";
        }
    }
}