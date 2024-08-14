﻿#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Configuration/LocationTypeConfiguration.cs
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
    /// Location type configuration
    /// </summary>
    public class LocationTypeConfiguration : IEntityTypeConfiguration<LocationType>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<LocationType> builder)
        {
            builder.ToTable(t => t.HasComment("Location Types"));

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Icon)
                .IsUnicode(true);

            builder.HasData(
                new LocationType()
                {
                    Id = 1,
                    Name = "Intersection",
                },
                new LocationType()
                {
                    Id = 2,
                    Name = "Ramp",
                },
                new LocationType()
                {
                    Id = 3,
                    Name = "Side Walk",
                },
                new LocationType()
                {
                    Id = 4,
                    Name = "Trail",
                });
        }
    }
}
