using System.Collections.Concurrent;
using JobSeeker.WebScraper.Application.Services.Proxy.Models;
using JobSeeker.WebScraper.Shared.Exceptions;

namespace JobSeeker.WebScraper.Application.Services.Proxy;

/// <summary>
///     Provides a service for managing local proxy instances to be used for specific domains.
///     The service ensures thread-safe allocation and release of proxy instances for domains,
///     avoiding conflicts and maintaining controlled access across multiple operations.
/// </summary>
public class LocalProxyFactoryService : IProxyFactoryService
{
    /// <summary>
    ///     A thread-safe dictionary that maps domain names to their corresponding semaphore locks.
    ///     Used to ensure synchronized access to shared resources related to the specified domain.
    /// </summary>
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _domainLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

    /// <summary>
    ///     A thread-safe dictionary that maps each domain (string) to a set of proxy details
    ///     currently being used for that domain. This is used to track the proxies actively
    ///     assigned to each domain to avoid assigning the same proxy to multiple requests
    ///     within the same domain simultaneously.
    /// </summary>
    private readonly ConcurrentDictionary<string, HashSet<ProxyDetails>> _domainProxyMap = new ConcurrentDictionary<string, HashSet<ProxyDetails>>();

    /// <summary>
    ///     Stores a collection of proxies available for use within the application.
    ///     This collection is used to assign and manage proxies for various operations,
    ///     ensuring efficient utilization and tracking of proxy usage.
    /// </summary>
    private readonly HashSet<ProxyDetails> _proxies =
    [
        new ProxyDetails(null, null, null)
    ];

    /// <summary>
    ///     Retrieves an available proxy for the specified domain. If no proxies are available,
    ///     a <see cref="NoAvailableProxyException" /> is thrown.
    /// </summary>
    /// <param name="domain">The domain for which the proxy is being requested.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="ProxyDetails" /> object containing the details of the assigned proxy.</returns>
    /// <exception cref="NoAvailableProxyException">Thrown when no proxies are available.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled via the cancellation token.</exception>
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

    /// <summary>
    ///     Releases a proxy for a specified domain, making it available for reuse.
    /// </summary>
    /// <param name="domain">The domain associated with the proxy.</param>
    /// <param name="proxy">The proxy details to be released.</param>
    /// <param name="cancellationToken">A cancellation token to observe during the release operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
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