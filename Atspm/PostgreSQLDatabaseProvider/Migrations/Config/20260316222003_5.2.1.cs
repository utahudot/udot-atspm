using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config
{
    /// <inheritdoc />
    public partial class _521 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "WatchDogLogEvents");

            migrationBuilder.DropColumn(
                name: "Key",
                table: "WatchDogIgnoreEvents");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "WatchDogLogEvents",
                type: "text",
                unicode: false,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "WatchDogIgnoreEvents",
                type: "text",
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
