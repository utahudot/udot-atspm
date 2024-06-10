using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Route distance configuration
    /// </summary>
    public class RouteDistanceConfiguration : IEntityTypeConfiguration<RouteDistance>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<RouteDistance> builder)
        {
            builder.ToTable(t => t.HasComment("Route Distances"));

            builder.HasIndex(e => new { e.LocationIdentifierA, e.LocationIdentifierB }).IsUnique();

            builder.Property(e => e.LocationIdentifierA)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(e => e.LocationIdentifierB)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}
