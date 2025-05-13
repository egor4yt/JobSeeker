using JobSeeker.WebScraper.Application.Jobs.Base;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory;
using JobSeeker.WebScraper.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Jobs.Common.UploadSearchResults;

public class UploadSearchResultsJob(ILogger<UploadSearchResultsJob> logger, ApplicationDbContext dbContext, PlaywrightFactoryService playwrightFactory) : IJob<JobParameters.Common.UploadSearchResults>
{
    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Common.UploadSearchResults _parameter = null!;

    public async Task RunAsync(JobParameters.Common.UploadSearchResults parameter, CancellationToken cancellationToken = default)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        logger.LogInformation("Started uploading scrap task results {ScrapTaskId}", _parameter.ScrapTaskId);

        await RunAsync();

        logger.LogInformation("Finished uploading scrap task results {ScrapTaskId}", _parameter.ScrapTaskId);
    }

    private async Task RunAsync()
    {
        var scrapTaskResults = await dbContext.ScrapTaskResults
            .Where(x => x.ScrapTaskId == _parameter.ScrapTaskId
                        && x.Uploaded == false)
            .ToListAsync(_cancellationToken);
    }
}