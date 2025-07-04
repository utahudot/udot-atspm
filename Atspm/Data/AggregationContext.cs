﻿#region license
// Copyright 2025 Utah Departement of Transportation
// for Data - Utah.Udot.Atspm.Data/AggregationContext.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Utah.Udot.Atspm.Data.Utility;

namespace Utah.Udot.Atspm.Data
{
    /// <summary>
    /// Atspm aggregation database context
    /// </summary>
    public partial class AggregationContext : DbContext
    {
        /// <inheritdoc/>
        public AggregationContext() { }

        /// <inheritdoc/>
        public AggregationContext(DbContextOptions<AggregationContext> options): base(options) { }

        /// <summary>
        /// Compressed data base table
        /// Use this table when accessing all recoreds regardless of datatype
        /// Returned compressed data will need to be cast to type specified in <see cref="CompressedAggregationBase.Data"/>
        /// </summary>
        public virtual DbSet<CompressedAggregationBase> CompressedAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="ApproachPcdAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<ApproachPcdAggregation>> ApproachPcdAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="ApproachSpeedAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<ApproachSpeedAggregation>> ApproachSpeedAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="ApproachSplitFailAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<ApproachSplitFailAggregation>> ApproachSplitFailAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="ApproachYellowRedActivationAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<ApproachYellowRedActivationAggregation>> ApproachYellowRedActivationAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="DetectorEventCountAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<DetectorEventCountAggregation>> DetectorEventCountAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="PhaseCycleAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<PhaseCycleAggregation>> PhaseCycleAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="PhaseLeftTurnGapAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<PhaseLeftTurnGapAggregation>> PhaseLeftTurnGapAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="PhaseSplitMonitorAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<PhaseSplitMonitorAggregation>> PhaseSplitMonitorAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="PhaseTerminationAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<PhaseTerminationAggregation>> PhaseTerminationAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="PreemptionAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<PreemptionAggregation>> PreemptionAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="PriorityAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<PriorityAggregation>> PriorityAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="SignalEventCountAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<SignalEventCountAggregation>> SignalEventCountAggregations { get; set; }

        /// <summary>
        /// <inheritdoc cref="SignalPlanAggregation"/>
        /// </summary>
        public virtual DbSet<CompressedAggregations<SignalPlanAggregation>> SignalPlanAggregations { get; set; }

        /// <inheritdoc/>
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().AreUnicode(false);

            if (Database.IsNpgsql())
                configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp");
            //else
            //    configurationBuilder.Properties<DateTime>().HaveColumnType("datetime");
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CompressedAggregationBase>(builder =>
            {
                builder.ToTable(t => t.HasComment("Compressed aggregations"));

                builder.HasKey(e => new { e.LocationIdentifier, e.ArchiveDate, e.DataType, e.Start, e.End });

                builder.Property(e => e.LocationIdentifier)
                    .IsRequired()
                    .HasMaxLength(10);

                builder.Property(e => e.ArchiveDate)
                //.IsRequired()
                .HasColumnType("Date")
                .HasConversion<DateTime>(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v));

                builder.Property(p => p.DataType)
                .HasMaxLength(32)
                .HasConversion(new CompressionTypeConverter(typeof(AggregationModelBase).Namespace.ToString(), typeof(AggregationModelBase).Assembly.ToString()));

                builder.HasDiscriminator(d => d.DataType)
                .AddCompressedTableDiscriminators(typeof(AggregationModelBase), typeof(CompressedAggregations<>));

                builder.Property(e => e.Data)
                .HasConversion<CompressedListConverter<AggregationModelBase>, AbstractListComparer<AggregationModelBase>>();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}