using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class RouteSignalConfiguration : IEntityTypeConfiguration<RouteSignal>
    {
        public void Configure(EntityTypeBuilder<RouteSignal> builder)
        {
            builder.HasComment("Route Signals");

            builder.HasIndex(e => e.RouteId);

            builder.Property(e => e.SignalIdentifier)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}
