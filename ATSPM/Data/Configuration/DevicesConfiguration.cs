using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Device configuration
    /// </summary>
    public class DevicesConfiguration : IEntityTypeConfiguration<Device>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Device> builder)
        {
            builder.ToTable(t => t.HasComment("Devices"));

            builder.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(15)
                .HasDefaultValueSql("('0.0.0.0')");

            builder.Property(e => e.DeviceStatus)
                .HasMaxLength(Enum.GetNames(typeof(DeviceStatus)).Max(m => m.Length))
                .HasDefaultValue(DeviceStatus.Unknown);

            builder.Property(e => e.DeviceType)
                .HasMaxLength(Enum.GetNames(typeof(DeviceTypes)).Max(m => m.Length))
                .HasDefaultValue(DeviceTypes.Unknown);

            builder.Property(e => e.Notes)
                .HasMaxLength(512);
        }
    }
}
