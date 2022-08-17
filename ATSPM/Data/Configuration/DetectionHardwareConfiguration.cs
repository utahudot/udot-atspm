using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class DetectionHardwareConfiguration : IEntityTypeConfiguration<DetectionHardware>
    {
        public void Configure(EntityTypeBuilder<DetectionHardware> builder)
        {
            builder.Property(e => e.Id)
                .ValueGeneratedNever();
                //.HasColumnName("ID");

            builder.Property(e => e.Name).IsRequired();
        }
    }
}
