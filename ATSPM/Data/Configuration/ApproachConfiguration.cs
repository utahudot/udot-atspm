using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Approach configuration
    /// </summary>
    public class ApproachConfiguration : IEntityTypeConfiguration<Approach>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Approach> builder)
        {
            builder.HasComment("Approaches");
            
            builder.HasIndex(e => e.DirectionTypeId);

            builder.HasIndex(e => e.LocationId);
        }
    }
}
