using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config
{
    /// <inheritdoc />
    public partial class V5_0_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2024, 12, 4, 14, 42, 43, 505, DateTimeKind.Local).AddTicks(1184),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2024, 9, 11, 20, 0, 18, 436, DateTimeKind.Local).AddTicks(213));

            migrationBuilder.CreateTable(
                name: "MapLayer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", unicode: false, nullable: true),
                    ServiceType = table.Column<string>(type: "text", unicode: false, nullable: true),
                    ShowByDefault = table.Column<bool>(type: "boolean", nullable: false),
                    MapLayerUrl = table.Column<string>(type: "text", unicode: false, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", unicode: false, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "timestamp", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", unicode: false, nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapLayer", x => x.Id);
                },
                comment: "Map Layer");

            migrationBuilder.CreateIndex(
                name: "IX_MapLayer_Name",
                table: "MapLayer",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapLayer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "VersionHistory",
                type: "timestamp",
                nullable: false,
                defaultValue: new DateTime(2024, 9, 11, 20, 0, 18, 436, DateTimeKind.Local).AddTicks(213),
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldDefaultValue: new DateTime(2024, 12, 4, 14, 42, 43, 505, DateTimeKind.Local).AddTicks(1184));
        }
    }
}
