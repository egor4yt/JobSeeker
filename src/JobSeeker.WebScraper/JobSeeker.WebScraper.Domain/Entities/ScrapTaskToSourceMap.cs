namespace JobSeeker.WebScraper.Domain.Entities;

public class ScrapTaskToSourceMap
{
    public int ScrapTaskId { get; set; }
    public int ScrapSourceId { get; set; }

    public virtual ScrapTask ScrapTask { get; set; } = null!;
    public virtual ScrapSource ScrapSource { get; set; } = null!;
}