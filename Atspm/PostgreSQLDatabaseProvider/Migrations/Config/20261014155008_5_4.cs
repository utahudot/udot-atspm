#region license
// Copyright 2026 Utah Departement of Transportation
// for PostgreSQLDatabaseProvider - Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations/20261014155008_5_4.cs
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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations
{
    /// <inheritdoc />
    public partial class _5_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MeasureType",
                columns: new[] { "Id", "Abbreviation", "DisplayOrder", "Modified", "ModifiedBy", "Name", "ShowOnAggregationSite", "ShowOnWebsite" },
                values: new object[] { 41, "TOD", 135, null, null, "Time Of Day", false, true });

            migrationBuilder.InsertData(
                table: "MeasureOptionPresets",
                columns: new[] { "Id", "MeasureTypeId", "Modified", "ModifiedBy", "Name", "Option" },
                values: new object[,]
                {
                    { 4101, 41, null, null, "Commuter Arterial", "{\"$type\":\"TimeOfDayOptions, \",\"LocationIdentifiers\":[],\"SelectedDates\":[],\"BinSizeMinutes\":15,\"DataSource\":0,\"AllDayPrimaryDirections\":[],\"AmPrimaryDirections\":[],\"PmPrimaryDirections\":[],\"AmEntryPctOfPeak\":0.6,\"AmExitPctOfPeak\":0.42,\"PmEntryPctOfPeak\":0.72,\"PmExitPctOfPeak\":0.4,\"FreeEntryPctOfDailyPeak\":0.2,\"FreeEntryPctOfDynamicRange\":0.16,\"EntrySustainedBins\":2,\"FreeSustainedBins\":4,\"FreeFallbackTime\":\"23:30\",\"MaxAmEndTime\":\"10:00\",\"MaxPmEndTime\":\"20:00\",\"LaneCapacityVehiclesPerHour\":800.0,\"ApproachVolumeAssumedLanes\":2.0,\"SplitReviewThresholdPercent\":35.0,\"ShoulderReviewThresholdPercent\":45.0,\"DirectionLaneCounts\":{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[System.Double, System.Private.CoreLib]], System.Private.CoreLib\"}}" },
                    { 4102, 41, null, null, "Suburban Mixed-Use", "{\"$type\":\"TimeOfDayOptions, \",\"LocationIdentifiers\":[],\"SelectedDates\":[],\"BinSizeMinutes\":15,\"DataSource\":0,\"AllDayPrimaryDirections\":[],\"AmPrimaryDirections\":[],\"PmPrimaryDirections\":[],\"AmEntryPctOfPeak\":0.55,\"AmExitPctOfPeak\":0.4,\"PmEntryPctOfPeak\":0.68,\"PmExitPctOfPeak\":0.38,\"FreeEntryPctOfDailyPeak\":0.22,\"FreeEntryPctOfDynamicRange\":0.18,\"EntrySustainedBins\":2,\"FreeSustainedBins\":4,\"FreeFallbackTime\":\"23:30\",\"MaxAmEndTime\":\"10:00\",\"MaxPmEndTime\":\"20:00\",\"LaneCapacityVehiclesPerHour\":800.0,\"ApproachVolumeAssumedLanes\":2.0,\"SplitReviewThresholdPercent\":35.0,\"ShoulderReviewThresholdPercent\":45.0,\"DirectionLaneCounts\":{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[System.Double, System.Private.CoreLib]], System.Private.CoreLib\"}}" },
                    { 4103, 41, null, null, "Retail / Commercial", "{\"$type\":\"TimeOfDayOptions, \",\"LocationIdentifiers\":[],\"SelectedDates\":[],\"BinSizeMinutes\":15,\"DataSource\":0,\"AllDayPrimaryDirections\":[],\"AmPrimaryDirections\":[],\"PmPrimaryDirections\":[],\"AmEntryPctOfPeak\":0.5,\"AmExitPctOfPeak\":0.38,\"PmEntryPctOfPeak\":0.62,\"PmExitPctOfPeak\":0.36,\"FreeEntryPctOfDailyPeak\":0.25,\"FreeEntryPctOfDynamicRange\":0.2,\"EntrySustainedBins\":3,\"FreeSustainedBins\":4,\"FreeFallbackTime\":\"23:30\",\"MaxAmEndTime\":\"10:00\",\"MaxPmEndTime\":\"20:00\",\"LaneCapacityVehiclesPerHour\":800.0,\"ApproachVolumeAssumedLanes\":2.0,\"SplitReviewThresholdPercent\":35.0,\"ShoulderReviewThresholdPercent\":45.0,\"DirectionLaneCounts\":{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[System.Double, System.Private.CoreLib]], System.Private.CoreLib\"}}" },
                    { 4104, 41, null, null, "Weekend / Recreation", "{\"$type\":\"TimeOfDayOptions, \",\"LocationIdentifiers\":[],\"SelectedDates\":[],\"BinSizeMinutes\":15,\"DataSource\":0,\"AllDayPrimaryDirections\":[],\"AmPrimaryDirections\":[],\"PmPrimaryDirections\":[],\"AmEntryPctOfPeak\":0.48,\"AmExitPctOfPeak\":0.36,\"PmEntryPctOfPeak\":0.6,\"PmExitPctOfPeak\":0.34,\"FreeEntryPctOfDailyPeak\":0.24,\"FreeEntryPctOfDynamicRange\":0.2,\"EntrySustainedBins\":3,\"FreeSustainedBins\":5,\"FreeFallbackTime\":\"23:30\",\"MaxAmEndTime\":\"10:00\",\"MaxPmEndTime\":\"20:00\",\"LaneCapacityVehiclesPerHour\":800.0,\"ApproachVolumeAssumedLanes\":2.0,\"SplitReviewThresholdPercent\":35.0,\"ShoulderReviewThresholdPercent\":45.0,\"DirectionLaneCounts\":{\"$type\":\"System.Collections.Generic.Dictionary`2[[System.String, System.Private.CoreLib],[System.Double, System.Private.CoreLib]], System.Private.CoreLib\"}}" }
                });

            migrationBuilder.InsertData(
                table: "MeasureOptions",
                columns: new[] { "Id", "MeasureTypeId", "Modified", "ModifiedBy", "Option", "Value" },
                values: new object[,]
                {
                    { 127, 41, null, null, "amEntryPctOfPeak", "0.55" },
                    { 128, 41, null, null, "amExitPctOfPeak", "0.40" },
                    { 129, 41, null, null, "pmEntryPctOfPeak", "0.68" },
                    { 130, 41, null, null, "pmExitPctOfPeak", "0.38" },
                    { 131, 41, null, null, "freeEntryPctOfDailyPeak", "0.22" },
                    { 132, 41, null, null, "freeEntryPctOfDynamicRange", "0.18" },
                    { 133, 41, null, null, "entrySustainedBins", "2" },
                    { 134, 41, null, null, "freeSustainedBins", "4" },
                    { 135, 41, null, null, "freeFallbackTime", "23:30" },
                    { 136, 41, null, null, "maxAmEndTime", "10:00" },
                    { 137, 41, null, null, "maxPmEndTime", "20:00" },
                    { 138, 41, null, null, "laneCapacityVehiclesPerHour", "800" },
                    { 139, 41, null, null, "approachVolumeAssumedLanes", "2" },
                    { 140, 41, null, null, "splitReviewThresholdPercent", "35" },
                    { 141, 41, null, null, "shoulderReviewThresholdPercent", "45" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MeasureOptionPresets",
                keyColumn: "Id",
                keyValue: 4101);

            migrationBuilder.DeleteData(
                table: "MeasureOptionPresets",
                keyColumn: "Id",
                keyValue: 4102);

            migrationBuilder.DeleteData(
                table: "MeasureOptionPresets",
                keyColumn: "Id",
                keyValue: 4103);

            migrationBuilder.DeleteData(
                table: "MeasureOptionPresets",
                keyColumn: "Id",
                keyValue: 4104);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 127);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 128);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 129);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 130);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 131);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 132);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 133);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 134);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 135);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 136);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 137);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 138);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 139);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 140);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 141);

            migrationBuilder.DeleteData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 41);
        }
    }
}
