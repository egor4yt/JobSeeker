using JobSeeker.WebScraper.Shared.Helpers;
using Microsoft.Playwright;

namespace JobSeeker.WebScraper.Application.Services.PlaywrightFactory;

public sealed class PlaywrightSession(IBrowserContext browserContext, int maxParallelPages, Func<Guid, Task>? onDisposeAsync) : IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(maxParallelPages, maxParallelPages);
    public Guid SessionId { get; } = Guid.NewGuid();

    public async ValueTask DisposeAsync()
    {
        await browserContext.CloseAsync();

        if (onDisposeAsync != null) await onDisposeAsync(SessionId);
    }

    public async Task<IPage> LoadPageAsync(string url, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);

        IPage page;

        try
        {
            page = await browserContext.NewPageAsync();
            await page.GotoAsync(url);

            var minWait = TimeSpan.FromSeconds(1);
            var maxWait = TimeSpan.FromSeconds(3);
            await Jitter.WaitAsync(minWait, maxWait, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }

        return page;
    }
}