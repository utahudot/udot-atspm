using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Detector configuration
    /// </summary>
    public class DetectorConfiguration : IEntityTypeConfiguration<Detector>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Detector> builder)
        {
            builder.ToTable(t => t.HasComment("Detectors"));

            builder.HasIndex(e => e.ApproachId);

            builder.Property(e => e.DectectorIdentifier)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
