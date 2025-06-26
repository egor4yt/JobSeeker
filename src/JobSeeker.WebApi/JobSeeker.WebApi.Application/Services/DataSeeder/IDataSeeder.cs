namespace JobSeeker.WebApi.Application.Services.DataSeeder;

/// <summary>
///     Defines a contract for seeding data into a data source.
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    ///     Seeds data into the database if the corresponding records do not already exist.
    /// </summary>
    /// <param name="cancellationToken">
    ///     A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///     Result indicates whether any data was seeded.
    /// </returns>
    Task<bool> SeedAsync(CancellationToken cancellationToken = default);
}