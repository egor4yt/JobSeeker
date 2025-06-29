using JobSeeker.PagesAnalyzer.Api.Configuration;
using JobSeeker.PagesAnalyzer.Application.Configuration;
using JobSeeker.PagesAnalyzer.MessageBroker.Configuration;
using JobSeeker.PagesAnalyzer.ObjectStorage.Configuration;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureLogging();
    builder.ConfigureMessageBroker();
    builder.ConfigureApplication();
    builder.ConfigureObjectStorage();

    var app = builder.Build();

    var appUrls = builder.Configuration["applicationUrl"]?.Split(';')
                  ?? builder.Configuration.GetValue<string>("urls")?.Split(';')
                  ?? [];

    Log.Warning("Application listening on {Addresses}", appUrls);

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