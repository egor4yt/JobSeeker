using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Producers;

public class KafkaMessageProducer<T>(ITopicProducer<T> producer, ILogger<KafkaMessageProducer<T>> logger) : IMessageProducer<T> where T : class
{
    public async Task ProduceAsync(T message, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Producing message type of {MessageType}", typeof(T).Name);
        await producer.Produce(message, cancellationToken);
    }
}