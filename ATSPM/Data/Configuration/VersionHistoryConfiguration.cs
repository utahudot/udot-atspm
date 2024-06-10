using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Version history configuration
    /// </summary>
    public class VersionHistoryConfiguration : IEntityTypeConfiguration<VersionHistory>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<VersionHistory> builder)
        {
            builder.ToTable(t => t.HasComment("Version History"));

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(e => e.Notes)
                .HasMaxLength(512);

            builder.Property(e => e.Date)
                .IsRequired()
                .HasDefaultValue(DateTime.Now);

            builder.Property(e => e.Version)
                .IsRequired();

            builder.HasOne(d => d.Parent).WithMany(m => m.Children).HasForeignKey(d => d.ParentId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
