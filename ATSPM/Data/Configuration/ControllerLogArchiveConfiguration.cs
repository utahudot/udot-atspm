using ATSPM.Data.Models;
using ATSPM.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ATSPM.Data.Configuration
{
    public class ControllerLogArchiveConfiguration : IEntityTypeConfiguration<ControllerLogArchive>
    {
        public void Configure(EntityTypeBuilder<ControllerLogArchive> builder)
        {
            builder.HasComment("Compressed Log Data");

            builder.HasKey(e => new { e.SignalID, e.ArchiveDate }).HasName("PK_Controller_Log_Archive");

            //entity.ToTable("Controller_Log_Archive");

            builder.HasIndex(e => new { e.SignalID, e.ArchiveDate }, "IX_Controller_Log_Archive")
                    .IsUnique();

            builder.Property(e => e.ArchiveDate)
                .HasColumnType("date")
                .Metadata.AddAnnotation("KeyNameFormat", "dd-MM-yyyy");


            builder.Property(e => e.SignalID)
                    .IsRequired()
                    .HasMaxLength(10);
            //.HasColumnName("SignalID");


            //https://docs.microsoft.com/en-us/ef/core/modeling/value-conversions?tabs=fluent-api

            builder.Property(e => e.LogData)
                    .HasConversion<byte[]>(
                    v => JsonSerializer.Serialize(v.Select(c => new { c.EventCode, c.EventParam, c.Timestamp }), new JsonSerializerOptions()).GZipCompressToByte(),
                    v => JsonSerializer.Deserialize<List<ControllerEventLog>>(v.GZipDecompressToString(), new JsonSerializerOptions()),

                    new ValueComparer<IList<ControllerEventLog>>((c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
        }
    }
}
