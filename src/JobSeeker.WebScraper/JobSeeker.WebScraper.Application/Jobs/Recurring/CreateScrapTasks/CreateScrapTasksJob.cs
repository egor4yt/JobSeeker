using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Domain.Entities;
using JobSeeker.WebScraper.MessageBroker.Producers;
using JobSeeker.WebScraper.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Jobs.Recurring.CreateScrapTasks;

public class CreateScrapTasksJob(
    ApplicationDbContext dbContext,
    ILogger<CreateScrapTasksJob> logger,
    IMessageProducer<MessageBroker.Messages.ScrapTask.Created> messageProducer) : IJob<JobParameters.Recurring.CreateScrapTasks>
{
    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Recurring.CreateScrapTasks _parameter = null!;

    public async Task RunAsync(JobParameters.Recurring.CreateScrapTasks parameter, CancellationToken cancellationToken = default)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        logger.LogInformation("Started creating scrap tasks");

        await RunAsync();

        logger.LogInformation("Scrap tasks created successfully");
    }

    private async Task RunAsync()
    {
        var groups = await dbContext.ScrapGroups
            .Include(x => x.ScrapTaskConfigurations)
            .OrderBy(g => g.Priority)
            .ToListAsync(_cancellationToken);

        var newScrapTasks = groups.SelectMany(CreateScrapTasks).ToList();

        await dbContext.AddRangeAsync(newScrapTasks, _cancellationToken);
        await dbContext.SaveChangesAsync(_cancellationToken);

        var messages = newScrapTasks.Select(x => new MessageBroker.Messages.ScrapTask.Created { ScrapTaskId = x.Id });
        foreach (var message in messages)
        {
            await messageProducer.ProduceAsync(message, _cancellationToken);
        }
    }

    private static List<ScrapTask> CreateScrapTasks(ScrapGroup group)
    {
        var response = group.ScrapTaskConfigurations
            .Select(configuration => new ScrapTask
            {
                ScrapGroupId = group.Id,
                CreatedAt = DateTime.UtcNow,
                Priority = configuration.Priority,
                Entrypoint = configuration.Entrypoint
            }).ToList();

        return response;
    }
}