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

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.Speed_Events",
                table: "Speed_Events",
                columns: new[] { "DetectorID", "Mph", "Kph", "Timestamp" });
        }
    }
}
