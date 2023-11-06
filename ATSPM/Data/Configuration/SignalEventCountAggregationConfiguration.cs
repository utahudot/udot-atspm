using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class SignalEventCountAggregationConfiguration : IEntityTypeConfiguration<SignalEventCountAggregation>
    {
        public void Configure(EntityTypeBuilder<SignalEventCountAggregation> builder)
        {
            builder.HasComment("Signal Event Count Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.SignalIdentifier });

            builder.Property(e => e.SignalIdentifier).HasMaxLength(10);
        }
    }
}
