﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.SqlDatabaseProvider.Migrations.EventLog
{
    /// <inheritdoc />
    public partial class EFCore6Upgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompressedData",
                columns: table => new
                {
                    LocationIdentifier = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "Date", nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompressedData", x => new { x.LocationIdentifier, x.DeviceId, x.ArchiveDate });
                });

            migrationBuilder.CreateTable(
                name: "ControllerLogArchives",
                columns: table => new
                {
                    LocationIdentifier = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogData = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControllerLogArchives", x => new { x.LocationIdentifier, x.ArchiveDate });
                },
                comment: "Compressed Event Log Data");

            migrationBuilder.CreateTable(
                name: "EventLogArchives",
                columns: table => new
                {
                    LocationIdentifier = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "Date", nullable: false),
                    LogData = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogArchives", x => new { x.LocationIdentifier, x.DeviceId, x.ArchiveDate });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompressedData");

            migrationBuilder.DropTable(
                name: "ControllerLogArchives");

            migrationBuilder.DropTable(
                name: "EventLogArchives");
        }
    }
}