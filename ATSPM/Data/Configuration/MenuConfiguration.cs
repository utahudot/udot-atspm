using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            builder.HasComment("Menu Items");

            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValueSql("('')");

            builder.Property(e => e.Application)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Controller)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValueSql("('')");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasData(
                new Menu
                {
                    Id = 1,
                    Name = "Measures",
                    Controller = "#",
                    Action = "#",
                    ParentId = 0,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 10
                },
                new Menu
                {
                    Id = 2,
                    Name = "Reports",
                    Controller = "#",
                    Action = "#",
                    ParentId = 0,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 20
                },
                new Menu
                {
                    Id = 3,
                    Name = "Log Action Taken",
                    Controller = "ActionLogs",
                    Action = "Create",
                    ParentId = 0,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 30
                },
                new Menu
                {
                    Id = 4,
                    Name = "Links",
                    Controller = "#",
                    Action = "#",
                    ParentId = 0,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 40
                },
                new Menu
                {
                    Id = 5,
                    Name = "FAQ",
                    Controller = "FAQs",
                    Action = "Display",
                    ParentId = 0,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 50
                },
                //new Menu
                //{
                //    Id = 32,
                //    Name = "UDOT Traffic Signal Documents",
                //    Controller = "#",
                //    Action = "#",
                //    ParentId = 0,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 60
                //},
                //new Menu
                //{
                //    Id = 6,
                //    Name = "ATSPM Manuals",
                //    Controller = "#",
                //    Action = "#",
                //    ParentId = 0,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 70
                //},
                //new Menu
                //{
                //    Id = 7,
                //    Name = "ATSPM Presentations",
                //    Controller = "#",
                //    Action = "#",
                //    ParentId = 0,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 80
                //},
                new Menu
                {
                    Id = 17,
                    Name = "Agency Configuration",
                    Controller = "Jurisdictions",
                    Action = "Index",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 80
                },
                new Menu
                {
                    Id = 27,
                    Name = "About",
                    Controller = "Home",
                    Action = "About",
                    ParentId = 0,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 90
                },
                new Menu
                {
                    Id = 11,
                    Name = "Admin",
                    Controller = "#",
                    Action = "#",
                    ParentId = 0,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 100
                },
                new Menu
                {
                    Id = 9,
                    Name = "Signal",
                    Controller = "DefaultCharts",
                    Action = "Index",
                    ParentId = 1,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 10
                },
                new Menu
                {
                    Id = 10,
                    Name = "Purdue Link Pivot",
                    Controller = "LinkPivot",
                    Action = "Analysis",
                    ParentId = 1,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 20
                },
                new Menu
                {
                    Id = 8,
                    Name = "Chart Usage",
                    Controller = "ActionLogs",
                    Action = "Usage",
                    ParentId = 2,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 10
                },
                new Menu
                {
                    Id = 71,
                    Name = "Configuration",
                    Controller = "Signals",
                    Action = "SignalDetail",
                    ParentId = 2,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 15
                },
                new Menu
                {
                    Id = 48,
                    Name = "Aggregate Data",
                    Controller = "AggregateDataExport",
                    Action = "Index",
                    ParentId = 2,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 20
                },
                new Menu
                {
                    Id = 58,
                    Name = "Left Turn Gap Analysis",
                    Controller = "LeftTurnGapReport",
                    Action = "Index",
                    ParentId = 2,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 25
                },
                //new Menu
                //{
                //    Id =42,
                //    Name = "GDOT ATSPM Installation Manual",
                //    Controller = "Images",
                //    Action = "ATSPM_Installation_Manual_2020-01-28.pdf",
                //    ParentId = 6,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 10
                //},
                //new Menu
                //{
                //    Id = 34,
                //    Name = "GDOT ATSPM Component Details",
                //    Controller = "Images",
                //    Action = "ATSPM_Component_Details_20200120.pdf",
                //    ParentId = 6,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 20
                //},
                //new Menu
                //{
                //    Id = 43,
                //    Name = "GDOT ATSPM Reporting Details",
                //    Controller = "Images",
                //    Action = "ATSPM_Reporting_Details_20200121.pdf",
                //    ParentId = 6,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 30
                //},
                //new Menu
                //{
                //    Id = 70,
                //    Name = "ATSPM_User Case Examples_Manual",
                //    Controller = "Images",
                //    Action = "ATSPM_User Case Examples_Manual_20200128.pdf",
                //    ParentId = 6,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 40
                //},
                //new Menu
                //{
                //    Id = 38,
                //    Name = "ATSPM ITS California 9-21-16",
                //    Controller = "Images",
                //    Action = "ATSPM_ITS_CA_9-21-16.pdf",
                //    ParentId = 7,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 10
                //},
                //new Menu
                //{
                //    Id = 37,
                //    Name = "ATSPM CO WY ITE & Rocky Mtn 10-20-16",
                //    Controller = "Images",
                //    Action = "ATSPM_CO-WY_ITE___ITS_Rocky_Mtn_10-20-16.pdf",
                //    ParentId = 7,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 20
                //},
                //new Menu
                //{
                //    Id = 36,
                //    Name = "ATSPM EDC4 Minnesota 10-25-16",
                //    Controller = "Images",
                //    Action = "ATSPM_EDC4_Minnesota_10-25-16.pdf",
                //    ParentId = 7,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 30
                //},
                //new Menu
                //{
                //    Id = 35,
                //    Name = "ATSPM UDOT Conference 11-2-16",
                //    Controller = "Images",
                //    Action = "ATSPM_UDOT_Conference_11-2-16.pdf",
                //    ParentId = 7,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 40
                //},
                //new Menu
                //{
                //    Id = 45,
                //    Name = "Mark Taylor",
                //    Controller = "Images",
                //    Action = "TTTMarkTaylor.pdf",
                //    ParentId = 7,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 50
                //},
                //new Menu
                //{
                //    Id = 62,
                //    Name = "ATSPM UDOT Conference 11-6-18",
                //    Controller = "Images",
                //    Action = "Session 27_ATSPMs_UDOT Conference_20181106.pdf",
                //    ParentId = 7,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 60
                //},
                //new Menu
                //{
                //    Id = 46,
                //    Name = "Jamie Mackey",
                //    Controller = "Images",
                //    Action = "TTTJamieMackey.pdf",
                //    ParentId = 7,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 70
                //},
                //new Menu
                //{
                //    Id = 47,
                //    Name = "Derek Lowe & Shane Johnson",
                //    Controller = "Images",
                //    Action = "TTTDerekLoweShaneJohnson.pdf",
                //    ParentId = 7,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 80
                //},
                new Menu
                {
                    Id = 12,
                    Name = "Signal Configuration",
                    Controller = "Signals",
                    Action = "Index",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 10
                },
                new Menu
                {
                    Id = 16,
                    Name = "Menu Configuration",
                    Controller = "Menus",
                    Action = "Index",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 20
                },
                new Menu
                {
                    Id = 13,
                    Name = "Route Configuration",
                    Controller = "Routes",
                    Action = "Index",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 30
                },
                new Menu
                {
                    Id = 57,
                    Name = "General Settings",
                    Controller = "GeneralSettings",
                    Action = "Edit",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 40
                },
                new Menu
                {
                    Id = 49,
                    Name = "Raw Data Export",
                    Controller = "DataExport",
                    Action = "RawDataExport",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 50
                },
                new Menu
                {
                    Id = 54,
                    Name = "Watch Dog",
                    Controller = "WatchDogApplicationSettings",
                    Action = "Edit",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 60
                },
                new Menu
                {
                    Id = 56,
                    Name = "Database Archive Settings",
                    Controller = "DatabaseArchiveSettings",
                    Action = "edit",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 70
                },
                new Menu
                {
                    Id = 52,
                    Name = "FAQs",
                    Controller = "FAQs",
                    Action = "Index",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 70
                },
                new Menu
                {
                    Id = 51,
                    Name = "Users",
                    Controller = "SPMUsers",
                    Action = "Index",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 90
                },
                new Menu
                {
                    Id = 15,
                    Name = "Roles",
                    Controller = "Account",
                    Action = "RoleAddToUser",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 100
                },
                new Menu
                {
                    Id = 100,
                    Name = "Measure Defaults Settings",
                    Controller = "MeasuresDefaults",
                    Action = "Index",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 45
                },
                new Menu
                {
                    Id = 66,
                    Name = "Area Configuration",
                    Controller = "Areas",
                    Action = "Index",
                    ParentId = 11,
                    Application = "SignalPerformanceMetrics",
                    DisplayOrder = 31
                }
                //new Menu
                //{
                //    Id = 61,
                //    Name = "NEMA Phase # Convention at UDOT",
                //    Controller = "Images",
                //    Action = "NEMA Phase # Convention UDOT.pdf",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 10
                //},
                //new Menu
                //{
                //    Id = 39,
                //    Name = "TSMP UDOT V1-2 2-5-16",
                //    Controller = "Images",
                //    Action = "TSMP_UDOT_v1-2_2-5-16.pdf",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 20
                //},
                //new Menu
                //{
                //    Id = 40,
                //    Name = "Emergency Traffic Signal Response Plan UDOT 5-6-16",
                //    Controller = "Images",
                //    Action = "EmergencyTrafficSignalResponsePlanUDOT5-6-16.pdf",
                //    ParentId = 4,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 1
                //},
                //new Menu
                //{
                //    Id = 41,
                //    Name = "Signal Ops QIT Final Report",
                //    Controller = "Images",
                //    Action = "Signal Ops QIT Final Report Released.pdf",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 40
                //},
                //new Menu
                //{
                //    Id = 55,
                //    Name = "Detector Accuracy Information",
                //    Controller = "Images",
                //    Action = "DetectorAccuracyInformation.pdf",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 50
                //},
                //new Menu
                //{
                //    Id = 60,
                //    Name = "Wavetronix Matrix Latency Information",
                //    Controller = "Images",
                //    Action = "WavetronixMatrixLatencyInformation.pdf",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 60
                //},
                //new Menu
                //{
                //    Id = 64,
                //    Name = "UDOT Detection Form 2019-04-09",
                //    Controller = "Images",
                //    Action = "UDOT Detection Form 2019-04-09.xlsm",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 70 
                //},
                //new Menu
                //{
                //    Id = 65,
                //    Name = "UDOT Detection Form Printable Tables 2019-04-09",
                //    Controller = "Images",
                //    Action = "UDOT Detection Form Printable Tables 20190409.pdf",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 80 
                //},
                //new Menu
                //{
                //    Id = 68,
                //    Name = "Examples of Detector Setup DZ",
                //    Controller = "Images",
                //    Action = "Examples of Detector Setup 2017-05-02.pdf",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 90
                //},
                //new Menu
                //{
                //    Id = 69,
                //    Name = "Configuration - Detection Type - Log Action Taken",
                //    Controller = "Images",
                //    Action = "Configuration-DetectionType-LogActionTaken.pdf",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 100
                //},
                //new Menu
                //{
                //    Id = 66,
                //    Name = "AWS LFT and Detection Worksheets 2019-04-10",
                //    Controller = "Images",
                //    Action = "AWS LFT and Detection Worksheets 2019-04-10.xlsm",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder =110
                //},
                //new Menu
                //{
                //    Id = 67,
                //    Name = "AWS LFT and Detection Worksheets Printable ",
                //    Controller = "Images",
                //    Action = "AWS LFT and Detection Worksheets Printable.pdf",
                //    ParentId = 32,
                //    Application = "SignalPerformanceMetrics",
                //    DisplayOrder = 120
                //}
                );
        }
    }
}
