namespace JobSeeker.WebApi.ObjectStorage.Models;

public class PutObjectOptions : RequestOptions
{
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public Stream FileStream { get; set; }
}