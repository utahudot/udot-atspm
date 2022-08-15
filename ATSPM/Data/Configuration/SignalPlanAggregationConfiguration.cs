using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class SignalPlanAggregationConfiguration : IEntityTypeConfiguration<SignalPlanAggregation>
    {
        public void Configure(EntityTypeBuilder<SignalPlanAggregation> builder)
        {
            builder.HasKey(e => new { e.SignalId, e.Start, e.End });
            //.HasName("PK_dbo.SignalPlanAggregations");

            //TODO: Find out if 128 is the correct value
            builder.Property(e => e.SignalId).HasMaxLength(128);

            builder.Property(e => e.Start).HasColumnType("datetime");

            builder.Property(e => e.End).HasColumnType("datetime");
        }
    }
}
