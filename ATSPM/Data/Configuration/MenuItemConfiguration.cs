using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using ATSPM.Domain.Extensions;

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
                .HasMaxLength(24);

            builder.Property(e => e.Icon)
                .HasMaxLength(1024);

            builder.Property(e => e.Link)
                .HasMaxLength(512);

            builder.HasOne(d => d.Parent).WithMany(m => m.Children).HasForeignKey(d => d.ParentId);

            builder.Ignore(i => i.HasLink).Ignore(i => i.HasDocument);
        }
    }
}
