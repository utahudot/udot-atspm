﻿// <auto-generated />
using System;
using ATSPM.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ATSPM.Infrastructure.Migrations.Aggregation
{
    [DbContext(typeof(AggregationContext))]
    [Migration("20220822235636_EFCore6Upgrade")]
    partial class EFCore6Upgrade
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("ATSPM.Data.Models.ApproachPcdAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("int");

                    b.Property<bool>("IsProtectedPhase")
                        .HasColumnType("bit");

                    b.Property<int>("ApproachID")
                        .HasColumnType("int");

                    b.Property<int>("ArrivalsOnGreen")
                        .HasColumnType("int");

                    b.Property<int>("ArrivalsOnRed")
                        .HasColumnType("int");

                    b.Property<int>("ArrivalsOnYellow")
                        .HasColumnType("int");

                    b.Property<int>("TotalDelay")
                        .HasColumnType("int");

                    b.Property<int>("Volume")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId", "PhaseNumber", "IsProtectedPhase");

                    b.ToTable("ApproachPcdAggregations");

                    b.HasComment("Approach Pcd Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.ApproachSpeedAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("ApproachID")
                        .HasColumnType("int");

                    b.Property<int>("Speed15th")
                        .HasColumnType("int");

                    b.Property<int>("Speed85th")
                        .HasColumnType("int");

                    b.Property<int>("SpeedVolume")
                        .HasColumnType("int");

                    b.Property<int>("SummedSpeed")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId", "ApproachID");

                    b.ToTable("ApproachSpeedAggregations");

                    b.HasComment("Approach Speed Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.ApproachSplitFailAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("ApproachID")
                        .HasColumnType("int");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("int");

                    b.Property<bool>("IsProtectedPhase")
                        .HasColumnType("bit");

                    b.Property<int>("Cycles")
                        .HasColumnType("int");

                    b.Property<int>("GreenOccupancySum")
                        .HasColumnType("int");

                    b.Property<int>("GreenTimeSum")
                        .HasColumnType("int");

                    b.Property<int>("RedOccupancySum")
                        .HasColumnType("int");

                    b.Property<int>("RedTimeSum")
                        .HasColumnType("int");

                    b.Property<int>("SplitFailures")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId", "ApproachID", "PhaseNumber", "IsProtectedPhase");

                    b.ToTable("ApproachSplitFailAggregations");

                    b.HasComment("Approach Split Fail Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.ApproachYellowRedActivationAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("int");

                    b.Property<bool>("IsProtectedPhase")
                        .HasColumnType("bit");

                    b.Property<int>("ApproachID")
                        .HasColumnType("int");

                    b.Property<int>("Cycles")
                        .HasColumnType("int");

                    b.Property<int>("SevereRedLightViolations")
                        .HasColumnType("int");

                    b.Property<int>("TotalRedLightViolations")
                        .HasColumnType("int");

                    b.Property<int>("ViolationTime")
                        .HasColumnType("int");

                    b.Property<int>("YellowActivations")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId", "PhaseNumber", "IsProtectedPhase");

                    b.ToTable("ApproachYellowRedActivationAggregations");

                    b.HasComment("Approach Yellow Red Activation Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.DetectorEventCountAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<int>("DetectorPrimaryID")
                        .HasColumnType("int");

                    b.Property<int>("ApproachID")
                        .HasColumnType("int");

                    b.Property<int>("EventCount")
                        .HasColumnType("int");

                    b.Property<string>("SignalId")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.HasKey("BinStartTime", "DetectorPrimaryID");

                    b.ToTable("DetectorEventCountAggregations");

                    b.HasComment("Detector Event Count Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.PhaseCycleAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("int");

                    b.Property<int>("ApproachId")
                        .HasColumnType("int");

                    b.Property<int>("GreenTime")
                        .HasColumnType("int");

                    b.Property<int>("RedTime")
                        .HasColumnType("int");

                    b.Property<int>("TotalGreenToGreenCycles")
                        .HasColumnType("int");

                    b.Property<int>("TotalRedToRedCycles")
                        .HasColumnType("int");

                    b.Property<int>("YellowTime")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId", "PhaseNumber");

                    b.ToTable("PhaseCycleAggregations");

                    b.HasComment("Phase Cycle Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.PhaseLeftTurnGapAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("int");

                    b.Property<int>("ApproachID")
                        .HasColumnType("int");

                    b.Property<int>("GapCount1")
                        .HasColumnType("int");

                    b.Property<int>("GapCount10")
                        .HasColumnType("int");

                    b.Property<int>("GapCount11")
                        .HasColumnType("int");

                    b.Property<int>("GapCount2")
                        .HasColumnType("int");

                    b.Property<int>("GapCount3")
                        .HasColumnType("int");

                    b.Property<int>("GapCount4")
                        .HasColumnType("int");

                    b.Property<int>("GapCount5")
                        .HasColumnType("int");

                    b.Property<int>("GapCount6")
                        .HasColumnType("int");

                    b.Property<int>("GapCount7")
                        .HasColumnType("int");

                    b.Property<int>("GapCount8")
                        .HasColumnType("int");

                    b.Property<int>("GapCount9")
                        .HasColumnType("int");

                    b.Property<double>("SumGapDuration1")
                        .HasColumnType("float");

                    b.Property<double>("SumGapDuration2")
                        .HasColumnType("float");

                    b.Property<double>("SumGapDuration3")
                        .HasColumnType("float");

                    b.Property<double>("SumGreenTime")
                        .HasColumnType("float");

                    b.HasKey("BinStartTime", "SignalId", "PhaseNumber");

                    b.ToTable("PhaseLeftTurnGapAggregations");

                    b.HasComment("Phase Left Turn Gap Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.PhaseSplitMonitorAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("int");

                    b.Property<int>("EightyFifthPercentileSplit")
                        .HasColumnType("int");

                    b.Property<int>("SkippedCount")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId", "PhaseNumber");

                    b.ToTable("PhaseSplitMonitorAggregations");

                    b.HasComment("Phase Split Monitor Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.PhaseTerminationAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("int");

                    b.Property<int>("ForceOffs")
                        .HasColumnType("int");

                    b.Property<int>("GapOuts")
                        .HasColumnType("int");

                    b.Property<int>("MaxOuts")
                        .HasColumnType("int");

                    b.Property<int>("Unknown")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId", "PhaseNumber");

                    b.ToTable("PhaseTerminationAggregations");

                    b.HasComment("Phase Termination Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.PreemptionAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("PreemptNumber")
                        .HasColumnType("int");

                    b.Property<int>("PreemptRequests")
                        .HasColumnType("int");

                    b.Property<int>("PreemptServices")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId", "PreemptNumber");

                    b.ToTable("PreemptionAggregations");

                    b.HasComment("Preemption Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.PriorityAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("PriorityNumber")
                        .HasColumnType("int");

                    b.Property<int>("PriorityRequests")
                        .HasColumnType("int");

                    b.Property<int>("PriorityServiceEarlyGreen")
                        .HasColumnType("int");

                    b.Property<int>("PriorityServiceExtendedGreen")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId", "PriorityNumber");

                    b.ToTable("PriorityAggregations");

                    b.HasComment("Priority Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.SignalEventCountAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("datetime");

                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<int>("EventCount")
                        .HasColumnType("int");

                    b.HasKey("BinStartTime", "SignalId");

                    b.ToTable("SignalEventCountAggregations");

                    b.HasComment("Signal Event Count Aggregation");
                });

            modelBuilder.Entity("ATSPM.Data.Models.SignalPlanAggregation", b =>
                {
                    b.Property<string>("SignalId")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("varchar(10)");

                    b.Property<DateTime>("Start")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("End")
                        .HasColumnType("datetime");

                    b.Property<int>("PlanNumber")
                        .HasColumnType("int");

                    b.HasKey("SignalId", "Start", "End");

                    b.ToTable("SignalPlanAggregations");

                    b.HasComment("Signal Plan Aggregation");
                });
#pragma warning restore 612, 618
        }
    }
}