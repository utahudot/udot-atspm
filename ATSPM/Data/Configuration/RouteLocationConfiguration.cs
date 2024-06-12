#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Configuration/RouteLocationConfiguration.cs
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
    /// Route Location configuration
    /// </summary>
    public class RouteLocationConfiguration : IEntityTypeConfiguration<RouteLocation>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<RouteLocation> builder)
        {
            builder.HasComment("Route Locations");

            builder.HasIndex(e => e.RouteId);

            builder.HasIndex(p => new { p.RouteId, p.LocationIdentifier }).IsUnique();

            builder.Property(e => e.LocationIdentifier)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasOne(p => p.PrimaryDirection).WithMany(a => a.PrimaryDirections).HasForeignKey(k => k.PrimaryDirectionId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.OpposingDirection).WithMany(a => a.OpposingDirections).HasForeignKey(k => k.OpposingDirectionId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.PreviousLocationDistance).WithMany(a => a.PreviousLocations).HasForeignKey(k => k.PreviousLocationDistanceId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.NextLocationDistance).WithMany(a => a.NextLocations).HasForeignKey(k => k.NextLocationDistanceId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
