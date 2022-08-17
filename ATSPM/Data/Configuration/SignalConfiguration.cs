using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class SignalConfiguration : IEntityTypeConfiguration<Signal>
    {
        public void Configure(EntityTypeBuilder<Signal> builder)
        {
            //builder.HasKey(e => e.Id);
            //.HasName("PK_dbo.Signals");

            //builder.HasIndex(e => e.ControllerTypeId, "IX_ControllerTypeId");

            //builder.HasIndex(e => e.JurisdictionId, "IX_JurisdictionId");

            //builder.HasIndex(e => e.RegionId, "IX_RegionID");

            //builder.HasIndex(e => e.VersionActionId, "IX_VersionActionId");

            //builder.Property(e => e.VersionId).HasColumnName("VersionID");

            //builder.Property(e => e.ControllerTypeId).HasColumnName("ControllerTypeId");

            builder.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(15)
                .HasDefaultValueSql("('127.0.0.1')");
                //.IsUnicode(false)
                //.HasColumnName("IPAddress")

            builder.Property(e => e.JurisdictionId).HasDefaultValueSql("((1))");

            builder.Property(e => e.Latitude)
                .IsRequired()
                .HasMaxLength(30);
            //.IsUnicode(false);

            builder.Property(e => e.Longitude)
                .IsRequired()
                .HasMaxLength(30);
                //.IsUnicode(false);

            builder.Property(e => e.Note)
                .IsRequired()
                .HasDefaultValueSql("('Initial')");

            builder.Property(e => e.PrimaryName)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValueSql("('')");
            //.IsUnicode(false)

            //builder.Property(e => e.RegionId).HasColumnName("RegionID");

            builder.Property(e => e.SecondaryName)
                //.IsRequired()
                .HasMaxLength(100);
            //.HasDefaultValueSql("('')");
            //.IsUnicode(false)

            builder.Property(e => e.SignalId)
                .IsRequired()
                .HasMaxLength(10);
            //.HasColumnName("SignalID");

            //builder.Property(e => e.Start).HasColumnType("datetime");

            builder.Property(e => e.VersionActionId).HasDefaultValueSql("((10))");

            //builder.HasOne(d => d.ControllerType)
            //    .WithMany(p => p.Signals)
            //    .HasForeignKey(d => d.ControllerTypeId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK_dbo.Signals_dbo.ControllerTypes_ControllerTypeId");

            //builder.HasOne(d => d.Jurisdiction)
            //    .WithMany(p => p.Signals)
            //    .HasForeignKey(d => d.JurisdictionId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK_dbo.Signals_dbo.Jurisdictions_JurisdictionId");

            //builder.HasOne(d => d.Region)
            //    .WithMany(p => p.Signals)
            //    .HasForeignKey(d => d.RegionId)
            //    .HasConstraintName("FK_dbo.Signals_dbo.Region_RegionID");
        }
    }
}
