using JobSeeker.WebScraper.Application.Jobs.Base;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks;

public class ParseSearchResultsLinksJob(ILogger<ParseSearchResultsLinksJob> logger) : IJob<JobParameters.Common.ParseSearchResultsLinks>
{
    private JobParameters.Common.ParseSearchResultsLinks _parameter = null!;
    private CancellationToken _cancellationToken = CancellationToken.None;
    
    public async Task RunAsync(JobParameters.Common.ParseSearchResultsLinks parameter, CancellationToken cancellationToken)
    {
        _parameter = parameter;
        _cancellationToken = cancellationToken;
        
        var uri = new Uri(parameter.BaseSearchUrl);
        var host = uri.Host.ToLower().Trim();
        var scopeData = new Dictionary<string, object>
        {
            ["Host"] = host,
            ["JobId"] = _parameter.JobId,
            ["JobType"] = nameof(ParseSearchResultsLinksJob)
        };
        
        using (logger.BeginScope(scopeData))
        {
            logger.LogInformation("Job started");

            var results = await RunAsync();
            await SaveResultsAsync(results);
            
            logger.LogInformation("Job finished");
        }
        
        throw new NotImplementedException();
    }

    private async Task<List<object>> RunAsync()
    {
        await Task.Delay(1000, _cancellationToken);

        throw new NotImplementedException();
    }

    private async Task SaveResultsAsync(IList<object> results)
    {
        await Task.Delay(1000, _cancellationToken);
        
        logger.LogInformation("Saved {VacanciesCount} vacancies", results.Count);

        throw new NotImplementedException();
    }
}