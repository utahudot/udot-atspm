using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachSpeedAggregationConfiguration : IEntityTypeConfiguration<ApproachSpeedAggregation>
    {
        public void Configure(EntityTypeBuilder<ApproachSpeedAggregation> builder)
        {
            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.ApproachID });
            //.HasName("PK_dbo.ApproachSpeedAggregations");

            builder.Property(e => e.BinStartTime).HasColumnType("datetime");

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
