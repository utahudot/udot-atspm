using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrasturcture.Migrations.Speed
{
    public partial class EFCore6Upgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.Speed_Events",
                table: "Speed_Events");

            migrationBuilder.RenameTable(
                name: "Speed_Events",
                newName: "SpeedEvents");

            migrationBuilder.AlterTable(
                name: "SpeedEvents",
                comment: "Speed Event Data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "SpeedEvents",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "DetectorID",
                table: "SpeedEvents",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SpeedEvents",
                table: "SpeedEvents",
                columns: new[] { "DetectorID", "Mph", "Kph", "Timestamp" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SpeedEvents",
                table: "SpeedEvents");

            migrationBuilder.RenameTable(
                name: "SpeedEvents",
                newName: "Speed_Events");

            migrationBuilder.AlterTable(
                name: "Speed_Events",
                oldComment: "Speed Event Data");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Timestamp",
                table: "Speed_Events",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<string>(
                name: "DetectorID",
                table: "Speed_Events",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.Speed_Events",
                table: "Speed_Events",
                columns: new[] { "DetectorID", "Mph", "Kph", "Timestamp" });
        }
    }
}
