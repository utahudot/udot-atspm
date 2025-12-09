#region license
// Copyright 2025 Utah Departement of Transportation
// for OracleDatabaseProvider - Utah.Udot.ATSPM.OracleDatabaseProvider.Migrations.EventLog/20251120164044_5_1_4.cs
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

namespace Utah.Udot.ATSPM.OracleDatabaseProvider.Migrations.EventLog
{
    /// <inheritdoc />
    public partial class _5_1_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompressedEvents",
                table: "CompressedEvents");

            migrationBuilder.DropColumn(
                name: "ArchiveDate",
                table: "CompressedEvents");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompressedEvents",
                table: "CompressedEvents",
                columns: new[] { "LocationIdentifier", "DeviceId", "DataType", "Start", "End" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompressedEvents",
                table: "CompressedEvents");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchiveDate",
                table: "CompressedEvents",
                type: "Date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompressedEvents",
                table: "CompressedEvents",
                columns: new[] { "LocationIdentifier", "DeviceId", "Start", "End" });
        }
    }
}
