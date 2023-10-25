using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Signal configuration
    /// </summary>
    public class SignalConfiguration : IEntityTypeConfiguration<Signal>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Signal> builder)
        {
            builder.HasComment("Signal Controllers");

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
                .HasDefaultValueSql("('Initial')");

            builder.Property(e => e.PrimaryName)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValueSql("('')");

            builder.Property(e => e.SecondaryName)
                .HasMaxLength(100);

            builder.Property(e => e.SignalIdentifier)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(e => e.VersionAction).HasDefaultValueSql("((10))");
        }
    }
}
