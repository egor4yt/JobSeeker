using System.Reflection;
using JobSeeker.WebScraper.Application.Behaviours;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobSeeker.WebScraper.Application.Configuration;

public static class DependencyInjection
{
    public static IHostApplicationBuilder ConfigureApplication(this IHostApplicationBuilder app)
    {
        var assembly = Assembly.GetExecutingAssembly();

        app.Services.AddMediatR(config => { config.RegisterServicesFromAssembly(assembly); });
        app.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));

        return app;
    }
}