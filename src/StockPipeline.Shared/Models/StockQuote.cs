namespace StockPipeline.Shared.Models;

public record StockQuote(
    string Symbol,
    decimal Price,
    decimal Open,
    decimal High,
    decimal Low,
    long Volume,
    DateTimeOffset Timestamp
);
