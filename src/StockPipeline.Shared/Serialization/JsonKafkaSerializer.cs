using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace StockPipeline.Shared.Serialization;

public class JsonKafkaSerializer<T> : ISerializer<T>, IDeserializer<T>
{
    public byte[] Serialize(T data, SerializationContext context) =>
        Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) =>
        isNull ? default! : JsonSerializer.Deserialize<T>(data)!;
}
