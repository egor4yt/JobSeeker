namespace JobSeeker.WebApi.MessageBroker.Producers;

public interface IMessageProducer<T> where T : class
{
    Task ProduceAsync(T message, CancellationToken cancellationToken = default);
}