using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace JobSeeker.WebScraper.Persistence.Configuration;

public static class DbInitializer
{
    /// <summary>
    ///     Applying pending migrations and seeding data
    /// </summary>
    /// <param name="application">Application</param>
    public static void UseInitializeDatabase(this IHost application)
    {
        using var serviceScope = application.Services.CreateScope();

        try
        {
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
            var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
           
            if (pendingMigrations.Count != 0)
            {
                logger.LogInformation("Applying migrations");

                // only call this method when there are pending migrations
                dbContext.Database.Migrate();

                logger.LogInformation("Applied {AppliedMigrationsCount} migrations", pendingMigrations.Count);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Error while applying migrations");
        }
    }
}