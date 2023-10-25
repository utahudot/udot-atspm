using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Direction type configuration
    /// </summary>
    public class DirectionTypeConfiguration : IEntityTypeConfiguration<DirectionType>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<DirectionType> builder)
        {
            builder.HasComment("Direction Types");
            
            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Abbreviation).HasMaxLength(5);

            builder.Property(e => e.Description).HasMaxLength(30);

            builder.HasData(typeof(DirectionTypes).GetFields().Where(t => t.FieldType == typeof(DirectionTypes)).Select(s => new DirectionType()
            {
                Id = (DirectionTypes)s.GetValue(s),
                Description = s.GetCustomAttribute<DisplayAttribute>().Name,
                Abbreviation = s.GetValue(s).ToString(),
                DisplayOrder = s.GetCustomAttribute<DisplayAttribute>().Order
            }));
        }
    }
}
