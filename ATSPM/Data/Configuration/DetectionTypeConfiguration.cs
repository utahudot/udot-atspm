using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Detection type configuration
    /// </summary>
    public class DetectionTypeConfiguration : IEntityTypeConfiguration<DetectionType>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<DetectionType> builder)
        {
            builder.ToTable(t => t.HasComment("Detector Types"));

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Abbreviation).HasMaxLength(5);

            builder.Property(e => e.Description).HasMaxLength(128).IsRequired();

            builder.HasData(typeof(DetectionTypes).GetFields().Where(t => t.FieldType == typeof(DetectionTypes)).Select(s => new DetectionType()
            {
                Id = (DetectionTypes)s.GetValue(s),
                Description = s.GetCustomAttribute<DisplayAttribute>().Name,
                Abbreviation = s.GetValue(s).ToString(),
                DisplayOrder = s.GetCustomAttribute<DisplayAttribute>().Order
            }));
        }
    }
}
