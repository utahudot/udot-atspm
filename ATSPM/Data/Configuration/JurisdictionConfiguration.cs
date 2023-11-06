using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Jurisdiction configuration
    /// </summary>
    public class JurisdictionConfiguration : IEntityTypeConfiguration<Jurisdiction>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Jurisdiction> builder)
        {
            builder.HasComment("Jurisdictions");

            builder.Property(e => e.CountyParish).HasMaxLength(50);

            builder.Property(e => e.Name).HasMaxLength(50);

            builder.Property(e => e.Mpo).HasMaxLength(50);

            builder.Property(e => e.OtherPartners).HasMaxLength(50);

            //builder.HasData(new Jurisdiction() { CountyParish = "Unknown", Name = "Unknown", OtherPartners = "Unknown"});
        }
    }
}
