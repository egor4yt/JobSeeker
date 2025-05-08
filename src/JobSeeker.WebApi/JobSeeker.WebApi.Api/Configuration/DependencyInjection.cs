using Serilog;

namespace JobSeeker.WebApi.Api.Configuration;

public static class DependencyInjection
{
    public static void ConfigureLogging(this IHostApplicationBuilder app)
    {
        app.Logging.ClearProviders();
        app.Logging.AddSerilog();

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .ReadFrom.Configuration(app.Configuration)
            .CreateLogger();
    }
}