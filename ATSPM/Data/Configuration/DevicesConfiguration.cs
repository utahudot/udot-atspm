﻿using ATSPM.Data.Enums;
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
            builder.HasComment("Devices");

            builder.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(15)
                .HasDefaultValueSql("('10.0.0.1')");

            builder.Property(e => e.DeviceStatus)
                .HasMaxLength(12)
                .HasDefaultValue(DeviceStatus.Unknown);

            builder.Property(e => e.Notes)
                .HasMaxLength(512);
        }
    }
}