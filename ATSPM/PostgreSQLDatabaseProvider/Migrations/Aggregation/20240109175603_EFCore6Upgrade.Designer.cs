﻿// <auto-generated />
using System;
using ATSPM.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ATSPM.Infrastructure.PostgreSQLDatabaseProvider.Migrations.Aggregation
{
    [DbContext(typeof(AggregationContext))]
    [Migration("20240109175603_EFCore6Upgrade")]
    partial class EFCore6Upgrade
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ATSPM.Data.Models.ApproachPcdAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("integer");

                    b.Property<bool>("IsProtectedPhase")
                        .HasColumnType("boolean");

                    b.Property<int>("ApproachId")
                        .HasColumnType("integer");

                    b.Property<int>("ArrivalsOnGreen")
                        .HasColumnType("integer");

                    b.Property<int>("ArrivalsOnRed")
                        .HasColumnType("integer");

                    b.Property<int>("ArrivalsOnYellow")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TotalDelay")
                        .HasColumnType("integer");

                    b.Property<int>("Volume")
                        .HasColumnType("integer");

                    b.HasKey("BinStartTime", "LocationIdentifier", "PhaseNumber", "IsProtectedPhase");

                    b.ToTable("ApproachPcdAggregations", t =>
                        {
                            t.HasComment("Approach Pcd Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.ApproachSpeedAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("ApproachId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Speed15th")
                        .HasColumnType("integer");

                    b.Property<int>("Speed85th")
                        .HasColumnType("integer");

                    b.Property<int>("SpeedVolume")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("SummedSpeed")
                        .HasColumnType("integer");

                    b.HasKey("BinStartTime", "LocationIdentifier", "ApproachId");

                    b.ToTable("ApproachSpeedAggregations", t =>
                        {
                            t.HasComment("Approach Speed Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.ApproachSplitFailAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("ApproachId")
                        .HasColumnType("integer");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("integer");

                    b.Property<bool>("IsProtectedPhase")
                        .HasColumnType("boolean");

                    b.Property<int>("Cycles")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("GreenOccupancySum")
                        .HasColumnType("integer");

                    b.Property<int>("GreenTimeSum")
                        .HasColumnType("integer");

                    b.Property<int>("RedOccupancySum")
                        .HasColumnType("integer");

                    b.Property<int>("RedTimeSum")
                        .HasColumnType("integer");

                    b.Property<int>("SplitFailures")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("BinStartTime", "LocationIdentifier", "ApproachId", "PhaseNumber", "IsProtectedPhase");

                    b.ToTable("ApproachSplitFailAggregations", t =>
                        {
                            t.HasComment("Approach Split Fail Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.ApproachYellowRedActivationAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("integer");

                    b.Property<bool>("IsProtectedPhase")
                        .HasColumnType("boolean");

                    b.Property<int>("ApproachId")
                        .HasColumnType("integer");

                    b.Property<int>("Cycles")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("SevereRedLightViolations")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TotalRedLightViolations")
                        .HasColumnType("integer");

                    b.Property<int>("ViolationTime")
                        .HasColumnType("integer");

                    b.Property<int>("YellowActivations")
                        .HasColumnType("integer");

                    b.HasKey("BinStartTime", "LocationIdentifier", "PhaseNumber", "IsProtectedPhase");

                    b.ToTable("ApproachYellowRedActivationAggregations", t =>
                        {
                            t.HasComment("Approach Yellow Red Activation Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.DetectorEventCountAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("DetectorPrimaryId")
                        .HasColumnType("integer");

                    b.Property<int>("ApproachId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("EventCount")
                        .HasColumnType("integer");

                    b.Property<string>("LocationIdentifier")
                        .IsRequired()
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("BinStartTime", "DetectorPrimaryId");

                    b.ToTable("DetectorEventCountAggregations", t =>
                        {
                            t.HasComment("Detector Event Count Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.LocationEventCountAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("EventCount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("BinStartTime", "LocationIdentifier");

                    b.ToTable("LocationEventCountAggregations", t =>
                        {
                            t.HasComment("Location Event Count Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.LocationPlanAggregation", b =>
                {
                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("PlanNumber")
                        .HasColumnType("integer");

                    b.HasKey("LocationIdentifier", "Start", "End");

                    b.ToTable("LocationPlanAggregations", t =>
                        {
                            t.HasComment("Location Plan Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.PhaseCycleAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("integer");

                    b.Property<int>("ApproachId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("GreenTime")
                        .HasColumnType("integer");

                    b.Property<int>("RedTime")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("TotalGreenToGreenCycles")
                        .HasColumnType("integer");

                    b.Property<int>("TotalRedToRedCycles")
                        .HasColumnType("integer");

                    b.Property<int>("YellowTime")
                        .HasColumnType("integer");

                    b.HasKey("BinStartTime", "LocationIdentifier", "PhaseNumber");

                    b.ToTable("PhaseCycleAggregations", t =>
                        {
                            t.HasComment("Phase Cycle Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.PhaseLeftTurnGapAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("integer");

                    b.Property<int>("ApproachId")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("GapCount1")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount10")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount11")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount2")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount3")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount4")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount5")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount6")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount7")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount8")
                        .HasColumnType("integer");

                    b.Property<int>("GapCount9")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<double>("SumGapDuration1")
                        .HasColumnType("double precision");

                    b.Property<double>("SumGapDuration2")
                        .HasColumnType("double precision");

                    b.Property<double>("SumGapDuration3")
                        .HasColumnType("double precision");

                    b.Property<double>("SumGreenTime")
                        .HasColumnType("double precision");

                    b.HasKey("BinStartTime", "LocationIdentifier", "PhaseNumber");

                    b.ToTable("PhaseLeftTurnGapAggregations", t =>
                        {
                            t.HasComment("Phase Left Turn Gap Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.PhaseSplitMonitorAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("integer");

                    b.Property<int>("EightyFifthPercentileSplit")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("SkippedCount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("BinStartTime", "LocationIdentifier", "PhaseNumber");

                    b.ToTable("PhaseSplitMonitorAggregations", t =>
                        {
                            t.HasComment("Phase Split Monitor Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.PhaseTerminationAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("PhaseNumber")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("ForceOffs")
                        .HasColumnType("integer");

                    b.Property<int>("GapOuts")
                        .HasColumnType("integer");

                    b.Property<int>("MaxOuts")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Unknown")
                        .HasColumnType("integer");

                    b.HasKey("BinStartTime", "LocationIdentifier", "PhaseNumber");

                    b.ToTable("PhaseTerminationAggregations", t =>
                        {
                            t.HasComment("Phase Termination Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.PreemptionAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("PreemptNumber")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("PreemptRequests")
                        .HasColumnType("integer");

                    b.Property<int>("PreemptServices")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("BinStartTime", "LocationIdentifier", "PreemptNumber");

                    b.ToTable("PreemptionAggregations", t =>
                        {
                            t.HasComment("Preemption Aggregation");
                        });
                });

            modelBuilder.Entity("ATSPM.Data.Models.PriorityAggregation", b =>
                {
                    b.Property<DateTime>("BinStartTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LocationIdentifier")
                        .HasMaxLength(10)
                        .IsUnicode(false)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("PriorityNumber")
                        .HasColumnType("integer");

                    b.Property<DateTime>("End")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("PriorityRequests")
                        .HasColumnType("integer");

                    b.Property<int>("PriorityServiceEarlyGreen")
                        .HasColumnType("integer");

                    b.Property<int>("PriorityServiceExtendedGreen")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Start")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("BinStartTime", "LocationIdentifier", "PriorityNumber");

                    b.ToTable("PriorityAggregations", t =>
                        {
                            t.HasComment("Priority Aggregation");
                        });
                });
#pragma warning restore 612, 618
        }
    }
}