#region license
// Copyright 2025 Utah Departement of Transportation
// for PostgreSQLDatabaseProvider - Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config/20241220192813_V5_0_2.cs
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

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config
{
    /// <inheritdoc />
    public partial class V5_0_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 20, 12, 28, 12, 764, DateTimeKind.Local).AddTicks(2478),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2024, 9, 11, 20, 0, 18, 436, DateTimeKind.Local).AddTicks(213));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2024, 9, 11, 20, 0, 18, 436, DateTimeKind.Local).AddTicks(213),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2024, 12, 20, 12, 28, 12, 764, DateTimeKind.Local).AddTicks(2478));
        }
    }
}
