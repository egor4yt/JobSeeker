using JobSeeker.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebApi.Persistence.Configuration.Entities;

public class OccupationGroupConfiguration : IEntityTypeConfiguration<OccupationGroup>
{
    public void Configure(EntityTypeBuilder<OccupationGroup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        
        builder
            .Property(x => x.Title)
            .HasColumnType("varchar(128)")
            .IsRequired();

        builder
            .HasMany(x => x.Occupations)
            .WithOne(x => x.OccupationGroup)
            .HasForeignKey(x => x.OccupationGroupId);
    }
}