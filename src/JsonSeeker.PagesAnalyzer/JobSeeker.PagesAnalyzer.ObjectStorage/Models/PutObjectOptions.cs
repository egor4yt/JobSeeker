namespace JobSeeker.PagesAnalyzer.ObjectStorage.Models;

public class PutObjectOptions
{
    public string Bucket { get; set; }
    public string Path { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public Stream FileStream { get; set; }
}