﻿using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachPcdAggregationConfiguration : IEntityTypeConfiguration<ApproachPcdAggregation>
    {
        public void Configure(EntityTypeBuilder<ApproachPcdAggregation> builder)
        {
            builder.HasComment("Approach Pcd Aggregation");

            builder.HasKey(e => new { e.BinStartTime, e.LocationIdentifier, e.PhaseNumber, e.IsProtectedPhase });

            builder.Property(e => e.LocationIdentifier).HasMaxLength(10);
        }
    }
}