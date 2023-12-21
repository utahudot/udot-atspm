using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PhaseTerminationAggregationConfiguration : IEntityTypeConfiguration<PhaseTerminationAggregation>
    {
        public void Configure(EntityTypeBuilder<PhaseTerminationAggregation> builder)
        {
            builder.HasComment("Phase Termination Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.LocationIdentifier, e.PhaseNumber });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}
