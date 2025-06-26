using Hangfire;
using JobSeeker.Deduplication.Application.Services.JobRunner;
using JobSeeker.Deduplication.MessageBroker.Consumers.ScrapTask;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.Deduplication.MessageBroker.Consumers.RawVacancy;

public class Deduplicated(ILogger<Deduplicated> logger, IBackgroundJobClient jobClient) : IConsumer<Messages.RawVacancy.Deduplicated>
{
    public Task Consume(ConsumeContext<Messages.RawVacancy.Deduplicated> context)
    {
        logger.LogDebug("New upload vacancy group requested {@Message}", context.Message);

        var parameter = new Application.JobParameters.Common.UploadVacancyGroup
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