#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data.Configuration/MeasureOptionPresetConfiguration.cs
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
using Utah.Udot.Atspm.Data.Models.MeasureOptions;
using Utah.Udot.Atspm.Data.Utility;

#nullable disable

namespace Utah.Udot.Atspm.Data.Configuration
{
    /// <summary>
    /// Measure option presets configuration
    /// </summary>
    public class MeasureOptionPresetConfiguration : IEntityTypeConfiguration<MeasureOptionPreset>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<MeasureOptionPreset> builder)
        {
            builder.ToTable(t => t.HasComment("Measure Option Presets"));

            builder.Property(e => e.Name).HasMaxLength(512);

            builder.Property(e => e.Option).HasConversion(v => JsonConvert.SerializeObject(v, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new AssemblySerializationBinder<AtspmOptionsBase>()
            }),
            v => JsonConvert.DeserializeObject<AtspmOptionsBase>(v, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new AssemblySerializationBinder<AtspmOptionsBase>()
            }));
        }
    }
}
