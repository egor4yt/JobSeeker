using System.Diagnostics;
using System.Web;
using Microsoft.Playwright;
using Serilog;
using JobSeeker.WebScraper.Application.Configuration;
using JobSeeker.WebScraper.MessageBroker.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

Log.Information("Starting web application");

builder.ConfigureMessageBroker();
builder.ConfigureApplication();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

#region Endpoints

app.MapGet("/start-scraping/head-hunter-slow", async () =>
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var searchText = HttpUtility.UrlEncode("C# backend");
        const string baseUrl = "https://hh.ru/search/vacancy";
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
            // Headless = false
        });

        var page = await browser.NewPageAsync();

        var vacancies = new List<Vacancy>();
        var currentPage = 0;
        int? pagesCount = null;

        do
        {
            await page.GotoAsync($"{baseUrl}?text={searchText}&page={currentPage}");

            if (pagesCount == null)
            {
                var pagesLocator = await page.Locator("css=a[data-qa=pager-page]").Last.TextContentAsync();
                if (int.TryParse(pagesLocator, out var lastPageNumber) == false) throw new Exception("Pages count not found");
                pagesCount = lastPageNumber;
            }

            var vacanciesLocator = page.Locator("css=[class^=vacancy-info]");
            var vacanciesCount = await vacanciesLocator.CountAsync();

            for (var vacancyIndex = 0; vacancyIndex < vacanciesCount; vacancyIndex++)
            {
                var vacancyElement = vacanciesLocator.Nth(vacancyIndex);

                var newVacancy = new Vacancy();
                var link = await vacancyElement.Locator("css=a[data-qa*=title]").GetAttributeAsync("href");
                if (link == null || link.Contains("adsrv.hh.ru")) continue;

                newVacancy.Link = link.Contains('?') ? link.Split('?')[0] : link;
                newVacancy.Title = await vacancyElement.Locator("css=[data-qa*=title-text]").TextContentAsync();

                vacancies.Add(newVacancy);
            }

            currentPage += 1;
        } while (currentPage < pagesCount);

        stopwatch.Stop();

        return new
        {
            TimeElapsed = stopwatch.Elapsed.TotalSeconds,
            Vacancies = vacancies
        };
    })
    .WithName("StartSlowScrapingHeadHunter")
    .WithOpenApi();

app.MapGet("/start-scraping/head-hunter-semaphore", async () =>
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var searchText = HttpUtility.UrlEncode("C#");
        const string baseUrl = "https://hh.ru/search/vacancy";
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
            // Headless = false
        });

        var page = await browser.NewPageAsync();

        var vacancies = new List<Vacancy>();
        int? pagesCount = null;

        await page.GotoAsync($"{baseUrl}?text={searchText}");

        if (pagesCount == null)
        {
            var pagesLocator = await page.Locator("css=a[data-qa=pager-page]").Last.TextContentAsync();
            if (int.TryParse(pagesLocator, out var lastPageNumber) == false) throw new Exception("Pages count not found");
            pagesCount = lastPageNumber;
        }

        var scraper = new PlaywrightSemaphoreScraper();
        await scraper.InitAsync();

        var tasks = Enumerable.Range(0, Math.Min(pagesCount.Value, 100)).Select(x => scraper.SearchVacanciesAsync($"{baseUrl}?text={searchText}&page={x}"));
        var all = await Task.WhenAll(tasks);
        vacancies.AddRange(all.SelectMany(x => x));

        stopwatch.Stop();

        var groups = vacancies.GroupBy(x => new { x.Employer, x.Title })
            .Select(g => new
            {
                Count = g.Count(),
                Vacancy = g.First()
            });

        Console.WriteLine(string.Join('\n', groups.OrderBy(x => x.Count).Select(x => $"{x.Vacancy.Employer} ищет '{x.Vacancy.Title}': {x.Vacancy.Link} ({x.Count} дубликатов)")));

        return new
        {
            TimeElapsed = stopwatch.Elapsed.TotalSeconds,
            Vacancies = vacancies
        };
    })
    .WithName("StartSemaphoreScrapingHeadHunter")
    .WithOpenApi();

#endregion

app.Run();

internal record Vacancy
{
    public string? Title { get; set; }
    public string? Link { get; set; }
    public string? Employer { get; set; }
}

internal class PlaywrightSemaphoreScraper
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(4);
    private IBrowser? _browser;
    private IPlaywright? _playwright;

    public async Task InitAsync()
    {
        _playwright ??= await Playwright.CreateAsync();
        _browser ??= await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
    }

    public async Task<List<Vacancy>> SearchVacanciesAsync(string url)
    {
        if (_browser == null) throw new NullReferenceException();
        if (_playwright == null) throw new NullReferenceException();

        await _semaphore.WaitAsync();
        var pageNumber = url.Split("page=").Last();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var vacancies = new List<Vacancy>();

        IBrowserContext? context = null;
        try
        {
            context = await GetBrowserContextAsync();

            var page = await context.NewPageAsync();
            await page.GotoAsync(url);

            var vacanciesLocator = page.Locator("css=[class^=vacancy-info]");
            var vacanciesCount = await vacanciesLocator.CountAsync();

            for (var vacancyIndex = 0; vacancyIndex < vacanciesCount; vacancyIndex++)
            {
                var vacancyElement = vacanciesLocator.Nth(vacancyIndex);

                var newVacancy = new Vacancy();
                var link = await vacancyElement.Locator("css=a[data-qa*=title]").GetAttributeAsync("href");
                if (link == null || link.Contains("adsrv.hh.ru")) continue;

                newVacancy.Link = link.Contains('?') ? link.Split('?')[0] : link;
                newVacancy.Title = await vacancyElement.Locator("css=[data-qa*=title-text]").TextContentAsync();
                newVacancy.Employer = await vacancyElement.Locator("css=[data-qa*=vacancy-employer-text]").First.TextContentAsync();

                vacancies.Add(newVacancy);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            Console.WriteLine($"Scraped page: {pageNumber}, elapsed: {stopwatch.Elapsed.TotalSeconds}");

            if (context != null) await context.DisposeAsync();
            _semaphore.Release();
        }

        return vacancies;
    }

    private async Task<IBrowserContext> GetBrowserContextAsync()
    {
        if (_browser == null) throw new NullReferenceException();

        var context = await _browser.NewContextAsync();
        await context.RouteAsync("**/*", async route =>
        {
            if (route.Request.ResourceType is "image" or "font")
                await route.AbortAsync();
            else
                await route.ContinueAsync();
        });

        return context;
    }
}