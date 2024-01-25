using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration.Aggregation
{
    public class PriorityAggregationConfiguration : IEntityTypeConfiguration<PriorityAggregation>
    {
        public void Configure(EntityTypeBuilder<PriorityAggregation> builder)
        {
            builder.HasComment("Priority Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.LocationIdentifier, e.PriorityNumber });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}
