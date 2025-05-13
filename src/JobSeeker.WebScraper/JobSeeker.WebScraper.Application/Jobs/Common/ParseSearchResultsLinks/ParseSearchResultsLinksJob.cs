using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks.Models;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory;
using JobSeeker.WebScraper.Domain.Entities;
using JobSeeker.WebScraper.MessageBroker.Producers;
using JobSeeker.WebScraper.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks;

public partial class ParseSearchResultsLinksJob(
    ILogger<ParseSearchResultsLinksJob> logger,
    ApplicationDbContext dbContext,
    PlaywrightFactoryService playwrightFactory,
    IMessageProducer<MessageBroker.Messages.ScrapTaskResult.Created> messageProducer
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
            List<SearchResult> newResults;
            if (scrapSource.Domain.EndsWith("hh.ru")) newResults = await ParseHeadHunterResultsAsync(scrapTask);
            // else if (scrapSource.Domain.EndsWith("other-domain.com")) newResults = await ParseOtherDomainResultsAsync(scrapTask);
            else
            {
                logger.LogWarning("Unsupported domain {Domain}", scrapSource.Domain);
                continue;
            }

            if (newResults.Count == 0) logger.LogWarning("Not found results for domain {Domain}, scrap task {ScrapTaskId}", scrapSource.Domain, _parameter.ScrapTaskId);
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