using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.PostgreSQLDatabaseProvider.Migrations.Aggregation
{
    /// <inheritdoc />
    public partial class EFCore6Upgrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApproachPcdAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "integer", nullable: false),
                    IsProtectedPhase = table.Column<bool>(type: "boolean", nullable: false),
                    ApproachId = table.Column<int>(type: "integer", nullable: false),
                    ArrivalsOnGreen = table.Column<int>(type: "integer", nullable: false),
                    ArrivalsOnRed = table.Column<int>(type: "integer", nullable: false),
                    ArrivalsOnYellow = table.Column<int>(type: "integer", nullable: false),
                    Volume = table.Column<int>(type: "integer", nullable: false),
                    TotalDelay = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApproachPcdAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.PhaseNumber, x.IsProtectedPhase });
                },
                comment: "Approach Pcd Aggregation");

            migrationBuilder.CreateTable(
                name: "ApproachSpeedAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    ApproachId = table.Column<int>(type: "integer", nullable: false),
                    SummedSpeed = table.Column<int>(type: "integer", nullable: false),
                    SpeedVolume = table.Column<int>(type: "integer", nullable: false),
                    Speed85th = table.Column<int>(type: "integer", nullable: false),
                    Speed15th = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApproachSpeedAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.ApproachId });
                },
                comment: "Approach Speed Aggregation");

            migrationBuilder.CreateTable(
                name: "ApproachSplitFailAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "integer", nullable: false),
                    ApproachId = table.Column<int>(type: "integer", nullable: false),
                    IsProtectedPhase = table.Column<bool>(type: "boolean", nullable: false),
                    SplitFailures = table.Column<int>(type: "integer", nullable: false),
                    GreenOccupancySum = table.Column<int>(type: "integer", nullable: false),
                    RedOccupancySum = table.Column<int>(type: "integer", nullable: false),
                    GreenTimeSum = table.Column<int>(type: "integer", nullable: false),
                    RedTimeSum = table.Column<int>(type: "integer", nullable: false),
                    Cycles = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApproachSplitFailAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.ApproachId, x.PhaseNumber, x.IsProtectedPhase });
                },
                comment: "Approach Split Fail Aggregation");

            migrationBuilder.CreateTable(
                name: "ApproachYellowRedActivationAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "integer", nullable: false),
                    IsProtectedPhase = table.Column<bool>(type: "boolean", nullable: false),
                    ApproachId = table.Column<int>(type: "integer", nullable: false),
                    SevereRedLightViolations = table.Column<int>(type: "integer", nullable: false),
                    TotalRedLightViolations = table.Column<int>(type: "integer", nullable: false),
                    YellowActivations = table.Column<int>(type: "integer", nullable: false),
                    ViolationTime = table.Column<int>(type: "integer", nullable: false),
                    Cycles = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApproachYellowRedActivationAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.PhaseNumber, x.IsProtectedPhase });
                },
                comment: "Approach Yellow Red Activation Aggregation");

            migrationBuilder.CreateTable(
                name: "DetectorEventCountAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DetectorPrimaryId = table.Column<int>(type: "integer", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    ApproachId = table.Column<int>(type: "integer", nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectorEventCountAggregations", x => new { x.BinStartTime, x.DetectorPrimaryId });
                },
                comment: "Detector Event Count Aggregation");

            migrationBuilder.CreateTable(
                name: "LocationEventCountAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    EventCount = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationEventCountAggregations", x => new { x.BinStartTime, x.LocationIdentifier });
                },
                comment: "Location Event Count Aggregation");

            migrationBuilder.CreateTable(
                name: "LocationPlanAggregations",
                columns: table => new
                {
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PlanNumber = table.Column<int>(type: "integer", nullable: false),
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationPlanAggregations", x => new { x.LocationIdentifier, x.Start, x.End });
                },
                comment: "Location Plan Aggregation");

            migrationBuilder.CreateTable(
                name: "PhaseCycleAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "integer", nullable: false),
                    ApproachId = table.Column<int>(type: "integer", nullable: false),
                    RedTime = table.Column<int>(type: "integer", nullable: false),
                    YellowTime = table.Column<int>(type: "integer", nullable: false),
                    GreenTime = table.Column<int>(type: "integer", nullable: false),
                    TotalRedToRedCycles = table.Column<int>(type: "integer", nullable: false),
                    TotalGreenToGreenCycles = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseCycleAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.PhaseNumber });
                },
                comment: "Phase Cycle Aggregation");

            migrationBuilder.CreateTable(
                name: "PhaseLeftTurnGapAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "integer", nullable: false),
                    ApproachId = table.Column<int>(type: "integer", nullable: false),
                    GapCount1 = table.Column<int>(type: "integer", nullable: false),
                    GapCount2 = table.Column<int>(type: "integer", nullable: false),
                    GapCount3 = table.Column<int>(type: "integer", nullable: false),
                    GapCount4 = table.Column<int>(type: "integer", nullable: false),
                    GapCount5 = table.Column<int>(type: "integer", nullable: false),
                    GapCount6 = table.Column<int>(type: "integer", nullable: false),
                    GapCount7 = table.Column<int>(type: "integer", nullable: false),
                    GapCount8 = table.Column<int>(type: "integer", nullable: false),
                    GapCount9 = table.Column<int>(type: "integer", nullable: false),
                    GapCount10 = table.Column<int>(type: "integer", nullable: false),
                    GapCount11 = table.Column<int>(type: "integer", nullable: false),
                    SumGapDuration1 = table.Column<double>(type: "double precision", nullable: false),
                    SumGapDuration2 = table.Column<double>(type: "double precision", nullable: false),
                    SumGapDuration3 = table.Column<double>(type: "double precision", nullable: false),
                    SumGreenTime = table.Column<double>(type: "double precision", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseLeftTurnGapAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.PhaseNumber });
                },
                comment: "Phase Left Turn Gap Aggregation");

            migrationBuilder.CreateTable(
                name: "PhaseSplitMonitorAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "integer", nullable: false),
                    EightyFifthPercentileSplit = table.Column<int>(type: "integer", nullable: false),
                    SkippedCount = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseSplitMonitorAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.PhaseNumber });
                },
                comment: "Phase Split Monitor Aggregation");

            migrationBuilder.CreateTable(
                name: "PhaseTerminationAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "integer", nullable: false),
                    GapOuts = table.Column<int>(type: "integer", nullable: false),
                    ForceOffs = table.Column<int>(type: "integer", nullable: false),
                    MaxOuts = table.Column<int>(type: "integer", nullable: false),
                    Unknown = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhaseTerminationAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.PhaseNumber });
                },
                comment: "Phase Termination Aggregation");

            migrationBuilder.CreateTable(
                name: "PreemptionAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PreemptNumber = table.Column<int>(type: "integer", nullable: false),
                    PreemptRequests = table.Column<int>(type: "integer", nullable: false),
                    PreemptServices = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreemptionAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.PreemptNumber });
                },
                comment: "Preemption Aggregation");

            migrationBuilder.CreateTable(
                name: "PriorityAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocationIdentifier = table.Column<string>(type: "character varying(10)", unicode: false, maxLength: 10, nullable: false),
                    PriorityNumber = table.Column<int>(type: "integer", nullable: false),
                    PriorityRequests = table.Column<int>(type: "integer", nullable: false),
                    PriorityServiceEarlyGreen = table.Column<int>(type: "integer", nullable: false),
                    PriorityServiceExtendedGreen = table.Column<int>(type: "integer", nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    End = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriorityAggregations", x => new { x.BinStartTime, x.LocationIdentifier, x.PriorityNumber });
                },
                comment: "Priority Aggregation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApproachPcdAggregations");

            migrationBuilder.DropTable(
                name: "ApproachSpeedAggregations");

            migrationBuilder.DropTable(
                name: "ApproachSplitFailAggregations");

            migrationBuilder.DropTable(
                name: "ApproachYellowRedActivationAggregations");

            migrationBuilder.DropTable(
                name: "DetectorEventCountAggregations");

            migrationBuilder.DropTable(
                name: "LocationEventCountAggregations");

            migrationBuilder.DropTable(
                name: "LocationPlanAggregations");

            migrationBuilder.DropTable(
                name: "PhaseCycleAggregations");

            migrationBuilder.DropTable(
                name: "PhaseLeftTurnGapAggregations");

            migrationBuilder.DropTable(
                name: "PhaseSplitMonitorAggregations");

            migrationBuilder.DropTable(
                name: "PhaseTerminationAggregations");

            migrationBuilder.DropTable(
                name: "PreemptionAggregations");

            migrationBuilder.DropTable(
                name: "PriorityAggregations");
        }
    }
}
