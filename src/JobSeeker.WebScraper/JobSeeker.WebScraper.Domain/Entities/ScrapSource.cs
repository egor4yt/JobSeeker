namespace JobSeeker.WebScraper.Domain.Entities;

public class ScrapSource
{
    public int Id { get; set; }
    public string Domain { get; set; } = null!;

    public virtual ICollection<ScrapTask> ScrapTasks { get; set; } = null!;
}