#region license
// Copyright 2026 Utah Departement of Transportation
// for PostgreSQLDatabaseProvider - Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations/20260727183000_AddTimeOfDayMeasureDefaults.cs
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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Utah.Udot.Atspm.Data;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ConfigContext))]
    [Migration("20260727183000_AddTimeOfDayMeasureDefaults")]
    public partial class AddTimeOfDayMeasureDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MeasureType",
                columns: new[] { "Id", "Abbreviation", "DisplayOrder", "Modified", "ModifiedBy", "Name", "ShowOnAggregationSite", "ShowOnWebsite" },
                columnTypes: new[] { "integer", "character varying(8)", "integer", "timestamp with time zone", "text", "character varying(50)", "boolean", "boolean" },
                values: new object[] { 41, "TOD", 135, null, null, "Time Of Day", false, true });

            migrationBuilder.InsertData(
                table: "MeasureOptions",
                columns: new[] { "Id", "MeasureTypeId", "Modified", "ModifiedBy", "Option", "Value" },
                columnTypes: new[] { "integer", "integer", "timestamp with time zone", "text", "character varying(128)", "character varying(512)" },
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

            migrationBuilder.InsertData(
                table: "MeasureOptionPresets",
                columns: new[] { "Id", "MeasureTypeId", "Modified", "ModifiedBy", "Name", "Option" },
                columnTypes: new[] { "integer", "integer", "timestamp with time zone", "text", "character varying(512)", "text" },
                values: new object[,]
                {
                    { 4101, 41, null, null, "Commuter Arterial", BuildPresetJson("0.60", "0.42", "0.72", "0.40", "0.20", "0.16", 2, 4) },
                    { 4102, 41, null, null, "Suburban Mixed-Use", BuildPresetJson("0.55", "0.40", "0.68", "0.38", "0.22", "0.18", 2, 4) },
                    { 4103, 41, null, null, "Retail / Commercial", BuildPresetJson("0.50", "0.38", "0.62", "0.36", "0.25", "0.20", 3, 4) },
                    { 4104, 41, null, null, "Weekend / Recreation", BuildPresetJson("0.48", "0.36", "0.60", "0.34", "0.24", "0.20", 3, 5) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MeasureOptionPresets",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValues: new object[] { 4101, 4102, 4103, 4104 });

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValues: new object[] { 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141 });

            migrationBuilder.DeleteData(
                table: "MeasureType",
                keyColumn: "Id",
                keyColumnType: "integer",
                keyValue: 41);
        }

        private static string BuildPresetJson(
            string amEntryPctOfPeak,
            string amExitPctOfPeak,
            string pmEntryPctOfPeak,
            string pmExitPctOfPeak,
            string freeEntryPctOfDailyPeak,
            string freeEntryPctOfDynamicRange,
            int entrySustainedBins,
            int freeSustainedBins)
        {
            return "{" +
                "\"$type\":\"TimeOfDayOptions\"," +
                "\"LocationIdentifiers\":[]," +
                "\"SelectedDates\":[]," +
                "\"BinSizeMinutes\":15," +
                "\"DataSource\":0," +
                "\"AllDayPrimaryDirections\":[]," +
                "\"AmPrimaryDirections\":[]," +
                "\"PmPrimaryDirections\":[]," +
                "\"AmEntryPctOfPeak\":" + amEntryPctOfPeak + "," +
                "\"AmExitPctOfPeak\":" + amExitPctOfPeak + "," +
                "\"PmEntryPctOfPeak\":" + pmEntryPctOfPeak + "," +
                "\"PmExitPctOfPeak\":" + pmExitPctOfPeak + "," +
                "\"FreeEntryPctOfDailyPeak\":" + freeEntryPctOfDailyPeak + "," +
                "\"FreeEntryPctOfDynamicRange\":" + freeEntryPctOfDynamicRange + "," +
                "\"EntrySustainedBins\":" + entrySustainedBins + "," +
                "\"FreeSustainedBins\":" + freeSustainedBins + "," +
                "\"FreeFallbackTime\":\"23:30\"," +
                "\"MaxAmEndTime\":\"10:00\"," +
                "\"MaxPmEndTime\":\"20:00\"," +
                "\"LaneCapacityVehiclesPerHour\":800.0," +
                "\"ApproachVolumeAssumedLanes\":2.0," +
                "\"SplitReviewThresholdPercent\":35.0," +
                "\"ShoulderReviewThresholdPercent\":45.0," +
                "\"DirectionLaneCounts\":{}" +
                "}";
        }
    }
}
