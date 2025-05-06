using Confluent.Kafka;
using JobSeeker.WebScraper.MessageBroker.Messages.Vacancy;
using JobSeeker.WebScraper.MessageBroker.Providers.Base;
using JobSeeker.WebScraper.MessageBroker.Providers.Kafka.Core;
using JobSeeker.WebScraper.MessageBroker.Providers.Kafka.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.MessageBroker.Providers.Kafka;

public class KafkaMessageConsumer(
    ILogger<KafkaMessageConsumer> logger,
    ConsumerConfig consumerConfig,
    IHostApplicationLifetime applicationLifetime,
    IServiceProvider serviceProvider) : IMessageConsumer
{
    private static readonly Dictionary<string, Type> MessagesToTopicsMap = new Dictionary<string, Type>
    {
        { KafkaTopic.VacancySearchRequest, typeof(SearchRequest) }
    };
    private readonly IConsumer<Ignore, object?> _consumer = new ConsumerBuilder<Ignore, object?>(consumerConfig)
        .SetValueDeserializer(new KafkaJsonDeserializer(MessagesToTopicsMap))
        .SetLogHandler((_, msg) => logger.LogDebug("Kafka sent a message with status {KafkaLevel}/{KafkaFacility}. Message: '{Message}'", msg.Level.ToString(), msg.Facility, msg.Message))
        .SetErrorHandler((_, error) => logger.LogError("Kafka couldn't send a message, error code: {KafkaErrorCode}. Reason: '{Reason}'", error.Code, error.Reason))
        .Build();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _ = ConsumeAsync(cancellationToken);
        _ = WaitForAppStartup(cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Unsubscribe();
        _consumer.Close();
        _consumer.Dispose();

        return Task.CompletedTask;
    }

    public async Task ConsumeAsync(CancellationToken cancellationToken)
    {
        if (await WaitForAppStartup(cancellationToken) == false) return;

        var topics = MessagesToTopicsMap.Keys.Distinct().ToList();
        if (topics.Count == 0) return;

        _consumer.Subscribe(topics);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumedResult = _consumer.Consume(cancellationToken);
                await using var asyncScope = serviceProvider.CreateAsyncScope();
                // var mediator = asyncScope.ServiceProvider.GetRequiredService<IMediator>();
                //
                // if (consumedResult.Message.Value is IMessage messageValue)
                // {
                //     var command = messageValue.ToApplicationCommand();
                //     logger.LogDebug("Command: '{@Command}' handled from topic '{Topic}'", command, consumedResult.Topic);
                //     await mediator.Send(command, cancellationToken);
                // }
                // else
                // {
                //     logger.LogWarning("Message: '{@Message}' from topic '{Topic}' couldn't be converted to application request type", consumedResult.Message.Value, consumedResult.Topic);
                //     continue;
                // }

                // _consumer.Commit(consumedResult);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Kafka consuming error");
            }
            finally
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
            }
        }

        _consumer.Close();
    }

    private async Task<bool> WaitForAppStartup(CancellationToken cancellationToken)
    {
        var startedSource = new TaskCompletionSource();
        await using var applicationStarted = applicationLifetime.ApplicationStarted.Register(() => startedSource.SetResult());

        var cancelledSource = new TaskCompletionSource();
        await using var applicationStopped = cancellationToken.Register(() => cancelledSource.SetResult());

        var completedTask = await Task.WhenAny(startedSource.Task, cancelledSource.Task).ConfigureAwait(false);

        return completedTask == startedSource.Task;
    }
}