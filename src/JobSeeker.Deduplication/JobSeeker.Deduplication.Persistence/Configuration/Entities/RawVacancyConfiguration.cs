using JobSeeker.Deduplication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.Deduplication.Persistence.Configuration.Entities;

public class RawVacancyConfiguration : IEntityTypeConfiguration<RawVacancy>
{
    public void Configure(EntityTypeBuilder<RawVacancy> builder)
    {
        builder
            .Property(x => x.Title)
            .HasColumnType("varchar(256)")
            .IsRequired();

        builder
            .Property(x => x.SourceDomain)
            .HasColumnType("varchar(64)")
            .IsRequired();

        builder
            .Property(x => x.SourceId)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder
            .Property(x => x.Location)
            .HasColumnType("varchar(64)");

        builder
            .Property(x => x.Company)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasColumnType("text")
            .IsRequired();

        builder
            .Property(x => x.HtmlDescription)
            .HasColumnType("text")
            .IsRequired();

        builder
            .Property(x => x.Fingerprint)
            .HasColumnType("varchar(64)")
            .IsRequired();

        builder
            .HasOne(x => x.OriginalRawVacancy)
            .WithMany(x => x.DuplicateVacancies)
            .HasForeignKey(x => x.OriginalRawVacancyId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}