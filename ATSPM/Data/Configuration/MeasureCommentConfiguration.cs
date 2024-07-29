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
            builder.ToTable(t => t.HasComment("Measure Comments"));

            builder.HasIndex(e => e.LocationIdentifier);

            builder.Property(e => e.Comment)
                .HasMaxLength(255);

            builder.Property(e => e.LocationIdentifier)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}
