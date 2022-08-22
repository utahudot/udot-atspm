using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class VersionActionConfiguration : IEntityTypeConfiguration<VersionAction>
    {
        public void Configure(EntityTypeBuilder<VersionAction> builder)
        {
            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.HasData(Enum.GetValues<SignaVersionActions>().Select(s => new VersionAction() { Id = s, Description = s.ToString() }));
        }
    }
}
