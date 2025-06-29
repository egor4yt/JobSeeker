using JobSeeker.WebUi.Shared;

namespace JobSeeker.WebUi.Application.Configuration;

public static class DependencyInjection
{
    public static void ConfigureApplication(this IHostApplicationBuilder app)
    {
        AddInfrastructure(app.Services);
        AddApiClients(app.Services, app.Configuration);
    }

    private static void AddInfrastructure(IServiceCollection services)
    {
        services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddCircuitOptions(x => { x.DetailedErrors = true; });
    }

    private static void AddApiClients(IServiceCollection services, IConfiguration configuration)
    {
        var apiUrlsSection = configuration.GetSection(ConfigurationKeys.HttpClientsSettings);
        foreach (var apiUrl in apiUrlsSection.GetChildren())
        {
            var baseUrl = apiUrl.Value;
            if (string.IsNullOrWhiteSpace(baseUrl) == false) services.AddHttpClient(apiUrl.Key, x => { x.BaseAddress = new Uri(baseUrl); });
        }
    }
}