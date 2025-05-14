using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.PagesAnalyzer.MessageBroker.Consumers.HealthCheck;

public class Perform(ILogger<Perform> logger) : IConsumer<MessageBroker.Messages.HealthCheck.Perform>
{
    public Task Consume(ConsumeContext<MessageBroker.Messages.HealthCheck.Perform> context)
    {
        logger.LogDebug("Health-check performed, message: '{Message}'", context.Message.Message);
        return Task.CompletedTask;
    }
}