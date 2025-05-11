using JobSeeker.WebScraper.Shared.Helpers;
using Microsoft.Playwright;

namespace JobSeeker.WebScraper.Application.Services.PlaywrightFactory;

/// <summary>
///     Represents a session for interacting with a Playwright browser context.
///     A session is associated with a unique identifier and provides thread-safe
///     functionality for loading pages while limiting the maximum number of parallel pages
///     that can be loaded.
/// </summary>
public sealed class PlaywrightSession(IBrowserContext browserContext, int maxParallelPages, Func<Guid, Task>? onDisposeAsync) : IAsyncDisposable
{
    /// <summary>
    ///     A semaphore used to control the number of concurrent parallel pages that can be opened in the browser context.
    /// </summary>
    /// <remarks>
    ///     The semaphore is initialized with a maximum number of permits equal to the value of <see cref="maxParallelPages" />.
    ///     It ensures that the maximum parallelism does not exceed the specified limit.
    /// </remarks>
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(maxParallelPages, maxParallelPages);

    /// <summary>
    ///     Gets the unique identifier for the current session.
    ///     This identifier is generated as a `Guid` upon the creation of the `PlaywrightSession` instance.
    ///     It is used to uniquely track and manage the lifecycle of a session, including actions such as cleanup
    ///     during disposal.
    /// </summary>
    public Guid SessionId { get; } = Guid.NewGuid();

    /// <summary>
    ///     Asynchronously disposes of the current session resources, including closing the underlying browser context
    ///     and invoking an optional disposal callback.
    /// </summary>
    /// <returns>A ValueTask representing the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await browserContext.CloseAsync();

        if (onDisposeAsync != null) await onDisposeAsync(SessionId);
    }

    /// <summary>
    ///     Loads a web page in a new browser tab and navigates to the specified URL.
    /// </summary>
    /// <param name="url">The URL of the page to navigate to.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A Task that represents the asynchronous operation. The Task result contains the loaded page.</returns>
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