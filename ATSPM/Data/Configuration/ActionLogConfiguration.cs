using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class ActionLogConfiguration : IEntityTypeConfiguration<ActionLog>
    {
        public void Configure(EntityTypeBuilder<ActionLog> builder)
        {
            //builder.HasIndex(e => e.AgencyId, "IX_AgencyID");

            //builder.Property(e => e.ActionLogId).HasColumnName("ActionLogID");

            //builder.Property(e => e.AgencyId).HasColumnName("AgencyID");

            builder.Property(e => e.Comment).HasMaxLength(255);

            //builder.Property(e => e.Date).HasColumnType("datetime");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.SignalId)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("SignalID");

            //    entity.HasOne(d => d.Agency)
            //        .WithMany(p => p.ActionLogs)
            //        .HasForeignKey(d => d.AgencyId)
            //        .HasConstraintName("FK_dbo.ActionLogs_dbo.Agencies_AgencyID");

            //    entity.HasMany(d => d.ActionActions)
            //        .WithMany(p => p.ActionLogActionLogs)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "ActionLogAction",
            //            l => l.HasOne<Models.Action>().WithMany().HasForeignKey("ActionActionId").HasConstraintName("FK_dbo.ActionLogActions_dbo.Actions_Action_ActionID"),
            //            r => r.HasOne<ActionLog>().WithMany().HasForeignKey("ActionLogActionLogId").HasConstraintName("FK_dbo.ActionLogActions_dbo.ActionLogs_ActionLog_ActionLogID"),
            //            j =>
            //            {
            //                j.HasKey("ActionLogActionLogId", "ActionActionId").HasName("PK_dbo.ActionLogActions");

            //                j.ToTable("ActionLogActions");

            //                j.HasIndex(new[] { "ActionLogActionLogId" }, "IX_ActionLog_ActionLogID");

            //                j.HasIndex(new[] { "ActionActionId" }, "IX_Action_ActionID");

            //                j.IndexerProperty<int>("ActionLogActionLogId").HasColumnName("ActionLog_ActionLogID");

            //                j.IndexerProperty<int>("ActionActionId").HasColumnName("Action_ActionID");
            //            });

            //    entity.HasMany(d => d.MetricTypeMetrics)
            //        .WithMany(p => p.ActionLogActionLogs)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "ActionLogMetricType",
            //            l => l.HasOne<MetricType>().WithMany().HasForeignKey("MetricTypeMetricId").HasConstraintName("FK_dbo.ActionLogMetricTypes_dbo.MetricTypes_MetricType_MetricID"),
            //            r => r.HasOne<ActionLog>().WithMany().HasForeignKey("ActionLogActionLogId").HasConstraintName("FK_dbo.ActionLogMetricTypes_dbo.ActionLogs_ActionLog_ActionLogID"),
            //            j =>
            //            {
            //                j.HasKey("ActionLogActionLogId", "MetricTypeMetricId").HasName("PK_dbo.ActionLogMetricTypes");

            //                j.ToTable("ActionLogMetricTypes");

            //                j.HasIndex(new[] { "ActionLogActionLogId" }, "IX_ActionLog_ActionLogID");

            //                j.HasIndex(new[] { "MetricTypeMetricId" }, "IX_MetricType_MetricID");

            //                j.IndexerProperty<int>("ActionLogActionLogId").HasColumnName("ActionLog_ActionLogID");

            //                j.IndexerProperty<int>("MetricTypeMetricId").HasColumnName("MetricType_MetricID");
            //            });
        }
    }
}
