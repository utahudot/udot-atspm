using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Region configuration
    /// </summary>
    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.ToTable(t => t.HasComment("Regions"));

            builder.Property(e => e.Description).HasMaxLength(50);
        }
    }
}
