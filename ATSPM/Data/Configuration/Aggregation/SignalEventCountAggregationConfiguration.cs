using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration.Aggregation
{
    public class LocationEventCountAggregationConfiguration : IEntityTypeConfiguration<LocationEventCountAggregation>
    {
        public void Configure(EntityTypeBuilder<LocationEventCountAggregation> builder)
        {
            builder.HasComment("Location Event Count Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.LocationIdentifier });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}
