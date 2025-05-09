using JobSeeker.WebScraper.Application.Commands.ScrapTasks.Create;

namespace JobSeeker.WebScraper.MessageBroker.Messages.ScrapTask;

public class Created
{
    public string SearchText { get; set; } = null!;
    public string? ExcludeWordList { get; set; }
    public string SearchDomains { get; set; } = null!;

    public CreateScrapTaskRequest ToCreateScrapTaskRequest()
    {
        var response = new CreateScrapTaskRequest
        {
            SearchText = SearchText,
            ExcludeWordsList = ExcludeWordList,
            SearchDomains = SearchDomains.ToLower().Trim().Split(",").ToList()
        };

        return response;
    }
}