using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JobSeeker.WebScraper.Application.Behaviours;

public class LoggingBehaviour<TRequest, TResponse>(ILogger<LoggingBehaviour<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestName = request.GetType().Name;
        var requestGuid = Guid.NewGuid().ToString();
        var requestDataAsJson = JsonSerializer.Serialize(request);

        logger.LogInformation("Handling {RequestName} {RequestId} {JsonRequestData} Elapsed={Elapsed}", requestName, requestGuid, requestDataAsJson, stopwatch.ElapsedMilliseconds);

        TResponse response;

        try
        {
            response = await next(cancellationToken);
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation("Handled {RequestName} {RequestId}, Execution time={ExecutionTime} ms", requestName, requestGuid, stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}