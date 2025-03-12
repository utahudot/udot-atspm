#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Configuration/DeviceConfigConfiguration.cs
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
using Newtonsoft.Json;
using Utah.Udot.Atspm.Data.Enums;
using Utah.Udot.Atspm.Data.Utility;

#nullable disable

namespace Utah.Udot.Atspm.Data.Configuration
{
    /// <summary>
    /// Device configuration configuration
    /// </summary>
    public class DeviceConfigConfiguration : IEntityTypeConfiguration<DeviceConfiguration>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<DeviceConfiguration> builder)
        {
            builder.ToTable(t => t.HasComment("Device Configuration"));

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(24);

            builder.Property(e => e.Notes)
                .HasMaxLength(512);

            builder.Property(e => e.Protocol)
                .HasMaxLength(Enum.GetNames(typeof(TransportProtocols)).Max().Length)
                .HasDefaultValue(TransportProtocols.Unknown);

            builder.Property(e => e.ConnectionProperties)
            .HasMaxLength(1024)
            .HasConversion<DictionaryToJsonValueConverter<string, object>>(new DictionaryValueComparer<string, object>());

            builder.Property(e => e.Port)
                .HasDefaultValueSql("((0))");

            builder.Property(e => e.Path)
                .IsRequired(false)
                .HasMaxLength(512);

            builder.Property(e => e.Query)
                .IsRequired(false)
                .HasDefaultValueSql("('[]')")
                .HasMaxLength(512)
                .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v));

            builder.Property(e => e.ConnectionTimeout)
                .HasDefaultValueSql("((2000))");

            builder.Property(e => e.OperationTimeout)
                .HasDefaultValueSql("((2000))");

            builder.Property(e => e.Decoders)
                .IsRequired(false)
                .HasDefaultValueSql("('[]')")
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
