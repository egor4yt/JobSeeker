using Hangfire;
using JobSeeker.Deduplication.Application.Services.JobRunner;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.Deduplication.MessageBroker.Consumers.ScrapTask;

public class Analyzed(ILogger<Analyzed> logger, IBackgroundJobClient jobClient) : IConsumer<MessageBroker.Messages.ScrapTask.Analyzed>
{
    public Task Consume(ConsumeContext<MessageBroker.Messages.ScrapTask.Analyzed> context)
    {
        logger.LogDebug("New scrap task deduplication requested {ScrapTaskId}", context.Message.ScrapTaskId);

        var parameter = new Application.JobParameters.Common.SaveRawVacancies
        {
            ScrapTaskId = context.Message.ScrapTaskId
        };
        jobClient.Enqueue<JobRunnerService>(x => x.RunAsync(parameter, null!, CancellationToken.None));

        return Task.CompletedTask;
    }
}