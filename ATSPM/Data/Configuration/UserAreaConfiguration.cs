using ATSPM.Data.Models.ConfigurationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class UserAreaConfiguration : IEntityTypeConfiguration<UserArea>
    {
        public void Configure(EntityTypeBuilder<UserArea> builder)
        {
            builder.ToTable(t => t.HasComment("UserAreas"));

            builder
            .HasKey(ur => new { ur.UserId, ur.AreaId });

            //builder
            //    .HasOne(ur => ur.User)
            //    .WithMany(u => u.UserAreas)
            //    .HasForeignKey(ur => ur.UserId);

            builder
                .HasOne(ur => ur.Area)
                .WithMany(r => r.UserAreas)
                .HasForeignKey(ur => ur.AreaId);
        }
    }
}
