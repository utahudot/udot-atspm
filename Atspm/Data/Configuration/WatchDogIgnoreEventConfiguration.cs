using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Utah.Udot.Atspm.Data.Configuration
{
    public class WatchDogIgnoreEventConfiguration : IEntityTypeConfiguration<WatchDogIgnoreEvent>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<WatchDogIgnoreEvent> builder)
        {
            builder
            .HasKey(w => new { w.LocationId, w.Start, w.End, w.IssueType, w.Phase, w.ComponentType, w.ComponentId });

            builder
                .HasOne(w => w.Location);
        }
    }
}
