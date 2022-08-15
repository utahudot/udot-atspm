using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PhaseTerminationAggregationConfiguration : IEntityTypeConfiguration<PhaseTerminationAggregation>
    {
        public void Configure(EntityTypeBuilder<PhaseTerminationAggregation> builder)
        {
            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber });
            //.HasName("PK_dbo.PhaseTerminationAggregations");

            builder.Property(e => e.BinStartTime).HasColumnType("datetime");

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
