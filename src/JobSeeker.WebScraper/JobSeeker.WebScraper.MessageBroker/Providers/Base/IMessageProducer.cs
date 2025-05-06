namespace JobSeeker.WebScraper.MessageBroker.Providers.Base;

/// <summary>
///     Async message producer
/// </summary>
public interface IMessageProducer
{
    /// <summary>
    ///     Produce message
    /// </summary>
    /// <param name="data">Message</param>
    /// <typeparam name="T">Type of message</typeparam>
    Task ProduceAsync<T>(T data);
}