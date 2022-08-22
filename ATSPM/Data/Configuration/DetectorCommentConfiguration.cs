using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class DetectorCommentConfiguration : IEntityTypeConfiguration<DetectorComment>
    {
        public void Configure(EntityTypeBuilder<DetectorComment> builder)
        {
            builder.HasComment("Detector Comments");

            builder.HasIndex(e => e.Id);
        }
    }
}
