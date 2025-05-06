using Microsoft.Extensions.Hosting;

namespace JobSeeker.WebScraper.MessageBroker.Providers.Base;

/// <summary>
///     Async message consumer
/// </summary>
public interface IMessageConsumer : IHostedService
{
    /// <summary>
    ///     Consume message
    /// </summary>
    Task ConsumeAsync(CancellationToken cancellationToken);
}