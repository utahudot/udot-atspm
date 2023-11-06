using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.Migrations.Aggregation
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApproachPcdAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "int", nullable: false),
                    IsProtectedPhase = table.Column<bool>(type: "bit", nullable: false),
                    ApproachID = table.Column<int>(type: "int", nullable: false),
                    ArrivalsOnGreen = table.Column<int>(type: "int", nullable: false),
                    ArrivalsOnRed = table.Column<int>(type: "int", nullable: false),
                    ArrivalsOnYellow = table.Column<int>(type: "int", nullable: false),
                    Volume = table.Column<int>(type: "int", nullable: false),
                    TotalDelay = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ApproachPcdAggregations", x => new { x.BinStartTime, x.SignalId, x.PhaseNumber, x.IsProtectedPhase });
                });

            migrationBuilder.CreateTable(
                name: "ApproachSpeedAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ApproachID = table.Column<int>(type: "int", nullable: false),
                    SummedSpeed = table.Column<int>(type: "int", nullable: false),
                    SpeedVolume = table.Column<int>(type: "int", nullable: false),
                    Speed85th = table.Column<int>(type: "int", nullable: false),
                    Speed15th = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ApproachSpeedAggregations", x => new { x.BinStartTime, x.SignalId, x.ApproachID });
                });

            migrationBuilder.CreateTable(
                name: "ApproachSplitFailAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ApproachID = table.Column<int>(type: "int", nullable: false),
                    PhaseNumber = table.Column<int>(type: "int", nullable: false),
                    IsProtectedPhase = table.Column<bool>(type: "bit", nullable: false),
                    SplitFailures = table.Column<int>(type: "int", nullable: false),
                    GreenOccupancySum = table.Column<int>(type: "int", nullable: false),
                    RedOccupancySum = table.Column<int>(type: "int", nullable: false),
                    GreenTimeSum = table.Column<int>(type: "int", nullable: false),
                    RedTimeSum = table.Column<int>(type: "int", nullable: false),
                    Cycles = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ApproachSplitFailAggregations", x => new { x.BinStartTime, x.SignalId, x.ApproachID, x.PhaseNumber, x.IsProtectedPhase });
                });

            migrationBuilder.CreateTable(
                name: "ApproachYellowRedActivationAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "int", nullable: false),
                    IsProtectedPhase = table.Column<bool>(type: "bit", nullable: false),
                    ApproachID = table.Column<int>(type: "int", nullable: false),
                    SevereRedLightViolations = table.Column<int>(type: "int", nullable: false),
                    TotalRedLightViolations = table.Column<int>(type: "int", nullable: false),
                    YellowActivations = table.Column<int>(type: "int", nullable: false),
                    ViolationTime = table.Column<int>(type: "int", nullable: false),
                    Cycles = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ApproachYellowRedActivationAggregations", x => new { x.BinStartTime, x.SignalId, x.PhaseNumber, x.IsProtectedPhase });
                });

            migrationBuilder.CreateTable(
                name: "DetectorEventCountAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    DetectorPrimaryID = table.Column<int>(type: "int", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ApproachID = table.Column<int>(type: "int", nullable: false),
                    EventCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.DetectorEventCountAggregations", x => new { x.BinStartTime, x.DetectorPrimaryID });
                });

            migrationBuilder.CreateTable(
                name: "PhaseCycleAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "int", nullable: false),
                    ApproachID = table.Column<int>(type: "int", nullable: false),
                    RedTime = table.Column<int>(type: "int", nullable: false),
                    YellowTime = table.Column<int>(type: "int", nullable: false),
                    GreenTime = table.Column<int>(type: "int", nullable: false),
                    TotalRedToRedCycles = table.Column<int>(type: "int", nullable: false),
                    TotalGreenToGreenCycles = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.PhaseCycleAggregations", x => new { x.BinStartTime, x.SignalId, x.PhaseNumber });
                });

            migrationBuilder.CreateTable(
                name: "PhaseLeftTurnGapAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "int", nullable: false),
                    ApproachID = table.Column<int>(type: "int", nullable: false),
                    GapCount1 = table.Column<int>(type: "int", nullable: false),
                    GapCount2 = table.Column<int>(type: "int", nullable: false),
                    GapCount3 = table.Column<int>(type: "int", nullable: false),
                    GapCount4 = table.Column<int>(type: "int", nullable: false),
                    GapCount5 = table.Column<int>(type: "int", nullable: false),
                    GapCount6 = table.Column<int>(type: "int", nullable: false),
                    GapCount7 = table.Column<int>(type: "int", nullable: false),
                    GapCount8 = table.Column<int>(type: "int", nullable: false),
                    GapCount9 = table.Column<int>(type: "int", nullable: false),
                    GapCount10 = table.Column<int>(type: "int", nullable: false),
                    GapCount11 = table.Column<int>(type: "int", nullable: false),
                    SumGapDuration1 = table.Column<double>(type: "float", nullable: false),
                    SumGapDuration2 = table.Column<double>(type: "float", nullable: false),
                    SumGapDuration3 = table.Column<double>(type: "float", nullable: false),
                    SumGreenTime = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.PhaseLeftTurnGapAggregations", x => new { x.BinStartTime, x.SignalId, x.PhaseNumber });
                });

            migrationBuilder.CreateTable(
                name: "PhaseSplitMonitorAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PhaseNumber = table.Column<int>(type: "int", nullable: false),
                    EightyFifthPercentileSplit = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.PhaseSplitMonitorAggregations", x => new { x.BinStartTime, x.SignalId, x.PhaseNumber });
                });

            migrationBuilder.CreateTable(
                name: "PhaseTerminationAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PhaseNumber = table.Column<int>(type: "int", nullable: false),
                    GapOuts = table.Column<int>(type: "int", nullable: false),
                    ForceOffs = table.Column<int>(type: "int", nullable: false),
                    MaxOuts = table.Column<int>(type: "int", nullable: false),
                    Unknown = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.PhaseTerminationAggregations", x => new { x.BinStartTime, x.SignalId, x.PhaseNumber });
                });

            migrationBuilder.CreateTable(
                name: "PreemptionAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PreemptNumber = table.Column<int>(type: "int", nullable: false),
                    PreemptRequests = table.Column<int>(type: "int", nullable: false),
                    PreemptServices = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.PreemptionAggregations", x => new { x.BinStartTime, x.SignalId, x.PreemptNumber });
                });

            migrationBuilder.CreateTable(
                name: "PriorityAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PriorityNumber = table.Column<int>(type: "int", nullable: false),
                    PriorityRequests = table.Column<int>(type: "int", nullable: false),
                    PriorityServiceEarlyGreen = table.Column<int>(type: "int", nullable: false),
                    PriorityServiceExtendedGreen = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.PriorityAggregations", x => new { x.BinStartTime, x.SignalId, x.PriorityNumber });
                });

            migrationBuilder.CreateTable(
                name: "SignalEventCountAggregations",
                columns: table => new
                {
                    BinStartTime = table.Column<DateTime>(type: "datetime", nullable: false),
                    SignalId = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    EventCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.SignalEventCountAggregations", x => new { x.BinStartTime, x.SignalId });
                });

            migrationBuilder.CreateTable(
                name: "SignalPlanAggregations",
                columns: table => new
                {
                    SignalId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Start = table.Column<DateTime>(type: "datetime", nullable: false),
                    End = table.Column<DateTime>(type: "datetime", nullable: false),
                    PlanNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.SignalPlanAggregations", x => new { x.SignalId, x.Start, x.End });
                });
        }

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

            migrationBuilder.DropTable(
                name: "SignalEventCountAggregations");

            migrationBuilder.DropTable(
                name: "SignalPlanAggregations");
        }
    }
}
