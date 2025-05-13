using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Consumers.ScrapTaskResult;

public class Created(ILogger<Created> logger) : IConsumer<Messages.ScrapTaskResult.Created>
{
    public async Task Consume(ConsumeContext<Messages.ScrapTaskResult.Created> context)
    {
        logger.LogDebug("New scrap task result {ScrapTaskId}", context.Message.ScrapTaskId);
    }
}