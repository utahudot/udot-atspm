using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class MeasuresDefaultConfiguration : IEntityTypeConfiguration<MeasuresDefault>
    {
        public void Configure(EntityTypeBuilder<MeasuresDefault> builder)
        {
            //builder.HasKey(e => new { e.Measure, e.OptionName })
            //    .HasName("PK_dbo.MeasuresDefaults");

            builder.HasIndex(i => i.Measure).IsUnique();

            builder.HasIndex(i => i.OptionName).IsUnique();

            builder.Property(e => e.Measure).HasMaxLength(128);

            builder.Property(e => e.OptionName).HasMaxLength(128);

            builder.Property(e => e.Value).HasMaxLength(512);
        }
    }
}
