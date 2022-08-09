using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace ATSPM.Data.Configuration
{
    public class ControllerLogArchiveConfiguration : IEntityTypeConfiguration<ControllerLogArchive>
    {
        public void Configure(EntityTypeBuilder<ControllerLogArchive> builder)
        {
            builder.HasComment("Compressed Event Log Data");

            builder.HasKey(e => new { e.SignalId, e.ArchiveDate }); //.HasName("PK_Controller_Log_Archive");

            //builder.HasIndex(e => new { e.SignalId, e.ArchiveDate },"IX_Controller_Log_Archive").IsUnique();

            builder.Property(e => e.ArchiveDate)
                .HasColumnType("date")
                .Metadata.AddAnnotation("KeyNameFormat", "dd-MM-yyyy");

            builder.Property(e => e.SignalId)
                    .IsRequired()
                    .HasMaxLength(10);
            //.HasColumnName("SignalId");

            //https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=fluent-api

            builder.Property(e => e.LogData)
                    .HasConversion<byte[]>(
                    v => JsonSerializer.Serialize(v.Select(c => new { c.EventCode, c.EventParam, c.Timestamp }), new JsonSerializerOptions()).GZipCompressToByte(),
                    v => JsonSerializer.Deserialize<List<ControllerEventLog>>(v.GZipDecompressToString(), new JsonSerializerOptions()),

                    new ValueComparer<ICollection<ControllerEventLog>>((c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        }
    }
}
