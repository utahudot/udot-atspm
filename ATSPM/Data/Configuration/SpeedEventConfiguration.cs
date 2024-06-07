using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class SpeedEventConfiguration : IEntityTypeConfiguration<OldSpeedEvent>
    {
        public void Configure(EntityTypeBuilder<OldSpeedEvent> builder)
        {
            builder.ToTable(t => t.HasComment("Speed Event Data"));

            builder.HasKey(e => new { e.DetectorId, e.Mph, e.Kph, e.TimeStamp });

            builder.Property(e => e.DetectorId).HasMaxLength(50);
        }
    }
}
