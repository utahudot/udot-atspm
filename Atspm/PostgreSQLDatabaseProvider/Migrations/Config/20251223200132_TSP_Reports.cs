using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations.Config
{
    /// <inheritdoc />
    public partial class TSP_Reports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransitSignalPriorityNumber",
                table: "Approaches",
                type: "integer",
                nullable: true);

            migrationBuilder.InsertData(
                table: "DetectionTypes",
                columns: new[] { "Id", "Abbreviation", "Created", "CreatedBy", "Description", "DisplayOrder", "Modified", "ModifiedBy" },
                values: new object[] { 12, "PP", null, null, "Priority and Preemption", 12, null, null });

            migrationBuilder.InsertData(
                table: "MeasureType",
                columns: new[] { "Id", "Abbreviation", "Created", "CreatedBy", "DisplayOrder", "Modified", "ModifiedBy", "Name", "ShowOnAggregationSite", "ShowOnWebsite" },
                values: new object[,]
                {
                    { 39, "TSPS", null, null, 133, null, null, "Transit Signal Priority Summary", false, true },
                    { 40, "TSPD", null, null, 134, null, null, "Transit Signal Priority Details", false, true }
                });

            migrationBuilder.InsertData(
                table: "MeasureOptions",
                columns: new[] { "Id", "Created", "CreatedBy", "MeasureTypeId", "Modified", "ModifiedBy", "Option", "Value" },
                values: new object[,]
                {
                    { 121, null, null, 39, null, null, "binSize", "15" },
                    { 122, null, null, 40, null, null, "binSize", "15" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DetectionTypes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 121);

            migrationBuilder.DeleteData(
                table: "MeasureOptions",
                keyColumn: "Id",
                keyValue: 122);

            migrationBuilder.DeleteData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "MeasureType",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DropColumn(
                name: "TransitSignalPriorityNumber",
                table: "Approaches");
        }
    }
}
