using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class DetectionTypeConfiguration : IEntityTypeConfiguration<DetectionType>
    {
        public void Configure(EntityTypeBuilder<DetectionType> builder)
        {
            //builder.Property(e => e.DetectionTypeId)
            //    .ValueGeneratedNever()
            //    .HasColumnName("DetectionTypeID");

            //builder.Property(e => e.Description).IsRequired();

            //builder.HasMany(d => d.MetricTypeMetrics)
            //    .WithMany(p => p.DetectionTypeDetectionTypes)
            //    .UsingEntity<Dictionary<string, object>>(
            //        "DetectionTypeMetricType",
            //        l => l.HasOne<MetricType>().WithMany().HasForeignKey("MetricTypeMetricId").HasConstraintName("FK_dbo.DetectionTypeMetricTypes_dbo.MetricTypes_MetricType_MetricID"),
            //        r => r.HasOne<DetectionType>().WithMany().HasForeignKey("DetectionTypeDetectionTypeId").HasConstraintName("FK_dbo.DetectionTypeMetricTypes_dbo.DetectionTypes_DetectionType_DetectionTypeID"),
            //        j =>
            //        {
            //            j.HasKey("DetectionTypeDetectionTypeId", "MetricTypeMetricId").HasName("PK_dbo.DetectionTypeMetricTypes");

            //            j.ToTable("DetectionTypeMetricTypes");

            //            j.HasIndex(new[] { "DetectionTypeDetectionTypeId" }, "IX_DetectionType_DetectionTypeID");

            //            j.HasIndex(new[] { "MetricTypeMetricId" }, "IX_MetricType_MetricID");

            //            j.IndexerProperty<int>("DetectionTypeDetectionTypeId").HasColumnName("DetectionType_DetectionTypeID");

            //            j.IndexerProperty<int>("MetricTypeMetricId").HasColumnName("MetricType_MetricID");
            //        });


            builder.Property(e => e.Id)
                .ValueGeneratedNever();
                //.HasColumnName("DirectionTypeID");

            builder.Property(e => e.Abbreviation).HasMaxLength(5);

            builder.Property(e => e.Description).IsRequired();

            builder.HasData(typeof(DetectionTypes).GetFields().Where(t => t.FieldType == typeof(DetectionTypes)).Select(s => new DetectionType()
            {
                Id = (DetectionTypes)s.GetValue(s),
                Description = s.GetCustomAttribute<DisplayAttribute>().Name,
                Abbreviation = s.GetValue(s).ToString(),
                DisplayOrder = s.GetCustomAttribute<DisplayAttribute>().Order
            }));
        }
    }
}
