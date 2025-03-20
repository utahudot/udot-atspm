#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Configuration/MeasureOptionsConfiguration.cs
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
    /// Measure options configuration
    /// </summary>
    public class MeasureOptionsSaveConfiguration : IEntityTypeConfiguration<MeasureOptionsSave>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<MeasureOptionsSave> builder)
        {
            builder.ToTable(t => t.HasComment("Measure Options Save"));

            builder.Property(e => e.Name).HasMaxLength(512);
            builder.Property(e => e.MeasureTypeId).IsRequired();
            builder.Property(e => e.ModifiedByUserId).IsRequired();
            builder.Property(e => e.ModifiedOn).IsRequired();
            builder.Property(e => e.CreatedByUserId).IsRequired();
            builder.Property(e => e.CreatedOn).IsRequired();
            builder.Property(e => e.SelectedParametersJson).IsRequired();
        }
    }
}
