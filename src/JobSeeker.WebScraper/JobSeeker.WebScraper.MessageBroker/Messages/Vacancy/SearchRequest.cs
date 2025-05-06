using JobSeeker.WebScraper.MessageBroker.Messages.Base;

namespace JobSeeker.WebScraper.MessageBroker.Messages.Vacancy;

public class SearchRequest : IMessage
{
    public object ToApplicationCommand()
    {
        throw new NotImplementedException();
    }
}