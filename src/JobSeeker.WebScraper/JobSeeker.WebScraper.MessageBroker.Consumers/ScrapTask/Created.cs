using Hangfire;
using JobSeeker.WebScraper.Application.Services.JobRunner;
using JobSeeker.WebScraper.Shared.Constants;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Consumers.ScrapTask;

public class Created(ILogger<Created> logger, IBackgroundJobClient jobClient) : IConsumer<Messages.ScrapTask.Created>
{
    public async Task Consume(ConsumeContext<Messages.ScrapTask.Created> context)
    {
        logger.LogDebug("New scrap task created {ScrapTaskId}", context.Message.ScrapTaskId);
        var parameter = new Application.JobParameters.Common.ParseSearchResultsLinks
        {
            ScrapTaskId = context.Message.ScrapTaskId
        };
        jobClient.Enqueue<JobRunnerService>(JobQueue.ScrapTasks, x => x.RunAsync(parameter, null!, CancellationToken.None));
    }
}