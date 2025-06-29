namespace JobSeeker.WebUi.Application.Contracts.WebApi.ProfessionKeys;

public class GetAllResponse
{
    public List<ProfessionKeyDto> Items { get; set; }
}

public class ProfessionKeyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}