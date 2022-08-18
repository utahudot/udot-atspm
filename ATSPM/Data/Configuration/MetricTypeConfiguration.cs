using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class MetricTypeConfiguration : IEntityTypeConfiguration<MetricType>
    {
        public void Configure(EntityTypeBuilder<MetricType> builder)
        {
            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Abbreviation).HasMaxLength(8);

            builder.Property(e => e.ChartName).HasMaxLength(50);
        }
    }
}
