namespace JobSeeker.WebScraper.Domain.Entities;

public class ScrapTask
{
    public int Id { get; set; }

    /// <summary>
    ///     List of words separated with ','
    /// </summary>
    public string? ExcludeWordsList { get; set; }

    public string SearchText { get; set; } = null!;

    public virtual ICollection<ScrapSource> ScrapSources { get; set; } = null!;
}