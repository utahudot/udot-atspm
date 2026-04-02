using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.OracleDatabaseProvider.Migrations
{
    /// <inheritdoc />
    public partial class _5_2_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "WatchDogLogEvents",
                type: "VARCHAR2(4000)",
                unicode: false,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "WatchDogIgnoreEvents",
                type: "VARCHAR2(4000)",
                unicode: false,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "WatchDogLogEvents");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "WatchDogIgnoreEvents");
        }
    }
}
