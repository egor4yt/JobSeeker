namespace JobSeeker.WebApi.ObjectStorage.Models;

public class GetAllObjectsOptions : RequestOptions
{
    public int? MaxObjects { get; set; }
}