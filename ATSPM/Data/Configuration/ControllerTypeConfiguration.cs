using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class ControllerTypeConfiguration : IEntityTypeConfiguration<ControllerType>
    {
        public void Configure(EntityTypeBuilder<ControllerType> builder)
        {
            builder.HasComment("Signal Controller Types");
            
            builder.Property(e => e.Id)
                .ValueGeneratedNever();

            builder.Property(e => e.Description)
                .HasMaxLength(50);

            builder.Property(e => e.Password)
                .HasMaxLength(50);

            builder.Property(e => e.UserName)
                .HasMaxLength(50);

            builder.HasData
                (
                    new ControllerType()
                    {
                        Id = 0,
                        Description = "Unknown",
                        Snmpport = 161,
                        Ftpdirectory = "root",
                        ActiveFtp = false,
                        UserName = "user",
                        Password = "password"
                    },
                    new ControllerType
                    {
                        Id = 1,
                        Description = "ASC3",
                        Snmpport = 161,
                        Ftpdirectory = "//Set1",
                        ActiveFtp = true,
                        UserName = "econolite",
                        Password = "ecpi2ecpi"
                    },
                new ControllerType
                {
                    Id = 2,
                    Description = "Cobalt",
                    Snmpport = 161,
                    Ftpdirectory = "/set1",
                    ActiveFtp = true,
                    UserName = "econolite",
                    Password = "ecpi2ecpi"
                },
                new ControllerType
                {
                    Id = 3,
                    Description = "ASC3 - 2070",
                    Snmpport = 161,
                    Ftpdirectory = "/set1",
                    ActiveFtp = true,
                    UserName = "econolite",
                    Password = "ecpi2ecpi"
                },
                new ControllerType
                {
                    Id = 4,
                    Description = "MaxTime",
                    Snmpport = 161,
                    Ftpdirectory = "v1/asclog/xml/full",
                    ActiveFtp = false,
                    UserName = "none",
                    Password = "none"
                },
                new ControllerType
                {
                    Id = 5,
                    Description = "Trafficware",
                    Snmpport = 161,
                    Ftpdirectory = "none",
                    ActiveFtp = true,
                    UserName = "none",
                    Password = "none"
                },
                new ControllerType
                {
                    Id = 6,
                    Description = "Siemens SEPAC",
                    Snmpport = 161,
                    Ftpdirectory = "/mnt/sd",
                    ActiveFtp = false,
                    UserName = "admin",
                    Password = "$adm*kon2"
                },
                new ControllerType
                {
                    Id = 7,
                    Description = "McCain ATC EX",
                    Snmpport = 161,
                    Ftpdirectory = " /mnt/rd/hiResData",
                    ActiveFtp = false,
                    UserName = "root",
                    Password = "root"
                },
                new ControllerType
                {
                    Id = 8,
                    Description = "Peek",
                    Snmpport = 161,
                    Ftpdirectory = "mnt/sram/cuLogging",
                    ActiveFtp = false,
                    UserName = "atc",
                    Password = "PeekAtc"
                },
                new ControllerType
                {
                    Id = 9,
                    Description = "EOS",
                    Snmpport = 161,
                    Ftpdirectory = "/set1",
                    ActiveFtp = true,
                    UserName = "econolite",
                    Password = "ecpi2ecpi"
                });
        }
    }
}
