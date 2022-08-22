using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ATSPM.Data.Configuration
{
    public class LaneTypeConfiguration : IEntityTypeConfiguration<LaneType>
    {
        public void Configure(EntityTypeBuilder<LaneType> builder)
        {
            builder.HasComment("Lane Types");
            
            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Abbreviation)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(30);

            builder.HasData(typeof(LaneTypes).GetFields().Where(t => t.FieldType == typeof(LaneTypes)).Select(s => new LaneType()
            {
                Id = (LaneTypes)s.GetValue(s),
                Description = s.GetCustomAttribute<DisplayAttribute>().Name,
                Abbreviation = s.GetValue(s).ToString()
            }));
        }
    }
}
