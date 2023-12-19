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

            //TODO: add this back in later
            //builder.HasData(
            //    new LocationType()
            //    {
            //        Name = "Intersection",
            //    },
            //    new LocationType()
            //    {
            //        Name = "Ramp",
            //    },
            //    new LocationType()
            //    {
            //        Name = "Side Walk",
            //    },
            //    new LocationType()
            //    {
            //        Name = "Trail",
            //    });
        }
    }
}
