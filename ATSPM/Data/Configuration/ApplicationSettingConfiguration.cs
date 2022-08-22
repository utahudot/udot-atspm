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
            builder.HasComment("Application Settings");

            builder.HasIndex(e => e.ApplicationId);

            builder.Property(e => e.Discriminator)
                .IsRequired()
                .HasMaxLength(128);
        }
    }
}
