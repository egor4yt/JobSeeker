namespace JobSeeker.WebScraper.Domain.Entities;

public class ScrapTaskConfiguration
{
    public int Id { get; set; }
    public int ScrapGroupId { get; set; }

    /// <inheritdoc cref="ScrapTask.Priority" />
    public int Priority { get; set; }

    /// <inheritdoc cref="ScrapTask.Entrypoint" />
    public string Entrypoint { get; set; } = null!;

    public virtual ScrapGroup ScrapGroup { get; set; }
}