using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Consumers.HealthCheck;

public class Perform(ILogger<Perform> logger) : IConsumer<Messages.HealthCheck.Perform>
{
    public async Task Consume(ConsumeContext<Messages.HealthCheck.Perform> context)
    {
        logger.LogDebug("Health-check performed, message: '{Message}'", context.Message.Message);
    }
}