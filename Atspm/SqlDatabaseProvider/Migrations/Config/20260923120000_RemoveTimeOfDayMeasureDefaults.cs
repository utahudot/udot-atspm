#region license
// Copyright 2026 Utah Departement of Transportation
// for SqlDatabaseProvider - Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations/20260923120000_RemoveTimeOfDayMeasureDefaults.cs
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

namespace Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ConfigContext))]
    [Migration("20260923120000_RemoveTimeOfDayMeasureDefaults")]
    public partial class RemoveTimeOfDayMeasureDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyColumnType: "int",
                keyValues: new object[] { 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MeasureOptions",
                columns: new[] { "Id", "MeasureTypeId", "Modified", "ModifiedBy", "Option", "Value" },
                columnTypes: new[] { "int", "int", "datetimeoffset", "varchar(max)", "varchar(128)", "varchar(512)" },
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
    }
}
