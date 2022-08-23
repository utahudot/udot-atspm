using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrasturcture.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.Id);
                },
                comment: "Action Log Types");

            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.Id);
                },
                comment: "Agency Type for Action Logs");

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                },
                comment: "Application Types");

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.Id);
                },
                comment: "Signal Area");

            migrationBuilder.CreateTable(
                name: "ControllerTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Snmpport = table.Column<long>(type: "bigint", nullable: false),
                    Ftpdirectory = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ActiveFtp = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControllerTypes", x => x.Id);
                },
                comment: "Signal Controller Types");

            migrationBuilder.CreateTable(
                name: "DetectionHardwares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionHardwares", x => x.Id);
                },
                comment: "Dectector Hardware Types");

            migrationBuilder.CreateTable(
                name: "DetectionTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
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
                    Name = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    Url = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
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
                    Header = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    Body = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false)
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
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Mpo = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    CountyParish = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    OtherPartners = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jurisdictions", x => x.Id);
                },
                comment: "Signal Jurisdictions");

            migrationBuilder.CreateTable(
                name: "LaneTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Abbreviation = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaneTypes", x => x.Id);
                },
                comment: "Lane Types");

            migrationBuilder.CreateTable(
                name: "MeasuresDefaults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Measure = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    OptionName = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Value = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasuresDefaults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Menus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    Application = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Controller = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValueSql: "('')"),
                    Action = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValueSql: "('')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menus", x => x.Id);
                },
                comment: "Menu Items");

            migrationBuilder.CreateTable(
                name: "MetricTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ChartName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Abbreviation = table.Column<string>(type: "varchar(8)", unicode: false, maxLength: 8, nullable: true),
                    ShowOnWebsite = table.Column<bool>(type: "bit", nullable: false),
                    ShowOnAggregationSite = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricTypes", x => x.Id);
                },
                comment: "Metric Types");

            migrationBuilder.CreateTable(
                name: "MovementTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Abbreviation = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementTypes", x => x.Id);
                },
                comment: "Movement Types");

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                },
                comment: "Regions");

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.Id);
                },
                comment: "Signal Routes");

            migrationBuilder.CreateTable(
                name: "VersionActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActionLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: true),
                    SignalId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionLogs_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Action Logs");

            migrationBuilder.CreateTable(
                name: "ApplicationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveCount = table.Column<int>(type: "int", nullable: true),
                    MinPhaseTerminations = table.Column<int>(type: "int", nullable: true),
                    PercentThreshold = table.Column<double>(type: "float", nullable: true),
                    MaxDegreeOfParallelism = table.Column<int>(type: "int", nullable: true),
                    ScanDayStartHour = table.Column<int>(type: "int", nullable: true),
                    ScanDayEndHour = table.Column<int>(type: "int", nullable: true),
                    PreviousDayPmpeakStart = table.Column<int>(type: "int", nullable: true),
                    PreviousDayPmpeakEnd = table.Column<int>(type: "int", nullable: true),
                    MinimumRecords = table.Column<int>(type: "int", nullable: true),
                    WeekdayOnly = table.Column<bool>(type: "bit", nullable: true),
                    DefaultEmailAddress = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    FromEmailAddress = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    LowHitThreshold = table.Column<int>(type: "int", nullable: true),
                    EmailServer = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    MaximumPedestrianEvents = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "varchar(128)", unicode: false, maxLength: 128, nullable: false),
                    ArchivePath = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    SelectedDeleteOrMove = table.Column<int>(type: "int", nullable: true),
                    NumberOfRows = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<int>(type: "int", nullable: true),
                    TimeDuration = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ImagePath = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    RawDataCountLimit = table.Column<int>(type: "int", nullable: true),
                    ReCaptchaPublicKey = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ReCaptchaSecretKey = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    EnableDatbaseArchive = table.Column<bool>(type: "bit", nullable: true),
                    SelectedTableScheme = table.Column<int>(type: "int", nullable: true),
                    MonthsToKeepIndex = table.Column<int>(type: "int", nullable: true),
                    MonthsToKeepData = table.Column<int>(type: "int", nullable: true),
                    EmailAllErrors = table.Column<bool>(type: "bit", nullable: true),
                    CycleCompletionSeconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationSettings_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Application Settings");

            migrationBuilder.CreateTable(
                name: "DetectionTypeMetricType",
                columns: table => new
                {
                    DetectionTypeDetectionTypesId = table.Column<int>(type: "int", nullable: false),
                    MetricTypeMetricsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionTypeMetricType", x => new { x.DetectionTypeDetectionTypesId, x.MetricTypeMetricsId });
                    table.ForeignKey(
                        name: "FK_DetectionTypeMetricType_DetectionTypes_DetectionTypeDetectionTypesId",
                        column: x => x.DetectionTypeDetectionTypesId,
                        principalTable: "DetectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetectionTypeMetricType_MetricTypes_MetricTypeMetricsId",
                        column: x => x.MetricTypeMetricsId,
                        principalTable: "MetricTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteSignals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    SignalId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSignals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RouteSignals_Routes_RouteId",
                        column: x => x.RouteId,
                        principalTable: "Routes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Route Signals");

            migrationBuilder.CreateTable(
                name: "Signals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SignalId = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    Latitude = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Longitude = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    PrimaryName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false, defaultValueSql: "('')"),
                    SecondaryName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    Ipaddress = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false, defaultValueSql: "('127.0.0.1')"),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    ControllerTypeId = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    VersionActionId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((10))"),
                    Note = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false, defaultValueSql: "('Initial')"),
                    Start = table.Column<DateTime>(type: "datetime", nullable: false),
                    JurisdictionId = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((0))"),
                    Pedsare1to1 = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signals_ControllerTypes_ControllerTypeId",
                        column: x => x.ControllerTypeId,
                        principalTable: "ControllerTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signals_Jurisdictions_JurisdictionId",
                        column: x => x.JurisdictionId,
                        principalTable: "Jurisdictions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signals_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signals_VersionActions_VersionActionId",
                        column: x => x.VersionActionId,
                        principalTable: "VersionActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Signals");

            migrationBuilder.CreateTable(
                name: "ActionActionLog",
                columns: table => new
                {
                    ActionLogsId = table.Column<int>(type: "int", nullable: false),
                    ActionsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionActionLog", x => new { x.ActionLogsId, x.ActionsId });
                    table.ForeignKey(
                        name: "FK_ActionActionLog_ActionLogs_ActionLogsId",
                        column: x => x.ActionLogsId,
                        principalTable: "ActionLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionActionLog_Actions_ActionsId",
                        column: x => x.ActionsId,
                        principalTable: "Actions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionLogMetricType",
                columns: table => new
                {
                    ActionLogActionLogsId = table.Column<int>(type: "int", nullable: false),
                    MetricTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionLogMetricType", x => new { x.ActionLogActionLogsId, x.MetricTypesId });
                    table.ForeignKey(
                        name: "FK_ActionLogMetricType_ActionLogs_ActionLogActionLogsId",
                        column: x => x.ActionLogActionLogsId,
                        principalTable: "ActionLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionLogMetricType_MetricTypes_MetricTypesId",
                        column: x => x.MetricTypesId,
                        principalTable: "MetricTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoutePhaseDirections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteSignalId = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    DirectionTypeId = table.Column<int>(type: "int", nullable: false),
                    IsOverlap = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryApproach = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutePhaseDirections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoutePhaseDirections_DirectionTypes_DirectionTypeId",
                        column: x => x.DirectionTypeId,
                        principalTable: "DirectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoutePhaseDirections_RouteSignals_RouteSignalId",
                        column: x => x.RouteSignalId,
                        principalTable: "RouteSignals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Route Phase Directions");

            migrationBuilder.CreateTable(
                name: "Approaches",
                columns: table => new
                {
                    ApproachId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SignalId = table.Column<int>(type: "int", nullable: false),
                    DirectionTypeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    Mph = table.Column<int>(type: "int", nullable: true),
                    ProtectedPhaseNumber = table.Column<int>(type: "int", nullable: false),
                    IsProtectedPhaseOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PermissivePhaseNumber = table.Column<int>(type: "int", nullable: true),
                    IsPermissivePhaseOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PedestrianPhaseNumber = table.Column<int>(type: "int", nullable: true),
                    IsPedestrianPhaseOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PedestrianDetectors = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approaches", x => x.ApproachId);
                    table.ForeignKey(
                        name: "FK_Approaches_DirectionTypes_DirectionTypeId",
                        column: x => x.DirectionTypeId,
                        principalTable: "DirectionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Approaches_Signals_SignalId",
                        column: x => x.SignalId,
                        principalTable: "Signals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Approaches");

            migrationBuilder.CreateTable(
                name: "AreaSignal",
                columns: table => new
                {
                    AreasId = table.Column<int>(type: "int", nullable: false),
                    SignalsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaSignal", x => new { x.AreasId, x.SignalsId });
                    table.ForeignKey(
                        name: "FK_AreaSignal_Areas_AreasId",
                        column: x => x.AreasId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AreaSignal_Signals_SignalsId",
                        column: x => x.SignalsId,
                        principalTable: "Signals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetricComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    CommentText = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    SignalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MetricComments_Signals_SignalId",
                        column: x => x.SignalId,
                        principalTable: "Signals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Metric Comments");

            migrationBuilder.CreateTable(
                name: "Detectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetectorId = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    DetChannel = table.Column<int>(type: "int", nullable: false),
                    DistanceFromStopBar = table.Column<int>(type: "int", nullable: true),
                    MinSpeedFilter = table.Column<int>(type: "int", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false),
                    DateDisabled = table.Column<DateTime>(type: "datetime", nullable: true),
                    LaneNumber = table.Column<int>(type: "int", nullable: true),
                    MovementTypeId = table.Column<int>(type: "int", nullable: false),
                    LaneTypeId = table.Column<int>(type: "int", nullable: false),
                    DecisionPoint = table.Column<int>(type: "int", nullable: true),
                    MovementDelay = table.Column<int>(type: "int", nullable: true),
                    ApproachId = table.Column<int>(type: "int", nullable: false),
                    DetectionHardwareId = table.Column<int>(type: "int", nullable: false),
                    LatencyCorrection = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Detectors_Approaches_ApproachId",
                        column: x => x.ApproachId,
                        principalTable: "Approaches",
                        principalColumn: "ApproachId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Detectors_DetectionHardwares_DetectionHardwareId",
                        column: x => x.DetectionHardwareId,
                        principalTable: "DetectionHardwares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Detectors_LaneTypes_LaneTypeId",
                        column: x => x.LaneTypeId,
                        principalTable: "LaneTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Detectors_MovementTypes_MovementTypeId",
                        column: x => x.MovementTypeId,
                        principalTable: "MovementTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Detectors");

            migrationBuilder.CreateTable(
                name: "MetricCommentMetricType",
                columns: table => new
                {
                    MetricCommentsId = table.Column<int>(type: "int", nullable: false),
                    MetricTypesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricCommentMetricType", x => new { x.MetricCommentsId, x.MetricTypesId });
                    table.ForeignKey(
                        name: "FK_MetricCommentMetricType_MetricComments_MetricCommentsId",
                        column: x => x.MetricCommentsId,
                        principalTable: "MetricComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MetricCommentMetricType_MetricTypes_MetricTypesId",
                        column: x => x.MetricTypesId,
                        principalTable: "MetricTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    DetectorId = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    CommentText = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true)
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
                table: "Actions",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { 0, "Unknown" },
                    { 1, "Actuated Coord." },
                    { 2, "Coord On/Off" },
                    { 3, "Cycle Length" },
                    { 4, "Detector Issue" },
                    { 5, "Offset" },
                    { 6, "Sequence" },
                    { 7, "Time Of Day" },
                    { 8, "Other" },
                    { 9, "All-Red Interval" },
                    { 10, "Modeling" },
                    { 11, "Traffic Study" },
                    { 12, "Yellow Interval" },
                    { 13, "Force Off Type" },
                    { 14, "Split Adjustment" },
                    { 15, "Manual Command" }
                });

            migrationBuilder.InsertData(
                table: "Agencies",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { 0, "Unknown" },
                    { 1, "Academics" },
                    { 2, "City Government" },
                    { 3, "Consultant" },
                    { 4, "County Government" },
                    { 5, "Federal Government" },
                    { 6, "MPO" },
                    { 7, "State Government" },
                    { 8, "Other" }
                });

            migrationBuilder.InsertData(
                table: "Applications",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 0, "NA" },
                    { 1, "ATSPM" },
                    { 2, "SPMWatchDog" },
                    { 3, "DatabaseArchive" },
                    { 4, "GeneralSetting" }
                });

            migrationBuilder.InsertData(
                table: "Areas",
                columns: new[] { "Id", "Name" },
                values: new object[] { 0, "Unknown" });

            migrationBuilder.InsertData(
                table: "ControllerTypes",
                columns: new[] { "Id", "ActiveFtp", "Description", "Ftpdirectory", "Password", "Snmpport", "UserName" },
                values: new object[,]
                {
                    { 0, false, "Unknown", "root", "password", 161L, "user" },
                    { 1, true, "ASC3", "//Set1", "ecpi2ecpi", 161L, "econolite" },
                    { 2, true, "Cobalt", "/set1", "ecpi2ecpi", 161L, "econolite" },
                    { 3, true, "ASC3 - 2070", "/set1", "ecpi2ecpi", 161L, "econolite" },
                    { 4, false, "MaxTime", "none", "none", 161L, "none" },
                    { 5, true, "Trafficware", "none", "none", 161L, "none" },
                    { 6, false, "Siemens SEPAC", "/mnt/sd", "$adm*kon2", 161L, "admin" },
                    { 7, false, "McCain ATC EX", " /mnt/rd/hiResData", "root", 161L, "root" },
                    { 8, false, "Peek", "mnt/sram/cuLogging", "PeekAtc", 161L, "atc" },
                    { 9, true, "EOS", "/set1", "ecpi2ecpi", 161L, "econolite" }
                });

            migrationBuilder.InsertData(
                table: "DetectionHardwares",
                columns: new[] { "Id", "Name" },
                values: new object[] { 0, "NA" });

            migrationBuilder.InsertData(
                table: "DetectionHardwares",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "WavetronixMatrix" },
                    { 2, "WavetronixAdvance" },
                    { 3, "InductiveLoops" },
                    { 4, "Sensys" },
                    { 5, "Video" },
                    { 6, "FLIRThermalCamera" }
                });

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
                    { 6, 6, "Alabama ATSPM", "http://signalmetrics.ua.edu" },
                    { 7, 7, "ATSPM Workshop 2016 SLC", "http://docs.lib.purdue.edu/atspmw/2016" },
                    { 8, 8, "Train The Trainer Webinar Day 1 - Morning", "https://connectdot.connectsolutions.com/p75dwqefphk   " },
                    { 9, 9, "Train The Trainer Webinar Day 1 - Afternoon", "https://connectdot.connectsolutions.com/p6l6jaoy3gj" },
                    { 10, 10, "Train The Trainer Webinar Day 2 - Morning", "https://connectdot.connectsolutions.com/p6mlkvekogo/" },
                    { 11, 11, "Train The Trainer Webinar Day 2 - Mid Morning", "https://connectdot.connectsolutions.com/p3ua8gtj09r/" }
                });

            migrationBuilder.InsertData(
                table: "Faqs",
                columns: new[] { "Id", "Body", "Header", "OrderNumber" },
                values: new object[,]
                {
                    { 1, "<b>There are two ways to navigate the UDOT Automated Traffic Signal Performance Measures website</b><br/><br/><u>MAP</u><ol><li>Zoom in on the map and click on the desired intersection (note: the map can be filtered by selecting “metric type” ).</li><li>Select the available chart on the map from the list of available measures for the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol><u>SIGNAL LIST</u><ol><li>Select the chart by clicking the checkbox for the desired chart.</li><li>Click the “Signal List” bar at the top of the map window.</li><li>Click “Select” next to the desired intersection.</li><li>Click a day and/or time range from the calendar.</li><li>Click “Create Chart”.  Wait, and then scroll down to see the data and charts.</li></ol>", "<b>How do I navigate the UDOT Automated Traffic Signal Performance Measures website?</b>", 1 },
                    { 2, "Automated Traffic Signal Performance Measures show real-time and a history of performance at signalized intersections.  The various measures will evaluate the quality of progression of traffic along the corridor, and displays any unused green time that may be available from various movements. This information informs UDOT of vehicle and pedestrian detector malfunctions, measures vehicle delay and lets us know volumes, speeds and travel time of vehicles.   The measures are used to optimize mobility and manage traffic signal timing and maintenance to reduce congestion, save fuel costs and improve safety.  There are several measures currently in use with others in development. ", "<b>What are Automated Traffic Signal Performance Measures</b>", 2 },
                    { 3, "The traffic signal controller manufactures (Econolite, Intelight, Siemens, McCain, TrafficWare and some others) wrote a “data-logger” program that runs in the background of the traffic signal controller firmware. The Indiana Traffic Signal Hi Resolution Data Logger Enumerations (http://docs.lib.purdue.edu/cgi/viewcontent.cgi?article=1002&context=jtrpdata) encode events to a resolution to the nearest 100 milliseconds.  The recorded enumerations will have events for “phase begin green”, “phase gap out”, “phase max out”, “phase begin yellow clearance”, “phase end yellow clearance”, “pedestrian begin walk”, “pedestrian begin clearance”, “detector off”, “detector on”, etc.  For each event, a time-stamp is given and the event is stored temporarily in the signal controller.  Over 125 various enumerations are currently in use.  Then, using an FTP connection from a remote server to the traffic signal controller, packets of the hi resolution data logger enumerations (with its 1/10th second resolution time-stamp) are retrieved and stored on a web server at the UDOT Traffic Operations Center about every 10 to 15 minutes (unless the “upload current data” checkbox is enabled, where an FTP connection will be immediately made and the data will be displayed in real-time).  Software was written in-house by UDOT that allows us to graph and display the various data-logger enumerations and to show the results on the UDOT Automated Traffic Signal Performance Measures website.", "<b>How do Automated Traffic Signal Performance Measures work?</b>", 3 },
                    { 4, "A central traffic management system is not used or needed for the UDOT Automated Traffic Signal Performance Measures.  It is all being done through FTP connections from a web server through the network directly to the traffic signal controller which currently has the Indiana Traffic Signal Hi Resolution Data Logger Enumerations running in the background of the controller firmware.  The UDOT Automated Traffic Signal Performance Measures are independent of any central traffic management system.\r\n", "<b>Which central traffic management system is used to get the Automated Traffic Signal Performance Measures</b>", 4 },
                    { 5, "In 2011, UDOT’s executive director assigned a Quality Improvement Team (QIT) to make recommendations that will result in UDOT providing “world-class traffic signal maintenance and operations”.  The QIT evaluated existing operations, national best practices, and NCHRP recommendations to better improve UDOT’s signal maintenance and operations practices.  One of the recommendations from the QIT was to “implement real-time monitoring of system health and quality of operations”.  The real-time Automated Signal Performance Measures allow us to greatly improve the quality of signal operations and to also know when equipment such as pedestrian detection or vehicle detection is not working properly.  We are simply able to do more with less and to manage traffic more effectively 24/7.  In addition, we are able to optimize intersections and corridors when they need to be re-optimized, instead of on a set schedule.", "<b>Why does Utah need Automated Traffic Signal Performance Measures?</b>", 5 },
                    { 6, "The UDOT Automated Traffic Signal Measures software was developed in-house at UDOT by the Department of Technology Services.  Purdue University and the Indiana Department of Transportation (INDOT) assisted in getting us started on this endeavor.", "<b>Where did you get the Automated Traffic Signal Performance Measure software?</b>", 6 },
                    { 7, "The Purdue coordination diagram concept was introduced in 2009 by Purdue University to visualize the temporal relationship between the coordinated phase indications and vehicle arrivals on a cycle-by-cycle basis.  The Indiana Traffic Signal HI Resolution Data Logger Enumerations (http://docs.lib.purdue.edu/cgi/viewcontent.cgi?article=1002&context=jtrpdata) was a joint transportation research program (updated in November 2012 but started earlier) that included people from Indiana DOT, Purdue University, Econolite, PEEK, and Siemens.  <br/><br/>After discussions with Dr. Darcy Bullock from Purdue University and INDOT’s James Sturdevant, UDOT started development of the UDOT Automated Signal Performance Measures website April of 2012.", "<b>How did the Automated Traffic Signal Performance Measures Begin?</b> ", 7 },
                    { 8, "UDOT’s goal is transparency and unrestricted access to all who have a desire for traffic signal data.  Our goal in optimizing mobility, improving safety, preserving infrastructure and strengthening the economy means that all who have a desire to use the data should have access to the data without restrictions.  This includes all of UDOT (various divisions and groups), consultants, academia, MPO’s, other jurisdictions, FHWA, the public, and others.  It is also UDOT’s goal to be the most transparent Department of Transportation in the country.  Having a website where real-time Automated Signal Performance Measures can be obtained without special software, passwords or restricted firewalls will help UDOT in achieving the goal of transparency, and allows everyone access to the data without any silos.", "<b>Why are there no passwords or firewalls to access the website and see the measures?</b>", 8 }
                });

            migrationBuilder.InsertData(
                table: "Faqs",
                columns: new[] { "Id", "Body", "Header", "OrderNumber" },
                values: new object[,]
                {
                    { 9, "There are many uses and benefits of the various measures.  Some of the key uses are:<br/><br/><u>Purdue Coordination Diagrams (PCD’s)</u> – Used to visualize the temporal relationship between the coordinated phase indications and vehicle arrivals on a cycle-by-cycle basis.  The PCD’s show the progression quality along the corridor and answer questions, such as “What percent of vehicles are arriving during the green?” or “What is the platoon ratio during various coordination patterns?” The PCD’s are instrumental in optimizing offsets, identifying oversaturated or under saturated splits for the coordinated phases, the effects of early return of green and coordinated phase actuations, impacts of queuing, adjacent signal synchronization, etc.<br/> <br/>In reading the PCD’s, between the green and yellow lines, the phase is green; between the yellow and red lines, the phase is yellow; and underneath the green line, the phase is red.  The long vertical red lines during the late night hours is showing the main street sitting in green as the side streets and left turns are being skipped.  The short vertical red lines show skipping of the side street / left turns or a late start of green for the side street or left turn.  AoG is the percent of vehicles arriving during the green phase.  GT is the percent of green split time for the phase and PR is the Platoon Ratio (Equation 15-4 from the 2000 Highway Capacity Manual).<br/><br/><u>Approach Volumes</u> – Counts the approach vehicle volumes as shown arriving upstream of the intersection about 350 ft – 400 ft.  The detection zones are in advance of the turning lanes, so the approach volumes don’t know if the vehicles are going straight through, turning left or right.  The accuracy of the approach volumes tends to undercount under heavy traffic and under multi-lane facilities.  Approach volumes are used in traffic models, as well as identifying directional splits in traffic. In addition, the measure is also used in evaluating the least disruptive time to allow lanes to be taken for maintenance and construction activities.<br/><br/><u/>Approach Speeds</u> – The speeds are obtained from the Wavetronix radar Advance Smartsensor.  As vehicles cross the 10-foot wide detector in advance of the intersection (350 ft – 400 ft upstream of the stop bar), the speed is captured, recorded, and time-stamped.  In graphing the results, a time filter is used that starts 15 seconds (user defined) after the initial green to the start of the yellow.  The time filter allows for free-flow speed conditions to be displayed that are independent of the traffic signal timings.  The approach speed measure is beneficial in knowing the approach speeds to use for modeling purposes – both for normal time-of-day coordination plans and for adverse weather or special event plans.  They are also beneficial in knowing when speed conditions degrade enough to warrant a change in time-of-day coordination plans to adverse weather or special event plans.  In addition, the speed data is used to set yellow and all-red intervals for signal timing, as well as for various speed studies.<br/><br/><u>Purdue Phase Termination Charts</u> – Shows how each phase terminates when it changes from green to red.  The measure will show if the termination occurred by a gapout, a maxout / forceoff, or skip.  A gapout means that not all of the programmed time was used.  A maxout occurs during fully actuated (free) operations, while forceoff’s occur during coordination.  Both a maxout and forceoff shows that all the programmed time was used. A skip means that the phase was not active and did not turn on.  In addition, the termination can be evaluated by consecutive occurrences in a approach.  For example, you can evaluate if three (range is between 1 and 5) gapouts or skips occurred in a approach.  This feature is helpful in areas where traffic volume fluctuations are high.  Also shown are the pedestrian activations for each phase.  What this measure does not show is the amount of gapout time remaining if a gapout occurred.  The Split Monitor measure is used to answer that question.<br/><br/>This measure is used to identify movements where split time may need to be taken from some phases and given to other phases.  Also, this measure is very useful in identifying problems with vehicle and pedestrian detection.  For example, if the phase is showing constant maxouts all night long, it is assumed that there is a detection problem.<br/><br/><u>Split Monitor</u> – This measure shows the amount of split time (green, yellow and all-red) used by the various phases at the intersection.  Greens show gapouts, reds show maxouts, blues show forceoffs and yellows show pedestrian activations.  This measure is useful to know the amount of split time each phase uses.Turning Movement Volume Counts – this measure shows the lane-by-lane vehicles per hour (vph) and total volume for each movement.  Three graphs are available for each approach (left, thru, right).  Also shown for each movement are the total volume, peak hour, peak hour factor and lane utilization factor.  The lane-by-lane volume counts are used for traffic models and traffic studies.<br/><br/><u>Approach Delay</u> – This measure shows a simplified approach delay by displaying the time between detector activations during the red phase and when the phase turns green for the coordinated movements.  This measure does not account for start-up delay, deceleration, or queue length that exceeds the detection zone.  This measure is beneficial in evaluating over time the delay per vehicle and delay per hour values for each coordinated approach.<br/><br/><u>Arrivals on Red</u> – This measure shows the percent of vehicles arriving on red (inverse of % vehicles arriving on green) and the percent red time for each coordination pattern.  The Y axis is graphing the volume (vph) and the secondary Y axis graphs the percent vehicles arriving on red.  This measure is useful in identifying areas where the progression quality is poor.<br/><br/><u>Yellow and Red Actuations</u> – This measure plots vehicle arrivals during the yellow and red portions of an intersection's movements where the speed of the vehicle is interpreted to be too fast to stop before entering the intersection. It provides users with a visual indication of occurrences, violations, and several related statistics. The purpose of this chart is to identify engineering countermeasures to deal with red light running.<br/><br/><u>Purdue Split Failure</u> – This measure calculates the percent of time that stop bar detectors are occupied during the green phase and then during the first five seconds of red. This measure is a good indication that at least one vehicle did not clear during the green.", "<b>How do you use the various Signal Performance Measures and what do they do?</b> ", 9 },
                    { 10, "The Automated Signal Performance Measures are an effective way to reduce congestion, save fuel costs and improve safety.  We are simply able to do more with less and are more effectively able to manage traffic every day of the week and at all times of the day, even when a traffic signal engineer is not available.  We have identified several detection problems, corrected time-of-day coordination errors in the traffic signal controller scheduler, corrected offsets, splits, among other things.  In addition, we have been able to use more accurate data in optimizing models and doing traffic studies, and have been able to more correctly set various signal timing parameters.", "<b>How effective are Automated Traffic Signal Performance Measures</b>", 10 },
                    { 11, "Although the UDOTAutomated Traffic Signal Performance Measures cannot guarantee you will only get green lights, the system does help make traveling through Utah more efficient.  UDOT Automatic Signal Performance Measures have already already helped to reduce the number of stops and delay at signalized intersections.  Continued benefits are anticipated.", "<b>Does this mean I never have to stop at a red light?</b>", 11 },
                    { 12, "Yes, UDOT Automated Traffic Signal Performance Measures has already saved Utahans time and money.  By increasing corridor speeds while reducing intersection delays, traffic signal stops, and the ability to monitor operations 24/7.", "<b>Will Automated Traffic Signal Performance Measures save me money?  If so, how are cost savings measured?</b>", 12 },
                    { 13, "By reducing congestion and reducing the percent of vehicles arriving on a red light, UDOT Automated Traffic Signal Performance Measures helps decrease the number of accidents that occur.  In addition, we are better able to manage detector failures and improve the duration of the change intervals and clearance times at intersections.", "<b>How do Automated Traffic Signal Performance Measures enhance safety?</b>", 13 },
                    { 14, "UDOT Automated Traffic Signal Performance Measures are designed to increase the safety and efficiency at signalized intersections.  It is not intended to identify speeders or enforce traffic laws.  No personal information is recorded or used in data gathering.", "<b>Can real-time Automated Traffic Signal Performance Measures be used as a law enforcement tool?</b>", 14 },
                    { 15, "We can estimate that each signal controller high resolution data requires approximately 19 MB of storage space each day.  For the UDOT system, we have approximately 2040 traffic signals bringing back about 1 TB of data per month. In addition to the high resolution data, version 4.2.0 and above also allows for the data to be rolled up into aggregated tables in 15-minute bins. UDOT averages approximately 6 GB of aggregated tables per month. UDOT uses a SAN server that holds approximately 40 TB that runs SQL 2016. Our goal is to keep between 24 months and 36 months of high resolution data files and then to cold storage the old high resolution  data files for up to five years after that. The cold storage flat files (comma deliminated file with no indexing) will require about 2 TB of storage per year. UDOT plans on keeping the aggregated tables in 15-minute bins indefinitely. ", "<b>Server and Data Storage Requirements</b>", 15 },
                    { 16, "The data has been useful for some of the following users in Utah:<br/><br/><ul><li><u>Signal engineers</u> in optimize and fine-tuning signal timing.</li><li><u>Maintenance signal technicians</u> in identifying broken detector problems and investigating trouble calls.</li><li><u>Traffic engineers</u> in conducting various traffic studies, such as speed studies, turning movement studies, modeling studies, and optimizing the intersection operations.</li><li><u>Consultants</u> in improving traffic signal operations, as UDOT outsources some of the signal operations, design and planning to consultants.</li><li><u>UDOT Traffic & Safety, UDOT Traffic Engineers, UDOT Resident Construction Engineers</u> in conducting various traffic studies and/or in determining the time-of-day where construction or maintenance activities would be least disruptive to the traveling motorists.</li><li><u>Metropolitan Planning Organizations</u> (MPO’s) in calibrating the regional traffic models.</li><li><u>Academia</u> in conducting various research studies, such as evaluating the effectiveness of operations during adverse weather, evaluating the optimum signal timing for innovative intersections such as DDI’s, CFI’s and Thru-Turns, etc.</li><li><u>City and County</u> Government in using the data in similar manner to UDOT.</li></ul>", "<b>Who uses the Automated Traffic Signal Performance Measures data?</b>", 16 },
                    { 17, "<table class='table table-bordered'>\r\n 	                            <tr>\r\n                                    <th> MEASURE </th>\r\n                                    <th> DETECTION NEEDED </th>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Coordination Diagram </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Volume </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Speed </td>\r\n                                    <td> Setback count (350 ft – 400 ft) using radar </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Phase Termination </td>\r\n                                    <td> No detection needed or used </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Split Monitor </td>\r\n                                    <td> No detection needed or used </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Turning Movement Counts </td>\r\n                                    <td> Stop bar (lane-by-lane) count </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Approach Delay </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Arrivals on Red </td>\r\n                                    <td> Setback count (350 ft – 400 ft) </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Yellow and Red Actuations </td>\r\n                                    <td> Stop bar (lane-by-lane) count that is either in front of the stop bar or has a speed filter enabled </td>\r\n                                </tr>\r\n                                <tr>\r\n                                    <td> Purdue Split Failure </td>\r\n                                    <td> Stop bar presence detection, either by lane group or individual lane </td>\r\n                                </tr>\r\n                        </table>\r\n                        <b> Automated Traffic Signal Performance Measures will work with any type of detector that is capable of counting vehicles, e.g., loops, video, pucks, radar. (The only exception to this is the speed measure, where UDOT’s Automated Signal Performance Measures for speeds will only work with the Wavetronix Advance SmartSensor.) Please note that two of the measures (Purdue Phase Termination and Split Monitor) do not use detection and are extremely useful measures.</b>", "<b>What are the detection requirements for each metric?</b> ", 17 },
                    { 18, "Some measures have different detection requirements than other measures. For example, for approach speeds, UDOT uses the Wavetronix Advance Smartsensor radar detector and has been using this detector since 2006 for dilemma zone purposes if the design speed is 40 mph or higher.  This same detector is what we use for our setback counts 350 feet – 400 feet upstream of the intersection.  In addition, we are also able to pull the raw radar speeds from the sensor back to the TOC server for the speed data.  Not all intersections have the Wavetronix Advance Smartsensors, therefore we are not able to display speed data, as well as the PCD’s, approach volume, arrivals on red or approach delay at each intersection.<br/><br/>The turning movement count measure requires lane-by-lane detection capable of counting vehicles in each lane.  Configuring the detection for lane-by-lane counts is time consuming and takes a commitment to financial resources.", "<b>Why do some intersections only show a few metrics and others have more?</b>", 18 },
                    { 19, " <b> System Requirements:</b>\r\n                        <b> Operating Systems and Software:</b>\r\n                        The UDOT Automated Signal Performance Measures system runs on Microsoft Windows Servers.\r\n                        The web components are hosted by Microsoft Internet Information Server(IIS).\r\n                        The database server is a Microsoft SQL 2016 server.\r\n                        <b> Storage and Processing:</b>\r\n                        Detector data uses about 40 % of the storage space of the UDOT system,\r\n                        so the number of detectors attached to a controller will have a huge impact on the amount of storage space required.Detector data is also the most important information we collect.\r\n                        We estimate that each signal will generate 19 MB of data per day.\r\n                        The amount of processing power required is highly dependant on how many signals are on the system,\r\n                        how many servers will be part of the system,\r\n                        and how many people will be using the system.  It is possible to host all of the system functions on one powerful server, or split them out into multiple, less expensive servers.  If your agency decided to make the Automated Signal Performance Measures available to the public, it might be best to have a web server separate from the database server.Much of the heavy processing for the charts is done by web services, and it is possible to host these services on a dedicated computer.\r\n                        While each agency should consult with their IT department for specific guidelines on how to best deliver a secure, stable and responsive solution, we can estimate that most mid-range to high-end servers will be able to handle the task of hosting and creating measures for most agencies.<ul>\r\n                        <li>Windows Server 2008 or newer installed</li>\r\n                        <li>.NET 4.5.2 Framework installed</li>\r\n                        <li>IIS 7 or better installed, along with ASP.NET 4.0 or later</li>\r\n                        <li>SQL Server Express, SQL Server 2008 R2, or newer installed</li>\r\n                        <li>Firewall exceptions for connections to the controllers</li>\r\n                        <li>If Watchdog features are desired, installation requires access to an SMTP (email) server. It will accept email from the Automated Signal Performance Measures (ATSPM) server. The SMTP server can reside on the same machine.</li>\r\n                        <li>Microsoft Visual Studio 2013 or later is recommended</li></ul>", "<b>What are the System Requirements?</b>", 19 },
                    { 20, "You can contact UDOT’s Traffic Signal Operations Engineer, Mark Taylor at marktaylor@utah.gov or phone at 801-887-3714 to find out more information about Automated Signal Performance Measures.", "<b>Who do I contact to find out more information about Automated Traffic Signal Performance Measures</b> ", 20 },
                    { 21, "You can download the source code at GitHub at: https://github.com/udotdevelopment/ATSPM. GitHub is more for development and those interested in further developing and modifying the code. We encourage developers to contribute the enhancements back to GitHub so others can benefit as well.  For those interested in the executable ATSPM files, those are found on the FHWA's open source portal at: https://www.itsforge.net/index.php/community/explore-applications#/30.", "<b>How do I get the source code for the Automated Traffic Signal Performance Measures Website?</b> ", 21 }
                });

            migrationBuilder.InsertData(
                table: "Jurisdictions",
                columns: new[] { "Id", "CountyParish", "Mpo", "Name", "OtherPartners" },
                values: new object[] { 0, "Unknown", null, "Unknown", "Unknown" });

            migrationBuilder.InsertData(
                table: "LaneTypes",
                columns: new[] { "Id", "Abbreviation", "Description" },
                values: new object[,]
                {
                    { 0, "NA", "Unknown" },
                    { 1, "V", "Vehicle" },
                    { 2, "Bike", "Bike" },
                    { 3, "Ped", "Pedestrian" },
                    { 4, "E", "Exit" },
                    { 5, "LRT", "Light Rail Transit" },
                    { 6, "Bus", "Bus" },
                    { 7, "HDV", "High Occupancy Vehicle" }
                });

            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Action", "Application", "Controller", "DisplayOrder", "Name", "ParentId" },
                values: new object[,]
                {
                    { 1, "#", "SignalPerformanceMetrics", "#", 10, "Measures", 0 },
                    { 2, "#", "SignalPerformanceMetrics", "#", 20, "Reports", 0 },
                    { 3, "Create", "SignalPerformanceMetrics", "ActionLogs", 30, "Log Action Taken", 0 },
                    { 4, "#", "SignalPerformanceMetrics", "#", 40, "Links", 0 },
                    { 5, "Display", "SignalPerformanceMetrics", "FAQs", 50, "FAQ", 0 },
                    { 8, "Usage", "SignalPerformanceMetrics", "ActionLogs", 10, "Chart Usage", 2 },
                    { 9, "Index", "SignalPerformanceMetrics", "DefaultCharts", 10, "Signal", 1 },
                    { 10, "Analysis", "SignalPerformanceMetrics", "LinkPivot", 20, "Purdue Link Pivot", 1 },
                    { 11, "#", "SignalPerformanceMetrics", "#", 100, "Admin", 0 },
                    { 12, "Index", "SignalPerformanceMetrics", "Signals", 10, "Signal Configuration", 11 },
                    { 13, "Index", "SignalPerformanceMetrics", "Routes", 30, "Route Configuration", 11 },
                    { 15, "RoleAddToUser", "SignalPerformanceMetrics", "Account", 100, "Roles", 11 },
                    { 16, "Index", "SignalPerformanceMetrics", "Menus", 20, "Menu Configuration", 11 },
                    { 17, "Index", "SignalPerformanceMetrics", "Jurisdictions", 80, "Agency Configuration", 11 },
                    { 27, "About", "SignalPerformanceMetrics", "Home", 90, "About", 0 },
                    { 48, "Index", "SignalPerformanceMetrics", "AggregateDataExport", 20, "Aggregate Data", 2 },
                    { 49, "RawDataExport", "SignalPerformanceMetrics", "DataExport", 50, "Raw Data Export", 11 },
                    { 51, "Index", "SignalPerformanceMetrics", "SPMUsers", 90, "Users", 11 },
                    { 52, "Index", "SignalPerformanceMetrics", "FAQs", 70, "FAQs", 11 },
                    { 54, "Edit", "SignalPerformanceMetrics", "WatchDogApplicationSettings", 60, "Watch Dog", 11 }
                });

            migrationBuilder.InsertData(
                table: "Menus",
                columns: new[] { "Id", "Action", "Application", "Controller", "DisplayOrder", "Name", "ParentId" },
                values: new object[,]
                {
                    { 56, "edit", "SignalPerformanceMetrics", "DatabaseArchiveSettings", 70, "Database Archive Settings", 11 },
                    { 57, "Edit", "SignalPerformanceMetrics", "GeneralSettings", 40, "General Settings", 11 },
                    { 58, "Index", "SignalPerformanceMetrics", "LeftTurnGapReport", 25, "Left Turn Gap Analysis", 2 },
                    { 66, "Index", "SignalPerformanceMetrics", "Areas", 31, "Area Configuration", 11 },
                    { 71, "SignalDetail", "SignalPerformanceMetrics", "Signals", 15, "Configuration", 2 },
                    { 100, "Index", "SignalPerformanceMetrics", "MeasuresDefaults", 45, "Measure Defaults Settings", 11 }
                });

            migrationBuilder.InsertData(
                table: "MetricTypes",
                columns: new[] { "Id", "Abbreviation", "ChartName", "DisplayOrder", "ShowOnAggregationSite", "ShowOnWebsite" },
                values: new object[,]
                {
                    { 1, "PPT", "Purdue Phase Termination", 1, false, true },
                    { 2, "SM", "Split Monitor", 5, false, true },
                    { 3, "PedD", "Pedestrian Delay", 10, false, true },
                    { 4, "PD", "Preemption Details", 15, false, true },
                    { 5, "TMC", "Turning Movement Counts", 40, false, true },
                    { 6, "PCD", "Purdue Coordination Diagram", 60, false, true },
                    { 7, "AV", "Approach Volume", 45, false, true },
                    { 8, "AD", "Approach Delay", 50, false, true },
                    { 9, "AoR", "Arrivals On Red", 55, false, true },
                    { 10, "Speed", "Approach Speed", 65, false, true },
                    { 11, "YRA", "Yellow and Red Actuations", 35, false, true },
                    { 12, "SF", "Purdue Split Failure", 30, false, true },
                    { 13, "LP", "Purdue Link Pivot", 70, false, false },
                    { 14, "PSR", "Preempt Service Request", 80, false, false },
                    { 15, "PS", "Preempt Service", 75, false, false },
                    { 16, "DVA", "Detector Activation Count", 85, true, false },
                    { 17, "TAA", "Timing And Actuation", 20, false, true },
                    { 18, "APCD", "Approach Pcd", 102, true, false },
                    { 19, "CA", "Approach Cycle", 103, true, false },
                    { 20, "SFA", "Approach Split Fail", 104, true, false },
                    { 22, "PreemptA", "Signal Preemption", 105, true, false },
                    { 24, "TSPA", "Signal Priority", 106, true, false },
                    { 25, "ASA", "Approach Speed", 107, true, false },
                    { 26, "YRAA", "Approach Yellow Red Activations", 108, true, false },
                    { 27, "SEC", "Signal Event Count", 109, true, false },
                    { 28, "AEC", "Approach Event Count", 110, true, false },
                    { 29, "AEC", "Phase Termination", 111, true, false },
                    { 30, "APD", "Phase Pedestrian Delay", 112, true, false },
                    { 31, "LTGA", "Left Turn Gap Analysis", 112, false, true },
                    { 32, "WT", "Wait Time", 113, false, true },
                    { 33, "GVD", "Gap Vs Demand", 115, false, false },
                    { 34, "LTG", "Left Turn Gap", 114, true, false },
                    { 35, "SM", "Split Monitor", 120, true, false }
                });

            migrationBuilder.InsertData(
                table: "MovementTypes",
                columns: new[] { "Id", "Abbreviation", "Description", "DisplayOrder" },
                values: new object[,]
                {
                    { 0, "NA", "Unknown", 6 },
                    { 1, "T", "Thru", 3 },
                    { 2, "R", "Right", 5 }
                });

            migrationBuilder.InsertData(
                table: "MovementTypes",
                columns: new[] { "Id", "Abbreviation", "Description", "DisplayOrder" },
                values: new object[,]
                {
                    { 3, "L", "Left", 1 },
                    { 4, "TR", "Thru-Right", 4 },
                    { 5, "TL", "Thru-Left", 2 },
                    { 6, "NW", "Northwest", 6 }
                });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "Description" },
                values: new object[] { 0, "Unknown" });

            migrationBuilder.InsertData(
                table: "VersionActions",
                columns: new[] { "Id", "Description" },
                values: new object[,]
                {
                    { 0, "Unknown" },
                    { 1, "New" },
                    { 2, "Edit" },
                    { 3, "Delete" },
                    { 4, "NewVersion" },
                    { 10, "Initial" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionActionLog_ActionsId",
                table: "ActionActionLog",
                column: "ActionsId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogMetricType_MetricTypesId",
                table: "ActionLogMetricType",
                column: "MetricTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogs_AgencyId",
                table: "ActionLogs",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationSettings_ApplicationId",
                table: "ApplicationSettings",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Approaches_DirectionTypeId",
                table: "Approaches",
                column: "DirectionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Approaches_SignalId",
                table: "Approaches",
                column: "SignalId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaSignal_SignalsId",
                table: "AreaSignal",
                column: "SignalsId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectionTypeDetector_DetectorsId",
                table: "DetectionTypeDetector",
                column: "DetectorsId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectionTypeMetricType_MetricTypeMetricsId",
                table: "DetectionTypeMetricType",
                column: "MetricTypeMetricsId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectorComments_DetectorId",
                table: "DetectorComments",
                column: "DetectorId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectorComments_Id",
                table: "DetectorComments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Detectors_ApproachId",
                table: "Detectors",
                column: "ApproachId");

            migrationBuilder.CreateIndex(
                name: "IX_Detectors_DetectionHardwareId",
                table: "Detectors",
                column: "DetectionHardwareId");

            migrationBuilder.CreateIndex(
                name: "IX_Detectors_LaneTypeId",
                table: "Detectors",
                column: "LaneTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Detectors_MovementTypeId",
                table: "Detectors",
                column: "MovementTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricCommentMetricType_MetricTypesId",
                table: "MetricCommentMetricType",
                column: "MetricTypesId");

            migrationBuilder.CreateIndex(
                name: "IX_MetricComments_SignalId",
                table: "MetricComments",
                column: "SignalId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutePhaseDirections_DirectionTypeId",
                table: "RoutePhaseDirections",
                column: "DirectionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RoutePhaseDirections_RouteSignalId",
                table: "RoutePhaseDirections",
                column: "RouteSignalId");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSignals_RouteId",
                table: "RouteSignals",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_Signals_ControllerTypeId",
                table: "Signals",
                column: "ControllerTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Signals_JurisdictionId",
                table: "Signals",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_Signals_RegionId",
                table: "Signals",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Signals_VersionActionId",
                table: "Signals",
                column: "VersionActionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionActionLog");

            migrationBuilder.DropTable(
                name: "ActionLogMetricType");

            migrationBuilder.DropTable(
                name: "ApplicationSettings");

            migrationBuilder.DropTable(
                name: "AreaSignal");

            migrationBuilder.DropTable(
                name: "DetectionTypeDetector");

            migrationBuilder.DropTable(
                name: "DetectionTypeMetricType");

            migrationBuilder.DropTable(
                name: "DetectorComments");

            migrationBuilder.DropTable(
                name: "ExternalLinks");

            migrationBuilder.DropTable(
                name: "Faqs");

            migrationBuilder.DropTable(
                name: "MeasuresDefaults");

            migrationBuilder.DropTable(
                name: "Menus");

            migrationBuilder.DropTable(
                name: "MetricCommentMetricType");

            migrationBuilder.DropTable(
                name: "RoutePhaseDirections");

            migrationBuilder.DropTable(
                name: "Actions");

            migrationBuilder.DropTable(
                name: "ActionLogs");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Areas");

            migrationBuilder.DropTable(
                name: "DetectionTypes");

            migrationBuilder.DropTable(
                name: "Detectors");

            migrationBuilder.DropTable(
                name: "MetricComments");

            migrationBuilder.DropTable(
                name: "MetricTypes");

            migrationBuilder.DropTable(
                name: "RouteSignals");

            migrationBuilder.DropTable(
                name: "Agencies");

            migrationBuilder.DropTable(
                name: "Approaches");

            migrationBuilder.DropTable(
                name: "DetectionHardwares");

            migrationBuilder.DropTable(
                name: "LaneTypes");

            migrationBuilder.DropTable(
                name: "MovementTypes");

            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "DirectionTypes");

            migrationBuilder.DropTable(
                name: "Signals");

            migrationBuilder.DropTable(
                name: "ControllerTypes");

            migrationBuilder.DropTable(
                name: "Jurisdictions");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "VersionActions");
        }
    }
}
