using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class DetectionHardwareConfiguration : IEntityTypeConfiguration<DetectionHardware>
    {
        public void Configure(EntityTypeBuilder<DetectionHardware> builder)
        {
            builder.HasComment("Dectector Hardware Types");
            
            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Name).IsRequired();

            builder.HasData(typeof(DetectionHardwareTypes).GetFields().Where(t => t.FieldType == typeof(DetectionHardwareTypes)).Select(s => new DetectionHardware()
            {
                Id = (DetectionHardwareTypes)s.GetValue(s),
                Name = s.GetValue(s).ToString()
            }));
        }
    }
}
