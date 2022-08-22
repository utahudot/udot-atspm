using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachSpeedAggregationConfiguration : IEntityTypeConfiguration<ApproachSpeedAggregation>
    {
        public void Configure(EntityTypeBuilder<ApproachSpeedAggregation> builder)
        {
            builder.HasComment("Approach Speed Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.ApproachID });

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
