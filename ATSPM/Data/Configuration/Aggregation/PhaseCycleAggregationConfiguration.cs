using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration.Aggregation
{
    public class PhaseCycleAggregationConfiguration : IEntityTypeConfiguration<PhaseCycleAggregation>
    {
        public void Configure(EntityTypeBuilder<PhaseCycleAggregation> builder)
        {
            builder.HasComment("Phase Cycle Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.LocationIdentifier, e.PhaseNumber });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}
