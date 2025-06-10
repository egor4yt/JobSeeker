using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Application.Services.SearchResultsParsing;
using JobSeeker.WebScraper.Application.Services.SearchResultsParsing.Models;
using JobSeeker.WebScraper.Domain.Entities;
using JobSeeker.WebScraper.MessageBroker.Producers;
using JobSeeker.WebScraper.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks;

public class ParseSearchResultsLinksJob(
    ILogger<ParseSearchResultsLinksJob> logger,
    ApplicationDbContext dbContext,
    IMessageProducer<MessageBroker.Messages.ScrapTaskResult.Created> messageProducer,
    ISearchResultsParsingStrategyFactory searchResultsParsingStrategyFactory
) : IJob<JobParameters.Common.ParseSearchResultsLinks>
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
        if (results.Count == 0) return;

        var message = new MessageBroker.Messages.ScrapTaskResult.Created
        {
            ScrapTaskId = _parameter.ScrapTaskId
        };
        await messageProducer.ProduceAsync(message, _cancellationToken);
    }

    private async Task<List<SearchResult>> RunAsync()
    {
        var scrapTask = await dbContext.ScrapTasks
            .Include(x => x.ScrapSources)
            .SingleAsync(x => x.Id == _parameter.ScrapTaskId, _cancellationToken);

        var results = new List<SearchResult>();

        foreach (var scrapSource in scrapTask.ScrapSources)
        {
            var strategy = searchResultsParsingStrategyFactory.GetStrategy(scrapSource);
            if (strategy == null)
            {
                logger.LogWarning("Unsupported domain {Domain}, scrap task {ScrapTaskId}", scrapSource.Domain, _parameter.ScrapTaskId);
                continue;
            }
          
            var newResults = await strategy.ParseAsync(scrapTask, _cancellationToken);
            if (newResults.Count == 0)
                logger.LogWarning("Not found results for domain {Domain}, scrap task {ScrapTaskId}", scrapSource.Domain, _parameter.ScrapTaskId);
            
            results.AddRange(newResults);
        }

        return results;
    }

    private async Task SaveResultsAsync(IList<SearchResult> results)
    {
        var entitiesToSave = results.Select(x => new ScrapTaskResult
        {
            Uploaded = false,
            Link = x.ResultLink,
            ScrapTaskId = _parameter.ScrapTaskId
        });

        await dbContext.ScrapTaskResults.AddRangeAsync(entitiesToSave, _cancellationToken);
        await dbContext.SaveChangesAsync(_cancellationToken);

        logger.LogInformation("Saved {ScrapTaskResultsCount} vacancies", results.Count);
    }
}