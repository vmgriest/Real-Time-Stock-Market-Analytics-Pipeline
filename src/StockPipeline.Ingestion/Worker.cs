using Confluent.Kafka;
using StockPipeline.Shared;
using StockPipeline.Shared.Models;

namespace StockPipeline.Ingestion;

public class Worker : BackgroundService
{
    private readonly AlphaVantageClient _avClient;
    private readonly IProducer<string, StockQuote> _producer;
    private readonly string[] _symbols;
    private readonly TimeSpan _pollInterval;
    // Alpha Vantage free tier: 5 calls/min — spread symbols across the interval
    private readonly TimeSpan _symbolDelay;
    private readonly ILogger<Worker> _logger;

    public Worker(AlphaVantageClient avClient, IProducer<string, StockQuote> producer,
        IConfiguration config, ILogger<Worker> logger)
    {
        _avClient = avClient;
        _producer = producer;
        _symbols = config.GetSection("Symbols").Get<string[]>()!;
        _pollInterval = TimeSpan.FromSeconds(config.GetValue<int>("PollIntervalSeconds", 60));
        _symbolDelay = TimeSpan.FromSeconds(config.GetValue<int>("SymbolDelaySeconds", 12));
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ingestion started. Tracking: {Symbols}", string.Join(", ", _symbols));

        while (!stoppingToken.IsCancellationRequested)
        {
            await PollAllSymbolsAsync(stoppingToken);
            await Task.Delay(_pollInterval, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task PollAllSymbolsAsync(CancellationToken ct)
    {
        foreach (var symbol in _symbols)
        {
            var quote = await _avClient.GetLatestQuoteAsync(symbol, ct);

            if (quote is not null)
            {
                await _producer.ProduceAsync(
                    KafkaTopics.RawQuotes,
                    new Message<string, StockQuote> { Key = symbol, Value = quote },
                    ct);

                _logger.LogInformation("{Symbol}: ${Price} vol={Volume}", symbol, quote.Price, quote.Volume);
            }

            await Task.Delay(_symbolDelay, ct).ConfigureAwait(false);
        }
    }

    public override void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
        base.Dispose();
    }
}
