using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class RegionConfiguration : IEntityTypeConfiguration<Region>
    {
        public void Configure(EntityTypeBuilder<Region> builder)
        {
            //builder.ToTable("Region");

            builder.Property(e => e.Id).ValueGeneratedNever();

            builder.Property(e => e.Description).HasMaxLength(50);

            builder.HasData(new Region() { Id = 1, Description = "Unknown"});
        }
    }
}
