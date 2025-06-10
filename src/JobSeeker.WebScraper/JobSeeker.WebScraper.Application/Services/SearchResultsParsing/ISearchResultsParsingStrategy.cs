using JobSeeker.WebScraper.Application.Services.SearchResultsParsing.Models;
using JobSeeker.WebScraper.Domain.Entities;

namespace JobSeeker.WebScraper.Application.Services.SearchResultsParsing;

public interface ISearchResultsParsingStrategy
{
    Task<IList<SearchResult>> ParseAsync(ScrapTask scrapTask, CancellationToken cancellationToken);
}