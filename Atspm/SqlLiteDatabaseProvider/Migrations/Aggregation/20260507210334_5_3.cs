using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.SqlLiteDatabaseProvider.Migrations.Aggregation
{
    /// <inheritdoc />
    public partial class _5_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SignalTimingPlans",
                columns: table => new
                {
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "TEXT", unicode: false, maxLength: 10, nullable: false),
                    PlanNumber = table.Column<short>(type: "INTEGER", nullable: false),
                    Valid = table.Column<bool>(type: "INTEGER", nullable: false, computedColumnSql: "\"End\" > \"Start\""),
                    End = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalTimingPlans", x => new { x.LocationIdentifier, x.PlanNumber, x.Start });
                },
                comment: "Signal Timing Plans");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SignalTimingPlans");
        }
    }
}
