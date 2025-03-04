using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.OracleDatabaseProvider.Migrations.EventLog
{
    /// <inheritdoc />
    public partial class _5_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompressedEvents",
                columns: table => new
                {
                    LocationIdentifier = table.Column<string>(type: "VARCHAR2(10)", unicode: false, maxLength: 10, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "Date", nullable: false),
                    DeviceId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Data = table.Column<byte[]>(type: "RAW(2000)", nullable: true),
                    DataType = table.Column<string>(type: "NVARCHAR2(32)", maxLength: 32, nullable: false)
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
