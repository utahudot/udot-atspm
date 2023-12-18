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
