using JobSeeker.WebScraper.Api.Configuration;
using JobSeeker.WebScraper.Application.Configuration;
using JobSeeker.WebScraper.MessageBroker.Configuration;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureLogging();
    builder.ConfigureMessageBroker();
    builder.ConfigureApplication();

    var app = builder.Build();

    var appUrls = builder.Configuration["applicationUrl"]?.Split(';')
                  ?? builder.Configuration.GetValue<string>("urls")?.Split(';')
                  ?? [];

    Log.Information("Application listening on {Addresses}", appUrls);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}