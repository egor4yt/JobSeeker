using JobSeeker.WebScraper.Application.Services.Proxy.Models;

namespace JobSeeker.WebScraper.Application.Services.Proxy;

public interface IProxyFactoryService
{
    /// <summary>
    ///     Get proxy for domain
    /// </summary>
    /// <param name="domain">Target domain</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>proxy</returns>
    Task<ProxyDetails> GetProxyAsync(string domain, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Get proxy for domain
    /// </summary>
    /// <param name="domain">Target domain</param>
    /// <param name="proxy">Proxy details to release</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>proxy</returns>
    Task ReleaseProxyAsync(string domain, ProxyDetails proxy, CancellationToken cancellationToken = default);
}