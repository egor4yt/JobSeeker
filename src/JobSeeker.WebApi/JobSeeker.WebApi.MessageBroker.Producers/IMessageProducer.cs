namespace JobSeeker.WebApi.MessageBroker.Producers;

public interface IMessageProducer<TMessage> where TMessage : class
{
    Task ProduceAsync(TMessage message, CancellationToken cancellationToken = default);
}