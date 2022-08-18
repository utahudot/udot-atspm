using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            //builder.ToTable("Menu");

            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Action)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValueSql("('')");

            builder.Property(e => e.Application)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Controller)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValueSql("('')");

            builder.Property(e => e.MenuName)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
