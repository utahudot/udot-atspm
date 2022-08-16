using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PhaseSplitMonitorAggregationConfiguration : IEntityTypeConfiguration<PhaseSplitMonitorAggregation>
    {
        public void Configure(EntityTypeBuilder<PhaseSplitMonitorAggregation> builder)
        {
            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber });
            //.HasName("PK_dbo.PhaseSplitMonitorAggregations");

            builder.Property(e => e.BinStartTime).HasColumnType("datetime");

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
