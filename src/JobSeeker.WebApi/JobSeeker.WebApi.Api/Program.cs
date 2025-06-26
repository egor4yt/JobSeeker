using JobSeeker.WebApi.Api.Configuration;
using JobSeeker.WebApi.Application.Configuration;
using JobSeeker.WebApi.Persistence.Configuration;
using Serilog;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.ConfigureApi();
    builder.ConfigurePersistence();
    builder.ConfigureApplication();

    var app = builder.Build();
    app.UseInitializeDatabase();
    app.UseRequestLogging();
    app.UseSwagger();
    app.UseSwaggerUI();

    var appUrls = builder.Configuration["applicationUrl"]?.Split(';')
                  ?? builder.Configuration.GetValue<string>("urls")?.Split(';')
                  ?? [];

    app.Logger.LogInformation("Application listening on {Addresses}", appUrls.Select(object? (x) => x));
    app.Logger.LogInformation("Swagger documentation listening on {SwaggerAddresses}", appUrls.Select(object? (x) => $"{x}/swagger/index.html"));

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