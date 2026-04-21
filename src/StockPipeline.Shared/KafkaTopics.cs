namespace StockPipeline.Shared;

public static class KafkaTopics
{
    public const string RawQuotes = "stock.quotes.raw";
    public const string EnrichedQuotes = "stock.quotes.enriched";
}
