using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config
{
    /// <inheritdoc />
    public partial class transitsignalpriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2025, 3, 2, 11, 15, 19, 354, DateTimeKind.Local).AddTicks(5945),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2025, 2, 10, 10, 24, 34, 63, DateTimeKind.Local).AddTicks(6320));

            migrationBuilder.CreateTable(
                name: "MeasureOptionsSave",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "text", unicode: false, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ModifiedByUserId = table.Column<string>(type: "text", unicode: false, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp", nullable: false),
                    SelectedParametersJson = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    MeasureTypeId = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.InsertData(
                table: "MeasureOptions",
                columns: new[] { "Id", "MeasureTypeId", "Option", "Value" },
                values: new object[,]
                {
                    { 116, 37, "combineLanes", "FALSE" },
                    { 117, 2, "yAxisDefault", "100" },
                    { 118, 6, "yAxisDefault", "150" },
                    { 119, 3, "yAxisDefault", "180" },
                    { 120, 11, "yAxisDefault", "20" }
                });

            migrationBuilder.InsertData(
                table: "MeasureType",
                columns: new[] { "Id", "Abbreviation", "DisplayOrder", "Name", "ShowOnAggregationSite", "ShowOnWebsite" },
                values: new object[] { 38, "TSP", 132, "Transit Signal Priority", false, false });

            migrationBuilder.CreateIndex(
                name: "IX_MeasureOptionsSave_MeasureTypeId",
                table: "MeasureOptionsSave",
                column: "MeasureTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeasureOptionsSave");

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 116);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 117);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 118);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 119);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 120);

            migrationBuilder.DeleteData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2025, 2, 10, 10, 24, 34, 63, DateTimeKind.Local).AddTicks(6320),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2025, 3, 2, 11, 15, 19, 354, DateTimeKind.Local).AddTicks(5945));
        }
    }
}
