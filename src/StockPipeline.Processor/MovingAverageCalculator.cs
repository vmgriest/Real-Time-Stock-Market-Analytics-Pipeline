using StockPipeline.Shared.Models;

namespace StockPipeline.Processor;

public class MovingAverageCalculator
{
    private readonly Dictionary<string, Queue<decimal>> _windows = new();
    private readonly Dictionary<string, decimal> _previousPrices = new();

    public EnrichedQuote Enrich(StockQuote quote)
    {
        if (!_windows.TryGetValue(quote.Symbol, out var window))
        {
            window = new Queue<decimal>();
            _windows[quote.Symbol] = window;
        }

        window.Enqueue(quote.Price);
        if (window.Count > 50)
            window.Dequeue();

        var prices = window.ToArray();

        decimal? ma5  = prices.Length >= 5  ? prices[^5..].Average()  : null;
        decimal? ma20 = prices.Length >= 20 ? prices[^20..].Average() : null;
        decimal? ma50 = prices.Length >= 50 ? prices.Average()        : null;

        decimal? changePct = null;
        if (_previousPrices.TryGetValue(quote.Symbol, out var prev) && prev != 0)
            changePct = Math.Round((quote.Price - prev) / prev * 100, 4);

        _previousPrices[quote.Symbol] = quote.Price;

        return new EnrichedQuote(
            quote.Symbol, quote.Price, quote.Open, quote.High, quote.Low, quote.Volume,
            quote.Timestamp, ma5, ma20, ma50, changePct
        );
    }
}
