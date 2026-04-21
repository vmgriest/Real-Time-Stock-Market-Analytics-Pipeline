using Confluent.Kafka;
using StockPipeline.Ingestion;
using StockPipeline.Shared.Models;
using StockPipeline.Shared.Serialization;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    return new AlphaVantageClient(http, config["AlphaVantage:ApiKey"]!,
        sp.GetRequiredService<ILogger<AlphaVantageClient>>());
});

builder.Services.AddSingleton<IProducer<string, StockQuote>>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var producerConfig = new ProducerConfig
    {
        BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
        Acks = Acks.All,
        EnableIdempotence = true,
        MessageSendMaxRetries = 3,
    };
    return new ProducerBuilder<string, StockQuote>(producerConfig)
        .SetValueSerializer(new JsonKafkaSerializer<StockQuote>())
        .Build();
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
