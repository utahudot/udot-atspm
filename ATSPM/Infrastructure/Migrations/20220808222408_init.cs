using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrasturcture.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    ActionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.ActionID);
                });

            migrationBuilder.CreateTable(
                name: "Agencies",
                columns: table => new
                {
                    AgencyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agencies", x => x.AgencyID);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Comment",
                columns: table => new
                {
                    CommentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    Entity = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ChartType = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "varchar(max)", unicode: false, nullable: false),
                    EntityType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => x.CommentID);
                });

            migrationBuilder.CreateTable(
                name: "ControllerTypes",
                columns: table => new
                {
                    ControllerTypeID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    SNMPPort = table.Column<long>(type: "bigint", nullable: false),
                    FTPDirectory = table.Column<string>(type: "varchar(max)", unicode: false, nullable: true),
                    ActiveFTP = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ControllerTypes", x => x.ControllerTypeID);
                });

            migrationBuilder.CreateTable(
                name: "DatabaseArchiveExcludedSignals",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SignalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DatabaseArchiveExcludedSignals", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DetectionHardwares",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionHardwares", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DetectionTypes",
                columns: table => new
                {
                    DetectionTypeID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectionTypes", x => x.DetectionTypeID);
                });

            migrationBuilder.CreateTable(
                name: "DirectionTypes",
                columns: table => new
                {
                    DirectionTypeID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Abbreviation = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DirectionTypes", x => x.DirectionTypeID);
                });

            migrationBuilder.CreateTable(
                name: "ExternalLinks",
                columns: table => new
                {
                    ExternalLinkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLinks", x => x.ExternalLinkID);
                });

            migrationBuilder.CreateTable(
                name: "FAQs",
                columns: table => new
                {
                    FAQID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Header = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FAQs", x => x.FAQID);
                });

            migrationBuilder.CreateTable(
                name: "Jurisdictions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JurisdictionName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MPO = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CountyParish = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OtherPartners = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jurisdictions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "LaneTypes",
                columns: table => new
                {
                    LaneTypeID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LaneTypes", x => x.LaneTypeID);
                });

            migrationBuilder.CreateTable(
                name: "MeasuresDefaults",
                columns: table => new
                {
                    Measure = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    OptionName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.MeasuresDefaults", x => new { x.Measure, x.OptionName });
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    MenuID = table.Column<int>(type: "int", nullable: false),
                    MenuName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    Application = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    Controller = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValueSql: "('')"),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValueSql: "('')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.MenuID);
                });

            migrationBuilder.CreateTable(
                name: "MetricComments",
                columns: table => new
                {
                    CommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SignalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TimeStamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.MetricComments", x => x.CommentID);
                });

            migrationBuilder.CreateTable(
                name: "MetricsFilterTypes",
                columns: table => new
                {
                    FilterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilterName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.MetricsFilterTypes", x => x.FilterID);
                });

            migrationBuilder.CreateTable(
                name: "MetricTypes",
                columns: table => new
                {
                    MetricID = table.Column<int>(type: "int", nullable: false),
                    ChartName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShowOnWebsite = table.Column<bool>(type: "bit", nullable: false),
                    ShowOnAggregationSite = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.MetricTypes", x => x.MetricID);
                });

            migrationBuilder.CreateTable(
                name: "MovementTypes",
                columns: table => new
                {
                    MovementTypeID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Abbreviation = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementTypes", x => x.MovementTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Region",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SignalToAggregates",
                columns: table => new
                {
                    SignalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.SignalToAggregates", x => x.SignalID);
                });

            migrationBuilder.CreateTable(
                name: "SPMWatchDogErrorEvents",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeStamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DetectorID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Direction = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SPMWatchDogErrorEvents", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "VersionActions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionActions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ActionLogs",
                columns: table => new
                {
                    ActionLogID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    AgencyID = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SignalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionLogs", x => x.ActionLogID);
                    table.ForeignKey(
                        name: "FK_dbo.ActionLogs_dbo.Agencies_AgencyID",
                        column: x => x.AgencyID,
                        principalTable: "Agencies",
                        principalColumn: "AgencyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationSettings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationID = table.Column<int>(type: "int", nullable: false),
                    ConsecutiveCount = table.Column<int>(type: "int", nullable: true),
                    MinPhaseTerminations = table.Column<int>(type: "int", nullable: true),
                    PercentThreshold = table.Column<double>(type: "float", nullable: true),
                    MaxDegreeOfParallelism = table.Column<int>(type: "int", nullable: true),
                    ScanDayStartHour = table.Column<int>(type: "int", nullable: true),
                    ScanDayEndHour = table.Column<int>(type: "int", nullable: true),
                    PreviousDayPMPeakStart = table.Column<int>(type: "int", nullable: true),
                    PreviousDayPMPeakEnd = table.Column<int>(type: "int", nullable: true),
                    MinimumRecords = table.Column<int>(type: "int", nullable: true),
                    WeekdayOnly = table.Column<bool>(type: "bit", nullable: true),
                    DefaultEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LowHitThreshold = table.Column<int>(type: "int", nullable: true),
                    EmailServer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaximumPedestrianEvents = table.Column<int>(type: "int", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ArchivePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelectedDeleteOrMove = table.Column<int>(type: "int", nullable: true),
                    NumberOfRows = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<int>(type: "int", nullable: true),
                    TimeDuration = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawDataCountLimit = table.Column<int>(type: "int", nullable: true),
                    ReCaptchaPublicKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReCaptchaSecretKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnableDatbaseArchive = table.Column<bool>(type: "bit", nullable: true),
                    SelectedTableScheme = table.Column<int>(type: "int", nullable: true),
                    MonthsToKeepIndex = table.Column<int>(type: "int", nullable: true),
                    MonthsToKeepData = table.Column<int>(type: "int", nullable: true),
                    EmailAllErrors = table.Column<bool>(type: "bit", nullable: true),
                    CycleCompletionSeconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationSettings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_dbo.ApplicationSettings_dbo.Applications_ApplicationID",
                        column: x => x.ApplicationID,
                        principalTable: "Applications",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Approaches",
                columns: table => new
                {
                    ApproachID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SignalID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DirectionTypeID = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MPH = table.Column<int>(type: "int", nullable: true),
                    ProtectedPhaseNumber = table.Column<int>(type: "int", nullable: false),
                    IsProtectedPhaseOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PermissivePhaseNumber = table.Column<int>(type: "int", nullable: true),
                    VersionID = table.Column<int>(type: "int", nullable: false),
                    IsPermissivePhaseOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PedestrianPhaseNumber = table.Column<int>(type: "int", nullable: true),
                    IsPedestrianPhaseOverlap = table.Column<bool>(type: "bit", nullable: false),
                    PedestrianDetectors = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approaches", x => x.ApproachID);
                    table.ForeignKey(
                        name: "FK_dbo.Approaches_dbo.DirectionTypes_DirectionTypeID",
                        column: x => x.DirectionTypeID,
                        principalTable: "DirectionTypes",
                        principalColumn: "DirectionTypeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetectionTypeMetricTypes",
                columns: table => new
                {
                    DetectionType_DetectionTypeID = table.Column<int>(type: "int", nullable: false),
                    MetricType_MetricID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.DetectionTypeMetricTypes", x => new { x.DetectionType_DetectionTypeID, x.MetricType_MetricID });
                    table.ForeignKey(
                        name: "FK_dbo.DetectionTypeMetricTypes_dbo.DetectionTypes_DetectionType_DetectionTypeID",
                        column: x => x.DetectionType_DetectionTypeID,
                        principalTable: "DetectionTypes",
                        principalColumn: "DetectionTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.DetectionTypeMetricTypes_dbo.MetricTypes_MetricType_MetricID",
                        column: x => x.MetricType_MetricID,
                        principalTable: "MetricTypes",
                        principalColumn: "MetricID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetricCommentMetricTypes",
                columns: table => new
                {
                    MetricComment_CommentID = table.Column<int>(type: "int", nullable: false),
                    MetricType_MetricID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.MetricCommentMetricTypes", x => new { x.MetricComment_CommentID, x.MetricType_MetricID });
                    table.ForeignKey(
                        name: "FK_dbo.MetricCommentMetricTypes_dbo.MetricComments_MetricComment_CommentID",
                        column: x => x.MetricComment_CommentID,
                        principalTable: "MetricComments",
                        principalColumn: "CommentID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.MetricCommentMetricTypes_dbo.MetricTypes_MetricType_MetricID",
                        column: x => x.MetricType_MetricID,
                        principalTable: "MetricTypes",
                        principalColumn: "MetricID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RouteSignals",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteID = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    SignalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RouteSignals", x => x.ID);
                    table.ForeignKey(
                        name: "FK_dbo.RouteSignals_dbo.Routes_RouteID",
                        column: x => x.RouteID,
                        principalTable: "Routes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Signals",
                columns: table => new
                {
                    VersionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SignalID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Latitude = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    Longitude = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false),
                    PrimaryName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false, defaultValueSql: "('')"),
                    SecondaryName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false, defaultValueSql: "('')"),
                    IPAddress = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false, defaultValueSql: "('')"),
                    RegionID = table.Column<int>(type: "int", nullable: false),
                    ControllerTypeID = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    VersionActionID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((10))"),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValueSql: "('Initial')"),
                    Start = table.Column<DateTime>(type: "datetime", nullable: false),
                    JurisdictionID = table.Column<int>(type: "int", nullable: false, defaultValueSql: "((1))"),
                    Pedsare1to1 = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Signals", x => x.VersionID);
                    table.ForeignKey(
                        name: "FK_dbo.Signals_dbo.ControllerTypes_ControllerTypeID",
                        column: x => x.ControllerTypeID,
                        principalTable: "ControllerTypes",
                        principalColumn: "ControllerTypeID");
                    table.ForeignKey(
                        name: "FK_dbo.Signals_dbo.Jurisdictions_JurisdictionID",
                        column: x => x.JurisdictionID,
                        principalTable: "Jurisdictions",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_dbo.Signals_dbo.Region_RegionID",
                        column: x => x.RegionID,
                        principalTable: "Region",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Signals_VersionActions_VersionActionID",
                        column: x => x.VersionActionID,
                        principalTable: "VersionActions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionLogActions",
                columns: table => new
                {
                    ActionLog_ActionLogID = table.Column<int>(type: "int", nullable: false),
                    Action_ActionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ActionLogActions", x => new { x.ActionLog_ActionLogID, x.Action_ActionID });
                    table.ForeignKey(
                        name: "FK_dbo.ActionLogActions_dbo.ActionLogs_ActionLog_ActionLogID",
                        column: x => x.ActionLog_ActionLogID,
                        principalTable: "ActionLogs",
                        principalColumn: "ActionLogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.ActionLogActions_dbo.Actions_Action_ActionID",
                        column: x => x.Action_ActionID,
                        principalTable: "Actions",
                        principalColumn: "ActionID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActionLogMetricTypes",
                columns: table => new
                {
                    ActionLog_ActionLogID = table.Column<int>(type: "int", nullable: false),
                    MetricType_MetricID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ActionLogMetricTypes", x => new { x.ActionLog_ActionLogID, x.MetricType_MetricID });
                    table.ForeignKey(
                        name: "FK_dbo.ActionLogMetricTypes_dbo.ActionLogs_ActionLog_ActionLogID",
                        column: x => x.ActionLog_ActionLogID,
                        principalTable: "ActionLogs",
                        principalColumn: "ActionLogID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.ActionLogMetricTypes_dbo.MetricTypes_MetricType_MetricID",
                        column: x => x.MetricType_MetricID,
                        principalTable: "MetricTypes",
                        principalColumn: "MetricID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Detectors",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DetectorID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DetChannel = table.Column<int>(type: "int", nullable: false),
                    DistanceFromStopBar = table.Column<int>(type: "int", nullable: true),
                    MinSpeedFilter = table.Column<int>(type: "int", nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime", nullable: false),
                    DateDisabled = table.Column<DateTime>(type: "datetime", nullable: true),
                    LaneNumber = table.Column<int>(type: "int", nullable: true),
                    MovementTypeID = table.Column<int>(type: "int", nullable: true),
                    LaneTypeID = table.Column<int>(type: "int", nullable: true),
                    DecisionPoint = table.Column<int>(type: "int", nullable: true),
                    MovementDelay = table.Column<int>(type: "int", nullable: true),
                    ApproachID = table.Column<int>(type: "int", nullable: false),
                    DetectionHardwareID = table.Column<int>(type: "int", nullable: false),
                    LatencyCorrection = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detectors", x => x.ID);
                    table.ForeignKey(
                        name: "FK_dbo.Detectors_dbo.Approaches_ApproachID",
                        column: x => x.ApproachID,
                        principalTable: "Approaches",
                        principalColumn: "ApproachID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.Detectors_dbo.DetectionHardwares_DetectionHardwareID",
                        column: x => x.DetectionHardwareID,
                        principalTable: "DetectionHardwares",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.Detectors_dbo.LaneTypes_LaneTypeID",
                        column: x => x.LaneTypeID,
                        principalTable: "LaneTypes",
                        principalColumn: "LaneTypeID");
                    table.ForeignKey(
                        name: "FK_dbo.Detectors_dbo.MovementTypes_MovementTypeID",
                        column: x => x.MovementTypeID,
                        principalTable: "MovementTypes",
                        principalColumn: "MovementTypeID");
                });

            migrationBuilder.CreateTable(
                name: "RoutePhaseDirections",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RouteSignalID = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<int>(type: "int", nullable: false),
                    DirectionTypeID = table.Column<int>(type: "int", nullable: false),
                    IsOverlap = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimaryApproach = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutePhaseDirections", x => x.ID);
                    table.ForeignKey(
                        name: "FK_dbo.RoutePhaseDirections_dbo.DirectionTypes_DirectionTypeID",
                        column: x => x.DirectionTypeID,
                        principalTable: "DirectionTypes",
                        principalColumn: "DirectionTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.RoutePhaseDirections_dbo.RouteSignals_RouteSignalID",
                        column: x => x.RouteSignalID,
                        principalTable: "RouteSignals",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Areas",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SignalVersionID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Areas", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Areas_Signals_SignalVersionID",
                        column: x => x.SignalVersionID,
                        principalTable: "Signals",
                        principalColumn: "VersionID");
                });

            migrationBuilder.CreateTable(
                name: "DetectionTypeDetector",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false),
                    DetectionTypeID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.DetectionTypeDetector", x => new { x.ID, x.DetectionTypeID });
                    table.ForeignKey(
                        name: "FK_dbo.DetectionTypeDetector_dbo.DetectionTypes_DetectionTypeID",
                        column: x => x.DetectionTypeID,
                        principalTable: "DetectionTypes",
                        principalColumn: "DetectionTypeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.DetectionTypeDetector_dbo.Detectors_ID",
                        column: x => x.ID,
                        principalTable: "Detectors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetectorComments",
                columns: table => new
                {
                    CommentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ID = table.Column<int>(type: "int", nullable: false),
                    TimeStamp = table.Column<DateTime>(type: "datetime", nullable: false),
                    CommentText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.DetectorComments", x => x.CommentID);
                    table.ForeignKey(
                        name: "FK_dbo.DetectorComments_dbo.Detectors_ID",
                        column: x => x.ID,
                        principalTable: "Detectors",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AreaSignals",
                columns: table => new
                {
                    Area_ID = table.Column<int>(type: "int", nullable: false),
                    Signal_VersionID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.AreaSignals", x => new { x.Area_ID, x.Signal_VersionID });
                    table.ForeignKey(
                        name: "FK_AreaSignals_Signals_Signal_VersionID",
                        column: x => x.Signal_VersionID,
                        principalTable: "Signals",
                        principalColumn: "VersionID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.AreaSignals_dbo.Areas_Area_ID",
                        column: x => x.Area_ID,
                        principalTable: "Areas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Action_ActionID",
                table: "ActionLogActions",
                column: "Action_ActionID");

            migrationBuilder.CreateIndex(
                name: "IX_ActionLog_ActionLogID",
                table: "ActionLogActions",
                column: "ActionLog_ActionLogID");

            migrationBuilder.CreateIndex(
                name: "IX_ActionLog_ActionLogID1",
                table: "ActionLogMetricTypes",
                column: "ActionLog_ActionLogID");

            migrationBuilder.CreateIndex(
                name: "IX_MetricType_MetricID",
                table: "ActionLogMetricTypes",
                column: "MetricType_MetricID");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyID",
                table: "ActionLogs",
                column: "AgencyID");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationID",
                table: "ApplicationSettings",
                column: "ApplicationID");

            migrationBuilder.CreateIndex(
                name: "IX_DirectionTypeID",
                table: "Approaches",
                column: "DirectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_VersionID",
                table: "Approaches",
                column: "VersionID");

            migrationBuilder.CreateIndex(
                name: "IX_Areas_SignalVersionID",
                table: "Areas",
                column: "SignalVersionID");

            migrationBuilder.CreateIndex(
                name: "IX_Area_ID",
                table: "AreaSignals",
                column: "Area_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Signal_VersionID",
                table: "AreaSignals",
                column: "Signal_VersionID");

            migrationBuilder.CreateIndex(
                name: "IX_DetectionTypeID",
                table: "DetectionTypeDetector",
                column: "DetectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_ID",
                table: "DetectionTypeDetector",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_DetectionType_DetectionTypeID",
                table: "DetectionTypeMetricTypes",
                column: "DetectionType_DetectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_MetricType_MetricID1",
                table: "DetectionTypeMetricTypes",
                column: "MetricType_MetricID");

            migrationBuilder.CreateIndex(
                name: "IX_ID1",
                table: "DetectorComments",
                column: "ID");

            migrationBuilder.CreateIndex(
                name: "IX_ApproachID",
                table: "Detectors",
                column: "ApproachID");

            migrationBuilder.CreateIndex(
                name: "IX_DetectionHardwareID",
                table: "Detectors",
                column: "DetectionHardwareID");

            migrationBuilder.CreateIndex(
                name: "IX_LaneTypeID",
                table: "Detectors",
                column: "LaneTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_MovementTypeID",
                table: "Detectors",
                column: "MovementTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_MetricComment_CommentID",
                table: "MetricCommentMetricTypes",
                column: "MetricComment_CommentID");

            migrationBuilder.CreateIndex(
                name: "IX_MetricType_MetricID2",
                table: "MetricCommentMetricTypes",
                column: "MetricType_MetricID");

            migrationBuilder.CreateIndex(
                name: "IX_VersionID1",
                table: "MetricComments",
                column: "VersionID");

            migrationBuilder.CreateIndex(
                name: "IX_DirectionTypeID1",
                table: "RoutePhaseDirections",
                column: "DirectionTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_RouteSignalID",
                table: "RoutePhaseDirections",
                column: "RouteSignalID");

            migrationBuilder.CreateIndex(
                name: "IX_RouteID",
                table: "RouteSignals",
                column: "RouteID");

            migrationBuilder.CreateIndex(
                name: "IX_ControllerTypeID",
                table: "Signals",
                column: "ControllerTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_JurisdictionID",
                table: "Signals",
                column: "JurisdictionID");

            migrationBuilder.CreateIndex(
                name: "IX_RegionID",
                table: "Signals",
                column: "RegionID");

            migrationBuilder.CreateIndex(
                name: "IX_VersionActionID",
                table: "Signals",
                column: "VersionActionID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionLogActions");

            migrationBuilder.DropTable(
                name: "ActionLogMetricTypes");

            migrationBuilder.DropTable(
                name: "ApplicationSettings");

            migrationBuilder.DropTable(
                name: "AreaSignals");

            migrationBuilder.DropTable(
                name: "Comment");

            migrationBuilder.DropTable(
                name: "DatabaseArchiveExcludedSignals");

            migrationBuilder.DropTable(
                name: "DetectionTypeDetector");

            migrationBuilder.DropTable(
                name: "DetectionTypeMetricTypes");

            migrationBuilder.DropTable(
                name: "DetectorComments");

            migrationBuilder.DropTable(
                name: "ExternalLinks");

            migrationBuilder.DropTable(
                name: "FAQs");

            migrationBuilder.DropTable(
                name: "MeasuresDefaults");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "MetricCommentMetricTypes");

            migrationBuilder.DropTable(
                name: "MetricsFilterTypes");

            migrationBuilder.DropTable(
                name: "RoutePhaseDirections");

            migrationBuilder.DropTable(
                name: "SignalToAggregates");

            migrationBuilder.DropTable(
                name: "SPMWatchDogErrorEvents");

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
                name: "Signals");

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
                name: "ControllerTypes");

            migrationBuilder.DropTable(
                name: "Jurisdictions");

            migrationBuilder.DropTable(
                name: "Region");

            migrationBuilder.DropTable(
                name: "VersionActions");

            migrationBuilder.DropTable(
                name: "DirectionTypes");
        }
    }
}
