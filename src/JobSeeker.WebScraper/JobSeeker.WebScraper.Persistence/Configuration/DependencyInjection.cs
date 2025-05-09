using JobSeeker.WebScraper.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace JobSeeker.WebScraper.Persistence.Configuration;

public static class DependencyInjection
{
    public static void ConfigurePersistence(this IHostApplicationBuilder app)
    {
        if (app.Environment.IsDevelopment())
            app.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(app.Configuration
                        .GetConnectionString(ConfigurationKeys.DatabaseConnectionString))
                    .LogTo(Log.Information, LogLevel.Information, DbContextLoggerOptions.Id | DbContextLoggerOptions.Category)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
            );
        else
            app.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(app.Configuration
                        .GetConnectionString(ConfigurationKeys.DatabaseConnectionString))
                    .LogTo(Log.Information, LogLevel.Information, DbContextLoggerOptions.Id | DbContextLoggerOptions.Category)
            );
    }
}