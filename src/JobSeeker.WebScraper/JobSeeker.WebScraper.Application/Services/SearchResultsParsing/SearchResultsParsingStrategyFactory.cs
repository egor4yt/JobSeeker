using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Services.SearchResultsParsing;

public class SearchResultsParsingStrategyFactory(IServiceScopeFactory serviceScopeFactory) : ISearchResultsParsingStrategyFactory
{
    public ISearchResultsParsingStrategy? GetStrategy(string domain)
    {
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