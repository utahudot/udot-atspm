using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Utah.Udot.Atspm.Data.Models.IdentityModels;

namespace Utah.Udot.Atspm.Data.Configuration.IdentityConfiguration
{
    /// <summary>
    /// Configuration for <see cref="ApiKey"/>
    /// </summary>
    public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<ApiKey> builder)
        {
            builder.ToTable(t => t.HasComment("API keys used for authentication and authorization."));

            builder.HasIndex(e => e.KeyHash).IsUnique();

            builder.Property(e => e.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(e => e.OwnerId)
                   .IsRequired();
        }
    }
}
