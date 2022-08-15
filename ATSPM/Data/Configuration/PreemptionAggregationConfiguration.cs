using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PreemptionAggregationConfiguration : IEntityTypeConfiguration<PreemptionAggregation>
    {
        public void Configure(EntityTypeBuilder<PreemptionAggregation> builder)
        {
            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.PreemptNumber });
            //.HasName("PK_dbo.PreemptionAggregations");

            builder.Property(e => e.BinStartTime).HasColumnType("datetime");

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
