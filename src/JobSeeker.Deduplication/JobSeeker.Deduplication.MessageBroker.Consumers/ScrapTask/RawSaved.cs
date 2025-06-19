using Hangfire;
using JobSeeker.Deduplication.Application.Services.JobRunner;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.Deduplication.MessageBroker.Consumers.ScrapTask;

public class RawSaved(ILogger<RawSaved> logger, IBackgroundJobClient jobClient) : IConsumer<MessageBroker.Messages.ScrapTask.RawSaved>
{
    public Task Consume(ConsumeContext<MessageBroker.Messages.ScrapTask.RawSaved> context)
    {
        logger.LogDebug("New specialization requested {ScrapTaskId}", context.Message.DownloadKey);

        // var parameter = new Application.JobParameters.Common.CalculateLsh
        // {
        //     DownloadKey = context.Message.DownloadKey
        // };
        // jobClient.Enqueue<JobRunnerService>(x => x.RunAsync(parameter, null!, CancellationToken.None));

        return Task.CompletedTask;
    }
}