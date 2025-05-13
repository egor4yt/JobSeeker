namespace JobSeeker.WebScraper.MessageBroker.Messages.ScrapTask;

public class Created
{
    public string SearchText { get; set; } = null!;
    public string? ExcludeWordList { get; set; }
    public string SearchDomains { get; set; } = null!;
}