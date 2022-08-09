using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class SpeedEventConfiguration : IEntityTypeConfiguration<SpeedEvent>
    {
        public void Configure(EntityTypeBuilder<SpeedEvent> builder)
        {
            builder.HasKey(e => new { e.DetectorID, e.Mph, e.Kph, e.Timestamp });
            //.HasName("PK_dbo.Speed_Events");

            builder.Property(e => e.DetectorID).HasMaxLength(50);
        }
    }
}
