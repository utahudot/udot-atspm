using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class MetricCommentConfiguration : IEntityTypeConfiguration<MetricComment>
    {
        public void Configure(EntityTypeBuilder<MetricComment> builder)
        {
            builder.HasComment("Metric Comments");

            //builder.HasIndex(e => e.SignalId);
        }
    }
}
