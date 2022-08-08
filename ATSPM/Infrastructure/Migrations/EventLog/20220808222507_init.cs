using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrasturcture.Migrations.EventLog
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ControllerLogArchives",
                columns: table => new
                {
                    SignalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "date", nullable: false),
                    LogData = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Controller_Log_Archive", x => new { x.SignalID, x.ArchiveDate });
                },
                comment: "Compressed Log Data");

            migrationBuilder.CreateIndex(
                name: "IX_Controller_Log_Archive",
                table: "ControllerLogArchives",
                columns: new[] { "SignalID", "ArchiveDate" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControllerLogArchives");
        }
    }
}
