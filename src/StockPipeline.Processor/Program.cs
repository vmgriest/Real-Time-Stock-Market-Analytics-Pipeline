using Confluent.Kafka;
using StockPipeline.Processor;
using StockPipeline.Shared.Models;
using StockPipeline.Shared.Serialization;

var builder = Host.CreateApplicationBuilder(args);
var config = builder.Configuration;

builder.Services.AddSingleton<MovingAverageCalculator>();

builder.Services.AddSingleton(_ =>
    new DatabaseWriter(config.GetConnectionString("Postgres")!));

builder.Services.AddSingleton<IConsumer<string, StockQuote>>(_ =>
{
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = config["Kafka:BootstrapServers"] ?? "localhost:9092",
        GroupId = "stock-processor",
        AutoOffsetReset = AutoOffsetReset.Earliest,
        EnableAutoCommit = false,
    };
    return new ConsumerBuilder<string, StockQuote>(consumerConfig)
        .SetValueDeserializer(new JsonKafkaSerializer<StockQuote>())
        .Build();
});

builder.Services.AddHostedService<Worker>();

builder.Build().Run();
