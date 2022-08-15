using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PhaseLeftTurnGapAggregationConfiguration : IEntityTypeConfiguration<PhaseLeftTurnGapAggregation>
    {
        public void Configure(EntityTypeBuilder<PhaseLeftTurnGapAggregation> builder)
        {
            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber });
            //.HasName("PK_dbo.PhaseLeftTurnGapAggregations");

            builder.Property(e => e.BinStartTime).HasColumnType("datetime");

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
