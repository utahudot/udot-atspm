using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.SqlLiteDatabaseProvider.Migrations
{
    /// <inheritdoc />
    public partial class _5_1_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VersionHistory");

            migrationBuilder.CreateTable(
                name: "UsageEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApiName = table.Column<string>(type: "TEXT", unicode: false, maxLength: 32, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TraceId = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    ConnectionId = table.Column<string>(type: "TEXT", unicode: false, maxLength: 100, nullable: true),
                    RemoteIp = table.Column<string>(type: "TEXT", unicode: false, maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", unicode: false, maxLength: 1024, nullable: true),
                    UserId = table.Column<string>(type: "TEXT", unicode: false, maxLength: 200, nullable: true),
                    Route = table.Column<string>(type: "TEXT", unicode: false, maxLength: 2000, nullable: true),
                    QueryString = table.Column<string>(type: "TEXT", unicode: false, maxLength: 2000, nullable: true),
                    Method = table.Column<string>(type: "TEXT", unicode: false, maxLength: 20, nullable: true),
                    StatusCode = table.Column<int>(type: "INTEGER", nullable: false),
                    DurationMs = table.Column<long>(type: "INTEGER", nullable: false),
                    Controller = table.Column<string>(type: "TEXT", unicode: false, maxLength: 200, nullable: true),
                    Action = table.Column<string>(type: "TEXT", unicode: false, maxLength: 200, nullable: true),
                    ResultCount = table.Column<int>(type: "INTEGER", nullable: true),
                    ResultSizeBytes = table.Column<long>(type: "INTEGER", nullable: true),
                    Success = table.Column<bool>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", unicode: false, maxLength: 2000, nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", unicode: false, nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsageEntries_Route",
                table: "UsageEntries",
                column: "Route");

            migrationBuilder.CreateIndex(
                name: "IX_UsageEntries_StatusCode",
                table: "UsageEntries",
                column: "StatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_UsageEntries_Timestamp",
                table: "UsageEntries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_UsageEntries_UserId",
                table: "UsageEntries",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsageEntries");

            migrationBuilder.CreateTable(
                name: "VersionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ParentId = table.Column<int>(type: "INTEGER", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", unicode: false, nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValue: new DateTime(2025, 7, 3, 7, 0, 43, 975, DateTimeKind.Local).AddTicks(6286)),
                    Modified = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", unicode: false, nullable: true),
                    Name = table.Column<string>(type: "TEXT", unicode: false, maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", unicode: false, maxLength: 512, nullable: true),
                    Version = table.Column<string>(type: "TEXT", unicode: false, maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VersionHistory_VersionHistory_ParentId",
                        column: x => x.ParentId,
                        principalTable: "VersionHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Version History");

            migrationBuilder.CreateIndex(
                name: "IX_VersionHistory_ParentId",
                table: "VersionHistory",
                column: "ParentId");
        }
    }
}
