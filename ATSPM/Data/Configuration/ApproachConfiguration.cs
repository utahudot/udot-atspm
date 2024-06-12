#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Configuration/ApproachConfiguration.cs
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
    /// Approach configuration
    /// </summary>
    public class ApproachConfiguration : IEntityTypeConfiguration<Approach>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Approach> builder)
        {
            builder.HasComment("Approaches");
            
            builder.HasIndex(e => e.DirectionTypeId);

            builder.HasIndex(e => e.LocationId);
        }
    }
}
