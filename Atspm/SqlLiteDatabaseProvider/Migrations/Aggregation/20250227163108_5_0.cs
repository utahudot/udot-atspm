#region license
// Copyright 2025 Utah Departement of Transportation
// for SqlLiteDatabaseProvider - Utah.Udot.ATSPM.SqlLiteDatabaseProvider.Migrations.Aggregation/20250227163108_5_0.cs
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

namespace Utah.Udot.ATSPM.SqlLiteDatabaseProvider.Migrations.Aggregation
{
    /// <inheritdoc />
    public partial class _5_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompressedAggregations",
                columns: table => new
                {
                    LocationIdentifier = table.Column<string>(type: "TEXT", unicode: false, maxLength: 10, nullable: false),
                    DataType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "Date", nullable: false),
                    Data = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompressedAggregations", x => new { x.LocationIdentifier, x.ArchiveDate, x.DataType });
                },
                comment: "Compressed aggregations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompressedAggregations");
        }
    }
}
