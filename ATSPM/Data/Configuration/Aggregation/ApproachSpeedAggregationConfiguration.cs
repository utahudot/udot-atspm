#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - %Namespace%/ApproachSpeedAggregationConfiguration.cs
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

namespace ATSPM.Data.Configuration.Aggregation
{
    public class ApproachSpeedAggregationConfiguration : IEntityTypeConfiguration<ApproachSpeedAggregation>
    {
        public void Configure(EntityTypeBuilder<ApproachSpeedAggregation> builder)
        {
            builder.HasComment("Approach Speed Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.LocationIdentifier, e.ApproachId });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}
