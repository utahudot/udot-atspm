﻿using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class PhaseLeftTurnGapAggregationConfiguration : IEntityTypeConfiguration<PhaseLeftTurnGapAggregation>
    {
        public void Configure(EntityTypeBuilder<PhaseLeftTurnGapAggregation> builder)
        {
            builder.HasComment("Phase Left Turn Gap Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.SignalIdentifier, e.PhaseNumber });

            builder.Property(e => e.SignalIdentifier).HasMaxLength(10);
        }
    }
}