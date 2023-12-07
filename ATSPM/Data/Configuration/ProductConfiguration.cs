using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Product configuration
    /// </summary>
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasComment("Products");

            builder.Property(e => e.Manufacturer)
                .IsRequired()
                .HasMaxLength(48);

            builder.Property(e => e.Model)
                .IsRequired()
                .HasMaxLength(48);

            builder.Property(e => e.DeviceType)
                .HasMaxLength(12)
                .HasDefaultValue(DeviceTypes.Unknown);

            builder.Property(e => e.Notes)
                .HasMaxLength(512);
        }
    }
}
