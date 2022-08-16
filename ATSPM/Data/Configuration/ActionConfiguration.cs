using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ActionConfiguration : IEntityTypeConfiguration<Models.Action>
    {
        public void Configure(EntityTypeBuilder<Models.Action> builder)
        {
            builder.Property(e => e.ActionId).HasColumnName("ActionID");

            builder.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
