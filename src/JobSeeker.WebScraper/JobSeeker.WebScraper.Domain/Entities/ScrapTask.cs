namespace JobSeeker.WebScraper.Domain.Entities;

public class ScrapTask
{
    public int Id { get; set; }
    public int ScrapGroupId { get; set; }
    public int ScrapSourceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorDetails { get; set; }

    /// <summary>
    ///     Lower priority tasks are processed sooner
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    ///     URL to the first page with search results without domain and protocol.
    /// </summary>
    public string EntrypointPath { get; set; } = null!;

    public virtual ScrapGroup ScrapGroup { get; set; } = null!;
    public virtual ScrapSource ScrapSource { get; set; } = null!;
    public virtual ICollection<ScrapTaskResult> ScrapTaskResults { get; set; } = null!;
}