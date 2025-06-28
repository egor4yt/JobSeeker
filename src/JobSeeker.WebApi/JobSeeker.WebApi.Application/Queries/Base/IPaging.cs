namespace JobSeeker.WebApi.Application.Queries.Base;

public interface IPaging
{
    public int Skip { get; set; }
    public int Take { get; set; }
}