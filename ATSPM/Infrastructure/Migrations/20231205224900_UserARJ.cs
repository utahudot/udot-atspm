using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.Migrations.Config
{
    /// <inheritdoc />
    public partial class UserARJ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserArea",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", unicode: false, nullable: false),
                    AreaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserArea", x => new { x.UserId, x.AreaId });
                    table.ForeignKey(
                        name: "FK_UserArea_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserJurisdiction",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", unicode: false, nullable: false),
                    JurisdictionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserJurisdiction", x => new { x.UserId, x.JurisdictionId });
                    table.ForeignKey(
                        name: "FK_UserJurisdiction_Jurisdictions_JurisdictionId",
                        column: x => x.JurisdictionId,
                        principalTable: "Jurisdictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRegion",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", unicode: false, nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRegion", x => new { x.UserId, x.RegionId });
                    table.ForeignKey(
                        name: "FK_UserRegion_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserArea_AreaId",
                table: "UserArea",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_UserJurisdiction_JurisdictionId",
                table: "UserJurisdiction",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRegion_RegionId",
                table: "UserRegion",
                column: "RegionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserArea");

            migrationBuilder.DropTable(
                name: "UserJurisdiction");

            migrationBuilder.DropTable(
                name: "UserRegion");
        }
    }
}
