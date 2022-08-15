using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachPcdAggregationConfiguration : IEntityTypeConfiguration<ApproachPcdAggregation>
    {
        public void Configure(EntityTypeBuilder<ApproachPcdAggregation> builder)
        {
            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber, e.IsProtectedPhase });
                    //.HasName("PK_dbo.ApproachPcdAggregations");

            builder.Property(e => e.BinStartTime).HasColumnType("datetime");

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
