using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachYellowRedActivationAggregationConfiguration : IEntityTypeConfiguration<ApproachYellowRedActivationAggregation>
    {
        public void Configure(EntityTypeBuilder<ApproachYellowRedActivationAggregation> builder)
        {
            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber, e.IsProtectedPhase });
            //.HasName("PK_dbo.ApproachYellowRedActivationAggregations");

            builder.Property(e => e.BinStartTime).HasColumnType("datetime");

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
