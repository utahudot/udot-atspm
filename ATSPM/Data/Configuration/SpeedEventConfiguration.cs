using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class SpeedEventConfiguration : IEntityTypeConfiguration<SpeedEvent>
    {
        public void Configure(EntityTypeBuilder<SpeedEvent> builder)
        {
            builder.HasComment("Speed Event Data");

            builder.HasKey(e => new { e.DetectorId, e.Mph, e.Kph, e.TimeStamp });

            builder.Property(e => e.DetectorId).HasMaxLength(50);
        }
    }
}
