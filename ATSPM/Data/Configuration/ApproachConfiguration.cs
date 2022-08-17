using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachConfiguration : IEntityTypeConfiguration<Approach>
    {
        public void Configure(EntityTypeBuilder<Approach> builder)
        {
            builder.HasIndex(e => e.DirectionTypeId);

            builder.HasIndex(e => e.SignalId);

            //builder.Property(e => e.ApproachId).HasColumnName("ApproachID");

            //builder.Property(e => e.DirectionTypeId).HasColumnName("DirectionTypeID");

            //builder.Property(e => e.Mph).HasColumnName("MPH");

            //builder.Property(e => e.SignalId).HasColumnName("SignalID");

            //builder.Property(e => e.VersionId).HasColumnName("VersionID");

            //builder.HasOne(d => d.DirectionType)
            //    .WithMany(p => p.Approaches)
            //    .HasForeignKey(d => d.DirectionTypeId)
            //    .HasConstraintName("FK_dbo.Approaches_dbo.DirectionTypes_DirectionTypeID");
        }
    }
}
