using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Area configuration
    /// </summary>
    public class AreaConfiguration : IEntityTypeConfiguration<Area>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            builder.ToTable(t => t.HasComment("Areas"));

            builder.Property(e => e.Name).HasMaxLength(50);

            //builder.HasData(new Area() { Name = "Unknown"});
        }
    }
}
