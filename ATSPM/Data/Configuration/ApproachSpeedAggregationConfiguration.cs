﻿using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachSpeedAggregationConfiguration : IEntityTypeConfiguration<ApproachSpeedAggregation>
    {
        public void Configure(EntityTypeBuilder<ApproachSpeedAggregation> builder)
        {
            builder.HasComment("Approach Speed Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.SignalIdentifier, e.ApproachId });

            builder.Property(e => e.SignalIdentifier).HasMaxLength(10);
        }
    }
}