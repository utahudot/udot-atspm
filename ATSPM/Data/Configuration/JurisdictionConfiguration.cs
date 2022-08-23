using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class JurisdictionConfiguration : IEntityTypeConfiguration<Jurisdiction>
    {
        public void Configure(EntityTypeBuilder<Jurisdiction> builder)
        {
            builder.HasComment("Signal Jurisdictions");

            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.CountyParish).HasMaxLength(50);

            builder.Property(e => e.Name).HasMaxLength(50);

            builder.Property(e => e.Mpo).HasMaxLength(50);

            builder.Property(e => e.OtherPartners).HasMaxLength(50);

            builder.HasData(new Jurisdiction() { Id = 0, CountyParish = "Unknown", Name = "Unknown", OtherPartners = "Unknown"});
        }
    }
}
