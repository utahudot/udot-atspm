using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class DetectorEventCountAggregationConfiguration : IEntityTypeConfiguration<DetectorEventCountAggregation>
    {
        public void Configure(EntityTypeBuilder<DetectorEventCountAggregation> builder)
        {
            builder.HasComment("Detector Event Count Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.DetectorPrimaryID });

            builder.Property(e => e.SignalId)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}
