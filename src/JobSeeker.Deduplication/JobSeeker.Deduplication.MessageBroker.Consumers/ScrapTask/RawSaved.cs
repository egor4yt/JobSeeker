using Hangfire;
using JobSeeker.Deduplication.Application.Services.JobRunner;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.Deduplication.MessageBroker.Consumers.ScrapTask;

public class RawSaved(ILogger<RawSaved> logger, IBackgroundJobClient jobClient) : IConsumer<MessageBroker.Messages.ScrapTask.RawSaved>
{
    public Task Consume(ConsumeContext<MessageBroker.Messages.ScrapTask.RawSaved> context)
    {
        logger.LogDebug("New vacancy group deduplication requested {@Message}", context.Message);

        var parameter = new Application.JobParameters.Common.DeduplicateVacancies
        {
            OccupationGroup = context.Message.OccupationGroup,
            Occupation = context.Message.Occupation,
            Specialization = context.Message.Specialization,
            SkillTag = context.Message.SkillTag
        };
        jobClient.Enqueue<JobRunnerService>(x => x.RunAsync(parameter, null!, CancellationToken.None));

        return Task.CompletedTask;
    }
}