using JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory;
using JobSeeker.WebScraper.Application.Services.SearchResultsParsing.Models;
using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace JobSeeker.WebScraper.Application.Services.SearchResultsParsing;

/// <summary>
///     Implements the strategy for parsing search results from the "habr.com" domain.
/// </summary>
public class HabrSearchResultsParsingStrategy(ILogger<ParseSearchResultsLinksJob> logger, PlaywrightFactoryService playwrightFactory) : ISearchResultsParsingStrategy
{
    public const string Domain = "habr.com";

    public async Task<IList<SearchResult>> ParseAsync(ScrapTask scrapTask, CancellationToken cancellationToken)
    {
        var response = new List<SearchResult>();

        await using var session = await playwrightFactory.CreateSessionAsync("habr.com", cancellationToken);

        var page = await session.LoadPageAsync(scrapTask.Entrypoint, cancellationToken);
        response.AddRange(await ParsePageResultsAsync(page));

        var searchTotalText = await page.Locator("div.search-total").TextContentAsync();

        if (string.IsNullOrWhiteSpace(searchTotalText))
        {
            logger.LogWarning("Can't find last page number {Url}", scrapTask.Entrypoint);
            return await ParsePageResultsAsync(page);
        }

        var lastPage = 0;
        foreach (var searchTotalTextPart in searchTotalText.Split(' '))
        {
            if (int.TryParse(searchTotalTextPart, out var parsedTotalItems))
            {
                var itemsPerPage = response.Count;
                lastPage = parsedTotalItems / itemsPerPage;
                if (parsedTotalItems % itemsPerPage != 0) lastPage += 1;

                break;
            }
        }

        if (lastPage == 0)
        {
            logger.LogWarning("Can't find last page number {Url}", scrapTask.Entrypoint);
            return response;
        }

        if (lastPage == 1) return response;
        
        await page.CloseAsync();

        var tasks = Enumerable.Range(2, lastPage - 1).Select(async x =>
        {
            var url = scrapTask.Entrypoint + $"&page={x}";
            List<SearchResult> results = [];
            IPage? currentPage = null;

            try
            {
                currentPage = await session.LoadPageAsync(url, cancellationToken);
                results = await ParsePageResultsAsync(currentPage);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to open page {Url}", url);
            }
            finally
            {
                if (currentPage != null) await currentPage.CloseAsync();
            }

            return results;
        }).ToList();

        var taskResults = await Task.WhenAll(tasks);
        response.AddRange(taskResults.SelectMany(x => x));

        return response;
    }

    private async Task<List<SearchResult>> ParsePageResultsAsync(IPage page)
    {
        var response = new List<SearchResult>();
        var linksLocators = await page.Locator("a.vacancy-card__title-link").AllAsync();

        foreach (var linkLocator in linksLocators)
        {
            var href = await linkLocator.GetAttributeAsync("href");
            if (href == null)
            {
                logger.LogDebug("Found invalid link {InvalidLink}", href);
                continue;
            }

            if (href.Contains('?')) href = href.Split('?')[0];

            var newSearchResult = new SearchResult();
            var currentUrl = new Uri(page.Url);
            newSearchResult.ResultLink = $"{currentUrl.Scheme}://{currentUrl.Host}{href}";
            response.Add(newSearchResult);
        }

        logger.LogDebug("Page {Url} scraped, found: {LinksCount}", page.Url, linksLocators.Count);

        return response;
    }
}