using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// External links configuration
    /// </summary>
    public class ExternalLinkConfiguration : IEntityTypeConfiguration<ExternalLink>
    {
        ///<inheritdoc/>
        public void Configure(EntityTypeBuilder<ExternalLink> builder)
        {
            builder.HasComment("External Links");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(512);

            builder.HasData(
                new ExternalLink
                {
                    Id = 1,
                    Name = "Indiana Hi Resolution Data Logger Enumerations",
                    DisplayOrder = 1,
                    Url = " https://docs.lib.purdue.edu/jtrpdata/3/"
                },
                new ExternalLink
                {
                    Id = 2,
                    Name = "Florida ATSPM",
                    DisplayOrder = 2,
                    Url = "https://atspm.cflsmartroads.com/ATSPM"
                },
                new ExternalLink
                {
                    Id = 3,
                    Name = "FAST (Southern Nevada)",
                    DisplayOrder = 3,
                    Url = "http://challenger.nvfast.org/spm"
                },
                new ExternalLink
                {
                    Id = 4,
                    Name = "Georgia ATSPM",
                    DisplayOrder = 4,
                    Url = "https://traffic.dot.ga.gov/atspm"
                },
                new ExternalLink
                {
                    Id = 5,
                    Name = "Arizona ATSPM",
                    DisplayOrder = 5,
                    Url = "http://spmapp01.mcdot-its.com/ATSPM"
                },
                new ExternalLink
                {
                    Id = 6,
                    Name = "Alabama ATSPM",
                    DisplayOrder = 6,
                    Url = "http://signalmetrics.ua.edu"
                },
                new ExternalLink
                {
                    Id = 7,
                    Name = "ATSPM Workshop 2016 SLC",
                    DisplayOrder = 7,
                    Url = "http://docs.lib.purdue.edu/atspmw/2016"
                },
                new ExternalLink
                {
                    Id = 8,
                    Name = "Train The Trainer Webinar Day 1 - Morning",
                    DisplayOrder = 8,
                    Url = "https://connectdot.connectsolutions.com/p75dwqefphk   "
                },
                new ExternalLink
                {
                    Id = 9,
                    Name = "Train The Trainer Webinar Day 1 - Afternoon",
                    DisplayOrder = 9,
                    Url = "https://connectdot.connectsolutions.com/p6l6jaoy3gj"
                },
                new ExternalLink
                {
                    Id = 10,
                    Name = "Train The Trainer Webinar Day 2 - Morning",
                    DisplayOrder = 10,
                    Url = "https://connectdot.connectsolutions.com/p6mlkvekogo/"
                },
                new ExternalLink
                {
                    Id = 11,
                    Name = "Train The Trainer Webinar Day 2 - Mid Morning",
                    DisplayOrder = 11,
                    Url = "https://connectdot.connectsolutions.com/p3ua8gtj09r/"
                });
        }
    }
}
