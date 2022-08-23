using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class ActionConfiguration : IEntityTypeConfiguration<Models.Action>
    {
        public void Configure(EntityTypeBuilder<Models.Action> builder)
        {
            builder.HasComment("Action Log Types");

            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasData(typeof(ActionTypes).GetFields().Where(t => t.FieldType == typeof(ActionTypes)).Select(s => new Models.Action()
            {
                Id = (ActionTypes)s.GetValue(s),
                Description = s.GetCustomAttribute<DisplayAttribute>().Name
            }));
        }
    }
}
