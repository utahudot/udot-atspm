using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.Migrations.Speed
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Speed_Events",
                columns: table => new
                {
                    DetectorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MPH = table.Column<int>(type: "int", nullable: false),
                    KPH = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Speed_Events", x => new { x.DetectorID, x.MPH, x.KPH, x.timestamp });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Speed_Events");
        }
    }
}
