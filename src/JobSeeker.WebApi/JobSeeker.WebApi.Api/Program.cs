using Hangfire;
using JobSeeker.WebApi.Api.Configuration;
using JobSeeker.WebApi.Application.Configuration;
using JobSeeker.WebApi.MessageBroker.Configuration;
using JobSeeker.WebApi.ObjectStorage.Configuration;
using JobSeeker.WebApi.Persistence.Configuration;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureApi();
    builder.ConfigurePersistence();
    builder.ConfigureApplication();
    builder.ConfigureMessageBroker();
    builder.ConfigureObjectStorage();

    var app = builder.Build();
    app.UseInitializeDatabase();
    app.UseRequestLogging();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard();

    var appUrls = builder.Configuration["applicationUrl"]?.Split(';')
                  ?? builder.Configuration.GetValue<string>("urls")?.Split(';')
                  ?? [];

    app.Logger.LogInformation("Application listening on {Addresses}", appUrls.Select(object? (x) => x));
    app.Logger.LogInformation("Swagger documentation listening on {SwaggerAddresses}", appUrls.Select(object? (x) => $"{x}/swagger/index.html"));
    app.Logger.LogInformation("Hangfire dashboard listening on {SwaggerAddresses}", appUrls.Select(object? (x) => $"{x}/hangfire/jobs/enqueued"));

    app.MapControllers();
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