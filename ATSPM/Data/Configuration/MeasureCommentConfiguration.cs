using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Measure comment configuration
    /// </summary>
    public class MeasureCommentConfiguration : IEntityTypeConfiguration<MeasureComment>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<MeasureComment> builder)
        {
            builder.HasComment("Measure Comments");

            builder.HasIndex(e => e.SignalIdentifier);

            builder.Property(e => e.Comment)
                .HasMaxLength(255);

            builder.Property(e => e.SignalIdentifier)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}
