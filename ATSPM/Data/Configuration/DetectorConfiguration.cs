using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class DetectorConfiguration : IEntityTypeConfiguration<Detector>
    {
        public void Configure(EntityTypeBuilder<Detector> builder)
        {
            builder.HasIndex(e => e.ApproachId);

            builder.HasIndex(e => e.DetectionHardwareId);

            builder.HasIndex(e => e.LaneTypeId);

            builder.HasIndex(e => e.MovementTypeId);

            //builder.Property(e => e.Id).HasColumnName("ID");

            //builder.Property(e => e.ApproachId).HasColumnName("ApproachID");

            //builder.Property(e => e.DateAdded).HasColumnType("datetime");

            //builder.Property(e => e.DateDisabled).HasColumnType("datetime");

            //builder.Property(e => e.DetectionHardwareId).HasColumnName("DetectionHardwareID");

            builder.Property(e => e.DetectorId)
                .IsRequired()
                .HasMaxLength(50);
                //.HasColumnName("DetectorID");

            //builder.Property(e => e.LaneTypeId).HasColumnName("LaneTypeID");

            //builder.Property(e => e.MovementTypeId).HasColumnName("MovementTypeID");

            //builder.HasOne(d => d.Approach)
            //    .WithMany(p => p.Detectors)
            //    .HasForeignKey(d => d.ApproachId)
            //    .HasConstraintName("FK_dbo.Detectors_dbo.Approaches_ApproachID");

            //builder.HasOne(d => d.DetectionHardware)
            //    .WithMany(p => p.Detectors)
            //    .HasForeignKey(d => d.DetectionHardwareId)
            //    .HasConstraintName("FK_dbo.Detectors_dbo.DetectionHardwares_DetectionHardwareID");

            //builder.HasOne(d => d.LaneType)
            //    .WithMany(p => p.Detectors)
            //    .HasForeignKey(d => d.LaneTypeId)
            //    .HasConstraintName("FK_dbo.Detectors_dbo.LaneTypes_LaneTypeID");

            //builder.HasOne(d => d.MovementType)
            //    .WithMany(p => p.Detectors)
            //    .HasForeignKey(d => d.MovementTypeId)
            //    .HasConstraintName("FK_dbo.Detectors_dbo.MovementTypes_MovementTypeID");

            //builder.HasMany(d => d.DetectionTypes)
            //    .WithMany(p => p.Ids)
            //    .UsingEntity<Dictionary<string, object>>(
            //        "DetectionTypeDetector",
            //        l => l.HasOne<DetectionType>().WithMany().HasForeignKey("DetectionTypeId").HasConstraintName("FK_dbo.DetectionTypeDetector_dbo.DetectionTypes_DetectionTypeID"),
            //        r => r.HasOne<Detector>().WithMany().HasForeignKey("Id").HasConstraintName("FK_dbo.DetectionTypeDetector_dbo.Detectors_ID"),
            //        j =>
            //        {
            //            j.HasKey("Id", "DetectionTypeId").HasName("PK_dbo.DetectionTypeDetector");

            //            j.ToTable("DetectionTypeDetector");

            //            j.HasIndex(new[] { "DetectionTypeId" }, "IX_DetectionTypeID");

            //            j.HasIndex(new[] { "Id" }, "IX_ID");

            //            j.IndexerProperty<int>("Id").HasColumnName("ID");

            //            j.IndexerProperty<int>("DetectionTypeId").HasColumnName("DetectionTypeID");
            //        });
        }
    }
}
