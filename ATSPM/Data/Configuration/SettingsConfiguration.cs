using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Settings configuration
    /// </summary>
    public class SettingsConfiguration : IEntityTypeConfiguration<Settings>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Settings> builder)
        {
            builder.HasComment("Application Settings");

            builder.HasIndex(e => e.Setting).IsUnique();

            builder.Property(e => e.Setting)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(128);
        }
    }
}
