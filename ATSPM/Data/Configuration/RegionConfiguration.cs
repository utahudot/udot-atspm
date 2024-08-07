﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Configuration/RegionConfiguration.cs
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
    /// Region configuration
    /// </summary>
    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            builder.ToTable(t => t.HasComment("Regions"));

            builder.Property(e => e.Description).HasMaxLength(50);
            builder.Property(e => e.Description).IsRequired();

        }
    }
}
