#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Configuration/VersionHistoryConfiguration.cs
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
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Version history configuration
    /// </summary>
    public class VersionHistoryConfiguration : IEntityTypeConfiguration<VersionHistory>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<VersionHistory> builder)
        {
            builder.HasComment("Version History");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(e => e.Notes)
                .HasMaxLength(512);

            builder.Property(e => e.Date)
                .IsRequired()
                .HasDefaultValue(DateTime.Now);

            builder.Property(e => e.Version)
                .IsRequired();

            builder.HasOne(d => d.Parent).WithMany(m => m.Children).HasForeignKey(d => d.ParentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
