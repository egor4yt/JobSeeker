using JobSeeker.WebScraper.Application.Commands.Base;
using JobSeeker.WebScraper.Application.Commands.ScrapTasks.Create;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Consumers.ScrapTask;

public class Created(ILogger<Created> logger, ICommandHandler<CreateScrapTaskRequest> handler) : IConsumer<Messages.ScrapTask.Created>
{
    public async Task Consume(ConsumeContext<Messages.ScrapTask.Created> context)
    {
        await handler.ExecuteAsync(context.Message.ToCreateScrapTaskRequest(), CancellationToken.None);
        logger.LogDebug("New scrap task created for '{SearchText}'", context.Message.SearchText);
    }
}