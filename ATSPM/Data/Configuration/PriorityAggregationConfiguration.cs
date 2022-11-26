using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PriorityAggregationConfiguration : IEntityTypeConfiguration<PriorityAggregation>
    {
        public void Configure(EntityTypeBuilder<PriorityAggregation> builder)
        {
            builder.HasComment("Priority Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.SignalId, e.PriorityNumber });

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
