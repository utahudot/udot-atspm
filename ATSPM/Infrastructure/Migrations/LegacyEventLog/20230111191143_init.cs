using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.Migrations.LegacyEventLog
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Controller_Event_Log",
                columns: table => new
                {
                    SignalID = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventCode = table.Column<int>(type: "int", nullable: false),
                    EventParam = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Controller_Event_Log", x => new { x.SignalID, x.Timestamp, x.EventCode, x.EventParam });
                },
                comment: "Old Log Data Table");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Controller_Event_Log");
        }
    }
}
