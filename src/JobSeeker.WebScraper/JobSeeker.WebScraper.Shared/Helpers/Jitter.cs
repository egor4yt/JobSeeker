namespace JobSeeker.WebScraper.Shared.Helpers;

public static class Jitter
{
    /// <summary>
    ///     Waiting random time between <paramref name="lowerBound" /> and <paramref name="upperBound" />
    /// </summary>
    /// <param name="lowerBound">Minimum wait in milliseconds</param>
    /// <param name="upperBound">Minimum wait in milliseconds</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public static async Task WaitAsync(TimeSpan lowerBound, TimeSpan upperBound, CancellationToken cancellationToken = default)
    {
        var minMilliseconds = (int)lowerBound.TotalMilliseconds;
        var maxMilliseconds = (int)upperBound.TotalMilliseconds;
        var waitTime = Random.Shared.Next(minMilliseconds, maxMilliseconds);

        await Task.Delay(waitTime, cancellationToken);
    }
}