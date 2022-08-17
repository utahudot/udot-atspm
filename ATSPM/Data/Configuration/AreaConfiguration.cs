using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ATSPM.Data.Configuration
{
    public class AreaConfiguration : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            builder.Property(e => e.Name).HasMaxLength(50);

            //builder.HasMany(d => d.Signals)
            //    .WithMany(p => p.Areas)
            //    .UsingEntity<Dictionary<string, object>>(
            //        "AreaSignal",
            //        l => l.HasOne<Signal>().WithMany().HasForeignKey("SignalVersionId").HasConstraintName("FK_dbo.AreaSignals_dbo.Signals_Signal_VersionID"),
            //        r => r.HasOne<Area>().WithMany().HasForeignKey("AreaId").HasConstraintName("FK_dbo.AreaSignals_dbo.Areas_Area_Id"),
            //        j =>
            //        {
            //            j.HasKey("AreaId", "SignalVersionId").HasName("PK_dbo.AreaSignals");

            //            j.ToTable("AreaSignals");

            //            j.HasIndex(new[] { "AreaId" }, "IX_Area_Id");

            //            j.HasIndex(new[] { "SignalVersionId" }, "IX_Signal_VersionID");

            //            j.IndexerProperty<int>("AreaId").HasColumnName("Area_Id");

            //            j.IndexerProperty<int>("SignalVersionId").HasColumnName("Signal_VersionID");
            //        });

            builder.HasData(new Area() { Id = 1, Name = "Unknown"});
        }
    }
}
