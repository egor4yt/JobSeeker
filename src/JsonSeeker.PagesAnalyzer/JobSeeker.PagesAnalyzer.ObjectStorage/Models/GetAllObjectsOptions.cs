namespace JobSeeker.PagesAnalyzer.ObjectStorage.Models;

public class GetAllObjectsOptions : RequestOptions
{
    public int? MaxObjects { get; set; }
}