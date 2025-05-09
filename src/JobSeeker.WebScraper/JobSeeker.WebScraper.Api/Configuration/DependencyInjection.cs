using Serilog;

namespace JobSeeker.WebScraper.Api.Configuration;

public static class DependencyInjection
{
    public static void ConfigureLogging(this IHostApplicationBuilder app)
    {
        app.Logging.ClearProviders();
        app.Logging.AddSerilog();


        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(app.Configuration)
            .CreateLogger();
    }
}