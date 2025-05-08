using MassTransit;

namespace JobSeeker.WebApi.MessageBroker.Producers;

public class KafkaMessageProducer<T>(ITopicProducer<T> producer) : IMessageProducer<T> where T : class
{
    public async Task ProduceAsync(T message, CancellationToken cancellationToken = default)
    {
        await producer.Produce(message, cancellationToken);
    }
}