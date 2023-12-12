using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Location configuration
    /// </summary>
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.HasComment("Location Controllers");

            builder.HasIndex(e => e.ControllerTypeId);

            builder.HasIndex(e => e.JurisdictionId);

            builder.HasIndex(e => e.RegionId);

            builder.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(15)
                .HasDefaultValueSql("('10.0.0.1')");

            builder.Property(e => e.JurisdictionId).HasDefaultValueSql("((0))");

            builder.Property(e => e.Latitude)
                .IsRequired();

            builder.Property(e => e.Longitude)
                .IsRequired();

            builder.Property(e => e.Note)
                .IsRequired()
                .HasMaxLength(256)
                .HasDefaultValueSql("('Initial')");

            builder.Property(e => e.PrimaryName)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValueSql("('')");

            builder.Property(e => e.SecondaryName)
                .HasMaxLength(100);

            builder.Property(e => e.LocationIdentifier)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(e => e.VersionAction).HasDefaultValueSql("((10))");
        }
    }
}
