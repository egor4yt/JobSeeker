using JobSeeker.WebScraper.Domain.Entities;

namespace JobSeeker.WebScraper.Application.Services.SearchResultsParsing;

/// <summary>
///     Factory interface for retrieving a search results parsing strategy
/// </summary>
public interface ISearchResultsParsingStrategyFactory
{
    /// <summary>
    ///     Retrieves the appropriate search results parsing strategy based on the specified scrap source.
    /// </summary>
    /// <param name="scrapSource">The source of the scrap task.</param>
    /// <returns>
    ///     An implementation of <see cref="ISearchResultsParsingStrategy" /> for the
    ///     provided scrap source, or null if no suitable strategy is found.
    /// </returns>
    ISearchResultsParsingStrategy? GetStrategy(ScrapSource scrapSource);
}