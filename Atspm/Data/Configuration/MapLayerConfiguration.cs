using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Utah.Udot.Atspm.Data.Configuration
{
    /// <summary>
    /// Map Layer configuration
    /// </summary>
    public class MapLayerConfiguration : IEntityTypeConfiguration<MapLayer>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<MapLayer> builder)
        {
            builder.ToTable(t => t.HasComment("Map Layer"));

            builder.HasIndex(e => e.Name);
        }
    }
}
