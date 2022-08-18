using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class ExternalLinkConfiguration : IEntityTypeConfiguration<ExternalLink>
    {
        public void Configure(EntityTypeBuilder<ExternalLink> builder)
        {
            //builder.Property(e => e.ExternalLinkId).HasColumnName("ExternalLinkID");

            builder.Property(e => e.Name).IsRequired();

            builder.Property(e => e.Url).IsRequired();
        }
    }
}
