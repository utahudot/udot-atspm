using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Measure options configuration
    /// </summary>
    public class MeasureOptionsConfiguration : IEntityTypeConfiguration<MeasureOption>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<MeasureOption> builder)
        {
            builder.HasComment("Measure Options");

            builder.Property(e => e.Option).HasMaxLength(128);

            builder.Property(e => e.Value).HasMaxLength(512);

            builder.HasData(
                new MeasureOption() { Id = 4, Option = "binSize", Value = "15", MeasureTypeId = 8 },
                new MeasureOption() { Id = 10, Option = "binSize", Value = "15", MeasureTypeId = 10 },
                new MeasureOption() { Id = 18, Option = "binSize", Value = "15", MeasureTypeId = 7 },
                new MeasureOption() { Id = 19, Option = "showAdvanceDetection", Value = "TRUE", MeasureTypeId = 7 },
                new MeasureOption() { Id = 20, Option = "showDirectionalSplits", Value = "TRUE", MeasureTypeId = 7 },
                new MeasureOption() { Id = 21, Option = "showNbEbVolume", Value = "TRUE", MeasureTypeId = 7 },
                new MeasureOption() { Id = 22, Option = "showSbWbVolume", Value = "TRUE", MeasureTypeId = 7 },
                new MeasureOption() { Id = 23, Option = "showTMCDetection", Value = "TRUE", MeasureTypeId = 7 },
                new MeasureOption() { Id = 24, Option = "showTotalVolume", Value = "FALSE", MeasureTypeId = 7 },
                new MeasureOption() { Id = 27, Option = "binSize", Value = "15", MeasureTypeId = 31 },
                new MeasureOption() { Id = 28, Option = "gap1Max", Value = "3.3", MeasureTypeId = 31 },
                new MeasureOption() { Id = 29, Option = "gap1Min", Value = "1", MeasureTypeId = 31 },
                new MeasureOption() { Id = 30, Option = "gap2Max", Value = "3.7", MeasureTypeId = 31 },
                new MeasureOption() { Id = 31, Option = "gap2Min", Value = "3.3", MeasureTypeId = 31 },
                new MeasureOption() { Id = 32, Option = "gap3Max", Value = "7.4", MeasureTypeId = 31 },
                new MeasureOption() { Id = 33, Option = "gap3Min", Value = "3.7", MeasureTypeId = 31 },
                new MeasureOption() { Id = 34, Option = "gap4Min", Value = "7.4", MeasureTypeId = 31 },
                new MeasureOption() { Id = 35, Option = "trendLineGapThreshold", Value = "7.4", MeasureTypeId = 31 },
                new MeasureOption() { Id = 36, Option = "binSize", Value = "15", MeasureTypeId = 6 },
                new MeasureOption() { Id = 39, Option = "showPlanStatistics", Value = "TRUE", MeasureTypeId = 6 },
                new MeasureOption() { Id = 40, Option = "showVolumes", Value = "TRUE", MeasureTypeId = 6 },
                new MeasureOption() { Id = 43, Option = "pedRecallThreshold", Value = "75", MeasureTypeId = 3 },
                new MeasureOption() { Id = 44, Option = "showCycleLength", Value = "TRUE", MeasureTypeId = 3 },
                new MeasureOption() { Id = 45, Option = "showPedBeginWalk", Value = "TRUE", MeasureTypeId = 3 },
                new MeasureOption() { Id = 46, Option = "showPedRecall", Value = "FALSE", MeasureTypeId = 3 },
                new MeasureOption() { Id = 47, Option = "showPercentDelay", Value = "TRUE", MeasureTypeId = 3 },
                new MeasureOption() { Id = 48, Option = "timeBuffer", Value = "15", MeasureTypeId = 3 },
                new MeasureOption() { Id = 50, Option = "selectedConsecutiveCount", Value = "1", MeasureTypeId = 1 },
                new MeasureOption() { Id = 54, Option = "firstSecondsOfRed", Value = "5", MeasureTypeId = 12 },
                new MeasureOption() { Id = 55, Option = "showAvgLines", Value = "TRUE", MeasureTypeId = 12 },
                new MeasureOption() { Id = 56, Option = "showFailLines", Value = "TRUE", MeasureTypeId = 12 },
                new MeasureOption() { Id = 57, Option = "showPercentFailLines", Value = "FALSE", MeasureTypeId = 12 },
                new MeasureOption() { Id = 58, Option = "percentileSplit", Value = "85", MeasureTypeId = 2 },
                new MeasureOption() { Id = 69, Option = "extendStartStopSearch", Value = "2", MeasureTypeId = 17 },
                new MeasureOption() { Id = 71, Option = "showAdvancedCount", Value = "TRUE", MeasureTypeId = 17 },
                new MeasureOption() { Id = 72, Option = "showAdvancedDilemmaZone", Value = "TRUE", MeasureTypeId = 17 },
                new MeasureOption() { Id = 73, Option = "showAllLanesInfo", Value = "FALSE", MeasureTypeId = 17 },
                new MeasureOption() { Id = 76, Option = "showLaneByLaneCount", Value = "TRUE", MeasureTypeId = 17 },
                new MeasureOption() { Id = 79, Option = "showPedestrianActuation", Value = "TRUE", MeasureTypeId = 17 },
                new MeasureOption() { Id = 80, Option = "showPedestrianIntervals", Value = "TRUE", MeasureTypeId = 17 },
                new MeasureOption() { Id = 83, Option = "showStopBarPresence", Value = "TRUE", MeasureTypeId = 17 },
                new MeasureOption() { Id = 85, Option = "binSize", Value = "15", MeasureTypeId = 5 },
                new MeasureOption() { Id = 92, Option = "severeLevelSeconds", Value = "5", MeasureTypeId = 11 },
                new MeasureOption() { Id = 102, Option = "getVolume", Value = "TRUE", MeasureTypeId = 8 },
                new MeasureOption() { Id = 103, Option = "getPermissivePhase", Value = "TRUE", MeasureTypeId = 8 },
                new MeasureOption() { Id = 104, Option = "usePermissivePhase", Value = "TRUE", MeasureTypeId = 9 },
                new MeasureOption() { Id = 105, Option = "showPlanStatistics", Value = "TRUE", MeasureTypeId = 9 },
                new MeasureOption() { Id = 106, Option = "binSize", Value = "15", MeasureTypeId = 9 },
                new MeasureOption() { Id = 107, Option = "showArrivalsOnGreen", Value = "TRUE", MeasureTypeId = 6 },
                new MeasureOption() { Id = 113, Option = "xAxisBinSize", Value = "15", MeasureTypeId = 36 },
                new MeasureOption() { Id = 114, Option = "yAxisBinSize", Value = "4", MeasureTypeId = 36 },
                new MeasureOption() { Id = 115, Option = "binSize", Value = "15", MeasureTypeId = 32 });
        }
    }
}
