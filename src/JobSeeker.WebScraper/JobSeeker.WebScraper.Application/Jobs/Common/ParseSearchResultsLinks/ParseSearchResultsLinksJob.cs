using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks;

public class ParseSearchResultsLinksJob(ILogger<ParseSearchResultsLinksJob> logger, ApplicationDbContext dbContext) : IJob<JobParameters.Common.ParseSearchResultsLinks>
{
    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Common.ParseSearchResultsLinks _parameter = null!;

    public async Task RunAsync(JobParameters.Common.ParseSearchResultsLinks parameter, CancellationToken cancellationToken)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        logger.LogInformation("Started parsing search results links job for scrap task {ScrapTaskId}", _parameter.ScrapTaskId);

        var results = await RunAsync();
        await SaveResultsAsync(results);

        logger.LogInformation("Finished parsing search results links job for scrap task {ScrapTaskId}", _parameter.ScrapTaskId);
    }

    private async Task<List<object>> RunAsync()
    {
        var scrapTask = await dbContext.ScrapTasks
            .Include(x => x.ScrapSources)
            .SingleAsync(x => x.Id == _parameter.ScrapTaskId, _cancellationToken);

        foreach (var scrapSource in scrapTask.ScrapSources)
        {
            logger.LogInformation("Trying to parse search results links for {SearchText} on {Domain}", scrapTask.SearchText, scrapSource.Domain);
            await Task.Delay(1000, _cancellationToken);
        }

        return [];
    }

    private async Task SaveResultsAsync(IList<object> results)
    {
        await Task.Delay(1000, _cancellationToken);

        logger.LogInformation("Saved {VacanciesCount} vacancies", results.Count);
    }
}