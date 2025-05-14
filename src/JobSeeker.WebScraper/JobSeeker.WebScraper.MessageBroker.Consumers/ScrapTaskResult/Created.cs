using Hangfire;
using JobSeeker.WebScraper.Application.Services.JobRunner;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Consumers.ScrapTaskResult;

public class Created(ILogger<Created> logger, IBackgroundJobClient jobClient) : IConsumer<Messages.ScrapTaskResult.Created>
{
    public async Task Consume(ConsumeContext<Messages.ScrapTaskResult.Created> context)
    {
        logger.LogDebug("New scrap task result {ScrapTaskId}", context.Message.ScrapTaskId);
        var parameter = new Application.JobParameters.Common.UploadSearchResults
        {
            ScrapTaskId = context.Message.ScrapTaskId
        };
        jobClient.Enqueue<JobRunnerService>(x => x.RunAsync(parameter, null!, CancellationToken.None));
    }
}