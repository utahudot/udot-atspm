using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Measure options configuration
    /// </summary>
    public class MeasureOptionsConfiguration : IEntityTypeConfiguration<MeasureOption>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<MeasureOption> builder)
        {
            builder.HasComment("Measure Options");

            builder.Property(e => e.Option).HasMaxLength(128);

            builder.Property(e => e.Value).HasMaxLength(512);
        }
    }
}
