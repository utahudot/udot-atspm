using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Aggregation
{
    /// <inheritdoc />
    public partial class _5_1_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompressedAggregations",
                table: "CompressedAggregations");

            migrationBuilder.DropColumn(
                name: "ArchiveDate",
                table: "CompressedAggregations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompressedAggregations",
                table: "CompressedAggregations",
                columns: new[] { "LocationIdentifier", "DataType", "Start", "End" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CompressedAggregations",
                table: "CompressedAggregations");

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchiveDate",
                table: "CompressedAggregations",
                type: "Date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CompressedAggregations",
                table: "CompressedAggregations",
                columns: new[] { "LocationIdentifier", "ArchiveDate", "DataType", "Start", "End" });
        }
    }
}
