namespace JobSeeker.PagesAnalyzer.ObjectStorage.Models;

public abstract class RequestOptions
{
    public required string Bucket { get; init; }
    public required string Path { get; init; }
}