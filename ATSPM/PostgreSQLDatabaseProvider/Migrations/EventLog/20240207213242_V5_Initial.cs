using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.PostgreSQLDatabaseProvider.Migrations.EventLog
{
    /// <inheritdoc />
    public partial class V5_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompressedEvents",
                columns: table => new
                {
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "Date", nullable: false),
                    DeviceId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: true),
                    DataType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompressedEvents", x => new { x.LocationIdentifier, x.DeviceId, x.ArchiveDate });
                },
                comment: "Compressed device data log events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompressedEvents");
        }
    }
}
