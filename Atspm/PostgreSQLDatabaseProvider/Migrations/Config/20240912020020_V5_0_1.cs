﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config
{
    /// <inheritdoc />
    public partial class V5_0_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2024, 9, 11, 20, 0, 18, 436, DateTimeKind.Local).AddTicks(213),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2024, 8, 14, 11, 58, 10, 324, DateTimeKind.Local).AddTicks(596));

            migrationBuilder.CreateTable(
                name: "WatchDogIgnoreEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationId = table.Column<int>(type: "integer", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "text", unicode: false, nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ComponentType = table.Column<int>(type: "integer", nullable: true),
                    ComponentId = table.Column<int>(type: "integer", nullable: true),
                    IssueType = table.Column<int>(type: "integer", nullable: false),
                    Phase = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchDogIgnoreEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchDogIgnoreEvents_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WatchDogIgnoreEvents_LocationId",
                table: "WatchDogIgnoreEvents",
                column: "LocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatchDogIgnoreEvents");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2024, 8, 14, 11, 58, 10, 324, DateTimeKind.Local).AddTicks(596),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2024, 9, 11, 20, 0, 18, 436, DateTimeKind.Local).AddTicks(213));
        }
    }
}
