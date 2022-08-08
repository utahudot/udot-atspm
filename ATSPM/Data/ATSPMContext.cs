using System;
using System.Collections.Generic;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Data
{
    public partial class ATSPMContext : DbContext
    {
        public ATSPMContext()
        {
        }

        public ATSPMContext(DbContextOptions<ATSPMContext> options)
            : base(options)
        {
        }

        //public virtual DbSet<Models.Action> Actions { get; set; } = null!;
        //public virtual DbSet<ActionLog> ActionLogs { get; set; } = null!;
        //public virtual DbSet<Agency> Agencies { get; set; } = null!;
        //public virtual DbSet<Application> Applications { get; set; } = null!;
        //public virtual DbSet<ApplicationEvent> ApplicationEvents { get; set; } = null!;
        //public virtual DbSet<ApplicationSetting> ApplicationSettings { get; set; } = null!;
        //public virtual DbSet<Approach> Approaches { get; set; } = null!;
        //public virtual DbSet<ApproachPcdAggregation> ApproachPcdAggregations { get; set; } = null!;
        //public virtual DbSet<ApproachSpeedAggregation> ApproachSpeedAggregations { get; set; } = null!;
        //public virtual DbSet<ApproachSplitFailAggregation> ApproachSplitFailAggregations { get; set; } = null!;
        //public virtual DbSet<ApproachYellowRedActivationAggregation> ApproachYellowRedActivationAggregations { get; set; } = null!;
        //public virtual DbSet<Area> Areas { get; set; } = null!;
        //public virtual DbSet<AspNetRole> AspNetRoles { get; set; } = null!;
        //public virtual DbSet<AspNetUser> AspNetUsers { get; set; } = null!;
        //public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; } = null!;
        //public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; } = null!;
        //public virtual DbSet<Comment> Comments { get; set; } = null!;
        //public virtual DbSet<ControllerEventLog> ControllerEventLogs { get; set; } = null!;
        //public virtual DbSet<ControllerType> ControllerTypes { get; set; } = null!;
        //public virtual DbSet<DatabaseArchiveExcludedSignal> DatabaseArchiveExcludedSignals { get; set; } = null!;
        //public virtual DbSet<DetectionHardware> DetectionHardwares { get; set; } = null!;
        //public virtual DbSet<DetectionType> DetectionTypes { get; set; } = null!;
        //public virtual DbSet<Detector> Detectors { get; set; } = null!;
        //public virtual DbSet<DetectorComment> DetectorComments { get; set; } = null!;
        //public virtual DbSet<DetectorEventCountAggregation> DetectorEventCountAggregations { get; set; } = null!;
        //public virtual DbSet<DirectionType> DirectionTypes { get; set; } = null!;
        //public virtual DbSet<ExternalLink> ExternalLinks { get; set; } = null!;
        //public virtual DbSet<Faq> Faqs { get; set; } = null!;
        //public virtual DbSet<Jurisdiction> Jurisdictions { get; set; } = null!;
        //public virtual DbSet<LaneType> LaneTypes { get; set; } = null!;
        //public virtual DbSet<MeasuresDefault> MeasuresDefaults { get; set; } = null!;
        //public virtual DbSet<Menu> Menus { get; set; } = null!;
        //public virtual DbSet<MetricComment> MetricComments { get; set; } = null!;
        //public virtual DbSet<MetricType> MetricTypes { get; set; } = null!;
        //public virtual DbSet<MetricsFilterType> MetricsFilterTypes { get; set; } = null!;
        //public virtual DbSet<MigrationHistory> MigrationHistories { get; set; } = null!;
        //public virtual DbSet<MovementType> MovementTypes { get; set; } = null!;
        //public virtual DbSet<PhaseCycleAggregation> PhaseCycleAggregations { get; set; } = null!;
        //public virtual DbSet<PhaseLeftTurnGapAggregation> PhaseLeftTurnGapAggregations { get; set; } = null!;
        //public virtual DbSet<PhaseSplitMonitorAggregation> PhaseSplitMonitorAggregations { get; set; } = null!;
        //public virtual DbSet<PhaseTerminationAggregation> PhaseTerminationAggregations { get; set; } = null!;
        //public virtual DbSet<PreemptionAggregation> PreemptionAggregations { get; set; } = null!;
        //public virtual DbSet<PriorityAggregation> PriorityAggregations { get; set; } = null!;
        //public virtual DbSet<Region> Regions { get; set; } = null!;
        //public virtual DbSet<Route> Routes { get; set; } = null!;
        //public virtual DbSet<RoutePhaseDirection> RoutePhaseDirections { get; set; } = null!;
        //public virtual DbSet<RouteSignal> RouteSignals { get; set; } = null!;
        public virtual DbSet<Signal> Signals { get; set; } = null!;
        //public virtual DbSet<SignalEventCountAggregation> SignalEventCountAggregations { get; set; } = null!;
        //public virtual DbSet<SignalPlanAggregation> SignalPlanAggregations { get; set; } = null!;
        //public virtual DbSet<SignalToAggregate> SignalToAggregates { get; set; } = null!;
        //public virtual DbSet<SpeedEvent> SpeedEvents { get; set; } = null!;
        //public virtual DbSet<SpmwatchDogErrorEvent> SpmwatchDogErrorEvents { get; set; } = null!;
        //public virtual DbSet<StatusOfProcessedTable> StatusOfProcessedTables { get; set; } = null!;
        //public virtual DbSet<TablePartitionProcessed> TablePartitionProcesseds { get; set; } = null!;
        //public virtual DbSet<ToBeProcessedTableIndex> ToBeProcessedTableIndexes { get; set; } = null!;
        //public virtual DbSet<ToBeProcessededIndex> ToBeProcessededIndexes { get; set; } = null!;
        //public virtual DbSet<ToBeProcessededTable> ToBeProcessededTables { get; set; } = null!;
        //public virtual DbSet<VersionAction> VersionActions { get; set; } = null!;

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            if (!optionsBuilder.IsConfigured)
//            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//                optionsBuilder.UseSqlServer("Data Source=srwtcdevdb;Initial Catalog=ATSPM_4_3;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
//            }
//        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Models.Action>(entity =>
            //{
            //    entity.Property(e => e.ActionId).HasColumnName("ActionID");

            //    entity.Property(e => e.Description).HasMaxLength(50);
            //});

            //modelBuilder.Entity<ActionLog>(entity =>
            //{
            //    entity.HasIndex(e => e.AgencyId, "IX_AgencyID");

            //    entity.Property(e => e.ActionLogId).HasColumnName("ActionLogID");

            //    entity.Property(e => e.AgencyId).HasColumnName("AgencyID");

            //    entity.Property(e => e.Comment).HasMaxLength(255);

            //    entity.Property(e => e.Date).HasColumnType("datetime");

            //    entity.Property(e => e.Name).HasMaxLength(100);

            //    entity.Property(e => e.SignalId)
            //        .HasMaxLength(10)
            //        .HasColumnName("SignalID");

            //    entity.HasOne(d => d.Agency)
            //        .WithMany(p => p.ActionLogs)
            //        .HasForeignKey(d => d.AgencyId)
            //        .HasConstraintName("FK_dbo.ActionLogs_dbo.Agencies_AgencyID");

            //    entity.HasMany(d => d.ActionActions)
            //        .WithMany(p => p.ActionLogActionLogs)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "ActionLogAction",
            //            l => l.HasOne<Models.Action>().WithMany().HasForeignKey("ActionActionId").HasConstraintName("FK_dbo.ActionLogActions_dbo.Actions_Action_ActionID"),
            //            r => r.HasOne<ActionLog>().WithMany().HasForeignKey("ActionLogActionLogId").HasConstraintName("FK_dbo.ActionLogActions_dbo.ActionLogs_ActionLog_ActionLogID"),
            //            j =>
            //            {
            //                j.HasKey("ActionLogActionLogId", "ActionActionId").HasName("PK_dbo.ActionLogActions");

            //                j.ToTable("ActionLogActions");

            //                j.HasIndex(new[] { "ActionLogActionLogId" }, "IX_ActionLog_ActionLogID");

            //                j.HasIndex(new[] { "ActionActionId" }, "IX_Action_ActionID");

            //                j.IndexerProperty<int>("ActionLogActionLogId").HasColumnName("ActionLog_ActionLogID");

            //                j.IndexerProperty<int>("ActionActionId").HasColumnName("Action_ActionID");
            //            });

            //    entity.HasMany(d => d.MetricTypeMetrics)
            //        .WithMany(p => p.ActionLogActionLogs)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "ActionLogMetricType",
            //            l => l.HasOne<MetricType>().WithMany().HasForeignKey("MetricTypeMetricId").HasConstraintName("FK_dbo.ActionLogMetricTypes_dbo.MetricTypes_MetricType_MetricID"),
            //            r => r.HasOne<ActionLog>().WithMany().HasForeignKey("ActionLogActionLogId").HasConstraintName("FK_dbo.ActionLogMetricTypes_dbo.ActionLogs_ActionLog_ActionLogID"),
            //            j =>
            //            {
            //                j.HasKey("ActionLogActionLogId", "MetricTypeMetricId").HasName("PK_dbo.ActionLogMetricTypes");

            //                j.ToTable("ActionLogMetricTypes");

            //                j.HasIndex(new[] { "ActionLogActionLogId" }, "IX_ActionLog_ActionLogID");

            //                j.HasIndex(new[] { "MetricTypeMetricId" }, "IX_MetricType_MetricID");

            //                j.IndexerProperty<int>("ActionLogActionLogId").HasColumnName("ActionLog_ActionLogID");

            //                j.IndexerProperty<int>("MetricTypeMetricId").HasColumnName("MetricType_MetricID");
            //            });
            //});

            //modelBuilder.Entity<Agency>(entity =>
            //{
            //    entity.Property(e => e.AgencyId).HasColumnName("AgencyID");

            //    entity.Property(e => e.Description).HasMaxLength(50);
            //});

            //modelBuilder.Entity<Application>(entity =>
            //{
            //    entity.Property(e => e.Id).HasColumnName("ID");
            //});

            //modelBuilder.Entity<ApplicationEvent>(entity =>
            //{
            //    entity.Property(e => e.Id).HasColumnName("ID");

            //    entity.Property(e => e.Timestamp).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<ApplicationSetting>(entity =>
            //{
            //    entity.HasIndex(e => e.ApplicationId, "IX_ApplicationID");

            //    entity.Property(e => e.Id).HasColumnName("ID");

            //    entity.Property(e => e.ApplicationId).HasColumnName("ApplicationID");

            //    entity.Property(e => e.Discriminator).HasMaxLength(128);

            //    entity.Property(e => e.PreviousDayPmpeakEnd).HasColumnName("PreviousDayPMPeakEnd");

            //    entity.Property(e => e.PreviousDayPmpeakStart).HasColumnName("PreviousDayPMPeakStart");

            //    entity.HasOne(d => d.Application)
            //        .WithMany(p => p.ApplicationSettings)
            //        .HasForeignKey(d => d.ApplicationId)
            //        .HasConstraintName("FK_dbo.ApplicationSettings_dbo.Applications_ApplicationID");
            //});

            //modelBuilder.Entity<Approach>(entity =>
            //{
            //    entity.HasIndex(e => e.DirectionTypeId, "IX_DirectionTypeID");

            //    entity.HasIndex(e => e.VersionId, "IX_VersionID");

            //    entity.Property(e => e.ApproachId).HasColumnName("ApproachID");

            //    entity.Property(e => e.DirectionTypeId).HasColumnName("DirectionTypeID");

            //    entity.Property(e => e.Mph).HasColumnName("MPH");

            //    entity.Property(e => e.SignalId).HasColumnName("SignalID");

            //    entity.Property(e => e.VersionId).HasColumnName("VersionID");

            //    entity.HasOne(d => d.DirectionType)
            //        .WithMany(p => p.Approaches)
            //        .HasForeignKey(d => d.DirectionTypeId)
            //        .HasConstraintName("FK_dbo.Approaches_dbo.DirectionTypes_DirectionTypeID");
            //});

            //modelBuilder.Entity<ApproachPcdAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber, e.IsProtectedPhase })
            //        .HasName("PK_dbo.ApproachPcdAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<ApproachSpeedAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.ApproachId })
            //        .HasName("PK_dbo.ApproachSpeedAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<ApproachSplitFailAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.ApproachId, e.PhaseNumber, e.IsProtectedPhase })
            //        .HasName("PK_dbo.ApproachSplitFailAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<ApproachYellowRedActivationAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber, e.IsProtectedPhase })
            //        .HasName("PK_dbo.ApproachYellowRedActivationAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<Area>(entity =>
            //{
            //    entity.Property(e => e.AreaName).HasMaxLength(50);

            //    entity.HasMany(d => d.SignalVersions)
            //        .WithMany(p => p.Areas)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "AreaSignal",
            //            l => l.HasOne<Signal>().WithMany().HasForeignKey("SignalVersionId").HasConstraintName("FK_dbo.AreaSignals_dbo.Signals_Signal_VersionID"),
            //            r => r.HasOne<Area>().WithMany().HasForeignKey("AreaId").HasConstraintName("FK_dbo.AreaSignals_dbo.Areas_Area_Id"),
            //            j =>
            //            {
            //                j.HasKey("AreaId", "SignalVersionId").HasName("PK_dbo.AreaSignals");

            //                j.ToTable("AreaSignals");

            //                j.HasIndex(new[] { "AreaId" }, "IX_Area_Id");

            //                j.HasIndex(new[] { "SignalVersionId" }, "IX_Signal_VersionID");

            //                j.IndexerProperty<int>("AreaId").HasColumnName("Area_Id");

            //                j.IndexerProperty<int>("SignalVersionId").HasColumnName("Signal_VersionID");
            //            });
            //});

            //modelBuilder.Entity<AspNetRole>(entity =>
            //{
            //    entity.HasIndex(e => e.Name, "RoleNameIndex")
            //        .IsUnique();

            //    entity.Property(e => e.Id).HasMaxLength(128);

            //    entity.Property(e => e.Discriminator).HasMaxLength(128);

            //    entity.Property(e => e.Name).HasMaxLength(256);
            //});

            //modelBuilder.Entity<AspNetUser>(entity =>
            //{
            //    entity.HasIndex(e => e.UserName, "UserNameIndex")
            //        .IsUnique();

            //    entity.Property(e => e.Id).HasMaxLength(128);

            //    entity.Property(e => e.Email).HasMaxLength(256);

            //    entity.Property(e => e.LockoutEndDateUtc).HasColumnType("datetime");

            //    entity.Property(e => e.UserName).HasMaxLength(256);

            //    entity.HasMany(d => d.Roles)
            //        .WithMany(p => p.Users)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "AspNetUserRole",
            //            l => l.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId").HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId"),
            //            r => r.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId").HasConstraintName("FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId"),
            //            j =>
            //            {
            //                j.HasKey("UserId", "RoleId").HasName("PK_dbo.AspNetUserRoles");

            //                j.ToTable("AspNetUserRoles");

            //                j.HasIndex(new[] { "RoleId" }, "IX_RoleId");

            //                j.HasIndex(new[] { "UserId" }, "IX_UserId");

            //                j.IndexerProperty<string>("UserId").HasMaxLength(128);

            //                j.IndexerProperty<string>("RoleId").HasMaxLength(128);
            //            });
            //});

            //modelBuilder.Entity<AspNetUserClaim>(entity =>
            //{
            //    entity.HasIndex(e => e.UserId, "IX_UserId");

            //    entity.Property(e => e.UserId).HasMaxLength(128);

            //    entity.HasOne(d => d.User)
            //        .WithMany(p => p.AspNetUserClaims)
            //        .HasForeignKey(d => d.UserId)
            //        .HasConstraintName("FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId");
            //});

            //modelBuilder.Entity<AspNetUserLogin>(entity =>
            //{
            //    entity.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId })
            //        .HasName("PK_dbo.AspNetUserLogins");

            //    entity.HasIndex(e => e.UserId, "IX_UserId");

            //    entity.Property(e => e.LoginProvider).HasMaxLength(128);

            //    entity.Property(e => e.ProviderKey).HasMaxLength(128);

            //    entity.Property(e => e.UserId).HasMaxLength(128);

            //    entity.HasOne(d => d.User)
            //        .WithMany(p => p.AspNetUserLogins)
            //        .HasForeignKey(d => d.UserId)
            //        .HasConstraintName("FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId");
            //});

            //modelBuilder.Entity<Comment>(entity =>
            //{
            //    entity.ToTable("Comment");

            //    entity.Property(e => e.CommentId).HasColumnName("CommentID");

            //    entity.Property(e => e.Comment1)
            //        .IsUnicode(false)
            //        .HasColumnName("Comment");

            //    entity.Property(e => e.Entity)
            //        .HasMaxLength(50)
            //        .IsUnicode(false);

            //    entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<ControllerEventLog>(entity =>
            //{
            //    entity.HasNoKey();

            //    entity.ToTable("Controller_Event_Log");

            //    entity.HasIndex(e => e.Timestamp, "IX_Clustered_Controller_Event_Log_Timestamp")
            //        .IsClustered();

            //    entity.HasIndex(e => new { e.SignalId, e.Timestamp, e.EventCode, e.EventParam }, "IX_SignalID_Timestamp_EventCode_EventParam");

            //    entity.Property(e => e.SignalId)
            //        .HasMaxLength(10)
            //        .HasColumnName("SignalID");

            //    entity.Property(e => e.Timestamp).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<ControllerType>(entity =>
            //{
            //    entity.Property(e => e.ControllerTypeId)
            //        .ValueGeneratedNever()
            //        .HasColumnName("ControllerTypeID");

            //    entity.Property(e => e.ActiveFtp).HasColumnName("ActiveFTP");

            //    entity.Property(e => e.Description)
            //        .HasMaxLength(50)
            //        .IsUnicode(false);

            //    entity.Property(e => e.Ftpdirectory)
            //        .IsUnicode(false)
            //        .HasColumnName("FTPDirectory");

            //    entity.Property(e => e.Password)
            //        .HasMaxLength(50)
            //        .IsUnicode(false);

            //    entity.Property(e => e.Snmpport).HasColumnName("SNMPPort");

            //    entity.Property(e => e.UserName)
            //        .HasMaxLength(50)
            //        .IsUnicode(false);
            //});

            //modelBuilder.Entity<DatabaseArchiveExcludedSignal>(entity =>
            //{
            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<DetectionHardware>(entity =>
            //{
            //    entity.Property(e => e.Id)
            //        .ValueGeneratedNever()
            //        .HasColumnName("ID");
            //});

            //modelBuilder.Entity<DetectionType>(entity =>
            //{
            //    entity.Property(e => e.DetectionTypeId)
            //        .ValueGeneratedNever()
            //        .HasColumnName("DetectionTypeID");

            //    entity.HasMany(d => d.MetricTypeMetrics)
            //        .WithMany(p => p.DetectionTypeDetectionTypes)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "DetectionTypeMetricType",
            //            l => l.HasOne<MetricType>().WithMany().HasForeignKey("MetricTypeMetricId").HasConstraintName("FK_dbo.DetectionTypeMetricTypes_dbo.MetricTypes_MetricType_MetricID"),
            //            r => r.HasOne<DetectionType>().WithMany().HasForeignKey("DetectionTypeDetectionTypeId").HasConstraintName("FK_dbo.DetectionTypeMetricTypes_dbo.DetectionTypes_DetectionType_DetectionTypeID"),
            //            j =>
            //            {
            //                j.HasKey("DetectionTypeDetectionTypeId", "MetricTypeMetricId").HasName("PK_dbo.DetectionTypeMetricTypes");

            //                j.ToTable("DetectionTypeMetricTypes");

            //                j.HasIndex(new[] { "DetectionTypeDetectionTypeId" }, "IX_DetectionType_DetectionTypeID");

            //                j.HasIndex(new[] { "MetricTypeMetricId" }, "IX_MetricType_MetricID");

            //                j.IndexerProperty<int>("DetectionTypeDetectionTypeId").HasColumnName("DetectionType_DetectionTypeID");

            //                j.IndexerProperty<int>("MetricTypeMetricId").HasColumnName("MetricType_MetricID");
            //            });
            //});

            //modelBuilder.Entity<Detector>(entity =>
            //{
            //    entity.HasIndex(e => e.ApproachId, "IX_ApproachID");

            //    entity.HasIndex(e => e.DetectionHardwareId, "IX_DetectionHardwareID");

            //    entity.HasIndex(e => e.LaneTypeId, "IX_LaneTypeID");

            //    entity.HasIndex(e => e.MovementTypeId, "IX_MovementTypeID");

            //    entity.Property(e => e.Id).HasColumnName("ID");

            //    entity.Property(e => e.ApproachId).HasColumnName("ApproachID");

            //    entity.Property(e => e.DateAdded).HasColumnType("datetime");

            //    entity.Property(e => e.DateDisabled).HasColumnType("datetime");

            //    entity.Property(e => e.DetectionHardwareId).HasColumnName("DetectionHardwareID");

            //    entity.Property(e => e.DetectorId)
            //        .HasMaxLength(50)
            //        .HasColumnName("DetectorID");

            //    entity.Property(e => e.LaneTypeId).HasColumnName("LaneTypeID");

            //    entity.Property(e => e.MovementTypeId).HasColumnName("MovementTypeID");

            //    entity.HasOne(d => d.Approach)
            //        .WithMany(p => p.Detectors)
            //        .HasForeignKey(d => d.ApproachId)
            //        .HasConstraintName("FK_dbo.Detectors_dbo.Approaches_ApproachID");

            //    entity.HasOne(d => d.DetectionHardware)
            //        .WithMany(p => p.Detectors)
            //        .HasForeignKey(d => d.DetectionHardwareId)
            //        .HasConstraintName("FK_dbo.Detectors_dbo.DetectionHardwares_DetectionHardwareID");

            //    entity.HasOne(d => d.LaneType)
            //        .WithMany(p => p.Detectors)
            //        .HasForeignKey(d => d.LaneTypeId)
            //        .HasConstraintName("FK_dbo.Detectors_dbo.LaneTypes_LaneTypeID");

            //    entity.HasOne(d => d.MovementType)
            //        .WithMany(p => p.Detectors)
            //        .HasForeignKey(d => d.MovementTypeId)
            //        .HasConstraintName("FK_dbo.Detectors_dbo.MovementTypes_MovementTypeID");

            //    entity.HasMany(d => d.DetectionTypes)
            //        .WithMany(p => p.Ids)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "DetectionTypeDetector",
            //            l => l.HasOne<DetectionType>().WithMany().HasForeignKey("DetectionTypeId").HasConstraintName("FK_dbo.DetectionTypeDetector_dbo.DetectionTypes_DetectionTypeID"),
            //            r => r.HasOne<Detector>().WithMany().HasForeignKey("Id").HasConstraintName("FK_dbo.DetectionTypeDetector_dbo.Detectors_ID"),
            //            j =>
            //            {
            //                j.HasKey("Id", "DetectionTypeId").HasName("PK_dbo.DetectionTypeDetector");

            //                j.ToTable("DetectionTypeDetector");

            //                j.HasIndex(new[] { "DetectionTypeId" }, "IX_DetectionTypeID");

            //                j.HasIndex(new[] { "Id" }, "IX_ID");

            //                j.IndexerProperty<int>("Id").HasColumnName("ID");

            //                j.IndexerProperty<int>("DetectionTypeId").HasColumnName("DetectionTypeID");
            //            });
            //});

            //modelBuilder.Entity<DetectorComment>(entity =>
            //{
            //    entity.HasKey(e => e.CommentId)
            //        .HasName("PK_dbo.DetectorComments");

            //    entity.HasIndex(e => e.Id, "IX_ID");

            //    entity.Property(e => e.CommentId).HasColumnName("CommentID");

            //    entity.Property(e => e.Id).HasColumnName("ID");

            //    entity.Property(e => e.TimeStamp).HasColumnType("datetime");

            //    entity.HasOne(d => d.IdNavigation)
            //        .WithMany(p => p.DetectorComments)
            //        .HasForeignKey(d => d.Id)
            //        .HasConstraintName("FK_dbo.DetectorComments_dbo.Detectors_ID");
            //});

            //modelBuilder.Entity<DetectorEventCountAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.DetectorPrimaryId })
            //        .HasName("PK_dbo.DetectorEventCountAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<DirectionType>(entity =>
            //{
            //    entity.Property(e => e.DirectionTypeId)
            //        .ValueGeneratedNever()
            //        .HasColumnName("DirectionTypeID");

            //    entity.Property(e => e.Abbreviation).HasMaxLength(5);

            //    entity.Property(e => e.Description).HasMaxLength(30);
            //});

            //modelBuilder.Entity<ExternalLink>(entity =>
            //{
            //    entity.Property(e => e.ExternalLinkId).HasColumnName("ExternalLinkID");
            //});

            //modelBuilder.Entity<Faq>(entity =>
            //{
            //    entity.ToTable("FAQs");

            //    entity.Property(e => e.Faqid).HasColumnName("FAQID");
            //});

            //modelBuilder.Entity<Jurisdiction>(entity =>
            //{
            //    entity.Property(e => e.CountyParish).HasMaxLength(50);

            //    entity.Property(e => e.JurisdictionName).HasMaxLength(50);

            //    entity.Property(e => e.Mpo)
            //        .HasMaxLength(50)
            //        .HasColumnName("MPO");

            //    entity.Property(e => e.OtherPartners).HasMaxLength(50);
            //});

            //modelBuilder.Entity<LaneType>(entity =>
            //{
            //    entity.Property(e => e.LaneTypeId)
            //        .ValueGeneratedNever()
            //        .HasColumnName("LaneTypeID");

            //    entity.Property(e => e.Abbreviation).HasMaxLength(5);

            //    entity.Property(e => e.Description).HasMaxLength(30);
            //});

            //modelBuilder.Entity<MeasuresDefault>(entity =>
            //{
            //    entity.HasKey(e => new { e.Measure, e.OptionName })
            //        .HasName("PK_dbo.MeasuresDefaults");

            //    entity.Property(e => e.Measure).HasMaxLength(128);

            //    entity.Property(e => e.OptionName).HasMaxLength(128);
            //});

            //modelBuilder.Entity<Menu>(entity =>
            //{
            //    entity.ToTable("Menu");

            //    entity.Property(e => e.MenuId).ValueGeneratedNever();

            //    entity.Property(e => e.Action)
            //        .HasMaxLength(50)
            //        .HasDefaultValueSql("('')");

            //    entity.Property(e => e.Application).HasMaxLength(50);

            //    entity.Property(e => e.Controller)
            //        .HasMaxLength(50)
            //        .HasDefaultValueSql("('')");

            //    entity.Property(e => e.MenuName).HasMaxLength(50);
            //});

            //modelBuilder.Entity<MetricComment>(entity =>
            //{
            //    entity.HasKey(e => e.CommentId)
            //        .HasName("PK_dbo.MetricComments");

            //    entity.HasIndex(e => e.VersionId, "IX_VersionID");

            //    entity.Property(e => e.CommentId).HasColumnName("CommentID");

            //    entity.Property(e => e.SignalId)
            //        .HasMaxLength(10)
            //        .HasColumnName("SignalID");

            //    entity.Property(e => e.TimeStamp).HasColumnType("datetime");

            //    entity.Property(e => e.VersionId).HasColumnName("VersionID");

            //    entity.HasMany(d => d.MetricTypeMetrics)
            //        .WithMany(p => p.MetricCommentComments)
            //        .UsingEntity<Dictionary<string, object>>(
            //            "MetricCommentMetricType",
            //            l => l.HasOne<MetricType>().WithMany().HasForeignKey("MetricTypeMetricId").HasConstraintName("FK_dbo.MetricCommentMetricTypes_dbo.MetricTypes_MetricType_MetricID"),
            //            r => r.HasOne<MetricComment>().WithMany().HasForeignKey("MetricCommentCommentId").HasConstraintName("FK_dbo.MetricCommentMetricTypes_dbo.MetricComments_MetricComment_CommentID"),
            //            j =>
            //            {
            //                j.HasKey("MetricCommentCommentId", "MetricTypeMetricId").HasName("PK_dbo.MetricCommentMetricTypes");

            //                j.ToTable("MetricCommentMetricTypes");

            //                j.HasIndex(new[] { "MetricCommentCommentId" }, "IX_MetricComment_CommentID");

            //                j.HasIndex(new[] { "MetricTypeMetricId" }, "IX_MetricType_MetricID");

            //                j.IndexerProperty<int>("MetricCommentCommentId").HasColumnName("MetricComment_CommentID");

            //                j.IndexerProperty<int>("MetricTypeMetricId").HasColumnName("MetricType_MetricID");
            //            });
            //});

            //modelBuilder.Entity<MetricType>(entity =>
            //{
            //    entity.HasKey(e => e.MetricId)
            //        .HasName("PK_dbo.MetricTypes");

            //    entity.Property(e => e.MetricId)
            //        .ValueGeneratedNever()
            //        .HasColumnName("MetricID");
            //});

            //modelBuilder.Entity<MetricsFilterType>(entity =>
            //{
            //    entity.HasKey(e => e.FilterId)
            //        .HasName("PK_dbo.MetricsFilterTypes");

            //    entity.Property(e => e.FilterId).HasColumnName("FilterID");
            //});

            //modelBuilder.Entity<MigrationHistory>(entity =>
            //{
            //    entity.HasKey(e => new { e.MigrationId, e.ContextKey })
            //        .HasName("PK_dbo.__MigrationHistory");

            //    entity.ToTable("__MigrationHistory");

            //    entity.Property(e => e.MigrationId).HasMaxLength(150);

            //    entity.Property(e => e.ContextKey).HasMaxLength(300);

            //    entity.Property(e => e.ProductVersion).HasMaxLength(32);
            //});

            //modelBuilder.Entity<MovementType>(entity =>
            //{
            //    entity.Property(e => e.MovementTypeId)
            //        .ValueGeneratedNever()
            //        .HasColumnName("MovementTypeID");

            //    entity.Property(e => e.Abbreviation).HasMaxLength(5);

            //    entity.Property(e => e.Description).HasMaxLength(30);
            //});

            //modelBuilder.Entity<PhaseCycleAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber })
            //        .HasName("PK_dbo.PhaseCycleAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<PhaseLeftTurnGapAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber })
            //        .HasName("PK_dbo.PhaseLeftTurnGapAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<PhaseSplitMonitorAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber })
            //        .HasName("PK_dbo.PhaseSplitMonitorAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(128);
            //});

            //modelBuilder.Entity<PhaseTerminationAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.PhaseNumber })
            //        .HasName("PK_dbo.PhaseTerminationAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<PreemptionAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.PreemptNumber })
            //        .HasName("PK_dbo.PreemptionAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<PriorityAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId, e.PriorityNumber })
            //        .HasName("PK_dbo.PriorityAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<Region>(entity =>
            //{
            //    entity.ToTable("Region");

            //    entity.Property(e => e.Id)
            //        .ValueGeneratedNever()
            //        .HasColumnName("ID");

            //    entity.Property(e => e.Description).HasMaxLength(50);
            //});

            //modelBuilder.Entity<RoutePhaseDirection>(entity =>
            //{
            //    entity.HasIndex(e => e.DirectionTypeId, "IX_DirectionTypeId");

            //    entity.HasIndex(e => e.RouteSignalId, "IX_RouteSignalId");

            //    entity.HasOne(d => d.DirectionType)
            //        .WithMany(p => p.RoutePhaseDirections)
            //        .HasForeignKey(d => d.DirectionTypeId)
            //        .HasConstraintName("FK_dbo.RoutePhaseDirections_dbo.DirectionTypes_DirectionTypeId");

            //    entity.HasOne(d => d.RouteSignal)
            //        .WithMany(p => p.RoutePhaseDirections)
            //        .HasForeignKey(d => d.RouteSignalId)
            //        .HasConstraintName("FK_dbo.RoutePhaseDirections_dbo.RouteSignals_RouteSignalId");
            //});

            //modelBuilder.Entity<RouteSignal>(entity =>
            //{
            //    entity.HasIndex(e => e.RouteId, "IX_RouteId");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);

            //    entity.HasOne(d => d.Route)
            //        .WithMany(p => p.RouteSignals)
            //        .HasForeignKey(d => d.RouteId)
            //        .HasConstraintName("FK_dbo.RouteSignals_dbo.Routes_RouteId");
            //});

            modelBuilder.Entity<Signal>(entity =>
            {
                entity.HasKey(e => e.VersionId)
                    .HasName("PK_dbo.Signals");

                entity.HasIndex(e => e.ControllerTypeId, "IX_ControllerTypeID");

                entity.HasIndex(e => e.JurisdictionId, "IX_JurisdictionId");

                entity.HasIndex(e => e.RegionId, "IX_RegionID");

                entity.HasIndex(e => e.VersionActionId, "IX_VersionActionId");

                entity.Property(e => e.VersionId).HasColumnName("VersionID");

                entity.Property(e => e.ControllerTypeId).HasColumnName("ControllerTypeID");

                entity.Property(e => e.Ipaddress)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("IPAddress")
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.JurisdictionId).HasDefaultValueSql("((1))");

                entity.Property(e => e.Latitude)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Longitude)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Note).HasDefaultValueSql("('Initial')");

                entity.Property(e => e.PrimaryName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.RegionId).HasColumnName("RegionID");

                entity.Property(e => e.SecondaryName)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('')");

                entity.Property(e => e.SignalId)
                    .HasMaxLength(10)
                    .HasColumnName("SignalID");

                entity.Property(e => e.Start).HasColumnType("datetime");

                entity.Property(e => e.VersionActionId).HasDefaultValueSql("((10))");

                entity.HasOne(d => d.ControllerType)
                    .WithMany(p => p.Signals)
                    .HasForeignKey(d => d.ControllerTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.Signals_dbo.ControllerTypes_ControllerTypeID");

                entity.HasOne(d => d.Jurisdiction)
                    .WithMany(p => p.Signals)
                    .HasForeignKey(d => d.JurisdictionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_dbo.Signals_dbo.Jurisdictions_JurisdictionId");

                entity.HasOne(d => d.Region)
                    .WithMany(p => p.Signals)
                    .HasForeignKey(d => d.RegionId)
                    .HasConstraintName("FK_dbo.Signals_dbo.Region_RegionID");
            });

            //modelBuilder.Entity<SignalEventCountAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.BinStartTime, e.SignalId })
            //        .HasName("PK_dbo.SignalEventCountAggregations");

            //    entity.Property(e => e.BinStartTime).HasColumnType("datetime");

            //    entity.Property(e => e.SignalId).HasMaxLength(10);
            //});

            //modelBuilder.Entity<SignalPlanAggregation>(entity =>
            //{
            //    entity.HasKey(e => new { e.SignalId, e.Start, e.End })
            //        .HasName("PK_dbo.SignalPlanAggregations");

            //    entity.Property(e => e.SignalId).HasMaxLength(128);

            //    entity.Property(e => e.Start).HasColumnType("datetime");

            //    entity.Property(e => e.End).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<SignalToAggregate>(entity =>
            //{
            //    entity.HasKey(e => e.SignalId)
            //        .HasName("PK_dbo.SignalToAggregates");

            //    entity.Property(e => e.SignalId)
            //        .HasMaxLength(10)
            //        .HasColumnName("SignalID");
            //});

            //modelBuilder.Entity<SpeedEvent>(entity =>
            //{
            //    entity.HasKey(e => new { e.DetectorId, e.Mph, e.Kph, e.Timestamp })
            //        .HasName("PK_dbo.Speed_Events");

            //    entity.ToTable("Speed_Events");

            //    entity.Property(e => e.DetectorId)
            //        .HasMaxLength(50)
            //        .HasColumnName("DetectorID");

            //    entity.Property(e => e.Mph).HasColumnName("MPH");

            //    entity.Property(e => e.Kph).HasColumnName("KPH");

            //    entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            //});

            //modelBuilder.Entity<SpmwatchDogErrorEvent>(entity =>
            //{
            //    entity.ToTable("SPMWatchDogErrorEvents");

            //    entity.Property(e => e.Id).HasColumnName("ID");

            //    entity.Property(e => e.DetectorId).HasColumnName("DetectorID");

            //    entity.Property(e => e.SignalId)
            //        .HasMaxLength(10)
            //        .HasColumnName("SignalID");

            //    entity.Property(e => e.TimeStamp).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<StatusOfProcessedTable>(entity =>
            //{
            //    entity.Property(e => e.SqlstatementOrMessage).HasColumnName("SQLStatementOrMessage");

            //    entity.Property(e => e.TimeEntered).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<TablePartitionProcessed>(entity =>
            //{
            //    entity.Property(e => e.TimeIndexdropped).HasColumnType("datetime");

            //    entity.Property(e => e.TimeSwappedTableDropped).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<ToBeProcessededTable>(entity =>
            //{
            //    entity.Property(e => e.UpdatedTime).HasColumnType("datetime");
            //});

            //modelBuilder.Entity<VersionAction>(entity =>
            //{
            //    entity.Property(e => e.Id)
            //        .ValueGeneratedNever()
            //        .HasColumnName("ID");
            //});

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
