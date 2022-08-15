using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class SignalEventCountAggregationConfiguration : IEntityTypeConfiguration<SignalEventCountAggregation>
    {
        public void Configure(EntityTypeBuilder<SignalEventCountAggregation> builder)
        {
            builder.HasKey(e => new { e.BinStartTime, e.SignalId });
            //.HasName("PK_dbo.SignalEventCountAggregations");

            builder.Property(e => e.BinStartTime).HasColumnType("datetime");

            builder.Property(e => e.SignalId).HasMaxLength(10);
        }
    }
}
