using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Device configuration configuration
    /// </summary>
    public class DeviceConfigConfiguration : IEntityTypeConfiguration<DeviceConfiguration>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<DeviceConfiguration> builder)
        {
            builder.HasComment("DeviceConfiguration");

            builder.Property(e => e.Firmware)
                .IsRequired()
                .HasMaxLength(16);

            builder.Property(e => e.Notes)
                .HasMaxLength(512);

            builder.Property(e => e.Protocol)
                .HasMaxLength(Enum.GetNames(typeof(TransportProtocols)).Max().Length)
                .HasDefaultValue(TransportProtocols.Unknown);

            builder.Property(e => e.Port)
                .HasDefaultValueSql("((0))");

            builder.Property(e => e.Directory)
                .IsRequired(false)
                .HasMaxLength(1024);

            builder.Property(e => e.SearchTerms)
                .IsRequired(false)
                .HasMaxLength(512)
                .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v));

            builder.Property(e => e.UserName)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(e => e.Password)
                .IsRequired(false)
                .HasMaxLength(50);
        }
    }
}
