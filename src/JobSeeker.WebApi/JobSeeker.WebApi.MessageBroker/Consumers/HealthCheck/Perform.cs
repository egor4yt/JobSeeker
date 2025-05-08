using JobSeeker.WebApi.MessageBroker.Messages.HealthCheck;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebApi.MessageBroker.Consumers.HealthCheck;

public class Perform(ILogger<Perform> logger) : IConsumer<Messages.HealthCheck.Perform>
{
    public async Task Consume(ConsumeContext<Messages.HealthCheck.Perform> context)
    {
        var wait = new Random().Next(3000, 6000);
        logger.LogWarning("Consuming warning {Message}. Wait for: {Wait}", context.Message.Message, wait);
        await Task.Delay(TimeSpan.FromMilliseconds(wait));
    }
}