#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Configuration/DetectionTypeConfiguration.cs
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
using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Detection type configuration
    /// </summary>
    public class DetectionTypeConfiguration : IEntityTypeConfiguration<DetectionType>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<DetectionType> builder)
        {
            builder.HasComment("Detector Types");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Abbreviation).HasMaxLength(5);

            builder.Property(e => e.Description).HasMaxLength(128).IsRequired();

            builder.HasData(typeof(DetectionTypes).GetFields().Where(t => t.FieldType == typeof(DetectionTypes)).Select(s => new DetectionType()
            {
                Id = (DetectionTypes)s.GetValue(s),
                Description = s.GetCustomAttribute<DisplayAttribute>().Name,
                Abbreviation = s.GetValue(s).ToString(),
                DisplayOrder = s.GetCustomAttribute<DisplayAttribute>().Order
            }));
        }
    }
}
