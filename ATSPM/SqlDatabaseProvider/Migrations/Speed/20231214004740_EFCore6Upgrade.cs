#region license
// Copyright 2024 Utah Departement of Transportation
// for SqlDatabaseProvider - ATSPM.Infrastructure.SqlDatabaseProvider.Migrations/20231214004740_EFCore6Upgrade.cs
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
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.SqlDatabaseProvider.Migrations
{
    /// <inheritdoc />
    public partial class EFCore6Upgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SpeedEvents",
                columns: table => new
                {
                    DetectorId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Mph = table.Column<int>(type: "int", nullable: false),
                    Kph = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpeedEvents", x => new { x.DetectorId, x.Mph, x.Kph, x.TimeStamp });
                },
                comment: "Speed Event Data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpeedEvents");
        }
    }
}
