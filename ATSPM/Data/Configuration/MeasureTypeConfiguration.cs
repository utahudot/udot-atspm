#region license
// Copyright 2024 Utah Departement of Transportation
// for Data - ATSPM.Data.Configuration/MeasureTypeConfiguration.cs
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
    /// Measure type configuration
    /// </summary>
    public class MeasureTypeConfiguration : IEntityTypeConfiguration<MeasureType>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<MeasureType> builder)
        {
            builder.HasComment("Measure Types");

            //builder.Property(e => e.Id)
            //    .ValueGeneratedNever();

            builder.Property(e => e.Abbreviation).HasMaxLength(8);

            builder.Property(e => e.Name).HasMaxLength(50);

            builder.HasData(
                new MeasureType
                {
                    Id = 1,
                    Name = "Purdue Phase Termination",
                    Abbreviation = "PPT",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 1
                },
                new MeasureType
                {
                    Id = 2,
                    Name = "Split Monitor",
                    Abbreviation = "SM",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 5
                },
                new MeasureType
                {
                    Id = 3,
                    Name = "Pedestrian Delay",
                    Abbreviation = "PedD",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 10
                },
                new MeasureType
                {
                    Id = 4,
                    Name = "Preemption Details",
                    Abbreviation = "PD",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 15
                },
                new MeasureType
                {
                    Id = 17,
                    Name = "Timing And Actuation",
                    Abbreviation = "TAA",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 20
                },
                new MeasureType
                {
                    Id = 12,
                    Name = "Purdue Split Failure",
                    Abbreviation = "SF",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 30
                },
                new MeasureType
                {
                    Id = 11,
                    Name = "Yellow and Red Actuations",
                    Abbreviation = "YRA",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 35
                },
                new MeasureType
                {
                    Id = 5,
                    Name = "Turning Movement Counts",
                    Abbreviation = "TMC",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 40
                },
                new MeasureType
                {
                    Id = 7,
                    Name = "Approach Volume",
                    Abbreviation = "AV",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 45
                },
                new MeasureType
                {
                    Id = 8,
                    Name = "Approach Delay",
                    Abbreviation = "AD",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 50
                },
                new MeasureType
                {
                    Id = 9,
                    Name = "Arrivals On Red",
                    Abbreviation = "AoR",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 55
                },
                new MeasureType
                {
                    Id = 6,
                    Name = "Purdue Coordination Diagram",
                    Abbreviation = "PCD",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 60
                },
                new MeasureType
                {
                    Id = 10,
                    Name = "Approach Speed",
                    Abbreviation = "Speed",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 65
                },
                new MeasureType
                {
                    Id = 13,
                    Name = "Purdue Link Pivot",
                    Abbreviation = "LP",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 70
                },
                new MeasureType
                {
                    Id = 15,
                    Name = "Preempt Service",
                    Abbreviation = "PS",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 75
                },
                new MeasureType
                {
                    Id = 14,
                    Name = "Preempt Service Request",
                    Abbreviation = "PSR",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 80
                },
                new MeasureType
                {
                    Id = 16,
                    Name = "Detector Activation Count",
                    Abbreviation = "DVA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 85
                },

                new MeasureType
                {
                    Id = 18,
                    Name = "Approach Pcd", //"Purdue Coodination",
                    Abbreviation = "APCD", // "PCDA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 102
                },
                new MeasureType
                {
                    Id = 19,
                    Name = "Approach Cycle", // "Cycle"
                    Abbreviation = "CA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 103
                },
                new MeasureType
                {
                    Id = 20,
                    Name = "Approach Split Fail", //"Purdue Split Failure",
                    Abbreviation = "SFA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 104
                },
                new MeasureType
                {
                    Id = 22,
                    Name = "Location Preemption", //"Preemption",
                    Abbreviation = "PreemptA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 105
                },
                new MeasureType
                {
                    Id = 24,
                    Name = "Location Priority", // "Transit Location Priority",
                    Abbreviation = "TSPA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 106
                },
                new MeasureType
                {
                    Id = 25,
                    Name = "Approach Speed",
                    Abbreviation = "ASA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 107
                },
                new MeasureType
                {
                    Id = 26,
                    Name = "Approach Yellow Red Activations", //"Yellow Red Activations",
                    Abbreviation = "YRAA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 108
                },
                new MeasureType
                {
                    Id = 27,
                    Name = "Location Event Count",
                    Abbreviation = "SEC",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 109
                },
                new MeasureType
                {
                    Id = 28,
                    Name = "Approach Event Count",
                    Abbreviation = "AEC",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 110
                },
                new MeasureType
                {
                    Id = 29,
                    Name = "Phase Termination",
                    Abbreviation = "AEC",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 111
                },
                new MeasureType
                {
                    Id = 30,
                    Name = "Phase Pedestrian Delay",
                    Abbreviation = "APD",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 112
                },
                new MeasureType
                {
                    Id = 31,
                    Name = "Left Turn Gap Analysis",
                    Abbreviation = "LTGA",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 112
                },
                new MeasureType
                {
                    Id = 32,
                    Name = "Wait Time",
                    Abbreviation = "WT",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 113
                },
                new MeasureType
                {
                    Id = 33,
                    Name = "Gap Vs Demand",
                    Abbreviation = "GVD",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 115
                },
                new MeasureType
                {
                    Id = 34,
                    Name = "Left Turn Gap",
                    Abbreviation = "LTG",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 114
                },
                new MeasureType
                {
                    Id = 35,
                    Name = "Split Monitor",
                    Abbreviation = "SM",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 120
                },
                new MeasureType
                {
                    Id = 36,
                    Name = "Green Time Utilization",
                    Abbreviation = "GTU",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 130
                });
        }
    }
}
