using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.Migrations.EventLog
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ControllerLogArchives",
                columns: table => new
                {
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "date", nullable: false),
                    LogData = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControllerLogArchives", x => new { x.SignalId, x.ArchiveDate });
                },
                comment: "Compressed Log Data");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControllerLogArchives");
        }
    }
}
