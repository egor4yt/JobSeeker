using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Services.SearchResultsParsing;

public class SearchResultsParsingStrategyFactory(IServiceScopeFactory serviceScopeFactory) : ISearchResultsParsingStrategyFactory
{
    public ISearchResultsParsingStrategy? GetStrategy(ScrapSource scrapSource)
    {
        var domain = string.Join(".", scrapSource.Domain.Split(".").TakeLast(2));
        using var scope = serviceScopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<SearchResultsParsingStrategyFactory>>();

        try
        {
            return scope.ServiceProvider.GetRequiredKeyedService<ISearchResultsParsingStrategy>(domain);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Can't get ISearchResultsParsingStrategy");
        }

        return null;
    }
}