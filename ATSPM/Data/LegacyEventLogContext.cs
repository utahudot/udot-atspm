﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using ATSPM.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ATSPM.Data
{
    public partial class LegacyEventLogContext : DbContext
    {
        public LegacyEventLogContext()
        {
        }

        public LegacyEventLogContext(DbContextOptions<LegacyEventLogContext> options) : base(options)
        {
        }

        public virtual DbSet<ControllerEventLog> ControllerEventLogs { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().AreUnicode(false);
            //configurationBuilder.Properties<DateTime>().HaveColumnType("datetime");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<ControllerEventLog>(builder =>
            {
                builder.ToTable("Controller_Event_Log");

                builder.HasComment("Old Log Data Table");

                builder.HasKey(e => new { e.SignalId, e.Timestamp, e.EventCode, e.EventParam });

                //builder.Property(e => e.ArchiveDate).Metadata.AddAnnotation("KeyNameFormat", "dd-MM-yyyy");

                builder.Property(e => e.SignalId)
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnName("SignalID");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}