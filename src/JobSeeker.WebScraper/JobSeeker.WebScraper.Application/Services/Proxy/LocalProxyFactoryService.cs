using System.Collections.Concurrent;
using JobSeeker.WebScraper.Application.Services.Proxy.Models;
using JobSeeker.WebScraper.Shared.Exceptions;

namespace JobSeeker.WebScraper.Application.Services.Proxy;

public class LocalProxyFactoryService : IProxyFactoryService
{
    /// <summary>
    ///     Getting proxy lock for domain
    /// </summary>
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _domainLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

    /// <summary>
    ///     Mapping {Domain: Proxies}
    /// </summary>
    private readonly ConcurrentDictionary<string, HashSet<ProxyDetails>> _domainProxyMap = new ConcurrentDictionary<string, HashSet<ProxyDetails>>();

    /// <summary>
    ///     Local storage of proxies. Null indicates you shouldn't use proxy
    /// </summary>
    private readonly HashSet<ProxyDetails> _proxies =
    [
        new ProxyDetails(null, null, null)
    ];

    public async Task<ProxyDetails> GetProxyAsync(string domain, CancellationToken cancellationToken = default)
    {
        var domainLock = _domainLocks.GetOrAdd(domain, _ => new SemaphoreSlim(1, 1));
        await domainLock.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        try
        {
            var busyProxies = _domainProxyMap.GetOrAdd(domain, []);
            var availableProxy = _proxies.FirstOrDefault(x => busyProxies.Contains(x) == false);
            if (availableProxy == null) throw new NoAvailableProxyException("No available proxy");

            busyProxies.Add(availableProxy);
            return availableProxy;
        }
        finally
        {
            domainLock.Release();
        }
    }

    public async Task ReleaseProxyAsync(string domain, ProxyDetails proxy, CancellationToken cancellationToken = default)
    {
        var domainLock = _domainLocks.GetOrAdd(domain, _ => new SemaphoreSlim(1, 1));
        await domainLock.WaitAsync(TimeSpan.FromSeconds(5), cancellationToken);

        try
        {
            if (_domainProxyMap.TryGetValue(domain, out var busyProxies)) busyProxies.Remove(proxy);
        }
        finally
        {
            domainLock.Release();
        }
    }
}