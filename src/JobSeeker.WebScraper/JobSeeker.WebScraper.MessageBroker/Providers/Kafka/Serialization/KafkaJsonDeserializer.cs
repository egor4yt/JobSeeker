using Confluent.Kafka;

namespace JobSeeker.WebScraper.MessageBroker.Providers.Kafka.Serialization;

public class KafkaJsonDeserializer(Dictionary<string, Type> deserializationMap) : IDeserializer<object?>
{
    public object? Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        if (isNull) return default;

        var response = System.Text.Json.JsonSerializer.Deserialize(data, deserializationMap[context.Topic.ToLower()]);
        return response;
    }
}