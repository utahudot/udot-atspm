using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Route configuration
    /// </summary>
    public class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<Route> builder)
        {
            builder.HasComment("Location Routes");

            builder.Property(e => e.Name).HasMaxLength(50);
        }
    }
}
