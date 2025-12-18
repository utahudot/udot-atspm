#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Configuration/AreaConfiguration.cs
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

namespace Utah.Udot.Atspm.Data.Configuration
{
    /// <summary>
    /// Entity Framework configuration for the <see cref="UsageEntry"/> entity.
    /// Defines table mapping, property constraints, and indexes.
    /// </summary>
    public class UsageEntryConfiguration : IEntityTypeConfiguration<UsageEntry>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<UsageEntry> builder)
        {
            // Primary key
            builder.HasKey(x => x.Id);

            // Required properties
            builder.Property(x => x.Timestamp)
                .IsRequired();

            builder.Property(x => x.StatusCode)
                .IsRequired();

            builder.Property(x => x.DurationMs)
                .IsRequired();

            builder.Property(x => x.Success)
                .IsRequired();

            // Strings with sensible max lengths
            builder.Property(x => x.ApiName)
               .HasMaxLength(32);

            builder.Property(x => x.TraceId)
                .HasMaxLength(100);

            builder.Property(x => x.ConnectionId)
                .HasMaxLength(100);

            builder.Property(x => x.RemoteIp)
                .HasMaxLength(45); // IPv6 max length

            builder.Property(x => x.UserAgent)
                .HasMaxLength(1024);

            builder.Property(x => x.UserId)
                .HasMaxLength(200);

            builder.Property(x => x.Route)
                .HasMaxLength(2000);

            builder.Property(x => x.QueryString)
                .HasMaxLength(2000);

            builder.Property(x => x.Method)
                .HasMaxLength(20);

            builder.Property(x => x.Controller)
                .HasMaxLength(200);

            builder.Property(x => x.Action)
                .HasMaxLength(200);

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            // Nullable properties
            builder.Property(x => x.ResultCount)
                .IsRequired(false);

            builder.Property(x => x.ResultSizeBytes)
                .IsRequired(false);

            builder.Property(x => x.ErrorMessage)
                .IsRequired(false);

            // Indexes for common queries
            builder.HasIndex(x => x.Timestamp);
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Route);
            builder.HasIndex(x => x.StatusCode);
        }
    }

}
