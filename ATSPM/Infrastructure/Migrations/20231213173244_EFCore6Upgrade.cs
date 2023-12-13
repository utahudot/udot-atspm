using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ATSPM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EFCore6Upgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                },
                comment: "Areas");

            migrationBuilder.CreateTable(
                name: "ControllerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Product = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Firmware = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: true),
                    Protocol = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Unknown"),
                    Port = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "((0))"),
                    Directory = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: true),
                    SearchTerm = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControllerTypes", x => x.Id);
                },
                comment: "Location Controller Types");

            migrationBuilder.CreateTable(
                name: "DetectionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    Abbreviation = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionTypes", x => x.Id);
                },
                comment: "Detector Types");

            migrationBuilder.CreateTable(
                name: "DirectionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    Abbreviation = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectionTypes", x => x.Id);
                },
                comment: "Direction Types");

            migrationBuilder.CreateTable(
                name: "ExternalLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Url = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLinks", x => x.Id);
                },
                comment: "External Links");

            migrationBuilder.CreateTable(
                name: "Faqs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Header = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    Body = table.Column<string>(type: "varchar(8000)", unicode: false, maxLength: 8000, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faqs", x => x.Id);
                },
                comment: "Frequently Asked Questions");

            migrationBuilder.CreateTable(
                name: "Jurisdictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Mpo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CountyParish = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    OtherPartners = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jurisdictions", x => x.Id);
                },
                comment: "Jurisdictions");

            migrationBuilder.CreateTable(
                name: "LocationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Icon = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationTypes", x => x.Id);
                },
                comment: "Location Types");

            migrationBuilder.CreateTable(
                name: "MeasureComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    LocationIdentifier = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureComments", x => x.Id);
                },
                comment: "Measure Comments");

            migrationBuilder.CreateTable(
                name: "MeasureType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Abbreviation = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    ShowOnWebsite = table.Column<bool>(type: "bit", nullable: false),
                    ShowOnAggregationSite = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureType", x => x.Id);
                },
                comment: "Measure Types");

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(24)", unicode: false, maxLength: 24, nullable: false),
                    Icon = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Link = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Document = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Menu Items");

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Manufacturer = table.Column<string>(type: "varchar(48)", unicode: false, maxLength: 48, nullable: false),
                    Model = table.Column<string>(type: "varchar(48)", unicode: false, maxLength: 48, nullable: false),
                    DeviceType = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Unknown"),
                    WebPage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                },
                comment: "Products");

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                },
                comment: "Regions");

            migrationBuilder.CreateTable(
                name: "RouteDistances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Distance = table.Column<double>(type: "float", nullable: false),
                    LocationIdentifierA = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    LocationIdentifierB = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteDistances", x => x.Id);
                },
                comment: "Route Distances");

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                },
                comment: "Location Routes");

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Setting = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false),
                    Value = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                },
                comment: "Application Settings");

            migrationBuilder.CreateTable(
                name: "VersionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(64)", unicode: false, maxLength: 64, nullable: false),
                    Notes = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValue: new DateTime(2023, 12, 13, 10, 32, 44, 487, DateTimeKind.Local).AddTicks(5546)),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "WatchDogLogEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    locationId = table.Column<int>(type: "int", nullable: false),
                    locationIdentifier = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ComponentType = table.Column<int>(type: "int", nullable: false),
                    ComponentId = table.Column<int>(type: "int", nullable: false),
                    IssueType = table.Column<int>(type: "int", nullable: false),
                    Details = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchDogLogEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserArea",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(900)", unicode: false, nullable: false),
                    AreaId = table.Column<int>(type: "int", nullable: false)
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
                },
                comment: "UserAreas");

            migrationBuilder.CreateTable(
                name: "UserJurisdiction",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(900)", unicode: false, nullable: false),
                    JurisdictionId = table.Column<int>(type: "int", nullable: false)
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
                name: "DetectionTypeMeasureType",
                columns: table => new
                {
                    DetectionTypesId = table.Column<int>(type: "int", nullable: false),
                    MeasureTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionTypeMeasureType", x => new { x.DetectionTypesId, x.MeasureTypesId });
                    table.ForeignKey(
                        name: "FK_DetectionTypeMeasureType_DetectionTypes_DetectionTypesId",
                        column: x => x.DetectionTypesId,
                        principalTable: "DetectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetectionTypeMeasureType_MeasureType_MeasureTypesId",
                        column: x => x.MeasureTypesId,
                        principalTable: "MeasureType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeasureCommentMeasureType",
                columns: table => new
                {
                    MeasureCommentsId = table.Column<int>(type: "int", nullable: false),
                    MeasureTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureCommentMeasureType", x => new { x.MeasureCommentsId, x.MeasureTypesId });
                    table.ForeignKey(
                        name: "FK_MeasureCommentMeasureType_MeasureComments_MeasureCommentsId",
                        column: x => x.MeasureCommentsId,
                        principalTable: "MeasureComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MeasureCommentMeasureType_MeasureType_MeasureTypesId",
                        column: x => x.MeasureTypesId,
                        principalTable: "MeasureType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeasureOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Option = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    Value = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    MeasureTypeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasureOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeasureOptions_MeasureType_MeasureTypeId",
                        column: x => x.MeasureTypeId,
                        principalTable: "MeasureType",
                        principalColumn: "Id");
                },
                comment: "Measure Options");

            migrationBuilder.CreateTable(
                name: "DeviceConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Firmware = table.Column<string>(type: "varchar(16)", unicode: false, maxLength: 16, nullable: false),
                    Notes = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    Protocol = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Unknown"),
                    Port = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "((0))"),
                    Directory = table.Column<string>(type: "varchar(1024)", unicode: false, maxLength: 1024, nullable: true),
                    SearchTerm = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: true),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceConfiguration_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "DeviceConfiguration");

            migrationBuilder.CreateTable(
                name: "Location",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    PrimaryName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false, defaultValueSql: "('')"),
                    SecondaryName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Ipaddress = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValueSql: "('10.0.0.1')"),
                    ChartEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LoggingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    VersionAction = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((10))"),
                    Note = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false, defaultValueSql: "('Initial')"),
                    Start = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pedsare1to1 = table.Column<bool>(type: "bit", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    ControllerTypeId = table.Column<int>(type: "int", nullable: false),
                    JurisdictionId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((0))"),
                    LocationTypeId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Location_ControllerTypes_ControllerTypeId",
                        column: x => x.ControllerTypeId,
                        principalTable: "ControllerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Location_Jurisdictions_JurisdictionId",
                        column: x => x.JurisdictionId,
                        principalTable: "Jurisdictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Location_LocationTypes_LocationTypeId",
                        column: x => x.LocationTypeId,
                        principalTable: "LocationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Location_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Locations");

            migrationBuilder.CreateTable(
                name: "UserRegion",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(900)", unicode: false, nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "RouteLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    PrimaryPhase = table.Column<int>(type: "int", nullable: false),
                    OpposingPhase = table.Column<int>(type: "int", nullable: false),
                    PrimaryDirectionId = table.Column<int>(type: "int", nullable: false),
                    OpposingDirectionId = table.Column<int>(type: "int", nullable: false),
                    IsPrimaryOverlap = table.Column<bool>(type: "bit", nullable: false),
                    IsOpposingOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PreviousLocationDistanceId = table.Column<int>(type: "int", nullable: true),
                    NextLocationDistanceId = table.Column<int>(type: "int", nullable: true),
                    LocationIdentifier = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    RouteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteLocations_DirectionTypes_OpposingDirectionId",
                        column: x => x.OpposingDirectionId,
                        principalTable: "DirectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteLocations_DirectionTypes_PrimaryDirectionId",
                        column: x => x.PrimaryDirectionId,
                        principalTable: "DirectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteLocations_RouteDistances_NextLocationDistanceId",
                        column: x => x.NextLocationDistanceId,
                        principalTable: "RouteDistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteLocations_RouteDistances_PreviousLocationDistanceId",
                        column: x => x.PreviousLocationDistanceId,
                        principalTable: "RouteDistances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RouteLocations_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Route Locations");

            migrationBuilder.CreateTable(
                name: "Approaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Mph = table.Column<int>(type: "int", nullable: true),
                    ProtectedPhaseNumber = table.Column<int>(type: "int", nullable: false),
                    IsProtectedPhaseOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PermissivePhaseNumber = table.Column<int>(type: "int", nullable: true),
                    IsPermissivePhaseOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PedestrianPhaseNumber = table.Column<int>(type: "int", nullable: true),
                    IsPedestrianPhaseOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PedestrianDetectors = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    DirectionTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Approaches_DirectionTypes_DirectionTypeId",
                        column: x => x.DirectionTypeId,
                        principalTable: "DirectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Approaches_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Approaches");

            migrationBuilder.CreateTable(
                name: "AreaLocation",
                columns: table => new
                {
                    AreasId = table.Column<int>(type: "int", nullable: false),
                    LocationsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaLocation", x => new { x.AreasId, x.LocationsId });
                    table.ForeignKey(
                        name: "FK_AreaLocation_Areas_AreasId",
                        column: x => x.AreasId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AreaLocation_Location_LocationsId",
                        column: x => x.LocationsId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoggingEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Ipaddress = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValueSql: "('10.0.0.1')"),
                    DeviceStatus = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValue: "Unknown"),
                    Notes = table.Column<string>(type: "varchar(512)", unicode: false, maxLength: 512, nullable: true),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    DeviceConfigurationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_DeviceConfiguration_DeviceConfigurationId",
                        column: x => x.DeviceConfigurationId,
                        principalTable: "DeviceConfiguration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Devices_Location_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Devices");

            migrationBuilder.CreateTable(
                name: "Detectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DectectorIdentifier = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DetectorChannel = table.Column<int>(type: "int", nullable: false),
                    DistanceFromStopBar = table.Column<int>(type: "int", nullable: true),
                    MinSpeedFilter = table.Column<int>(type: "int", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateDisabled = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LaneNumber = table.Column<int>(type: "int", nullable: true),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    LaneType = table.Column<int>(type: "int", nullable: false),
                    DetectionHardware = table.Column<int>(type: "int", nullable: false),
                    DecisionPoint = table.Column<int>(type: "int", nullable: true),
                    MovementDelay = table.Column<int>(type: "int", nullable: true),
                    LatencyCorrection = table.Column<double>(type: "float", nullable: false),
                    ApproachId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Detectors_Approaches_ApproachId",
                        column: x => x.ApproachId,
                        principalTable: "Approaches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Detectors");

            migrationBuilder.CreateTable(
                name: "DetectionTypeDetector",
                columns: table => new
                {
                    DetectionTypesId = table.Column<int>(type: "int", nullable: false),
                    DetectorsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionTypeDetector", x => new { x.DetectionTypesId, x.DetectorsId });
                    table.ForeignKey(
                        name: "FK_DetectionTypeDetector_DetectionTypes_DetectionTypesId",
                        column: x => x.DetectionTypesId,
                        principalTable: "DetectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetectionTypeDetector_Detectors_DetectorsId",
                        column: x => x.DetectorsId,
                        principalTable: "Detectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetectorComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comment = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: false),
                    DetectorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectorComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetectorComments_Detectors_DetectorId",
                        column: x => x.DetectorId,
                        principalTable: "Detectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Detector Comments");

            migrationBuilder.InsertData(
                table: "DetectionTypes",
                columns: new[] { "Id", "Abbreviation", "Description", "DisplayOrder" },
                values: new object[,]
                {
                    { 0, "NA", "Unknown", 0 },
                    { 1, "B", "Basic", 1 },
                    { 2, "AC", "Advanced Count", 2 },
                    { 3, "AS", "Advanced Speed", 3 },
                    { 4, "LLC", "Lane-by-lane Count", 4 },
                    { 5, "LLS", "Lane-by-lane with Speed Restriction", 5 },
                    { 6, "SBP", "Stop Bar Presence", 6 },
                    { 7, "AP", "Advanced Presence", 7 }
                });

            migrationBuilder.InsertData(
                table: "DirectionTypes",
                columns: new[] { "Id", "Abbreviation", "Description", "DisplayOrder" },
                values: new object[,]
                {
                    { 0, "NA", "Unknown", 0 },
                    { 1, "NB", "Northbound", 3 },
                    { 2, "SB", "Southbound", 4 },
                    { 3, "EB", "Eastbound", 1 },
                    { 4, "WB", "Westbound", 2 },
                    { 5, "NE", "Northeast", 5 },
                    { 6, "NW", "Northwest", 6 },
                    { 7, "SE", "Southeast", 7 },
                    { 8, "SW", "Southwest", 8 }
                });

            migrationBuilder.InsertData(
                table: "ExternalLinks",
                columns: new[] { "Id", "DisplayOrder", "Name", "Url" },
                values: new object[,]
                {
                    { 1, 1, "Indiana Hi Resolution Data Logger Enumerations", " https://docs.lib.purdue.edu/jtrpdata/3/" },
                    { 2, 2, "Florida ATSPM", "https://atspm.cflsmartroads.com/ATSPM" },
                    { 3, 3, "FAST (Southern Nevada)", "http://challenger.nvfast.org/spm" },
                    { 4, 4, "Georgia ATSPM", "https://traffic.dot.ga.gov/atspm" },
                    { 5, 5, "Arizona ATSPM", "http://spmapp01.mcdot-its.com/ATSPM" },
                    { 6, 6, "Alabama ATSPM", "http://Locationmetrics.ua.edu" },
                    { 7, 7, "ATSPM Workshop 2016 SLC", "http://docs.lib.purdue.edu/atspmw/2016" },
                    { 8, 8, "Train The Trainer Webinar Day 1 - Morning", "https://connectdot.connectsolutions.com/p75dwqefphk   " },
                    { 9, 9, "Train The Trainer Webinar Day 1 - Afternoon", "https://connectdot.connectsolutions.com/p6l6jaoy3gj" },
                    { 10, 10, "Train The Trainer Webinar Day 2 - Morning", "https://connectdot.connectsolutions.com/p6mlkvekogo/" },
                    { 11, 11, "Train The Trainer Webinar Day 2 - Mid Morning", "https://connectdot.connectsolutions.com/p3ua8gtj09r/" }
                });

            migrationBuilder.InsertData(
                table: "Faqs",
                columns: new[] { "Id", "Body", "DisplayOrder", "Header" },
                values: new object[,]
                {
                    { 1, "<b>There are two ways to navigate the UDOT Automated Traffic Location Performance Measures website</b><br/><br/><u>MAP</u><ol><li>Zoom in on the map and click on the desired intersection (note: the map can be filtered by selecting “metric type” ).</li><li>Select the available chart on the map from the list of available measures for the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol><u>Location LIST</u><ol><li>Select the chart by clicking the checkbox for the desired chart.</li><li>Click the “Location List” bar at the top of the map window.</li><li>Click “Select” next to the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol>", 1, "<b>How do I navigate the UDOT Automated Traffic Location Performance Measures website?</b>" },
                    { 2, "Automated Traffic Location Performance Measures show real-time and a history of performance at Locationized intersections.  The various measures will evaluate the quality of progression of traffic along the corridor, and displays any unused green time that may be available from various movements. This information informs UDOT of vehicle and pedestrian detector malfunctions, measures vehicle delay and lets us know volumes, speeds and travel time of vehicles.   The measures are used to optimize mobility and manage traffic Location timing and maintenance to reduce congestion, save fuel costs and improve safety.  There are several measures currently in use with others in development. ", 2, "<b>What are Automated Traffic Location Performance Measures</b>" },
                    { 3, "The traffic Location controller manufactures (Econolite, Intelight, Siemens, McCain, TrafficWare and some others) wrote a “data-logger” program that runs in the background of the traffic Location controller firmware. The Indiana Traffic Location Hi Resolution Data Logger Enumerations (http://docs.lib.purdue.edu/cgi/viewcontent.cgi?article=1002&context=jtrpdata) encode events to a resolution to the nearest 100 milliseconds.  The recorded enumerations will have events for “phase begin green”, “phase gap out”, “phase max out”, “phase begin yellow clearance”, “phase end yellow clearance”, “pedestrian begin walk”, “pedestrian begin clearance”, “detector off”, “detector on”, etc.  For each event, a time-stamp is given and the event is stored temporarily in the Location controller.  Over 125 various enumerations are currently in use.  Then, using an FTP connection from a remote server to the traffic Location controller, packets of the hi resolution data logger enumerations (with its 1/10th second resolution time-stamp) are retrieved and stored on a web server at the UDOT Traffic Operations Center about every 10 to 15 minutes (unless the “upload current data” checkbox is enabled, where an FTP connection will be immediately made and the data will be displayed in real-time).  Software was written in-house by UDOT that allows us to graph and display the various data-logger enumerations and to show the results on the UDOT Automated Traffic Location Performance Measures website.", 3, "<b>How do Automated Traffic Location Performance Measures work?</b>" },
                    { 4, "A central traffic management system is not used or needed for the UDOT Automated Traffic Location Performance Measures.  It is all being done through FTP connections from a web server through the network directly to the traffic Location controller which currently has the Indiana Traffic Location Hi Resolution Data Logger Enumerations running in the background of the controller firmware.  The UDOT Automated Traffic Location Performance Measures are independent of any central traffic management system.\r\n", 4, "<b>Which central traffic management system is used to get the Automated Traffic Location Performance Measures</b>" },
                    { 5, "In 2011, UDOT’s executive director assigned a Quality Improvement Team (QIT) to make recommendations that will result in UDOT providing “world-class traffic Location maintenance and operations”.  The QIT evaluated existing operations, national best practices, and NCHRP recommendations to better improve UDOT’s Location maintenance and operations practices.  One of the recommendations from the QIT was to “implement real-time monitoring of system health and quality of operations”.  The real-time Automated Location Performance Measures allow us to greatly improve the quality of Location operations and to also know when equipment such as pedestrian detection or vehicle detection is not working properly.  We are simply able to do more with less and to manage traffic more effectively 24/7.  In addition, we are able to optimize intersections and corridors when they need to be re-optimized, instead of on a set schedule.", 5, "<b>Why does Utah need Automated Traffic Location Performance Measures?</b>" },
                    { 6, "The UDOT Automated Traffic Location Measures software was developed in-house at UDOT by the Department of Technology Services.  Purdue University and the Indiana Department of Transportation (INDOT) assisted in getting us started on this endeavor.", 6, "<b>Where did you get the Automated Traffic Location Performance Measure software?</b>" },
                    { 7, "The Purdue coordination diagram concept was introduced in 2009 by Purdue University to visualize the temporal relationship between the coordinated phase indications and vehicle arrivals on a cycle-by-cycle basis.  The Indiana Traffic Location HI Resolution Data Logger Enumerations (http://docs.lib.purdue.edu/cgi/viewcontent.cgi?article=1002&context=jtrpdata) was a joint transportation research program (updated in November 2012 but started earlier) that included people from Indiana DOT, Purdue University, Econolite, PEEK, and Siemens.  <br/><br/>After discussions with Dr. Darcy Bullock from Purdue University and INDOT’s James Sturdevant, UDOT started development of the UDOT Automated Location Performance Measures website April of 2012.", 7, "<b>How did the Automated Traffic Location Performance Measures Begin?</b> " },
                    { 8, "UDOT’s goal is transparency and unrestricted access to all who have a desire for traffic Location data.  Our goal in optimizing mobility, improving safety, preserving infrastructure and strengthening the economy means that all who have a desire to use the data should have access to the data without restrictions.  This includes all of UDOT (various divisions and groups), consultants, academia, MPO’s, other jurisdictions, FHWA, the public, and others.  It is also UDOT’s goal to be the most transparent Department of Transportation in the country.  Having a website where real-time Automated Location Performance Measures can be obtained without special software, passwords or restricted firewalls will help UDOT in achieving the goal of transparency, and allows everyone access to the data without any silos.", 8, "<b>Why are there no passwords or firewalls to access the website and see the measures?</b>" },
                    { 9, "There are many uses and benefits of the various measures.  Some of the key uses are:<br/><br/><u>Purdue Coordination Diagrams (PCD’s)</u> – Used to visualize the temporal relationship between the coordinated phase indications and vehicle arrivals on a cycle-by-cycle basis.  The PCD’s show the progression quality along the corridor and answer questions, such as “What percent of vehicles are arriving during the green?” or “What is the platoon ratio during various coordination patterns?” The PCD’s are instrumental in optimizing offsets, identifying oversaturated or under saturated splits for the coordinated phases, the effects of early return of green and coordinated phase actuations, impacts of queuing, adjacent Location synchronization, etc.<br/> <br/>In reading the PCD’s, between the green and yellow lines, the phase is green; between the yellow and red lines, the phase is yellow; and underneath the green line, the phase is red.  The long vertical red lines during the late night hours is showing the main street sitting in green as the side streets and left turns are being skipped.  The short vertical red lines show skipping of the side street / left turns or a late start of green for the side street or left turn.  AoG is the percent of vehicles arriving during the green phase.  GT is the percent of green split time for the phase and PR is the Platoon Ratio (Equation 15-4 from the 2000 Highway Capacity Manual).<br/><br/><u>Approach Volumes</u> – Counts the approach vehicle volumes as shown arriving upstream of the intersection about 350 ft – 400 ft.  The detection zones are in advance of the turning lanes, so the approach volumes don’t know if the vehicles are going straight through, turning left or right.  The accuracy of the approach volumes tends to undercount under heavy traffic and under multi-lane facilities.  Approach volumes are used in traffic models, as well as identifying directional splits in traffic. In addition, the measure is also used in evaluating the least disruptive time to allow lanes to be taken for maintenance and construction activities.<br/><br/><u/>Approach Speeds</u> – The speeds are obtained from the Wavetronix radar Advance Smartsensor.  As vehicles cross the 10-foot wide detector in advance of the intersection (350 ft – 400 ft upstream of the stop bar), the speed is captured, recorded, and time-stamped.  In graphing the results, a time filter is used that starts 15 seconds (user defined) after the initial green to the start of the yellow.  The time filter allows for free-flow speed conditions to be displayed that are independent of the traffic Location timings.  The approach speed measure is beneficial in knowing the approach speeds to use for modeling purposes – both for normal time-of-day coordination plans and for adverse weather or special event plans.  They are also beneficial in knowing when speed conditions degrade enough to warrant a change in time-of-day coordination plans to adverse weather or special event plans.  In addition, the speed data is used to set yellow and all-red intervals for Location timing, as well as for various speed studies.<br/><br/><u>Purdue Phase Termination Charts</u> – Shows how each phase terminates when it changes from green to red.  The measure will show if the termination occurred by a gapout, a maxout / forceoff, or skip.  A gapout means that not all of the programmed time was used.  A maxout occurs during fully actuated (free) operations, while forceoff’s occur during coordination.  Both a maxout and forceoff shows that all the programmed time was used. A skip means that the phase was not active and did not turn on.  In addition, the termination can be evaluated by consecutive occurrences in a approach.  For example, you can evaluate if three (range is between 1 and 5) gapouts or skips occurred in a approach.  This feature is helpful in areas where traffic volume fluctuations are high.  Also shown are the pedestrian activations for each phase.  What this measure does not show is the amount of gapout time remaining if a gapout occurred.  The Split Monitor measure is used to answer that question.<br/><br/>This measure is used to identify movements where split time may need to be taken from some phases and given to other phases.  Also, this measure is very useful in identifying problems with vehicle and pedestrian detection.  For example, if the phase is showing constant maxouts all night long, it is assumed that there is a detection problem.<br/><br/><u>Split Monitor</u> – This measure shows the amount of split time (green, yellow and all-red) used by the various phases at the intersection.  Greens show gapouts, reds show maxouts, blues show forceoffs and yellows show pedestrian activations.  This measure is useful to know the amount of split time each phase uses.Turning Movement Volume Counts – this measure shows the lane-by-lane vehicles per hour (vph) and total volume for each movement.  Three graphs are available for each approach (left, thru, right).  Also shown for each movement are the total volume, peak hour, peak hour factor and lane utilization factor.  The lane-by-lane volume counts are used for traffic models and traffic studies.<br/><br/><u>Approach Delay</u> – This measure shows a simplified approach delay by displaying the time between detector activations during the red phase and when the phase turns green for the coordinated movements.  This measure does not account for start-up delay, deceleration, or queue length that exceeds the detection zone.  This measure is beneficial in evaluating over time the delay per vehicle and delay per hour values for each coordinated approach.<br/><br/><u>Arrivals on Red</u> – This measure shows the percent of vehicles arriving on red (inverse of % vehicles arriving on green) and the percent red time for each coordination pattern.  The Y axis is graphing the volume (vph) and the secondary Y axis graphs the percent vehicles arriving on red.  This measure is useful in identifying areas where the progression quality is poor.<br/><br/><u>Yellow and Red Actuations</u> – This measure plots vehicle arrivals during the yellow and red portions of an intersection's movements where the speed of the vehicle is interpreted to be too fast to stop before entering the intersection. It provides users with a visual indication of occurrences, violations, and several related statistics. The purpose of this chart is to identify engineering countermeasures to deal with red light running.<br/><br/><u>Purdue Split Failure</u> – This measure calculates the percent of time that stop bar detectors are occupied during the green phase and then during the first five seconds of red. This measure is a good indication that at least one vehicle did not clear during the green.", 9, "<b>How do you use the various Location Performance Measures and what do they do?</b> " },
                    { 10, "The Automated Location Performance Measures are an effective way to reduce congestion, save fuel costs and improve safety.  We are simply able to do more with less and are more effectively able to manage traffic every day of the week and at all times of the day, even when a traffic Location engineer is not available.  We have identified several detection problems, corrected time-of-day coordination errors in the traffic Location controller scheduler, corrected offsets, splits, among other things.  In addition, we have been able to use more accurate data in optimizing models and doing traffic studies, and have been able to more correctly set various Location timing parameters.", 10, "<b>How effective are Automated Traffic Location Performance Measures</b>" },
                    { 11, "Although the UDOTAutomated Traffic Location Performance Measures cannot guarantee you will only get green lights, the system does help make traveling through Utah more efficient.  UDOT Automatic Location Performance Measures have already already helped to reduce the number of stops and delay at Locationized intersections.  Continued benefits are anticipated.", 11, "<b>Does this mean I never have to stop at a red light?</b>" },
                    { 12, "Yes, UDOT Automated Traffic Location Performance Measures has already saved Utahans time and money.  By increasing corridor speeds while reducing intersection delays, traffic Location stops, and the ability to monitor operations 24/7.", 12, "<b>Will Automated Traffic Location Performance Measures save me money?  If so, how are cost savings measured?</b>" },
                    { 13, "By reducing congestion and reducing the percent of vehicles arriving on a red light, UDOT Automated Traffic Location Performance Measures helps decrease the number of accidents that occur.  In addition, we are better able to manage detector failures and improve the duration of the change intervals and clearance times at intersections.", 13, "<b>How do Automated Traffic Location Performance Measures enhance safety?</b>" },
                    { 14, "UDOT Automated Traffic Location Performance Measures are designed to increase the safety and efficiency at Locationized intersections.  It is not intended to identify speeders or enforce traffic laws.  No personal information is recorded or used in data gathering.", 14, "<b>Can real-time Automated Traffic Location Performance Measures be used as a law enforcement tool?</b>" },
                    { 15, "We can estimate that each Location controller high resolution data requires approximately 19 MB of storage space each day.  For the UDOT system, we have approximately 2040 traffic Locations bringing back about 1 TB of data per month. In addition to the high resolution data, version 4.2.0 and above also allows for the data to be rolled up into aggregated tables in 15-minute bins. UDOT averages approximately 6 GB of aggregated tables per month. UDOT uses a SAN server that holds approximately 40 TB that runs SQL 2016. Our goal is to keep between 24 months and 36 months of high resolution data files and then to cold storage the old high resolution  data files for up to five years after that. The cold storage flat files (comma deliminated file with no indexing) will require about 2 TB of storage per year. UDOT plans on keeping the aggregated tables in 15-minute bins indefinitely. ", 15, "<b>Server and Data Storage Requirements</b>" },
                    { 16, "The data has been useful for some of the following users in Utah:<br/><br/><ul><li><u>Location engineers</u> in optimize and fine-tuning Location timing.</li><li><u>Maintenance Location technicians</u> in identifying broken detector problems and investigating trouble calls.</li><li><u>Traffic engineers</u> in conducting various traffic studies, such as speed studies, turning movement studies, modeling studies, and optimizing the intersection operations.</li><li><u>Consultants</u> in improving traffic Location operations, as UDOT outsources some of the Location operations, design and planning to consultants.</li><li><u>UDOT Traffic & Safety, UDOT Traffic Engineers, UDOT Resident Construction Engineers</u> in conducting various traffic studies and/or in determining the time-of-day where construction or maintenance activities would be least disruptive to the traveling motorists.</li><li><u>Metropolitan Planning Organizations</u> (MPO’s) in calibrating the regional traffic models.</li><li><u>Academia</u> in conducting various research studies, such as evaluating the effectiveness of operations during adverse weather, evaluating the optimum Location timing for innovative intersections such as DDI’s, CFI’s and Thru-Turns, etc.</li><li><u>City and County</u> Government in using the data in similar manner to UDOT.</li></ul>", 16, "<b>Who uses the Automated Traffic Location Performance Measures data?</b>" },
                    { 17, "<table class='table table-bordered'>\r\n 	                            <tr>\r\n                                    <th> MEASURE </th>\r\n                                    <th> DETECTION NEEDED </th>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Coordination Diagram </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Volume </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Speed </td>\r\n                                    <td> Setback count (350 ft – 400 ft) using radar </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Phase Termination </td>\r\n                                    <td> No detection needed or used </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Split Monitor </td>\r\n                                    <td> No detection needed or used </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Turning Movement Counts </td>\r\n                                    <td> Stop bar (lane-by-lane) count </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Delay </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Arrivals on Red </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Yellow and Red Actuations </td>\r\n                                    <td> Stop bar (lane-by-lane) count that is either in front of the stop bar or has a speed filter enabled </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Split Failure </td>\r\n                                    <td> Stop bar presence detection, either by lane group or individual lane </td>\r\n                                </tr>\r\n                        </table>\r\n                        <b> Automated Traffic Location Performance Measures will work with any type of detector that is capable of counting vehicles, e.g., loops, video, pucks, radar. (The only exception to this is the speed measure, where UDOT’s Automated Location Performance Measures for speeds will only work with the Wavetronix Advance SmartSensor.) Please note that two of the measures (Purdue Phase Termination and Split Monitor) do not use detection and are extremely useful measures.</b>", 17, "<b>What are the detection requirements for each metric?</b> " },
                    { 18, "Some measures have different detection requirements than other measures. For example, for approach speeds, UDOT uses the Wavetronix Advance Smartsensor radar detector and has been using this detector since 2006 for dilemma zone purposes if the design speed is 40 mph or higher.  This same detector is what we use for our setback counts 350 feet – 400 feet upstream of the intersection.  In addition, we are also able to pull the raw radar speeds from the sensor back to the TOC server for the speed data.  Not all intersections have the Wavetronix Advance Smartsensors, therefore we are not able to display speed data, as well as the PCD’s, approach volume, arrivals on red or approach delay at each intersection.<br/><br/>The turning movement count measure requires lane-by-lane detection capable of counting vehicles in each lane.  Configuring the detection for lane-by-lane counts is time consuming and takes a commitment to financial resources.", 18, "<b>Why do some intersections only show a few metrics and others have more?</b>" },
                    { 19, " <b> System Requirements:</b>\r\n                        <b> Operating Systems and Software:</b>\r\n                        The UDOT Automated Location Performance Measures system runs on Microsoft Windows Servers.\r\n                        The web components are hosted by Microsoft Internet Information Server(IIS).\r\n                        The database server is a Microsoft SQL 2016 server.\r\n                        <b> Storage and Processing:</b>\r\n                        Detector data uses about 40 % of the storage space of the UDOT system,\r\n                        so the number of detectors attached to a controller will have a huge impact on the amount of storage space required.Detector data is also the most important information we collect.\r\n                        We estimate that each Location will generate 19 MB of data per day.\r\n                        The amount of processing power required is highly dependant on how many Locations are on the system,\r\n                        how many servers will be part of the system,\r\n                        and how many people will be using the system.  It is possible to host all of the system functions on one powerful server, or split them out into multiple, less expensive servers.  If your agency decided to make the Automated Location Performance Measures available to the public, it might be best to have a web server separate from the database server.Much of the heavy processing for the charts is done by web services, and it is possible to host these services on a dedicated computer.\r\n                        While each agency should consult with their IT department for specific guidelines on how to best deliver a secure, stable and responsive solution, we can estimate that most mid-range to high-end servers will be able to handle the task of hosting and creating measures for most agencies.<ul>\r\n                        <li>Windows Server 2008 or newer installed</li>\r\n                        <li>.NET 4.5.2 Framework installed</li>\r\n                        <li>IIS 7 or better installed, along with ASP.NET 4.0 or later</li>\r\n                        <li>SQL Server Express, SQL Server 2008 R2, or newer installed</li>\r\n                        <li>Firewall exceptions for connections to the controllers</li>\r\n                        <li>If Watchdog features are desired, installation requires access to an SMTP (email) server. It will accept email from the Automated Location Performance Measures (ATSPM) server. The SMTP server can reside on the same machine.</li>\r\n                        <li>Microsoft Visual Studio 2013 or later is recommended</li></ul>", 19, "<b>What are the System Requirements?</b>" },
                    { 20, "You can contact UDOT’s Traffic Location Operations Engineer, Mark Taylor at marktaylor@utah.gov or phone at 801-887-3714 to find out more information about Automated Location Performance Measures.", 20, "<b>Who do I contact to find out more information about Automated Traffic Location Performance Measures</b> " },
                    { 21, "You can download the source code at GitHub at: https://github.com/udotdevelopment/ATSPM. GitHub is more for development and those interested in further developing and modifying the code. We encourage developers to contribute the enhancements back to GitHub so others can benefit as well.  For those interested in the executable ATSPM files, those are found on the FHWA's open source portal at: https://www.itsforge.net/index.php/community/explore-applications#/30.", 21, "<b>How do I get the source code for the Automated Traffic Location Performance Measures Website?</b> " }
                });

            migrationBuilder.InsertData(
                table: "MeasureType",
                columns: new[] { "Id", "Abbreviation", "DisplayOrder", "Name", "ShowOnAggregationSite", "ShowOnWebsite" },
                values: new object[,]
                {
                    { 1, "PPT", 1, "Purdue Phase Termination", false, true },
                    { 2, "SM", 5, "Split Monitor", false, true },
                    { 3, "PedD", 10, "Pedestrian Delay", false, true },
                    { 4, "PD", 15, "Preemption Details", false, true },
                    { 5, "TMC", 40, "Turning Movement Counts", false, true },
                    { 6, "PCD", 60, "Purdue Coordination Diagram", false, true },
                    { 7, "AV", 45, "Approach Volume", false, true },
                    { 8, "AD", 50, "Approach Delay", false, true },
                    { 9, "AoR", 55, "Arrivals On Red", false, true },
                    { 10, "Speed", 65, "Approach Speed", false, true },
                    { 11, "YRA", 35, "Yellow and Red Actuations", false, true },
                    { 12, "SF", 30, "Purdue Split Failure", false, true },
                    { 13, "LP", 70, "Purdue Link Pivot", false, false },
                    { 14, "PSR", 80, "Preempt Service Request", false, false },
                    { 15, "PS", 75, "Preempt Service", false, false },
                    { 16, "DVA", 85, "Detector Activation Count", true, false },
                    { 17, "TAA", 20, "Timing And Actuation", false, true },
                    { 18, "APCD", 102, "Approach Pcd", true, false },
                    { 19, "CA", 103, "Approach Cycle", true, false },
                    { 20, "SFA", 104, "Approach Split Fail", true, false },
                    { 22, "PreemptA", 105, "Location Preemption", true, false },
                    { 24, "TSPA", 106, "Location Priority", true, false },
                    { 25, "ASA", 107, "Approach Speed", true, false },
                    { 26, "YRAA", 108, "Approach Yellow Red Activations", true, false },
                    { 27, "SEC", 109, "Location Event Count", true, false },
                    { 28, "AEC", 110, "Approach Event Count", true, false },
                    { 29, "AEC", 111, "Phase Termination", true, false },
                    { 30, "APD", 112, "Phase Pedestrian Delay", true, false },
                    { 31, "LTGA", 112, "Left Turn Gap Analysis", false, true },
                    { 32, "WT", 113, "Wait Time", false, true },
                    { 33, "GVD", 115, "Gap Vs Demand", false, false },
                    { 34, "LTG", 114, "Left Turn Gap", true, false },
                    { 35, "SM", 120, "Split Monitor", true, false }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Approaches_DirectionTypeId",
                table: "Approaches",
                column: "DirectionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Approaches_LocationId",
                table: "Approaches",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaLocation_LocationsId",
                table: "AreaLocation",
                column: "LocationsId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectionTypeDetector_DetectorsId",
                table: "DetectionTypeDetector",
                column: "DetectorsId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectionTypeMeasureType_MeasureTypesId",
                table: "DetectionTypeMeasureType",
                column: "MeasureTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectorComments_DetectorId",
                table: "DetectorComments",
                column: "DetectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Detectors_ApproachId",
                table: "Detectors",
                column: "ApproachId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceConfiguration_ProductId",
                table: "DeviceConfiguration",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_DeviceConfigurationId",
                table: "Devices",
                column: "DeviceConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_LocationId",
                table: "Devices",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ControllerTypeId",
                table: "Location",
                column: "ControllerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_JurisdictionId",
                table: "Location",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_LocationTypeId",
                table: "Location",
                column: "LocationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_RegionId",
                table: "Location",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_MeasureCommentMeasureType_MeasureTypesId",
                table: "MeasureCommentMeasureType",
                column: "MeasureTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_MeasureComments_LocationIdentifier",
                table: "MeasureComments",
                column: "LocationIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_MeasureOptions_MeasureTypeId",
                table: "MeasureOptions",
                column: "MeasureTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ParentId",
                table: "MenuItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteDistances_LocationIdentifierA_LocationIdentifierB",
                table: "RouteDistances",
                columns: new[] { "LocationIdentifierA", "LocationIdentifierB" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RouteLocations_NextLocationDistanceId",
                table: "RouteLocations",
                column: "NextLocationDistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteLocations_OpposingDirectionId",
                table: "RouteLocations",
                column: "OpposingDirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteLocations_PreviousLocationDistanceId",
                table: "RouteLocations",
                column: "PreviousLocationDistanceId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteLocations_PrimaryDirectionId",
                table: "RouteLocations",
                column: "PrimaryDirectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteLocations_RouteId",
                table: "RouteLocations",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteLocations_RouteId_LocationIdentifier",
                table: "RouteLocations",
                columns: new[] { "RouteId", "LocationIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Settings_Setting",
                table: "Settings",
                column: "Setting",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_VersionHistory_ParentId",
                table: "VersionHistory",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AreaLocation");

            migrationBuilder.DropTable(
                name: "DetectionTypeDetector");

            migrationBuilder.DropTable(
                name: "DetectionTypeMeasureType");

            migrationBuilder.DropTable(
                name: "DetectorComments");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "ExternalLinks");

            migrationBuilder.DropTable(
                name: "Faqs");

            migrationBuilder.DropTable(
                name: "MeasureCommentMeasureType");

            migrationBuilder.DropTable(
                name: "MeasureOptions");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "RouteLocations");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "UserArea");

            migrationBuilder.DropTable(
                name: "UserJurisdiction");

            migrationBuilder.DropTable(
                name: "UserRegion");

            migrationBuilder.DropTable(
                name: "VersionHistory");

            migrationBuilder.DropTable(
                name: "WatchDogLogEvents");

            migrationBuilder.DropTable(
                name: "DetectionTypes");

            migrationBuilder.DropTable(
                name: "Detectors");

            migrationBuilder.DropTable(
                name: "DeviceConfiguration");

            migrationBuilder.DropTable(
                name: "MeasureComments");

            migrationBuilder.DropTable(
                name: "MeasureType");

            migrationBuilder.DropTable(
                name: "RouteDistances");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "Approaches");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "DirectionTypes");

            migrationBuilder.DropTable(
                name: "Location");

            migrationBuilder.DropTable(
                name: "ControllerTypes");

            migrationBuilder.DropTable(
                name: "Jurisdictions");

            migrationBuilder.DropTable(
                name: "LocationTypes");

            migrationBuilder.DropTable(
                name: "Regions");
        }
    }
}
