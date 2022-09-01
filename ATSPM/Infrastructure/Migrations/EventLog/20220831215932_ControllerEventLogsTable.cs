using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrasturcture.Migrations.EventLog
{
    public partial class ControllerEventLogsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterTable(
            //    name: "ControllerLogArchives",
            //    comment: "Compressed Event Log Data",
            //    oldComment: "Compressed Log Data");

            //migrationBuilder.AlterColumn<DateTime>(
            //    name: "ArchiveDate",
            //    table: "ControllerLogArchives",
            //    type: "datetime",
            //    nullable: false,
            //    oldClrType: typeof(DateTime),
            //    oldType: "date");

            //migrationBuilder.AlterColumn<string>(
            //    name: "SignalId",
            //    table: "ControllerLogArchives",
            //    type: "varchar(10)",
            //    unicode: false,
            //    maxLength: 10,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(10)",
            //    oldMaxLength: 10);

            migrationBuilder.CreateTable(
                name: "Controller_Event_Log",
                columns: table => new
                {
                    SignalID = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    EventCode = table.Column<int>(type: "int", nullable: false),
                    EventParam = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Controller_Event_Log", x => new { x.SignalID, x.Timestamp, x.EventCode, x.EventParam });
                },
                comment: "Old Log Data Table");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Controller_Event_Log");

            migrationBuilder.AlterTable(
                name: "ControllerLogArchives",
                comment: "Compressed Log Data",
                oldComment: "Compressed Event Log Data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ArchiveDate",
                table: "ControllerLogArchives",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "ControllerLogArchives",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);
        }
    }
}
