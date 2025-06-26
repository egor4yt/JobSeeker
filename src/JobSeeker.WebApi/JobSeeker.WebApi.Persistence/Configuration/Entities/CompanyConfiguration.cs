using JobSeeker.WebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSeeker.WebApi.Persistence.Configuration.Entities;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder
            .Property(x => x.Name)
            .HasColumnType("varchar(128)")
            .IsRequired();
        
        builder
            .Property(x => x.Slug)
            .HasColumnType("varchar(256)")
            .IsRequired();
        
        builder
            .HasIndex(x => x.Slug, "UX_Company_Slug")
            .IsUnique();
    }
}