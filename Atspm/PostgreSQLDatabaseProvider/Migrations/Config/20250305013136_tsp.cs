using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config
{
    /// <inheritdoc />
    public partial class tsp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeasureOptionsSave");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2025, 3, 4, 18, 31, 33, 965, DateTimeKind.Local).AddTicks(7341),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2025, 3, 2, 11, 15, 19, 354, DateTimeKind.Local).AddTicks(5945));

            migrationBuilder.CreateTable(
                name: "MeasureOptionPresets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    Option = table.Column<string>(type: "text", nullable: true),
                    MeasureTypeId = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", unicode: false, nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureOptionPresets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeasureOptionPresets_MeasureType_MeasureTypeId",
                        column: x => x.MeasureTypeId,
                        principalTable: "MeasureType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Measure Option Presets");

            migrationBuilder.CreateIndex(
                name: "IX_MeasureOptionPresets_MeasureTypeId",
                table: "MeasureOptionPresets",
                column: "MeasureTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeasureOptionPresets");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2025, 3, 2, 11, 15, 19, 354, DateTimeKind.Local).AddTicks(5945),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2025, 3, 4, 18, 31, 33, 965, DateTimeKind.Local).AddTicks(7341));

            migrationBuilder.CreateTable(
                name: "MeasureOptionsSave",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MeasureTypeId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", unicode: false, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ModifiedByUserId = table.Column<string>(type: "text", unicode: false, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Name = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    SelectedParametersJson = table.Column<JsonDocument>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureOptionsSave", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeasureOptionsSave_MeasureType_MeasureTypeId",
                        column: x => x.MeasureTypeId,
                        principalTable: "MeasureType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Measure Options Save");

            migrationBuilder.CreateIndex(
                name: "IX_MeasureOptionsSave_MeasureTypeId",
                table: "MeasureOptionsSave",
                column: "MeasureTypeId");
        }
    }
}
