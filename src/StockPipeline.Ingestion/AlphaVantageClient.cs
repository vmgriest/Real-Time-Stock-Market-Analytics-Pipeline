using System.Text.Json;
using StockPipeline.Shared.Models;

namespace StockPipeline.Ingestion;

public class AlphaVantageClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly ILogger<AlphaVantageClient> _logger;

    public AlphaVantageClient(HttpClient http, string apiKey, ILogger<AlphaVantageClient> logger)
    {
        _http = http;
        _apiKey = apiKey;
        _logger = logger;
    }

    public async Task<StockQuote?> GetLatestQuoteAsync(string symbol, CancellationToken ct)
    {
        var url = $"https://www.alphavantage.co/query?function=GLOBAL_QUOTE&symbol={symbol}&apikey={_apiKey}";

        try
        {
            var json = await _http.GetStringAsync(url, ct);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("Global Quote", out var quote))
            {
                _logger.LogWarning("No 'Global Quote' in response for {Symbol}. Rate-limited?", symbol);
                return null;
            }

            if (!quote.TryGetProperty("05. price", out _))
            {
                _logger.LogWarning("Empty quote for {Symbol}", symbol);
                return null;
            }

            return new StockQuote(
                Symbol: symbol,
                Price: decimal.Parse(quote.GetProperty("05. price").GetString()!),
                Open: decimal.Parse(quote.GetProperty("02. open").GetString()!),
                High: decimal.Parse(quote.GetProperty("03. high").GetString()!),
                Low: decimal.Parse(quote.GetProperty("04. low").GetString()!),
                Volume: long.Parse(quote.GetProperty("06. volume").GetString()!),
                Timestamp: DateTimeOffset.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch quote for {Symbol}", symbol);
            return null;
        }
    }
}
