using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PhaseSplitMonitorAggregationConfiguration : IEntityTypeConfiguration<PhaseSplitMonitorAggregation>
    {
        public void Configure(EntityTypeBuilder<PhaseSplitMonitorAggregation> builder)
        {
            builder.HasComment("Phase Split Monitor Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.LocationIdentifier, e.PhaseNumber });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}
