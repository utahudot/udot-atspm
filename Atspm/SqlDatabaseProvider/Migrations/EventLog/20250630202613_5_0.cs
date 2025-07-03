#region license
// Copyright 2025 Utah Departement of Transportation
// for SqlDatabaseProvider - Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations.EventLog/20250630202613_5_0.cs
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

namespace Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations.EventLog
{
    /// <inheritdoc />
    public partial class _5_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompressedEvents",
                columns: table => new
                {
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "Date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompressedEvents", x => new { x.LocationIdentifier, x.DeviceId, x.Start, x.End });
                },
                comment: "Compressed device data log events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompressedEvents");
        }
    }
}
