using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PreemptionAggregationConfiguration : IEntityTypeConfiguration<PreemptionAggregation>
    {
        public void Configure(EntityTypeBuilder<PreemptionAggregation> builder)
        {
            builder.HasComment("Preemption Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.LocationIdentifier, e.PreemptNumber });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}
