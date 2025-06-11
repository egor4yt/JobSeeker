namespace JobSeeker.WebScraper.Application.Services.DataSeeder;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}