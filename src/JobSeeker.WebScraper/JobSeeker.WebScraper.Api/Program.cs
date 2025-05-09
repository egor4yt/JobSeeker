using Hangfire;
using JobSeeker.WebScraper.Api.Configuration;
using JobSeeker.WebScraper.Application.Configuration;
using JobSeeker.WebScraper.MessageBroker.Configuration;
using JobSeeker.WebScraper.Persistence.Configuration;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureLogging();
    builder.ConfigureMessageBroker();
    builder.ConfigureApplication();
    builder.ConfigurePersistence();

    var app = builder.Build();
    app.UseHangfireDashboard();
    app.UseInitializeDatabase();

    var appUrls = builder.Configuration["applicationUrl"]?.Split(';')
                  ?? builder.Configuration.GetValue<string>("urls")?.Split(';')
                  ?? [];

    Log.Information("Application listening on {Addresses}", appUrls);

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException && ex.Source != "Microsoft.EntityFrameworkCore.Design") // see https://github.com/dotnet/efcore/issues/29923
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}