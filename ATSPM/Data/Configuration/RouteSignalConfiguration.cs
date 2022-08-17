using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class RouteSignalConfiguration : IEntityTypeConfiguration<RouteSignal>
    {
        public void Configure(EntityTypeBuilder<RouteSignal> builder)
        {
            builder.HasIndex(e => e.RouteId);

            //builder.HasIndex(e => e.RouteId, "IX_RouteId");

            builder.Property(e => e.SignalId)
                .IsRequired()
                .HasMaxLength(10);

            //builder.HasOne(d => d.Route)
            //    .WithMany(p => p.RouteSignals)
            //    .HasForeignKey(d => d.RouteId)
            //    .HasConstraintName("FK_dbo.RouteSignals_dbo.Routes_RouteId");
        }
    }
}
