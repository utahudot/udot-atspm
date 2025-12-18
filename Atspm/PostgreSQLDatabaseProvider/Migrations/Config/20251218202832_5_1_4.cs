using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Utah.Udot.ATSPM.PostgreSQLDatabaseProvider.Migrations
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApiName = table.Column<string>(type: "character varying(32)", unicode: false, maxLength: 32, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp", nullable: false),
                    TraceId = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    ConnectionId = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    RemoteIp = table.Column<string>(type: "character varying(45)", unicode: false, maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(1024)", unicode: false, maxLength: 1024, nullable: true),
                    UserId = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: true),
                    Route = table.Column<string>(type: "character varying(2000)", unicode: false, maxLength: 2000, nullable: true),
                    QueryString = table.Column<string>(type: "character varying(2000)", unicode: false, maxLength: 2000, nullable: true),
                    Method = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: true),
                    StatusCode = table.Column<int>(type: "integer", nullable: false),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Controller = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: true),
                    Action = table.Column<string>(type: "character varying(200)", unicode: false, maxLength: 200, nullable: true),
                    ResultCount = table.Column<int>(type: "integer", nullable: true),
                    ResultSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", unicode: false, maxLength: 2000, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Modified = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", unicode: false, nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", unicode: false, nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", unicode: false, nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp", nullable: false, defaultValue: new DateTime(2025, 7, 3, 6, 55, 1, 778, DateTimeKind.Local).AddTicks(1158)),
                    Modified = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", unicode: false, nullable: true),
                    Name = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    Version = table.Column<string>(type: "character varying(64)", unicode: false, maxLength: 64, nullable: false)
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
