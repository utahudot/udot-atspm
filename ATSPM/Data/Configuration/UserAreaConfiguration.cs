using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class UserAreaConfiguration : IEntityTypeConfiguration<UserArea>
    {
        public void Configure(EntityTypeBuilder<UserArea> builder)
        {
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
