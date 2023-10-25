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
        }
    }
}
