#region license
// Copyright 2025 Utah Departement of Transportation
// for SqlDatabaseProvider - Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations.Aggregation/20250703130822_5_1.cs
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

namespace Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations.Aggregation
{
    /// <inheritdoc />
    public partial class _5_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompressedAggregations",
                table: "CompressedAggregations");

            migrationBuilder.AddColumn<DateTime>(
                name: "Start",
                table: "CompressedAggregations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "End",
                table: "CompressedAggregations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompressedAggregations",
                table: "CompressedAggregations",
                columns: new[] { "LocationIdentifier", "ArchiveDate", "DataType", "Start", "End" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompressedAggregations",
                table: "CompressedAggregations");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "CompressedAggregations");

            migrationBuilder.DropColumn(
                name: "End",
                table: "CompressedAggregations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompressedAggregations",
                table: "CompressedAggregations",
                columns: new[] { "LocationIdentifier", "ArchiveDate", "DataType" });
        }
    }
}
