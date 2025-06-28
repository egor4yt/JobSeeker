using JobSeeker.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebApi.Persistence.Configuration.Entities;

public class VacancySourceConfiguration : IEntityTypeConfiguration<VacancySource>
{
    public void Configure(EntityTypeBuilder<VacancySource> builder)
    {
        builder
            .Property(x => x.SourceKey)
            .HasColumnType("varchar(256)")
            .IsRequired();

        builder
            .HasOne(x => x.Source)
            .WithMany(x => x.VacancySources)
            .HasForeignKey(x => x.SourceId);

        builder
            .HasOne(x => x.Location)
            .WithMany(x => x.VacancySources)
            .HasForeignKey(x => x.LocationId);

        builder
            .HasOne(x => x.Vacancy)
            .WithMany(x => x.VacancySources)
            .HasForeignKey(x => x.VacancyId);
    }
}