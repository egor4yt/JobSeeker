using JobSeeker.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebApi.Persistence.Configuration.Entities;

public class VacancyConfiguration : IEntityTypeConfiguration<Vacancy>
{
    public void Configure(EntityTypeBuilder<Vacancy> builder)
    {
        builder
            .Property(x => x.Title)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasColumnType("text")
            .IsRequired();

        builder
            .HasOne(x => x.Company)
            .WithMany(x => x.Vacancies)
            .HasForeignKey(x => x.CompanyId);

        builder
            .HasOne(x => x.ProfessionKey)
            .WithMany(x => x.Vacancies)
            .HasForeignKey(x => x.ProfessionKeyId);

        builder
            .HasMany(x => x.VacancySources)
            .WithOne(x => x.Vacancy)
            .HasForeignKey(x => x.VacancyId);
    }
}