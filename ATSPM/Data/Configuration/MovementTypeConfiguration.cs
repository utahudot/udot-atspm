using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ATSPM.Data.Configuration
{
    public class MovementTypeConfiguration : IEntityTypeConfiguration<MovementType>
    {
        public void Configure(EntityTypeBuilder<MovementType> builder)
        {
            builder.HasComment("Movement Types");

            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Abbreviation)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasData(typeof(MovementTypes).GetFields().Where(t => t.FieldType == typeof(MovementTypes)).Select(s => new MovementType()
            {
                Id = (MovementTypes)s.GetValue(s),
                Description = s.GetCustomAttribute<DisplayAttribute>().Name,
                Abbreviation = s.GetValue(s).ToString(),
                DisplayOrder = s.GetCustomAttribute<DisplayAttribute>().Order
            }));
        }
    }
}
