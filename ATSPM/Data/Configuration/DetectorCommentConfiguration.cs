using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Detector comment configuration
    /// </summary>
    public class DetectorCommentConfiguration : IEntityTypeConfiguration<DetectorComment>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<DetectorComment> builder)
        {
            builder.HasComment("Detector Comments");

            builder.Property(e => e.Comment)
                .IsRequired()
                .HasMaxLength(256);
        }
    }
}
