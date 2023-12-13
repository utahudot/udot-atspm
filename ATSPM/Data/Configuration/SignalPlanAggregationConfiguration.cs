using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class LocationPlanAggregationConfiguration : IEntityTypeConfiguration<LocationPlanAggregation>
    {
        public void Configure(EntityTypeBuilder<LocationPlanAggregation> builder)
        {
            builder.HasComment("Location Plan Aggregation");

            builder.HasKey(e => new { e.LocationIdentifier, e.Start, e.End });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}
