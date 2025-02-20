#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Configuration/DevicesConfiguration.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Utility;

namespace Utah.Udot.Atspm.Data.Configuration
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

            builder.Property(e => e.DeviceIdentifier)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.DeviceProperties)
                .HasMaxLength(1024)
                .HasConversion<DictionaryToJsonValueConverter<string, object>>(new DictionaryValueComparer<string, object>());

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
