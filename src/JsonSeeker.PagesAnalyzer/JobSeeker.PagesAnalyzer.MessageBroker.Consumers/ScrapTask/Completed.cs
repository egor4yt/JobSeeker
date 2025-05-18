using Hangfire;
using JobSeeker.PagesAnalyzer.Application.Services.JobRunner;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.PagesAnalyzer.MessageBroker.Consumers.ScrapTask;

public class Completed(ILogger<Completed> logger, IBackgroundJobClient jobClient) : IConsumer<MessageBroker.Messages.ScrapTask.Completed>
{
    public Task Consume(ConsumeContext<MessageBroker.Messages.ScrapTask.Completed> context)
    {
        logger.LogDebug("New scrap task requested {ScrapTaskId}", context.Message.ScrapTaskId);

        var parameter = new Application.JobParameters.Common.AnalyzeScrapTaskResults
        {
            ScrapTaskId = context.Message.ScrapTaskId
        };
        jobClient.Enqueue<JobRunnerService>(x => x.RunAsync(parameter, null!, CancellationToken.None));

        return Task.CompletedTask;
    }
}