using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrastructure.Migrations.Aggregation
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

            migrationBuilder.RenameColumn(
                name: "ApproachID",
                table: "PhaseCycleAggregations",
                newName: "ApproachId");

            migrationBuilder.AlterTable(
                name: "SignalPlanAggregations",
                comment: "Signal Plan Aggregation");

            migrationBuilder.AlterTable(
                name: "SignalEventCountAggregations",
                comment: "Signal Event Count Aggregation");

            migrationBuilder.AlterTable(
                name: "PriorityAggregations",
                comment: "Priority Aggregation");

            migrationBuilder.AlterTable(
                name: "PreemptionAggregations",
                comment: "Preemption Aggregation");

            migrationBuilder.AlterTable(
                name: "PhaseTerminationAggregations",
                comment: "Phase Termination Aggregation");

            migrationBuilder.AlterTable(
                name: "PhaseSplitMonitorAggregations",
                comment: "Phase Split Monitor Aggregation");

            migrationBuilder.AlterTable(
                name: "PhaseLeftTurnGapAggregations",
                comment: "Phase Left Turn Gap Aggregation");

            migrationBuilder.AlterTable(
                name: "PhaseCycleAggregations",
                comment: "Phase Cycle Aggregation");

            migrationBuilder.AlterTable(
                name: "DetectorEventCountAggregations",
                comment: "Detector Event Count Aggregation");

            migrationBuilder.AlterTable(
                name: "ApproachYellowRedActivationAggregations",
                comment: "Approach Yellow Red Activation Aggregation");

            migrationBuilder.AlterTable(
                name: "ApproachSplitFailAggregations",
                comment: "Approach Split Fail Aggregation");

            migrationBuilder.AlterTable(
                name: "ApproachSpeedAggregations",
                comment: "Approach Speed Aggregation");

            migrationBuilder.AlterTable(
                name: "ApproachPcdAggregations",
                comment: "Approach Pcd Aggregation");

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "SignalPlanAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "SignalEventCountAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PriorityAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PreemptionAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PhaseTerminationAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PhaseSplitMonitorAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PhaseLeftTurnGapAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PhaseCycleAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "DetectorEventCountAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "ApproachYellowRedActivationAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "ApproachSplitFailAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "ApproachSpeedAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "ApproachPcdAggregations",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

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

            migrationBuilder.RenameColumn(
                name: "ApproachId",
                table: "PhaseCycleAggregations",
                newName: "ApproachID");

            migrationBuilder.AlterTable(
                name: "SignalPlanAggregations",
                oldComment: "Signal Plan Aggregation");

            migrationBuilder.AlterTable(
                name: "SignalEventCountAggregations",
                oldComment: "Signal Event Count Aggregation");

            migrationBuilder.AlterTable(
                name: "PriorityAggregations",
                oldComment: "Priority Aggregation");

            migrationBuilder.AlterTable(
                name: "PreemptionAggregations",
                oldComment: "Preemption Aggregation");

            migrationBuilder.AlterTable(
                name: "PhaseTerminationAggregations",
                oldComment: "Phase Termination Aggregation");

            migrationBuilder.AlterTable(
                name: "PhaseSplitMonitorAggregations",
                oldComment: "Phase Split Monitor Aggregation");

            migrationBuilder.AlterTable(
                name: "PhaseLeftTurnGapAggregations",
                oldComment: "Phase Left Turn Gap Aggregation");

            migrationBuilder.AlterTable(
                name: "PhaseCycleAggregations",
                oldComment: "Phase Cycle Aggregation");

            migrationBuilder.AlterTable(
                name: "DetectorEventCountAggregations",
                oldComment: "Detector Event Count Aggregation");

            migrationBuilder.AlterTable(
                name: "ApproachYellowRedActivationAggregations",
                oldComment: "Approach Yellow Red Activation Aggregation");

            migrationBuilder.AlterTable(
                name: "ApproachSplitFailAggregations",
                oldComment: "Approach Split Fail Aggregation");

            migrationBuilder.AlterTable(
                name: "ApproachSpeedAggregations",
                oldComment: "Approach Speed Aggregation");

            migrationBuilder.AlterTable(
                name: "ApproachPcdAggregations",
                oldComment: "Approach Pcd Aggregation");

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "SignalPlanAggregations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "SignalEventCountAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PriorityAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PreemptionAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PhaseTerminationAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PhaseSplitMonitorAggregations",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PhaseLeftTurnGapAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "PhaseCycleAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "DetectorEventCountAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "ApproachYellowRedActivationAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "ApproachSplitFailAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "ApproachSpeedAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "SignalId",
                table: "ApproachPcdAggregations",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

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
