using Confluent.Kafka;
using JobSeeker.WebScraper.MessageBroker.Providers.Base;
using JobSeeker.WebScraper.MessageBroker.Providers.Kafka.Serialization;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Providers.Kafka;

/// <summary>
///     Kafka message broker
/// </summary>
public class KafkaMessageProducer(ILogger<KafkaMessageProducer> logger, ProducerConfig producerConfig) : IMessageProducer
{
    /// <summary>
    ///     Message type : target topics
    /// </summary>
    private readonly Dictionary<Type, string[]> _messagesToTopicsMap = new Dictionary<Type, string[]>();

    public async Task ProduceAsync<T>(T data)
    {
        var topics = GetTopicsForMessage<T>();
        using var producer = new ProducerBuilder<Null, T>(producerConfig)
            .SetValueSerializer(new KafkaJsonSerializer<T>())
            .SetLogHandler((_, msg) => logger.LogDebug("Kafka sent a message with status {KafkaLevel}/{KafkaFacility}. Message: '{Message}'", msg.Level.ToString(), msg.Facility, msg.Message))
            .SetErrorHandler((_, error) => logger.LogError("Kafka couldn't send a message, error code: {KafkaErrorCode}. Reason: '{Reason}'", error.Code, error.Reason))
            .Build();

        foreach (var topic in topics)
        {
            try
            {
                var message = new Message<Null, T> { Value = data };
                var result = await producer.ProduceAsync(topic, message);
                logger.LogDebug("Message of type {Type} sent to topic '{Topic}' with status '{Status}'", typeof(T).ToString(), topic, result.Status);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Kafka couldn't send message of type '{Type}' to topic '{Topic}'", typeof(T).ToString(), topic);
            }
        }
    }

    /// <summary>
    ///     Get list of topics names for a message
    /// </summary>
    /// <typeparam name="TMessage">Type of message</typeparam>
    /// <returns>Kafka topics list</returns>
    /// <exception cref="InvalidOperationException">Message doesn't supported with kafka provider</exception>
    private string[] GetTopicsForMessage<TMessage>()
    {
        if (_messagesToTopicsMap.TryGetValue(typeof(TMessage), out var topics)) return topics;
        throw new InvalidOperationException("Unsupported message type");
    }
}