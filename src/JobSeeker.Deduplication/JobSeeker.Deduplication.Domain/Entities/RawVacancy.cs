namespace JobSeeker.Deduplication.Domain.Entities;

/// <summary>
///     Represents the raw vacancy data
/// </summary>
public class RawVacancy
{
    public int Id { get; set; }
    public string? Location { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Company { get; set; } = null!;
    public string SourceDomain { get; set; } = null!;
    public bool DeduplicationCompleted { get; set; }

    /// <summary>
    ///     S3 root location
    /// </summary>
    public string DownloadKey { get; set; } = null!;

    public string Fingerprint { get; set; } = null!;
}