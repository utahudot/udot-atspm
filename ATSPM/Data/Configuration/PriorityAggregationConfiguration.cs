using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PriorityAggregationConfiguration : IEntityTypeConfiguration<PriorityAggregation>
    {
        public void Configure(EntityTypeBuilder<PriorityAggregation> builder)
        {
            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.PriorityNumber });
            //.HasName("PK_dbo.PriorityAggregations");

            builder.Property(e => e.BinStartTime).HasColumnType("datetime");

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
