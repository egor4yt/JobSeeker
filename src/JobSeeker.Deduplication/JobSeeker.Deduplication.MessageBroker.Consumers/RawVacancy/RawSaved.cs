using Hangfire;
using JobSeeker.Deduplication.Application.Services.JobRunner;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.Deduplication.MessageBroker.Consumers.RawVacancy;

public class RawSaved(ILogger<RawSaved> logger, IBackgroundJobClient jobClient) : IConsumer<Messages.RawVacancy.RawSaved>
{
    public Task Consume(ConsumeContext<Messages.RawVacancy.RawSaved> context)
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