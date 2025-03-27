using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.EventLog
{
    /// <inheritdoc />
    public partial class EventLogStartEnd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompressedEvents",
                table: "CompressedEvents");

            migrationBuilder.AddColumn<DateTime>(
                name: "Start",
                table: "CompressedEvents",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "End",
                table: "CompressedEvents",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompressedEvents",
                table: "CompressedEvents",
                columns: new[] { "LocationIdentifier", "DeviceId", "Start", "End" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompressedEvents",
                table: "CompressedEvents");

            migrationBuilder.DropColumn(
                name: "Start",
                table: "CompressedEvents");

            migrationBuilder.DropColumn(
                name: "End",
                table: "CompressedEvents");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompressedEvents",
                table: "CompressedEvents",
                columns: new[] { "LocationIdentifier", "DeviceId", "ArchiveDate" });
        }
    }
}
