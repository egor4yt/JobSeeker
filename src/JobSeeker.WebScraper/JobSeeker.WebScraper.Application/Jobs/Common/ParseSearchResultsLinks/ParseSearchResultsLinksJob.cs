using JobSeeker.WebScraper.Application.Jobs.Base;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks;

public class ParseSearchResultsLinksJob(ILogger<ParseSearchResultsLinksJob> logger) : IJob<JobParameters.Common.ParseSearchResultsLinks>
{
    private CancellationToken _cancellationToken = CancellationToken.None;
    private JobParameters.Common.ParseSearchResultsLinks _parameter = null!;

    public async Task RunAsync(JobParameters.Common.ParseSearchResultsLinks parameter, CancellationToken cancellationToken)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;

        var uri = new Uri(_parameter.BaseSearchUrl);
        var host = uri.Host.ToLower().Trim();

        logger.LogInformation("Started parsing search results links job for host {Host}", host);

        var results = await RunAsync();
        await SaveResultsAsync(results);

        logger.LogInformation("Finished parsing search results links job for host {Host}", host);
    }

    private async Task<List<object>> RunAsync()
    {
        await Task.Delay(1000, _cancellationToken);
        return [];
    }

    private async Task SaveResultsAsync(IList<object> results)
    {
        await Task.Delay(1000, _cancellationToken);

        logger.LogInformation("Saved {VacanciesCount} vacancies", results.Count);
    }
}