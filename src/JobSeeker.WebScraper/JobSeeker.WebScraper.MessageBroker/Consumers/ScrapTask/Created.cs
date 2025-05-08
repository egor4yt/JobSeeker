using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Consumers.ScrapTask;

public class Created(ILogger<Created> logger) : IConsumer<Messages.ScrapTask.Created>
{
    public async Task Consume(ConsumeContext<Messages.ScrapTask.Created> context)
    {
        logger.LogDebug("New scrap task created for '{SearchText}'", context.Message.SearchText);
    }
}