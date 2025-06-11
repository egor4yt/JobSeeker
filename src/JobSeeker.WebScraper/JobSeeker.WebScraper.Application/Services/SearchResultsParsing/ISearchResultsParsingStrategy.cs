using JobSeeker.WebScraper.Application.Services.SearchResultsParsing.Models;
using JobSeeker.WebScraper.Domain.Entities;

namespace JobSeeker.WebScraper.Application.Services.SearchResultsParsing;

/// <summary>
///     Defines the strategy for parsing search results from a given scraping task and domain.
/// </summary>
public interface ISearchResultsParsingStrategy
{
    /// <summary>
    ///     Parses search results from a specific source for a given scraping task.
    /// </summary>
    /// <param name="scrapTask">
    ///     The scraping task containing search parameters and related metadata.
    /// </param>
    /// <param name="cancellationToken">
    ///     The cancellation token to observe during the asynchronous operation.
    /// </param>
    /// <returns>
    ///     A list of parsed search results.
    /// </returns>
    Task<IList<SearchResult>> ParseAsync(ScrapTask scrapTask, CancellationToken cancellationToken);
}