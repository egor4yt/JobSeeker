using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.PagesAnalyzer.MessageBroker.Consumers.ScrapTask;

public class Completed(ILogger<Completed> logger) : IConsumer<MessageBroker.Messages.ScrapTask.Completed>
{
    public Task Consume(ConsumeContext<MessageBroker.Messages.ScrapTask.Completed> context)
    {
        logger.LogDebug("New scrap task requested {ScrapTaskId}", context.Message.ScrapTaskId);
        return Task.CompletedTask;
    }
}