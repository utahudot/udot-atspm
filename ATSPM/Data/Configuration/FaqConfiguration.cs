using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace ATSPM.Data.Configuration
{
    public class FaqConfiguration : IEntityTypeConfiguration<Faq>
    {
        public void Configure(EntityTypeBuilder<Faq> builder)
        {
            //builder.ToTable("FAQs");

            //builder.Property(e => e.Faqid).HasColumnName("FAQID");

            builder.Property(e => e.Body).IsRequired();

            builder.Property(e => e.Header).IsRequired();
        }
    }
}
