using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATSPM.Data.Models;

namespace ATSPM.Data.Configuration
{
    public class UserJurisdictionConfiguration : IEntityTypeConfiguration<UserJurisdiction>
    {
        public void Configure(EntityTypeBuilder<UserJurisdiction> builder)
        {
            builder
            .HasKey(ur => new { ur.UserId, ur.JurisdictionId });

            //builder
            //    .HasOne(ur => ur.User)
            //    .WithMany(u => u.UserJurisdictions)
            //    .HasForeignKey(ur => ur.UserId);

            builder
                .HasOne(ur => ur.Jurisdiction)
                .WithMany(r => r.UserJurisdictions)
                .HasForeignKey(ur => ur.JurisdictionId);
        }
    }
}
