using Hangfire;
using JobSeeker.WebScraper.Application.Commands.Base;
using JobSeeker.WebScraper.Application.Services.JobRunner;
using JobSeeker.WebScraper.Domain.Entities;
using JobSeeker.WebScraper.Persistence;

namespace JobSeeker.WebScraper.Application.Commands.ScrapTasks.Create;

public class CreateScrapTaskHandler(ApplicationDbContext dbContext, IBackgroundJobClient jobClient) : ICommandHandler<CreateScrapTaskRequest>
{
    public async Task ExecuteAsync(CreateScrapTaskRequest request, CancellationToken cancellationToken = default)
    {
        var scrapSources = dbContext.ScrapSources
            .Where(x => request.SearchDomains.Contains(x.Domain))
            .ToList();

        var newScrapSources = request.SearchDomains
            .Except(scrapSources.Select(x => x.Domain))
            .Select(x => new ScrapSource
            {
                Domain = x.ToLower().Trim()
            })
            .ToList();

        await dbContext.ScrapSources.AddRangeAsync(newScrapSources, cancellationToken);

        var newScrapTask = new ScrapTask
        {
            SearchText = request.SearchText,
            ExcludeWordsList = request.ExcludeWordsList,
            ScrapSources = new List<ScrapSource>()
        };
        foreach (var scrapSource in scrapSources.Concat(newScrapSources))
        {
            newScrapTask.ScrapSources.Add(scrapSource);
        }

        await dbContext.ScrapTasks.AddAsync(newScrapTask, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var jobParameter = new JobParameters.Common.ParseSearchResultsLinks
        {
            ScrapTaskId = newScrapTask.Id
        };
        jobClient.Enqueue<JobRunnerService>(x => x.RunAsync(jobParameter, null!, CancellationToken.None));
    }
}