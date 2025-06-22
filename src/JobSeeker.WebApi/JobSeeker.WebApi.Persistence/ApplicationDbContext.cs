using JobSeeker.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobSeeker.WebApi.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<Vacancy> Vacancies { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<ProfessionKey> ProfessionKeys { get; set; }
    public DbSet<OccupationGroup> OccupationGroups { get; set; }
    public DbSet<Occupation> Occupations { get; set; }
    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<SkillTag> SkillTags { get; set; }
    public DbSet<Source> OuterSources { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}