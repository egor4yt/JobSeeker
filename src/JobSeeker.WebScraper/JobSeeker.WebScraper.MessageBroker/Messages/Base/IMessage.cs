namespace JobSeeker.WebScraper.MessageBroker.Messages.Base;

public interface IMessage
{
    object ToApplicationCommand();
}