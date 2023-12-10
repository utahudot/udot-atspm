using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ATSPM.Infrastructure.Migrations.Configuration
{
    /// <inheritdoc />
    public partial class Watchdog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WatchDogLogEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SignalId = table.Column<int>(type: "integer", nullable: false),
                    SignalIdentifier = table.Column<string>(type: "text", unicode: false, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp", nullable: false),
                    ComponentType = table.Column<int>(type: "integer", nullable: false),
                    ComponentId = table.Column<int>(type: "integer", nullable: false),
                    IssueType = table.Column<int>(type: "integer", nullable: false),
                    Details = table.Column<string>(type: "text", unicode: false, nullable: false),
                    Phase = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchDogLogEvents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WatchDogLogEvents");
        }
    }
}
