using JobSeeker.WebScraper.Application.Commands.Base;
using JobSeeker.WebScraper.Application.Commands.ScrapTasks.Create;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Consumers.ScrapTask;

public class Created(ILogger<Created> logger, ICommandHandler<CreateScrapTaskRequest> handler) : IConsumer<Messages.ScrapTask.Created>
{
    public async Task Consume(ConsumeContext<Messages.ScrapTask.Created> context)
    {
        logger.LogDebug("New scrap task requested {SearchText}", context.Message.SearchText);
        await handler.ExecuteAsync(new CreateScrapTaskRequest(context.Message), CancellationToken.None);
    }
}