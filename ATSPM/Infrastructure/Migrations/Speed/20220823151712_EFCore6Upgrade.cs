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

            migrationBuilder.RenameColumn(
                name: "timestamp",
                table: "SpeedEvents",
                newName: "Timestamp");

            migrationBuilder.RenameColumn(
                name: "KPH",
                table: "SpeedEvents",
                newName: "Kph");

            migrationBuilder.RenameColumn(
                name: "MPH",
                table: "SpeedEvents",
                newName: "Mph");

            migrationBuilder.RenameColumn(
                name: "DetectorID",
                table: "SpeedEvents",
                newName: "DetectorId");

            migrationBuilder.AlterTable(
                name: "SpeedEvents",
                comment: "Speed Event Data");

            migrationBuilder.AlterColumn<string>(
                name: "DetectorId",
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
                columns: new[] { "DetectorId", "Mph", "Kph", "Timestamp" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SpeedEvents",
                table: "SpeedEvents");

            migrationBuilder.RenameTable(
                name: "SpeedEvents",
                newName: "Speed_Events");

            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Speed_Events",
                newName: "timestamp");

            migrationBuilder.RenameColumn(
                name: "Kph",
                table: "Speed_Events",
                newName: "KPH");

            migrationBuilder.RenameColumn(
                name: "Mph",
                table: "Speed_Events",
                newName: "MPH");

            migrationBuilder.RenameColumn(
                name: "DetectorId",
                table: "Speed_Events",
                newName: "DetectorID");

            migrationBuilder.AlterTable(
                name: "Speed_Events",
                oldComment: "Speed Event Data");

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
                columns: new[] { "DetectorID", "MPH", "KPH", "timestamp" });
        }
    }
}
