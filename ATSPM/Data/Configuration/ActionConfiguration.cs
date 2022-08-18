using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class ActionConfiguration : IEntityTypeConfiguration<Models.Action>
    {
        public void Configure(EntityTypeBuilder<Models.Action> builder)
        {
            //builder.Property(e => e.Id).HasColumnName("ActionID");

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
