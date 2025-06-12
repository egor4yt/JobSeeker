using JobSeeker.WebScraper.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebScraper.Persistence.Configuration.Entities;

public class ScrapGroupConfiguration : IEntityTypeConfiguration<ScrapGroup>
{
    public void Configure(EntityTypeBuilder<ScrapGroup> builder)
    {
        builder.HasIndex(x => x.Priority, "IX_ScrapTask_Priority");
        builder.HasIndex(x => new
            {
                x.Occupation,
                x.OccupationGroup,
                x.Specialization,
                x.SkillTag
            }, "UX_ScrapTask_VacancyKey")
            .IsUnique();
    }
}