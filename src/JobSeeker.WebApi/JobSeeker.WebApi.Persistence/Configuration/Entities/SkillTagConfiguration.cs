using JobSeeker.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebApi.Persistence.Configuration.Entities;

public class SkillTagConfiguration : IEntityTypeConfiguration<SkillTag>
{
    public void Configure(EntityTypeBuilder<SkillTag> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder
            .Property(x => x.Title)
            .HasColumnType("varchar(128)")
            .IsRequired();
    }
}