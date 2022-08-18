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
            //builder.HasKey(e => e.CommentId)
            //    .HasName("PK_dbo.MetricComments");

            //builder.HasIndex(e => e.VersionId, "IX_VersionID");

            //builder.Property(e => e.CommentId).HasColumnName("CommentID");

            //builder.Property(e => e.CommentText).IsRequired();

            //builder.Property(e => e.SignalId)
            //    .HasMaxLength(10)
            //    .HasColumnName("SignalID");

            //builder.Property(e => e.TimeStamp).HasColumnType("datetime");

            //builder.Property(e => e.VersionId).HasColumnName("VersionID");

            //builder.HasMany(d => d.MetricTypeMetrics)
            //    .WithMany(p => p.MetricCommentComments)
            //    .UsingEntity<Dictionary<string, object>>(
            //        "MetricCommentMetricType",
            //        l => l.HasOne<MetricType>().WithMany().HasForeignKey("MetricTypeMetricId").HasConstraintName("FK_dbo.MetricCommentMetricTypes_dbo.MetricTypes_MetricType_MetricID"),
            //        r => r.HasOne<MetricComment>().WithMany().HasForeignKey("MetricCommentCommentId").HasConstraintName("FK_dbo.MetricCommentMetricTypes_dbo.MetricComments_MetricComment_CommentID"),
            //        j =>
            //        {
            //            j.HasKey("MetricCommentCommentId", "MetricTypeMetricId").HasName("PK_dbo.MetricCommentMetricTypes");

            //            j.ToTable("MetricCommentMetricTypes");

            //            j.HasIndex(new[] { "MetricCommentCommentId" }, "IX_MetricComment_CommentID");

            //            j.HasIndex(new[] { "MetricTypeMetricId" }, "IX_MetricType_MetricID");

            //            j.IndexerProperty<int>("MetricCommentCommentId").HasColumnName("MetricComment_CommentID");

            //            j.IndexerProperty<int>("MetricTypeMetricId").HasColumnName("MetricType_MetricID");
            //        });
        }
    }
}
