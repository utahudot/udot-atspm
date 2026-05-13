using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.SqlDatabaseProvider.Migrations.Aggregation
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
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    PlanNumber = table.Column<short>(type: "smallint", nullable: false),
                    Valid = table.Column<bool>(type: "bit", nullable: false, computedColumnSql: "[End] > [Start]"),
                    End = table.Column<DateTime>(type: "datetime2", nullable: false)
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
