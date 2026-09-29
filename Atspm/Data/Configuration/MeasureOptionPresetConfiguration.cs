#region license
// Copyright 2026 Utah Departement of Transportation
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

            builder.HasData(
                new MeasureOptionPreset
                {
                    Id = 4101,
                    Name = "Commuter Arterial",
                    MeasureTypeId = 41,
                    Option = BuildTimeOfDayPreset(
                        0.60,
                        0.42,
                        0.72,
                        0.40,
                        0.20,
                        0.16,
                        2,
                        4)
                },
                new MeasureOptionPreset
                {
                    Id = 4102,
                    Name = "Suburban Mixed-Use",
                    MeasureTypeId = 41,
                    Option = BuildTimeOfDayPreset(
                        0.55,
                        0.40,
                        0.68,
                        0.38,
                        0.22,
                        0.18,
                        2,
                        4)
                },
                new MeasureOptionPreset
                {
                    Id = 4103,
                    Name = "Retail / Commercial",
                    MeasureTypeId = 41,
                    Option = BuildTimeOfDayPreset(
                        0.50,
                        0.38,
                        0.62,
                        0.36,
                        0.25,
                        0.20,
                        3,
                        4)
                },
                new MeasureOptionPreset
                {
                    Id = 4104,
                    Name = "Weekend / Recreation",
                    MeasureTypeId = 41,
                    Option = BuildTimeOfDayPreset(
                        0.48,
                        0.36,
                        0.60,
                        0.34,
                        0.24,
                        0.20,
                        3,
                        5)
                });
        }

        private static TimeOfDayOptions BuildTimeOfDayPreset(
            double amEntryPctOfPeak,
            double amExitPctOfPeak,
            double pmEntryPctOfPeak,
            double pmExitPctOfPeak,
            double freeEntryPctOfDailyPeak,
            double freeEntryPctOfDynamicRange,
            int entrySustainedBins,
            int freeSustainedBins)
        {
            return new TimeOfDayOptions
            {
                AmEntryPctOfPeak = amEntryPctOfPeak,
                AmExitPctOfPeak = amExitPctOfPeak,
                PmEntryPctOfPeak = pmEntryPctOfPeak,
                PmExitPctOfPeak = pmExitPctOfPeak,
                FreeEntryPctOfDailyPeak = freeEntryPctOfDailyPeak,
                FreeEntryPctOfDynamicRange = freeEntryPctOfDynamicRange,
                EntrySustainedBins = entrySustainedBins,
                FreeSustainedBins = freeSustainedBins,
                FreeFallbackTime = "23:30",
                MaxAmEndTime = "10:00",
                MaxPmEndTime = "20:00",
                LaneCapacityVehiclesPerHour = 800,
                ApproachVolumeAssumedLanes = 2,
                SplitReviewThresholdPercent = 35,
                ShoulderReviewThresholdPercent = 45
            };
        }
    }
}
