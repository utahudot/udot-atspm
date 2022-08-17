using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class RoutePhaseDirectionConfiguration : IEntityTypeConfiguration<RoutePhaseDirection>
    {
        public void Configure(EntityTypeBuilder<RoutePhaseDirection> builder)
        {
            builder.HasIndex(e => e.DirectionTypeId);

            builder.HasIndex(e => e.RouteSignalId);

            //builder.HasIndex(e => e.DirectionTypeId, "IX_DirectionTypeId");

            //builder.HasIndex(e => e.RouteSignalId, "IX_RouteSignalId");

            //builder.HasOne(d => d.DirectionType)
            //    .WithMany(p => p.RoutePhaseDirections)
            //    .HasForeignKey(d => d.DirectionTypeId)
            //    .HasConstraintName("FK_dbo.RoutePhaseDirections_dbo.DirectionTypes_DirectionTypeId");

            //builder.HasOne(d => d.RouteSignal)
            //    .WithMany(p => p.RoutePhaseDirections)
            //    .HasForeignKey(d => d.RouteSignalId)
            //    .HasConstraintName("FK_dbo.RoutePhaseDirections_dbo.RouteSignals_RouteSignalId");
        }
    }
}
