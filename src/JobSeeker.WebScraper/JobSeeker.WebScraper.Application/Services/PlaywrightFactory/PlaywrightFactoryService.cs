using System.Collections.Concurrent;
using JobSeeker.WebScraper.Application.Services.PlaywrightFactory.Models;
using JobSeeker.WebScraper.Application.Services.Proxy;
using Microsoft.Playwright;

namespace JobSeeker.WebScraper.Application.Services.PlaywrightFactory;

public class PlaywrightFactoryService(IProxyFactoryService proxyFactoryService)
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _domainLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
    private readonly SemaphoreSlim _initializeSemaphore = new SemaphoreSlim(1, 1);
    /// <summary>
    ///     Map session to pair {domain, proxy}
    /// </summary>
    private readonly ConcurrentDictionary<Guid, DomainToProxyBinding> _sessionToProxyMap = new ConcurrentDictionary<Guid, DomainToProxyBinding>();
    private IBrowser? _browser;
    private IPlaywright? _playwright;

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
            newSession = new PlaywrightSession(context, 2, OnSessionDisposedAsync);

            if (_sessionToProxyMap.TryAdd(newSession.SessionId, new DomainToProxyBinding(domain, proxy)) == false) throw new InvalidOperationException("Duplicate of session ID");
            ;
        }
        finally
        {
            domainLock.Release();
        }

        return newSession;
    }

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

    private async Task OnSessionDisposedAsync(Guid sessionId)
    {
        if (_sessionToProxyMap.TryRemove(sessionId, out var binding) == false) return;

        await proxyFactoryService.ReleaseProxyAsync(binding.Domain, binding.Proxy);
    }
}