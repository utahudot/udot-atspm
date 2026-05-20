using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Utah.Udot.Atspm.Data.Models.IdentityModels;

namespace Utah.Udot.Atspm.Data.Configuration.IdentityConfiguration
{
    /// <summary>
    /// Configuration for <see cref="ApiKeyClaim"/>
    /// </summary>
    public class ApiKeyClaimConfiguration : IEntityTypeConfiguration<ApiKeyClaim>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<ApiKeyClaim> builder)
        {
            builder.ToTable(t => t.HasComment("Specific permissions or claims associated with an API key."));

            builder.Property(e => e.Type)
                   .IsRequired();

            builder.Property(e => e.Value)
                   .IsRequired();
        }
    }
}
