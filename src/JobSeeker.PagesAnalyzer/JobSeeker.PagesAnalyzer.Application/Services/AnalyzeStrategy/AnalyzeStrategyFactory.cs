using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy;

public class AnalyzeStrategyFactory(IServiceScopeFactory serviceScopeFactory) : IAnalyzeStrategyFactory
{
    public IAnalyzeStrategy? GetStrategy(string domain)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AnalyzeStrategyFactory>>();

        var fullDomainParts = domain.Split('.');
        var domains = new List<string>();
        for (var i = 0; i < fullDomainParts.Length - 1; i++)
        {
            var newDomain = fullDomainParts.Skip(i).Aggregate((x, y) => $"{x}.{y}");
            domains.Add(newDomain);
        }

        IAnalyzeStrategy? strategy = null;

        foreach (var currentDomain in domains)
        {
            try
            {
                strategy = scope.ServiceProvider.GetRequiredKeyedService<IAnalyzeStrategy>(currentDomain);
                break;
            }
            catch
            {
                // ignored
            }
        }

        if (strategy == null) logger.LogWarning("Can't get IAnalyzeStrategy for domain {Domain}", domain);

        return strategy;
    }
}