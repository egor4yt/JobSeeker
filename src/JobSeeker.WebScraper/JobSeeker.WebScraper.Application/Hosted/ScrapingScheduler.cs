using Hangfire;
using JobSeeker.WebScraper.Application.Services.JobRunner;
using JobSeeker.WebScraper.Shared.Constants;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Hosted;

public class ScrapingScheduler(IRecurringJobManager recurringJobManager, ILogger<ScrapingScheduler> logger) : IHostedService
{
    private const string JobId = "nightly-scraping";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var parameter = new JobParameters.Recurring.CreateScrapTasks();

        recurringJobManager.AddOrUpdate<JobRunnerService>(
            JobId,
            JobQueue.ScrapGroups,
            x => x.RunAsync(parameter, null!, cancellationToken),
            Cron.Daily(),
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local
            });

        logger.LogInformation("Configured recurring job {JobId}", JobId);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        recurringJobManager.RemoveIfExists("nightly-scraping");
        return Task.CompletedTask;
    }
}