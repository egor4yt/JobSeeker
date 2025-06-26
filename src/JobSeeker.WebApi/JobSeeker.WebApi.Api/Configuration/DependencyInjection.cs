using JobSeeker.WebApi.Api.Filters;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JobSeeker.WebApi.Api.Configuration;

/// <summary>
///     API configuration
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    ///     Add HTTP requests logging
    /// </summary>
    /// <param name="app">App instance</param>
    public static void UseRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} Status={StatusCode} Elapsed time={Elapsed} ms";
            options.GetLevel = (_, _, _) => LogEventLevel.Debug;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            };
        });
    }

    /// <summary>
    ///     API configuration
    /// </summary>
    /// <param name="builder">App builder instance</param>
    public static void ConfigureApi(this IHostApplicationBuilder builder)
    {
        ConfigureLogging(builder.Services, builder.Logging, builder.Configuration);
        ConfigureInfrastructure(builder.Services);
    }

    private static void ConfigureInfrastructure(IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddControllers(options => options.Filters.Add<ApiExceptionFilterAttribute>());
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, SwaggerConfiguration>();
    }

    private static void ConfigureLogging(IServiceCollection services, ILoggingBuilder logging, IConfiguration configuration)
    {
        logging.ClearProviders();
        logging.AddSerilog();
        services.AddSerilog();

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }
}