using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class ApplicationConfiguration : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.HasComment("Application Types");

            builder.HasData(typeof(ApplicationTypes).GetFields().Where(t => t.FieldType == typeof(ApplicationTypes)).Select(s => new Application()
            {
                Id = (ApplicationTypes)s.GetValue(s),
                Name = s.GetValue(s).ToString()
            }));
        }
    }
}
