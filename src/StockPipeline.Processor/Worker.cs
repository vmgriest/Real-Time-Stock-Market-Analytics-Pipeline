using Confluent.Kafka;
using StockPipeline.Shared;
using StockPipeline.Shared.Models;

namespace StockPipeline.Processor;

public class Worker : BackgroundService
{
    private readonly IConsumer<string, StockQuote> _consumer;
    private readonly MovingAverageCalculator _calculator;
    private readonly DatabaseWriter _db;
    private readonly ILogger<Worker> _logger;

    public Worker(IConsumer<string, StockQuote> consumer, MovingAverageCalculator calculator,
        DatabaseWriter db, ILogger<Worker> logger)
    {
        _consumer = consumer;
        _calculator = calculator;
        _db = db;
        _logger = logger;
    }

    // Consume is synchronous/blocking — offload to thread pool so we don't starve the host
    protected override Task ExecuteAsync(CancellationToken stoppingToken) =>
        Task.Run(async () =>
        {
            _consumer.Subscribe(KafkaTopics.RawQuotes);
            _logger.LogInformation("Processor consuming from {Topic}", KafkaTopics.RawQuotes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    var quote = result.Message.Value;

                    var enriched = _calculator.Enrich(quote);

                    await _db.WriteQuoteAsync(quote);
                    await _db.WriteEnrichedAsync(enriched);

                    _consumer.Commit(result);

                    _logger.LogInformation(
                        "{Symbol}: ${Price:F2} | MA5={Ma5} MA20={Ma20} MA50={Ma50} Δ={Chg}%",
                        enriched.Symbol, enriched.Price,
                        enriched.Ma5?.ToString("F2") ?? "—",
                        enriched.Ma20?.ToString("F2") ?? "—",
                        enriched.Ma50?.ToString("F2") ?? "—",
                        enriched.PriceChangePct?.ToString("F4") ?? "—");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Processing error");
                }
            }

            _consumer.Close();
        }, stoppingToken);

    public override void Dispose()
    {
        _consumer.Dispose();
        base.Dispose();
    }
}
