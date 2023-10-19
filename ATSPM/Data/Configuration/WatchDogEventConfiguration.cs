using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class WatchDogEventConfiguration : IEntityTypeConfiguration<WatchDogLogEvent>
    {
        public void Configure(EntityTypeBuilder<WatchDogLogEvent> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
               .ValueGeneratedOnAdd();
            builder.Property(e => e.Details)
               .HasMaxLength(250)
               .IsRequired();
        }
    }
}
