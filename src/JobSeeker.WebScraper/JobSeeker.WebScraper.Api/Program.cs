using Hangfire;
using JobSeeker.WebScraper.Api.Configuration;
using JobSeeker.WebScraper.Application.Configuration;
using JobSeeker.WebScraper.Application.JobParameters.Common;
using JobSeeker.WebScraper.Application.Services.JobRunner;
using JobSeeker.WebScraper.MessageBroker.Configuration;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureLogging();
    builder.ConfigureMessageBroker();
    builder.ConfigureApplication();

    var app = builder.Build();
    app.UseHangfireDashboard();

    // Temporary solution for local development
#if DEBUG
    var job = new ParseSearchResultsLinks
    {
        BaseSearchUrl = "https://hh.ru/search/vacancy",
        JobId = Guid.NewGuid()
    };

    var jobManager = app.Services.CreateScope().ServiceProvider.GetRequiredService<IBackgroundJobClient>();
    jobManager.Enqueue<JobRunnerService>(x => x.RunAsync(job, CancellationToken.None));
#endif

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