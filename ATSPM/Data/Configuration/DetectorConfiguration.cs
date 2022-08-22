using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class DetectorConfiguration : IEntityTypeConfiguration<Detector>
    {
        public void Configure(EntityTypeBuilder<Detector> builder)
        {
            builder.HasComment("Detectors");

            builder.HasIndex(e => e.ApproachId);

            builder.HasIndex(e => e.DetectionHardwareId);

            builder.HasIndex(e => e.LaneTypeId);

            builder.HasIndex(e => e.MovementTypeId);

            builder.Property(e => e.DetectorId)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
