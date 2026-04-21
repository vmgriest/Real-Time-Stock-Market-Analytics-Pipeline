namespace StockPipeline.Shared.Models;

public record EnrichedQuote(
    string Symbol,
    decimal Price,
    decimal Open,
    decimal High,
    decimal Low,
    long Volume,
    DateTimeOffset Timestamp,
    decimal? Ma5,
    decimal? Ma20,
    decimal? Ma50,
    decimal? PriceChangePct
);
