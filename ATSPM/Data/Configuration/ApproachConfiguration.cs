using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ApproachConfiguration : IEntityTypeConfiguration<Approach>
    {
        public void Configure(EntityTypeBuilder<Approach> builder)
        {
            builder.HasComment("Approaches");
            
            builder.HasIndex(e => e.DirectionTypeId);

            builder.HasIndex(e => e.SignalId);
        }
    }
}
