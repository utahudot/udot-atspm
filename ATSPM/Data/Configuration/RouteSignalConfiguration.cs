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

            builder.HasIndex(e => e.PrimaryDirectionId);

            builder.HasIndex(e => e.OpposingDirectionId);

            builder.Property(e => e.SignalIdentifier)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasOne(p => p.PrimaryDirection).WithOne().HasForeignKey<RouteSignal>(k => k.PrimaryDirectionId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.OpposingDirection).WithOne().HasForeignKey<RouteSignal>(k => k.OpposingDirectionId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
