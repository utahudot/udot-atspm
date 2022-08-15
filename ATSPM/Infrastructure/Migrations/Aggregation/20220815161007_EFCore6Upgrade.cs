using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrasturcture.Migrations.Aggregation
{
    public partial class EFCore6Upgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.SignalPlanAggregations",
                table: "SignalPlanAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.SignalEventCountAggregations",
                table: "SignalEventCountAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.PriorityAggregations",
                table: "PriorityAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.PreemptionAggregations",
                table: "PreemptionAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.PhaseTerminationAggregations",
                table: "PhaseTerminationAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.PhaseSplitMonitorAggregations",
                table: "PhaseSplitMonitorAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.PhaseLeftTurnGapAggregations",
                table: "PhaseLeftTurnGapAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.PhaseCycleAggregations",
                table: "PhaseCycleAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.DetectorEventCountAggregations",
                table: "DetectorEventCountAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.ApproachYellowRedActivationAggregations",
                table: "ApproachYellowRedActivationAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.ApproachSplitFailAggregations",
                table: "ApproachSplitFailAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.ApproachSpeedAggregations",
                table: "ApproachSpeedAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.ApproachPcdAggregations",
                table: "ApproachPcdAggregations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SignalPlanAggregations",
                table: "SignalPlanAggregations",
                columns: new[] { "SignalId", "Start", "End" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_SignalEventCountAggregations",
                table: "SignalEventCountAggregations",
                columns: new[] { "BinStartTime", "SignalId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PriorityAggregations",
                table: "PriorityAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PriorityNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PreemptionAggregations",
                table: "PreemptionAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PreemptNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhaseTerminationAggregations",
                table: "PhaseTerminationAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhaseSplitMonitorAggregations",
                table: "PhaseSplitMonitorAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhaseLeftTurnGapAggregations",
                table: "PhaseLeftTurnGapAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PhaseCycleAggregations",
                table: "PhaseCycleAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_DetectorEventCountAggregations",
                table: "DetectorEventCountAggregations",
                columns: new[] { "BinStartTime", "DetectorPrimaryID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApproachYellowRedActivationAggregations",
                table: "ApproachYellowRedActivationAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber", "IsProtectedPhase" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApproachSplitFailAggregations",
                table: "ApproachSplitFailAggregations",
                columns: new[] { "BinStartTime", "SignalId", "ApproachID", "PhaseNumber", "IsProtectedPhase" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApproachSpeedAggregations",
                table: "ApproachSpeedAggregations",
                columns: new[] { "BinStartTime", "SignalId", "ApproachID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApproachPcdAggregations",
                table: "ApproachPcdAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber", "IsProtectedPhase" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_SignalPlanAggregations",
                table: "SignalPlanAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SignalEventCountAggregations",
                table: "SignalEventCountAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PriorityAggregations",
                table: "PriorityAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PreemptionAggregations",
                table: "PreemptionAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhaseTerminationAggregations",
                table: "PhaseTerminationAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhaseSplitMonitorAggregations",
                table: "PhaseSplitMonitorAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhaseLeftTurnGapAggregations",
                table: "PhaseLeftTurnGapAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PhaseCycleAggregations",
                table: "PhaseCycleAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DetectorEventCountAggregations",
                table: "DetectorEventCountAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApproachYellowRedActivationAggregations",
                table: "ApproachYellowRedActivationAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApproachSplitFailAggregations",
                table: "ApproachSplitFailAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApproachSpeedAggregations",
                table: "ApproachSpeedAggregations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApproachPcdAggregations",
                table: "ApproachPcdAggregations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.SignalPlanAggregations",
                table: "SignalPlanAggregations",
                columns: new[] { "SignalId", "Start", "End" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.SignalEventCountAggregations",
                table: "SignalEventCountAggregations",
                columns: new[] { "BinStartTime", "SignalId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.PriorityAggregations",
                table: "PriorityAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PriorityNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.PreemptionAggregations",
                table: "PreemptionAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PreemptNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.PhaseTerminationAggregations",
                table: "PhaseTerminationAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.PhaseSplitMonitorAggregations",
                table: "PhaseSplitMonitorAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.PhaseLeftTurnGapAggregations",
                table: "PhaseLeftTurnGapAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.PhaseCycleAggregations",
                table: "PhaseCycleAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.DetectorEventCountAggregations",
                table: "DetectorEventCountAggregations",
                columns: new[] { "BinStartTime", "DetectorPrimaryID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.ApproachYellowRedActivationAggregations",
                table: "ApproachYellowRedActivationAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber", "IsProtectedPhase" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.ApproachSplitFailAggregations",
                table: "ApproachSplitFailAggregations",
                columns: new[] { "BinStartTime", "SignalId", "ApproachID", "PhaseNumber", "IsProtectedPhase" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.ApproachSpeedAggregations",
                table: "ApproachSpeedAggregations",
                columns: new[] { "BinStartTime", "SignalId", "ApproachID" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.ApproachPcdAggregations",
                table: "ApproachPcdAggregations",
                columns: new[] { "BinStartTime", "SignalId", "PhaseNumber", "IsProtectedPhase" });
        }
    }
}
