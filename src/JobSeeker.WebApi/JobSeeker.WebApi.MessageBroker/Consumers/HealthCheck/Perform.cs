using JobSeeker.WebApi.MessageBroker.Producers;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebApi.MessageBroker.Consumers.HealthCheck;

public class Perform(ILogger<Perform> logger, IMessageProducer<Messages.ScrapTask.Created> producer) : IConsumer<Messages.HealthCheck.Perform>
{
    public async Task Consume(ConsumeContext<Messages.HealthCheck.Perform> context)
    {
        logger.LogDebug("Health-check performed, message: '{Message}'", context.Message.Message);
    }
}