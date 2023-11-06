using ATSPM.Data.Enums;
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    /// <summary>
    /// Controller type configuration
    /// </summary>
    public class ControllerTypeConfiguration : IEntityTypeConfiguration<ControllerType>
    {
        /// <inheritdoc/>
        public void Configure(EntityTypeBuilder<ControllerType> builder)
        {
            builder.HasComment("Signal Controller Types");

            builder.Property(e => e.Product)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Firmware)
                .HasMaxLength(32);

            builder.Property(e => e.Protocol)
                .HasMaxLength(12)
                .HasDefaultValue(TransportProtocols.Unknown);

            builder.Property(e => e.Port)
                .HasDefaultValueSql("((0))");

            builder.Property(e => e.Directory)
                .IsRequired(false)
                .HasMaxLength(1024);

            builder.Property(e => e.SearchTerm)
                .IsRequired(false)
                .HasMaxLength(128);

            builder.Property(e => e.Password)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(e => e.UserName)
                .IsRequired(false)
                .HasMaxLength(50);

            //Christian, we will want to change the "Controller Type" names in ATSPM to the following so they make sense: "ASC3" - change to "ASC3-2.54+"; "Cobalt" - change to "ASC3-32.54+"; "EOS" - change to "ASC3 32.67.20+"; create a new one that says, "ASC3-32.68.40+".

            //builder.HasData
            //    (
            //        new ControllerType()
            //        {
            //            Id = 0,
            //            Description = "Unknown",
            //            Snmpport = 161,
            //            LogDirectory = "root",
            //            ActiveFtp = false,
            //            UserName = "user",
            //            Password = "password"
            //        },
            //        new ControllerType
            //        {
            //            Id = 1,
            //            Description = "ASC3",
            //            Snmpport = 161,
            //            LogDirectory = "//Set1",
            //            ActiveFtp = true,
            //            UserName = "econolite",
            //            Password = "ecpi2ecpi"
            //        },
            //    new ControllerType
            //    {
            //        Id = 2,
            //        Description = "Cobalt",
            //        Snmpport = 161,
            //        LogDirectory = "/set1",
            //        ActiveFtp = true,
            //        UserName = "econolite",
            //        Password = "ecpi2ecpi"
            //    },
            //    new ControllerType
            //    {
            //        Id = 3,
            //        Description = "ASC3 - 2070",
            //        Snmpport = 161,
            //        LogDirectory = "/set1",
            //        ActiveFtp = true,
            //        UserName = "econolite",
            //        Password = "ecpi2ecpi"
            //    },
            //    new ControllerType
            //    {
            //        Id = 4,
            //        Description = "MaxTime",
            //        Snmpport = 161,
            //        LogDirectory = "v1/asclog/xml/full",
            //        ActiveFtp = false,
            //        UserName = "none",
            //        Password = "none"
            //    },
            //    new ControllerType
            //    {
            //        Id = 5,
            //        Description = "Trafficware",
            //        Snmpport = 161,
            //        LogDirectory = "none",
            //        ActiveFtp = true,
            //        UserName = "none",
            //        Password = "none"
            //    },
            //    new ControllerType
            //    {
            //        Id = 6,
            //        Description = "Siemens SEPAC",
            //        Snmpport = 161,
            //        LogDirectory = "/mnt/sd",
            //        ActiveFtp = false,
            //        UserName = "admin",
            //        Password = "$adm*kon2"
            //    },
            //    new ControllerType
            //    {
            //        Id = 7,
            //        Description = "McCain ATC EX",
            //        Snmpport = 161,
            //        LogDirectory = " /mnt/rd/hiResData",
            //        ActiveFtp = false,
            //        UserName = "root",
            //        Password = "root"
            //    },
            //    new ControllerType
            //    {
            //        Id = 8,
            //        Description = "Peek",
            //        Snmpport = 161,
            //        LogDirectory = "mnt/sram/cuLogging",
            //        ActiveFtp = false,
            //        UserName = "atc",
            //        Password = "PeekAtc"
            //    },
            //    new ControllerType
            //    {
            //        Id = 9,
            //        Description = "EOS",
            //        Snmpport = 161,
            //        LogDirectory = "/set1",
            //        ActiveFtp = true,
            //        UserName = "econolite",
            //        Password = "ecpi2ecpi"
            //    },
            //    new ControllerType
            //    {
            //        Id = 10,
            //        Description = "New Cobalt",
            //        Snmpport = 161,
            //        LogDirectory = "/opt/econolite/set1",
            //        ActiveFtp = true,
            //        UserName = "econolite",
            //        Password = "ecpi2ecpi"
            //    });
        }
    }
}
