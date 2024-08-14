using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Aggregation
{
    /// <inheritdoc />
    public partial class _5_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompressedAggregations",
                columns: table => new
                {
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    DataType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "Date", nullable: false),
                    Data = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompressedAggregations", x => new { x.LocationIdentifier, x.ArchiveDate, x.DataType });
                },
                comment: "Compressed aggregations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompressedAggregations");
        }
    }
}
