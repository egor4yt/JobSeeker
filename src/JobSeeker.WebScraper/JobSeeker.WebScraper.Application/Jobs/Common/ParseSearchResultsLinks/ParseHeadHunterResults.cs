using JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks.Models;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory;
using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks;

public partial class ParseSearchResultsLinksJob
{
    private async Task<List<SearchResult>> ParseHeadHunterResultsAsync(ScrapTask scrapTask)
    {
        logger.LogDebug("Started parsing head hunter results for {SearchText}", scrapTask.SearchText);

        var response = new List<SearchResult>();

        var query = new Dictionary<string, string[]>
        {
            ["search_field"] = ["name", "company_name", "description"],
            ["text"] = [Uri.EscapeDataString(scrapTask.SearchText)]
        };

        var baseUrl = "https://hh.ru/search/vacancy?" + string.Join("&", query.SelectMany(pair => pair.Value.Select(value => $"{pair.Key}={value}")));

        var currentPage = 0;
        int? lastPage = null;

        await using var session = await playwrightFactory.CreateSessionAsync("hh.ru", _cancellationToken);

        var tasks = Enumerable.Range(0, 10).Select(x => ParseHeadHunterPageResultsAsync(x, baseUrl + $"&page={x}", session)).ToList();
        await Task.WhenAll(tasks);
        foreach (var task in tasks)
        {
            response.AddRange(task.Result);
        }

        logger.LogDebug("Completed head hunter results for {SearchText}", scrapTask.SearchText);
        return response;
    }

    private async Task<List<SearchResult>> ParseHeadHunterPageResultsAsync(int pageIndex, string url, PlaywrightSession session)
    {
        var response = new List<SearchResult>();

        var page = await session.LoadPageAsync(url, _cancellationToken);
        var linksLocators = await page.Locator("a[data-qa='serp-item__title']").AllAsync();

        foreach (var linkLocator in linksLocators)
        {
            var href = await linkLocator.GetAttributeAsync("href");
            if (href == null || href.Contains("adsrv.hh.ru"))
            {
                logger.LogDebug("Found invalid link {InvalidLink}", href);
                continue;
            }

            if (href.Contains('?')) href = href.Split('?')[0];

            var newSearchResult = new SearchResult();
            newSearchResult.ResultLink = href;
            response.Add(newSearchResult);
        }

        await page.CloseAsync();

        logger.LogDebug("Page {PageIndex} scraped, found: {LinksCount}", pageIndex, linksLocators.Count);


        return response;
    }
}