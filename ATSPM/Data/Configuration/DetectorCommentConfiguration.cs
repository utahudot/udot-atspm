using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class DetectorCommentConfiguration : IEntityTypeConfiguration<DetectorComment>
    {
        public void Configure(EntityTypeBuilder<DetectorComment> builder)
        {
            //builder.HasKey(e => e.CommentId)
            //    .HasName("PK_dbo.DetectorComments");

            //builder.HasIndex(e => e.Id, "IX_ID");

            //builder.Property(e => e.CommentId).HasColumnName("CommentID");

            //builder.Property(e => e.CommentText).IsRequired();

            //builder.Property(e => e.Id).HasColumnName("ID");

            //builder.Property(e => e.TimeStamp).HasColumnType("datetime");

            //builder.HasOne(d => d.IdNavigation)
            //    .WithMany(p => p.DetectorComments)
            //    .HasForeignKey(d => d.Id)
            //    .HasConstraintName("FK_dbo.DetectorComments_dbo.Detectors_ID");
        }
    }
}
