using JobSeeker.WebScraper.Application.Services.DataSeeder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Hosted;

public class DataSeedingHostedService(IServiceProvider serviceProvider, ILogger<DataSeedingHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting data seeding...");

        using var scope = serviceProvider.CreateScope();
        var seeders = scope.ServiceProvider.GetRequiredService<IEnumerable<IDataSeeder>>();

        foreach (var seeder in seeders)
        {
            try
            {
                var seeded = await seeder.SeedAsync(cancellationToken);
                if (seeded) logger.LogInformation("Data seeding type of {SeedType} completed successfully", seeder.GetType().Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during data seeding");
                throw;
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}