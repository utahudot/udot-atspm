﻿using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PhaseCycleAggregationConfiguration : IEntityTypeConfiguration<PhaseCycleAggregation>
    {
        public void Configure(EntityTypeBuilder<PhaseCycleAggregation> builder)
        {
            builder.HasComment("Phase Cycle Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.SignalIdentifier, e.PhaseNumber });

            builder.Property(e => e.SignalIdentifier).HasMaxLength(10);
        }
    }
}