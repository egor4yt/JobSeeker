using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.Deduplication.MessageBroker.Producers;

public class KafkaMessageProducer<TMessage>(ITopicProducer<TMessage> producer, ILogger<KafkaMessageProducer<TMessage>> logger) : IMessageProducer<TMessage> where TMessage : class
{
    public async Task ProduceAsync(TMessage message, CancellationToken cancellationToken = default)
    {
        logger.LogDebug("Producing message type of {MessageType}", typeof(TMessage).Name);
        await producer.Produce(message, cancellationToken);
    }
}