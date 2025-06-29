﻿using JobSeeker.WebScraper.Application.Jobs.Common.ParseSearchResultsLinks;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory;
using JobSeeker.WebScraper.Application.Services.SearchResultsParsing.Models;
using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace JobSeeker.WebScraper.Application.Services.SearchResultsParsing;

/// <summary>
///     Implements the strategy for parsing search results from the "hh.ru" domain.
/// </summary>
public class HhSearchResultsParsingStrategy(ILogger<ParseSearchResultsLinksJob> logger, PlaywrightFactoryService playwrightFactory) : ISearchResultsParsingStrategy
{
    public const string Domain = "hh.ru";

    public async Task<IList<SearchResult>> ParseAsync(ScrapTask scrapTask, CancellationToken cancellationToken)
    {
        // logger.LogDebug("Started parsing head hunter results for {SearchText}", scrapTask.SearchText);

        var response = new List<SearchResult>();

        await using var session = await playwrightFactory.CreateSessionAsync("hh.ru", cancellationToken);

        var page = await session.LoadPageAsync(scrapTask.Entrypoint, cancellationToken);
        var lastPagerItem = page.Locator("a[data-qa='pager-page']").Last;

        if (await lastPagerItem.CountAsync() == 0
            || int.TryParse(await lastPagerItem.TextContentAsync(), out var lastPage) == false)
        {
            logger.LogWarning("Can't find last page number {Url}", scrapTask.Entrypoint);
            return await ParseHeadHunterPageResultsAsync(page);
        }

        response.AddRange(await ParseHeadHunterPageResultsAsync(page));
        await page.CloseAsync();

        var tasks = Enumerable.Range(1, lastPage - 1).Select(async x =>
        {
            var url = scrapTask.Entrypoint + $"&page={x}";
            List<SearchResult> results = [];
            IPage? currentPage = null;

            try
            {
                currentPage = await session.LoadPageAsync(url, cancellationToken);
                results = await ParseHeadHunterPageResultsAsync(currentPage);
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

        // logger.LogDebug("Completed head hunter results for {SearchText}", scrapTask.SearchText);
        return response;
    }

    private async Task<List<SearchResult>> ParseHeadHunterPageResultsAsync(IPage page)
    {
        var response = new List<SearchResult>();
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

        logger.LogDebug("Page {Url} scraped, found: {LinksCount}", page.Url, linksLocators.Count);

        return response;
    }
}