using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ControllerTypeConfiguration : IEntityTypeConfiguration<ControllerType>
    {
        public void Configure(EntityTypeBuilder<ControllerType> builder)
        {
            builder.Property(e => e.Id)
                .ValueGeneratedNever();
            //.HasColumnName("ControllerTypeId");

            //builder.Property(e => e.ActiveFtp).HasColumnName("ActiveFTP");

            builder.Property(e => e.Description)
                .HasMaxLength(50);
                //.IsUnicode(false);

            //builder.Property(e => e.Ftpdirectory)
                //.IsUnicode(false);
            //.HasColumnName("FTPDirectory");

            builder.Property(e => e.Password)
                .HasMaxLength(50);
                //.IsUnicode(false);

            //builder.Property(e => e.Snmpport).HasColumnName("SNMPPort");

            builder.Property(e => e.UserName)
                .HasMaxLength(50);
            //.IsUnicode(false);

            builder.HasData(new ControllerType() { Id = 0, Description = "Unknown", Ftpdirectory = "root", UserName = "user", Password = "password" });
        }
    }
}
