using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JobSeeker.PagesAnalyzer.Application.Services.AnalyzeStrategy;

public class AnalyzeStrategyFactory(IServiceScopeFactory serviceScopeFactory) : IAnalyzeStrategyFactory
{
    public IAnalyzeStrategy? GetStrategy(string domain)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AnalyzeStrategyFactory>>();

        try
        {
            return scope.ServiceProvider.GetRequiredKeyedService<IAnalyzeStrategy>(domain);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Can't get IAnalyzeStrategy");
        }

        return null;
    }
}