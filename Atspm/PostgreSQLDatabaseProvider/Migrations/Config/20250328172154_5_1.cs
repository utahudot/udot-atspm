using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config
{
    /// <inheritdoc />
    public partial class _5_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2025, 3, 28, 11, 21, 54, 311, DateTimeKind.Local).AddTicks(2615),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2025, 2, 27, 9, 24, 49, 960, DateTimeKind.Local).AddTicks(1714));

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
                defaultValue: new DateTime(2025, 2, 27, 9, 24, 49, 960, DateTimeKind.Local).AddTicks(1714),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2025, 3, 28, 11, 21, 54, 311, DateTimeKind.Local).AddTicks(2615));
        }
    }
}
