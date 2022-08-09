using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrasturcture.Migrations.Speed
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
                    Mph = table.Column<int>(type: "int", nullable: false),
                    Kph = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Speed_Events", x => new { x.DetectorID, x.Mph, x.Kph, x.Timestamp });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Speed_Events");
        }
    }
}
