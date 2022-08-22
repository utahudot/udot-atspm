using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATSPM.Infrasturcture.Migrations.Aggregation
{
    public partial class EFCore6Upgrade : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

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
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                type: "nvarchar(10)",
                maxLength: 10,
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
                type: "nvarchar(10)",
                maxLength: 10,
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
        }
    }
}
