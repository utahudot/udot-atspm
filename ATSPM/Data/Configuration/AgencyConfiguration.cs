using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
    {
        public void Configure(EntityTypeBuilder<Agency> builder)
        {
            builder.HasComment("Agency Type for Action Logs");

            builder.Property(e => e.Description).HasMaxLength(50);

            builder.HasData(typeof(AgencyTypes).GetFields().Where(t => t.FieldType == typeof(AgencyTypes)).Select(s => new Models.Agency()
            {
                Id = (AgencyTypes)s.GetValue(s),
                Description = s.GetCustomAttribute<DisplayAttribute>().Name
            }));
        }
    }
}
