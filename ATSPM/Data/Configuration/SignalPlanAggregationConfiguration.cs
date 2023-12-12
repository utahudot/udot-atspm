using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class SignalPlanAggregationConfiguration : IEntityTypeConfiguration<SignalPlanAggregation>
    {
        public void Configure(EntityTypeBuilder<SignalPlanAggregation> builder)
        {
            builder.HasComment("Signal Plan Aggregation");

            builder.HasKey(e => new { e.LocationIdentifier, e.Start, e.End });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}
