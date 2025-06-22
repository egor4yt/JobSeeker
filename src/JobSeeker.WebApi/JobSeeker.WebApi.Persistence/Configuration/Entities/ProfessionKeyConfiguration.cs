using JobSeeker.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebApi.Persistence.Configuration.Entities;

public class ProfessionKeyConfiguration : IEntityTypeConfiguration<ProfessionKey>
{
    public void Configure(EntityTypeBuilder<ProfessionKey> builder)
    {
        builder
            .Property(x => x.Title)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder
            .HasOne(x => x.OccupationGroup)
            .WithMany(x => x.ProfessionKeys)
            .HasForeignKey(x => x.OccupationGroupId);

        builder
            .HasOne(x => x.Occupation)
            .WithMany(x => x.ProfessionKeys)
            .HasForeignKey(x => x.OccupationId);

        builder
            .HasOne(x => x.Specialization)
            .WithMany(x => x.ProfessionKeys)
            .HasForeignKey(x => x.SpecializationId);

        builder
            .HasOne(x => x.SkillTag)
            .WithMany(x => x.ProfessionKeys)
            .HasForeignKey(x => x.SkillTagId);
    }
}