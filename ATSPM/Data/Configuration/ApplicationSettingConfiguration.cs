using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class ApplicationSettingConfiguration : IEntityTypeConfiguration<ApplicationSetting>
    {
        public void Configure(EntityTypeBuilder<ApplicationSetting> builder)
        {
            //builder.HasIndex(e => e.ApplicationId, "IX_ApplicationID");

            //builder.Property(e => e.Id).HasColumnName("ID");

            //builder.Property(e => e.ApplicationId).HasColumnName("ApplicationID");

            builder.Property(e => e.Discriminator)
                .IsRequired()
                .HasMaxLength(128);

            //builder.Property(e => e.PreviousDayPmpeakEnd).HasColumnName("PreviousDayPMPeakEnd");

            //builder.Property(e => e.PreviousDayPmpeakStart).HasColumnName("PreviousDayPMPeakStart");

            //builder.HasOne(d => d.Application)
            //    .WithMany(p => p.ApplicationSettings)
            //    .HasForeignKey(d => d.ApplicationId)
            //    .HasConstraintName("FK_dbo.ApplicationSettings_dbo.Applications_ApplicationID");
        }
    }
}
