using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Route Location configuration
    /// </summary>
    public class RouteLocationConfiguration : IEntityTypeConfiguration<RouteLocation>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<RouteLocation> builder)
        {
            builder.ToTable(t => t.HasComment("Route Locations"));

            builder.HasIndex(e => e.RouteId);

            builder.HasIndex(p => new { p.RouteId, p.LocationIdentifier }).IsUnique();

            builder.Property(e => e.LocationIdentifier)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasOne(p => p.PrimaryDirection).WithMany(a => a.PrimaryDirections).HasForeignKey(k => k.PrimaryDirectionId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.OpposingDirection).WithMany(a => a.OpposingDirections).HasForeignKey(k => k.OpposingDirectionId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.PreviousLocationDistance).WithMany(a => a.PreviousLocations).HasForeignKey(k => k.PreviousLocationDistanceId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.NextLocationDistance).WithMany(a => a.NextLocations).HasForeignKey(k => k.NextLocationDistanceId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
