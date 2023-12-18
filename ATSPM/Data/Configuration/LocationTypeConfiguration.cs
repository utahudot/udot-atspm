using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Location type configuration
    /// </summary>
    public class LocationTypeConfiguration : IEntityTypeConfiguration<LocationType>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<LocationType> builder)
        {
            builder.HasComment("Location Types");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(e => e.Icon)
                .HasMaxLength(1024);
        }
    }
}
