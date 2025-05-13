namespace JobSeeker.WebScraper.Domain.Entities;

public class ScrapTaskResult
{
    public int Id { get; set; }
    public string Link { get; set; } = null!;

    /// <summary>
    ///     Indicates whether the result of the scraping task has been uploaded to S3 or not.
    /// </summary>
    public bool Uploaded { get; set; }

    public int ScrapTaskId { get; set; }

    public virtual ScrapTask ScrapTask { get; set; } = null!;
}