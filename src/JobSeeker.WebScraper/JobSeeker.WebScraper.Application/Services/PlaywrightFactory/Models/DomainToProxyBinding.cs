using JobSeeker.WebScraper.Application.Services.Proxy.Models;

namespace JobSeeker.WebScraper.Application.Services.PlaywrightFactory.Models;

public record DomainToProxyBinding(string Domain, ProxyDetails Proxy);