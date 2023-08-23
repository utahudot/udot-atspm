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
            builder.HasComment("Action Logs");
            
            builder.HasIndex(e => e.AgencyId);

            builder.Property(e => e.Comment)
                .HasMaxLength(255);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.SignalIdentifier)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}
