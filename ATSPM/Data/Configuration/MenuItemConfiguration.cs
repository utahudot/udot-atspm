using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Menu item configuration
    /// </summary>
    public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<MenuItem> builder)
        {
            builder.HasComment("Menu Items");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(e => e.Icon)
                .IsUnicode(true);

            builder.Property(e => e.Link)
                .HasMaxLength(4000);

            builder.HasOne(d => d.Parent).WithMany(m => m.Children).HasForeignKey(d => d.ParentId).OnDelete(DeleteBehavior.Restrict);

            builder.Ignore(i => i.HasLink).Ignore(i => i.HasDocument);
        }
    }
}
