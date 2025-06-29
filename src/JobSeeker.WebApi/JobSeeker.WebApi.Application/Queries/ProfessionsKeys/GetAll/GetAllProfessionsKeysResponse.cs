namespace JobSeeker.WebApi.Application.Queries.ProfessionsKeys.GetAll;

public class GetAllProfessionsKeysResponse
{
    public List<ProfessionKeyDto> Items { get; set; }
}

public class ProfessionKeyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
}