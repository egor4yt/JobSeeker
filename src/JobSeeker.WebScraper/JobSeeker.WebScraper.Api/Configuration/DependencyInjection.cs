using Serilog;

namespace JobSeeker.WebScraper.Api.Configuration;

public static class DependencyInjection
{
    public static void ConfigureLogging(this IHostApplicationBuilder app)
    {
        app.Logging.ClearProviders();

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .ReadFrom.Configuration(app.Configuration)
            .CreateLogger();

        Log.Information("Starting web application");
    }
}