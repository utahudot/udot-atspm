using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class MetricTypeConfiguration : IEntityTypeConfiguration<MetricType>
    {
        public void Configure(EntityTypeBuilder<MetricType> builder)
        {
            builder.HasComment("Metric Types");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Abbreviation).HasMaxLength(8);

            builder.Property(e => e.ChartName).HasMaxLength(50);

            builder.HasData(
                new MetricType
                {
                    Id = 1,
                    ChartName = "Purdue Phase Termination",
                    Abbreviation = "PPT",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 1
                },
                new MetricType
                {
                    Id = 2,
                    ChartName = "Split Monitor",
                    Abbreviation = "SM",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 5
                },
                new MetricType
                {
                    Id = 3,
                    ChartName = "Pedestrian Delay",
                    Abbreviation = "PedD",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 10
                },
                new MetricType
                {
                    Id = 4,
                    ChartName = "Preemption Details",
                    Abbreviation = "PD",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 15
                },
                new MetricType
                {
                    Id = 17,
                    ChartName = "Timing And Actuation",
                    Abbreviation = "TAA",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 20
                },
                new MetricType
                {
                    Id = 12,
                    ChartName = "Purdue Split Failure",
                    Abbreviation = "SF",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 30
                },
                new MetricType
                {
                    Id = 11,
                    ChartName = "Yellow and Red Actuations",
                    Abbreviation = "YRA",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 35
                },
                new MetricType
                {
                    Id = 5,
                    ChartName = "Turning Movement Counts",
                    Abbreviation = "TMC",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 40
                },
                new MetricType
                {
                    Id = 7,
                    ChartName = "Approach Volume",
                    Abbreviation = "AV",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 45
                },
                new MetricType
                {
                    Id = 8,
                    ChartName = "Approach Delay",
                    Abbreviation = "AD",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 50
                },
                new MetricType
                {
                    Id = 9,
                    ChartName = "Arrivals On Red",
                    Abbreviation = "AoR",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 55
                },
                new MetricType
                {
                    Id = 6,
                    ChartName = "Purdue Coordination Diagram",
                    Abbreviation = "PCD",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 60
                },
                new MetricType
                {
                    Id = 10,
                    ChartName = "Approach Speed",
                    Abbreviation = "Speed",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 65
                },
                new MetricType
                {
                    Id = 13,
                    ChartName = "Purdue Link Pivot",
                    Abbreviation = "LP",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 70
                },
                new MetricType
                {
                    Id = 15,
                    ChartName = "Preempt Service",
                    Abbreviation = "PS",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 75
                },
                new MetricType
                {
                    Id = 14,
                    ChartName = "Preempt Service Request",
                    Abbreviation = "PSR",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 80
                },
                new MetricType
                {
                    Id = 16,
                    ChartName = "Detector Activation Count",
                    Abbreviation = "DVA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 85
                },

                new MetricType
                {
                    Id = 18,
                    ChartName = "Approach Pcd", //"Purdue Coodination",
                    Abbreviation = "APCD", // "PCDA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 102
                },
                new MetricType
                {
                    Id = 19,
                    ChartName = "Approach Cycle", // "Cycle"
                    Abbreviation = "CA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 103
                },
                new MetricType
                {
                    Id = 20,
                    ChartName = "Approach Split Fail", //"Purdue Split Failure",
                    Abbreviation = "SFA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 104
                },
                new MetricType
                {
                    Id = 22,
                    ChartName = "Signal Preemption", //"Preemption",
                    Abbreviation = "PreemptA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 105
                },
                new MetricType
                {
                    Id = 24,
                    ChartName = "Signal Priority", // "Transit Signal Priority",
                    Abbreviation = "TSPA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 106
                },
                new MetricType
                {
                    Id = 25,
                    ChartName = "Approach Speed",
                    Abbreviation = "ASA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 107
                },
                new MetricType
                {
                    Id = 26,
                    ChartName = "Approach Yellow Red Activations", //"Yellow Red Activations",
                    Abbreviation = "YRAA",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 108
                },
                new MetricType
                {
                    Id = 27,
                    ChartName = "Signal Event Count",
                    Abbreviation = "SEC",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 109
                },
                new MetricType
                {
                    Id = 28,
                    ChartName = "Approach Event Count",
                    Abbreviation = "AEC",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 110
                },
                new MetricType
                {
                    Id = 29,
                    ChartName = "Phase Termination",
                    Abbreviation = "AEC",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 111
                },
                new MetricType
                {
                    Id = 30,
                    ChartName = "Phase Pedestrian Delay",
                    Abbreviation = "APD",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 112
                },
                new MetricType
                {
                    Id = 31,
                    ChartName = "Left Turn Gap Analysis",
                    Abbreviation = "LTGA",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 112
                },
                new MetricType
                {
                    Id = 32,
                    ChartName = "Wait Time",
                    Abbreviation = "WT",
                    ShowOnWebsite = true,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 113
                },
                new MetricType
                {
                    Id = 33,
                    ChartName = "Gap Vs Demand",
                    Abbreviation = "GVD",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = false,
                    DisplayOrder = 115
                },
                new MetricType
                {
                    Id = 34,
                    ChartName = "Left Turn Gap",
                    Abbreviation = "LTG",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 114
                },
                new MetricType
                {
                    Id = 35,
                    ChartName = "Split Monitor",
                    Abbreviation = "SM",
                    ShowOnWebsite = false,
                    ShowOnAggregationSite = true,
                    DisplayOrder = 120
                });
        }
    }
}
