namespace JobSeeker.WebScraper.Application.Commands.ScrapTasks.Create;

public class CreateScrapTaskRequest
{
    public CreateScrapTaskRequest(MessageBroker.Messages.ScrapTask.Created from)
    {
        SearchText = from.SearchText;
        ExcludeWordsList = from.ExcludeWordList;
        SearchDomains = from.SearchDomains.ToLower().Trim().Split(",").ToList();
    }

    public string SearchText { get; set; } = null!;
    public string? ExcludeWordsList { get; set; }
    public List<string> SearchDomains { get; set; } = null!;
}