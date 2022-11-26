using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class RoutePhaseDirectionConfiguration : IEntityTypeConfiguration<RoutePhaseDirection>
    {
        public void Configure(EntityTypeBuilder<RoutePhaseDirection> builder)
        {
            builder.HasComment("Route Phase Directions");

            builder.HasIndex(e => e.DirectionTypeId);

            builder.HasIndex(e => e.RouteSignalId);
        }
    }
}
