using Hangfire;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebApi.MessageBroker.Consumers.DeduplicatedVacancy;

public class Uploaded(ILogger<Uploaded> logger, IBackgroundJobClient jobClient) : IConsumer<Messages.DeduplicatedVacancy.Uploaded>
{
    public Task Consume(ConsumeContext<Messages.DeduplicatedVacancy.Uploaded> context)
    {
        logger.LogDebug("New deduplicated vacancies group uploaded {@Message}", context.Message);

        // var parameter = new Application.JobParameters.Common.UploadVacancyGroup
        // {
        //     OccupationGroup = context.Message.OccupationGroup,
        //     Occupation = context.Message.Occupation,
        //     Specialization = context.Message.Specialization,
        //     SkillTag = context.Message.SkillTag
        // };
        // jobClient.Enqueue<JobRunnerService>(x => x.RunAsync(parameter, null!, CancellationToken.None));

        return Task.CompletedTask;
    }
}