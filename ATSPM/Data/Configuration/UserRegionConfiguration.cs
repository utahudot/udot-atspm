using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ATSPM.Data.Models;

namespace ATSPM.Data.Configuration
{
    public class UserRegionConfiguration : IEntityTypeConfiguration<UserRegion>
    {
        public void Configure(EntityTypeBuilder<UserRegion> builder)
        {
            builder
            .HasKey(ur => new { ur.UserId, ur.RegionId });

            //builder
            //    .HasOne(ur => ur.User)
            //    .WithMany(u => u.UserRegions)
            //    .HasForeignKey(ur => ur.UserId);

            builder
                .HasOne(ur => ur.Region)
                .WithMany(r => r.UserRegions)
                .HasForeignKey(ur => ur.RegionId);
        }
    }
}
