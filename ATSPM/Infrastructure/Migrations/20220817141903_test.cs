using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrasturcture.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ControllerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Snmpport = table.Column<long>(type: "bigint", nullable: false),
                    Ftpdirectory = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ActiveFtp = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControllerTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jurisdictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Mpo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CountyParish = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    OtherPartners = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jurisdictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VersionActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Signals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SignalId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Latitude = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Longitude = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    PrimaryName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false, defaultValueSql: "('')"),
                    SecondaryName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Ipaddress = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValueSql: "('127.0.0.1')"),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    ControllerTypeId = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    VersionActionId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((10))"),
                    Note = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false, defaultValueSql: "('Initial')"),
                    Start = table.Column<DateTime>(type: "datetime", nullable: false),
                    JurisdictionId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((1))"),
                    Pedsare1to1 = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signals_ControllerTypes_ControllerTypeId",
                        column: x => x.ControllerTypeId,
                        principalTable: "ControllerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signals_Jurisdictions_JurisdictionId",
                        column: x => x.JurisdictionId,
                        principalTable: "Jurisdictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signals_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signals_VersionActions_VersionActionId",
                        column: x => x.VersionActionId,
                        principalTable: "VersionActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AreaSignal",
                columns: table => new
                {
                    AreasId = table.Column<int>(type: "int", nullable: false),
                    SignalsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaSignal", x => new { x.AreasId, x.SignalsId });
                    table.ForeignKey(
                        name: "FK_AreaSignal_Areas_AreasId",
                        column: x => x.AreasId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AreaSignal_Signals_SignalsId",
                        column: x => x.SignalsId,
                        principalTable: "Signals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Unknown" });

            migrationBuilder.InsertData(
                table: "ControllerTypes",
                columns: new[] { "Id", "ActiveFtp", "Description", "Ftpdirectory", "Password", "Snmpport", "UserName" },
                values: new object[] { 0, false, "Unknown", "root", "password", 0L, "user" });

            migrationBuilder.InsertData(
                table: "Jurisdictions",
                columns: new[] { "Id", "CountyParish", "Mpo", "Name", "OtherPartners" },
                values: new object[] { 1, "Unknown", null, "Unknown", "Unknown" });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "Description" },
                values: new object[] { 1, "Unknown" });

            migrationBuilder.InsertData(
                table: "VersionActions",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { 0, "Unknown" },
                    { 1, "New" },
                    { 2, "Edit" },
                    { 3, "Delete" },
                    { 4, "NewVersion" },
                    { 10, "Initial" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AreaSignal_SignalsId",
                table: "AreaSignal",
                column: "SignalsId");

            migrationBuilder.CreateIndex(
                name: "IX_Signals_ControllerTypeId",
                table: "Signals",
                column: "ControllerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Signals_JurisdictionId",
                table: "Signals",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_Signals_RegionId",
                table: "Signals",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Signals_VersionActionId",
                table: "Signals",
                column: "VersionActionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreaSignal");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "Signals");

            migrationBuilder.DropTable(
                name: "ControllerTypes");

            migrationBuilder.DropTable(
                name: "Jurisdictions");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "VersionActions");
        }
    }
}
