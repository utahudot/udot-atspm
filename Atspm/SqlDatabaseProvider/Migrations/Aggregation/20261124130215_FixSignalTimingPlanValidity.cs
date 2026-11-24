#region license
// Copyright 2026 Utah Department of Transportation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore.Migrations;

namespace Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations.Aggregation
{
    /// <inheritdoc />
    public partial class FixSignalTimingPlanValidity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Valid",
                table: "SignalTimingPlans",
                type: "bit",
                nullable: false,
                computedColumnSql: "CAST(CASE WHEN [End] > [Start] THEN 1 ELSE 0 END AS bit)",
                oldClrType: typeof(bool),
                oldType: "bit",
                oldComputedColumnSql: "[End] > [Start]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Keep the working computed expression because the original predicate is invalid on SQL Server.
        }
    }
}
