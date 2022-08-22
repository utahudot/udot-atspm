using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachYellowRedActivationAggregationConfiguration : IEntityTypeConfiguration<ApproachYellowRedActivationAggregation>
    {
        public void Configure(EntityTypeBuilder<ApproachYellowRedActivationAggregation> builder)
        {
            builder.HasComment("Approach Yellow Red Activation Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber, e.IsProtectedPhase });

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
