using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.PostgreSQLDatabaseProvider.Migrations.EventLog
{
    /// <inheritdoc />
    public partial class EFCore6Upgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ControllerLogArchives",
                columns: table => new
                {
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LogData = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControllerLogArchives", x => new { x.LocationIdentifier, x.ArchiveDate });
                },
                comment: "Compressed Event Log Data");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ControllerLogArchives");
        }
    }
}
