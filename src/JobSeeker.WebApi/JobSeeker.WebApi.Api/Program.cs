using JobSeeker.WebApi.Api.Configuration;
using JobSeeker.WebApi.MessageBroker.Configuration;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureLogging();
    builder.ConfigureMessageBroker();

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