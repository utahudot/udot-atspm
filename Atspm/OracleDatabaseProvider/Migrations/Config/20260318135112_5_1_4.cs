#region license
// Copyright 2026 Utah Departement of Transportation
// for OracleDatabaseProvider - Utah.Udot.ATSPM.OracleDatabaseProvider.Migrations/20260318135112_5_1_4.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Utah.Udot.ATSPM.OracleDatabaseProvider.Migrations
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
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ApiName = table.Column<string>(type: "VARCHAR2(32)", unicode: false, maxLength: 32, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    TraceId = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: true),
                    ConnectionId = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: true),
                    RemoteIp = table.Column<string>(type: "VARCHAR2(45)", unicode: false, maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "VARCHAR2(1024)", unicode: false, maxLength: 1024, nullable: true),
                    UserId = table.Column<string>(type: "VARCHAR2(200)", unicode: false, maxLength: 200, nullable: true),
                    Route = table.Column<string>(type: "VARCHAR2(2000)", unicode: false, maxLength: 2000, nullable: true),
                    QueryString = table.Column<string>(type: "VARCHAR2(2000)", unicode: false, maxLength: 2000, nullable: true),
                    Method = table.Column<string>(type: "VARCHAR2(20)", unicode: false, maxLength: 20, nullable: true),
                    StatusCode = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DurationMs = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Controller = table.Column<string>(type: "VARCHAR2(200)", unicode: false, maxLength: 200, nullable: true),
                    Action = table.Column<string>(type: "VARCHAR2(200)", unicode: false, maxLength: 200, nullable: true),
                    ResultCount = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    ResultSizeBytes = table.Column<long>(type: "NUMBER(19)", nullable: true),
                    Success = table.Column<bool>(type: "BOOLEAN", nullable: false),
                    ErrorMessage = table.Column<string>(type: "VARCHAR2(2000)", unicode: false, maxLength: 2000, nullable: true),
                    Created = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    Modified = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, nullable: true),
                    ModifiedBy = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, nullable: true)
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
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ParentId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Created = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    CreatedBy = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, nullable: true),
                    Date = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false, defaultValue: new DateTime(2025, 7, 3, 7, 2, 10, 247, DateTimeKind.Local).AddTicks(939)),
                    Modified = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, nullable: true),
                    Name = table.Column<string>(type: "VARCHAR2(64)", unicode: false, maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "VARCHAR2(512)", unicode: false, maxLength: 512, nullable: true),
                    Version = table.Column<string>(type: "VARCHAR2(64)", unicode: false, maxLength: 64, nullable: false)
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
