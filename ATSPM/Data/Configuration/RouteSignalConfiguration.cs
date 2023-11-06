using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Route signal configuration
    /// </summary>
    public class RouteSignalConfiguration : IEntityTypeConfiguration<RouteSignal>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<RouteSignal> builder)
        {
            builder.HasComment("Route Signals");

            builder.HasIndex(e => e.RouteId);

            builder.HasIndex(p => new { p.RouteId, p.SignalIdentifier }).IsUnique();

            builder.Property(e => e.SignalIdentifier)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasOne(p => p.PrimaryDirection).WithMany(a => a.PrimaryDirections).HasForeignKey(k => k.PrimaryDirectionId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.OpposingDirection).WithMany(a => a.OpposingDirections).HasForeignKey(k => k.OpposingDirectionId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
