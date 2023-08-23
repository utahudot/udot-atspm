using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachSplitFailAggregationConfiguration : IEntityTypeConfiguration<ApproachSplitFailAggregation>
    {
        public void Configure(EntityTypeBuilder<ApproachSplitFailAggregation> builder)
        {
            builder.HasComment("Approach Split Fail Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.SignalIdentifier, e.ApproachId, e.PhaseNumber, e.IsProtectedPhase });

            builder.Property(e => e.SignalIdentifier).HasMaxLength(10);
        }
    }
}
