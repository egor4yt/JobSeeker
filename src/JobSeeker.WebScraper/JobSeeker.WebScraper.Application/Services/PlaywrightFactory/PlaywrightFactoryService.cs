using System.Collections.Concurrent;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory.Models;
using JobSeeker.WebScraper.Application.Services.Proxy;
using Microsoft.Playwright;

namespace JobSeeker.WebScraper.Application.Services.PlaywrightFactory;

/// <summary>
///     Provides functionality to manage the creation and initialization of Playwright sessions
///     for web scraping, ensuring proper handling of browser contexts and proxy settings
///     specific to domain-level configurations.
/// </summary>
public class PlaywrightFactoryService(IProxyFactoryService proxyFactoryService)
{
    /// <summary>
    ///     Thread-safe access and synchronization for operations per domain.
    /// </summary>
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _domainLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

    /// <summary>
    ///     Thread-safe initialization of <see cref="_playwright" /> and <see cref="_browser" />.
    /// </summary>
    private readonly SemaphoreSlim _initializeSemaphore = new SemaphoreSlim(1, 1);


    /// <summary>
    ///     Maintains a thread-safe mapping between session IDs and their corresponding domain-to-proxy bindings.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, DomainToProxyBinding> _sessionToProxyMap = new ConcurrentDictionary<Guid, DomainToProxyBinding>();

    /// <summary>
    ///     Browser instance used for creating and managing browser contexts
    /// </summary>
    private IBrowser? _browser;

    /// <summary>
    ///     Playwright instance used for creating and managing browser contexts and sessions.
    /// </summary>
    private IPlaywright? _playwright;

    /// <summary>
    ///     Asynchronously creates a new Playwright session for a specified domain.
    ///     The method initializes a browser context with optional proxy configurations
    ///     and ensures thread-safe access for sessions related to the domain.
    /// </summary>
    /// <param name="domain">
    ///     The domain for which the Playwright session will be created.
    /// </param>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     A <see cref="PlaywrightSession" /> representing the created session
    ///     with an initialized browser context.
    /// </returns>
    public async Task<PlaywrightSession> CreateSessionAsync(string domain, CancellationToken cancellationToken = default)
    {
        await TryInitializePlaywrightAsync(cancellationToken);

        var domainLock = _domainLocks.GetOrAdd(domain, new SemaphoreSlim(1, 1));
        await domainLock.WaitAsync(cancellationToken);

        PlaywrightSession? newSession;
        try
        {
            var contextOptions = new BrowserNewContextOptions();
            var proxy = await proxyFactoryService.GetProxyAsync(domain, cancellationToken);
            if (proxy.Host != null)
                contextOptions.Proxy = new Microsoft.Playwright.Proxy
                {
                    Server = proxy.Host,
                    Username = proxy.User,
                    Password = proxy.Password
                };

            var context = await _browser!.NewContextAsync(contextOptions);
            newSession = new PlaywrightSession(context, 3, OnSessionDisposedAsync);

            if (_sessionToProxyMap.TryAdd(newSession.SessionId, new DomainToProxyBinding(domain, proxy)) == false) throw new InvalidOperationException("Duplicate of session ID");
        }
        finally
        {
            domainLock.Release();
        }

        return newSession;
    }

    /// <summary>
    ///     Attempts to initialize the Playwright instance and browser in a thread-safe manner.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task TryInitializePlaywrightAsync(CancellationToken cancellationToken = default)
    {
        if (_playwright != null && _browser != null)
            return;

        await _initializeSemaphore.WaitAsync(cancellationToken);

        try
        {
            _playwright ??= await Playwright.CreateAsync();
            _browser ??= await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }
        finally
        {
            _initializeSemaphore.Release();
        }
    }

    /// <summary>
    ///     Handles the disposal of a Playwright session by releasing the associated proxy and domain mapping.
    /// </summary>
    /// <param name="sessionId">The unique identifier for the disposed session.</param>
    private async Task OnSessionDisposedAsync(Guid sessionId)
    {
        if (_sessionToProxyMap.TryRemove(sessionId, out var binding) == false) return;

        await proxyFactoryService.ReleaseProxyAsync(binding.Domain, binding.Proxy);
    }
}