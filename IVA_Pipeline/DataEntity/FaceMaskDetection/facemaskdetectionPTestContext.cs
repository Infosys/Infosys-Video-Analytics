/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.IO;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics
{
    public partial class facemaskdetectionPTestContext : DbContext
    {
        public facemaskdetectionPTestContext()
        {
        }

        public facemaskdetectionPTestContext(DbContextOptions<facemaskdetectionPTestContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Action> Actions { get; set; }
        public virtual DbSet<ActionParam> ActionParams { get; set; }
        public virtual DbSet<Actionstage> Actionstages { get; set; }
        public virtual DbSet<Actiontype> Actiontypes { get; set; }
        public virtual DbSet<AnomalyDetail> AnomalyDetails { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        public virtual DbSet<Configuration> Configurations { get; set; }
        public virtual DbSet<EnvironmentScanMetric> EnvironmentScanMetrics { get; set; }
        public virtual DbSet<EnvironmentScanMetricAnomalyDetail> EnvironmentScanMetricAnomalyDetails { get; set; }
        public virtual DbSet<EnvironmentScanMetricDetail> EnvironmentScanMetricDetails { get; set; }
        public virtual DbSet<Error> Errors { get; set; }
        public virtual DbSet<FeedProcessorMaster> FeedProcessorMasters { get; set; }
        public virtual DbSet<HealthcheckDetail> HealthcheckDetails { get; set; }
        public virtual DbSet<HealthcheckIterationTracker> HealthcheckIterationTrackers { get; set; }
        public virtual DbSet<HealthcheckIterationTrackerDetail> HealthcheckIterationTrackerDetails { get; set; }
        public virtual DbSet<HealthcheckMaster> HealthcheckMasters { get; set; }
        public virtual DbSet<HealthcheckTracker> HealthcheckTrackers { get; set; }
        public virtual DbSet<JobExecution> JobExecutions { get; set; }
        public virtual DbSet<JobMaster> JobMasters { get; set; }
        public virtual DbSet<JobScheduler> JobSchedulers { get; set; }
        public virtual DbSet<NotificationConfiguration> NotificationConfigurations { get; set; }
        public virtual DbSet<Observable> Observables { get; set; }
        public virtual DbSet<ObservableResourceMap> ObservableResourceMaps { get; set; }
        public virtual DbSet<Observation> Observations { get; set; }
        public virtual DbSet<Operator> Operators { get; set; }
        public virtual DbSet<Platform> Platforms { get; set; }
        public virtual DbSet<RecipientConfiguration> RecipientConfigurations { get; set; }
        public virtual DbSet<RemdiationPlanActionParamMap> RemdiationPlanActionParamMaps { get; set; }
        public virtual DbSet<RemediationPlan> RemediationPlans { get; set; }
        public virtual DbSet<RemediationPlanActionMap> RemediationPlanActionMaps { get; set; }
        public virtual DbSet<RemediationPlanExecution> RemediationPlanExecutions { get; set; }
        public virtual DbSet<RemediationPlanExecutionAction> RemediationPlanExecutionActions { get; set; }
        public virtual DbSet<Resource> Resources { get; set; }
        public virtual DbSet<ResourceAttribute> ResourceAttributes { get; set; }
        public virtual DbSet<ResourceDependencyMap> ResourceDependencyMaps { get; set; }
        public virtual DbSet<ResourceObservableActionMap> ResourceObservableActionMaps { get; set; }
        public virtual DbSet<ResourceObservableRemediationPlanMap> ResourceObservableRemediationPlanMaps { get; set; }
        public virtual DbSet<Resourcetype> Resourcetypes { get; set; }
        public virtual DbSet<ResourcetypeDependencyMap> ResourcetypeDependencyMaps { get; set; }
        public virtual DbSet<ResourcetypeMetadatum> ResourcetypeMetadata { get; set; }
        public virtual DbSet<ResourcetypeObservableActionMap> ResourcetypeObservableActionMaps { get; set; }
        public virtual DbSet<ResourcetypeObservableMap> ResourcetypeObservableMaps { get; set; }
        public virtual DbSet<ResourcetypeObservableRemediationPlanMap> ResourcetypeObservableRemediationPlanMaps { get; set; }
        public virtual DbSet<ResourcetypeServiceDetail> ResourcetypeServiceDetails { get; set; }
        public virtual DbSet<Tenant> Tenants { get; set; }
        public virtual DbSet<Ticket> Tickets { get; set; }
        public virtual DbSet<UnhealthyReportRemediation> UnhealthyReportRemediations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                AppSettings appSettings = Config.AppSettings;
                switch (appSettings.DBProvider)
                {
                    case "postgres":
                        optionsBuilder.UseNpgsql(appSettings.FaceMaskDetectionEntities);
                        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                        break;
                    default:
                        optionsBuilder.UseSqlServer(appSettings.FaceMaskDetectionEntities);
                        break;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Action>(entity =>
            {
                entity.ToTable("action");

                entity.Property(e => e.ActionName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.AutomationEngineName)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.CategoryName)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.EndpointUri)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);
            });

            modelBuilder.Entity<ActionParam>(entity =>
            {
                entity.HasKey(e => e.ParamId)
                    .HasName("PK__action_p__C132B1244DF2FF3C");

                entity.ToTable("action_params");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DefaultValue)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.FieldToMap)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ParamType)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Actionstage>(entity =>
            {
                entity.ToTable("actionstages");

                entity.Property(e => e.ActionStageId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ActionStageName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Category)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);
            });

            modelBuilder.Entity<Actiontype>(entity =>
            {
                entity.ToTable("actiontype");

                entity.Property(e => e.ActionType1)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("ActionType");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);
            });

            modelBuilder.Entity<AnomalyDetail>(entity =>
            {
                entity.HasKey(e => e.AnomalyId)
                    .HasName("anomaly_details_pkey");

                entity.ToTable("anomaly_details");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.EventType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IsNotified)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.NotifiedTime).HasPrecision(6);

                entity.Property(e => e.ObservableName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ObservationStatus)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ObservationTime).HasPrecision(6);

                entity.Property(e => e.RemediationStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Source)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SourceIp)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.State)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.LogId);

                entity.ToTable("Audit_Logs");

                entity.HasIndex(e => new { e.TransactionId, e.ResourceId, e.ObservableId, e.TenantId }, "idx_audit_logs");

                entity.Property(e => e.LogId).HasColumnName("LogID");

                entity.Property(e => e.ActionId).HasColumnName("ActionID");

                entity.Property(e => e.IncidentId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ObservableId).HasColumnName("ObservableID");

                entity.Property(e => e.PlatformId).HasColumnName("PlatformID");

                entity.Property(e => e.PortfolioId)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ResourceID");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TenantId).HasColumnName("TenantID");

                entity.Property(e => e.TransactionId)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.HasOne(d => d.Action)
                    .WithMany(p => p.AuditLogs)
                    .HasForeignKey(d => d.ActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Audit_Logs_action");

                entity.HasOne(d => d.Observable)
                    .WithMany(p => p.AuditLogs)
                    .HasForeignKey(d => d.ObservableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Observable_ID");

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.AuditLogs)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Resource_ID");
            });

            modelBuilder.Entity<Configuration>(entity =>
            {
                entity.ToTable("configuration");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ReferenceKey).IsRequired();

                entity.Property(e => e.ReferenceType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferenceValue).IsRequired();
            });

            modelBuilder.Entity<EnvironmentScanMetric>(entity =>
            {
                entity.ToTable("Environment_Scan_Metric");

                entity.Property(e => e.EnvironmentScanMetricId).HasColumnName("EnvironmentScanMetricID");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ObservableId).HasColumnName("ObservableID");

                entity.Property(e => e.PlatformId).HasColumnName("PlatformID");

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ResourceID");

                entity.Property(e => e.TenantId).HasColumnName("TenantID");
            });

            modelBuilder.Entity<EnvironmentScanMetricAnomalyDetail>(entity =>
            {
                entity.ToTable("Environment_Scan_Metric_Anomaly_Details");

                entity.Property(e => e.AttributeName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeStatus)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeValue)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MetricKey)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.MetricName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.OldValue)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<EnvironmentScanMetricDetail>(entity =>
            {
                entity.ToTable("Environment_Scan_Metric_Details");

                entity.Property(e => e.AttributeName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeValue)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.EnvironmentScanMetricId).HasColumnName("EnvironmentScanMetricID");

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.MetricId).HasColumnName("MetricID");

                entity.Property(e => e.MetricKey)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.MetricName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TenantId).HasColumnName("TenantID");

                entity.HasOne(d => d.EnvironmentScanMetric)
                    .WithMany(p => p.EnvironmentScanMetricDetails)
                    .HasForeignKey(d => d.EnvironmentScanMetricId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__baseline___Basel__469FD34E1");
            });

            modelBuilder.Entity<Error>(entity =>
            {
                entity.ToTable("errors");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Createdate)
                    .HasColumnType("date")
                    .HasColumnName("createdate");

                entity.Property(e => e.Errorcode)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("errorcode");

                entity.Property(e => e.Errordesc)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("errordesc");

                entity.Property(e => e.Parametername)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("parametername");

                entity.Property(e => e.Systemname)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("systemname");
            });

            modelBuilder.Entity<FeedProcessorMaster>(entity =>
            {
                entity.ToTable("feed_processor_master ");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.FeedUri)
                    .HasMaxLength(250)
                    .IsUnicode(false)
                    .HasColumnName("FeedURI");

                entity.Property(e => e.FileName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.MachineName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<HealthcheckDetail>(entity =>
            {
                entity.HasKey(e => new { e.ConfigId, e.ResourceId })
                    .HasName("PK__healthch__27512B4A889ABCAD");

                entity.ToTable("healthcheck_details");

                entity.Property(e => e.ConfigId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.Validityend)
                    .HasPrecision(6)
                    .HasColumnName("validityend");

                entity.Property(e => e.Validitystart)
                    .HasPrecision(6)
                    .HasColumnName("validitystart");

                entity.HasOne(d => d.Config)
                    .WithMany(p => p.HealthcheckDetails)
                    .HasForeignKey(d => d.ConfigId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__healthche__Confi__7B5B524B");

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.HealthcheckDetails)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__healthche__Resou__7C4F7684");
            });

            modelBuilder.Entity<HealthcheckIterationTracker>(entity =>
            {
                entity.HasKey(e => e.TrackingId)
                    .HasName("PK__healthch__3C19EDF184847CDB");

                entity.ToTable("healthcheck_iteration_tracker");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasPrecision(6);

                entity.Property(e => e.EndTime).HasPrecision(6);

                entity.Property(e => e.Error)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.HealthcheckSource)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.IpAddress)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasPrecision(6);

                entity.Property(e => e.StartTime).HasPrecision(6);
            });

            modelBuilder.Entity<HealthcheckIterationTrackerDetail>(entity =>
            {
                entity.HasKey(e => e.TrackingDetailsId)
                    .HasName("PK__healthch__7BB78FCED83E0DBB");

                entity.ToTable("healthcheck_iteration_tracker_details");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasPrecision(6);

                entity.Property(e => e.Error).IsUnicode(false);

                entity.Property(e => e.HealthcheckSource)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.HealthcheckTrackingId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasPrecision(6);

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StartTime).HasPrecision(6);
            });

            modelBuilder.Entity<HealthcheckMaster>(entity =>
            {
                entity.HasKey(e => e.ConfigId)
                    .HasName("PK__healthch__C3BC335C8F9EE8EC");

                entity.ToTable("healthcheck_master");

                entity.Property(e => e.ConfigId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigurationName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.Validityend)
                    .HasPrecision(6)
                    .HasColumnName("validityend");

                entity.Property(e => e.Validitystart)
                    .HasPrecision(6)
                    .HasColumnName("validitystart");
            });

            modelBuilder.Entity<HealthcheckTracker>(entity =>
            {
                entity.HasKey(e => e.HealthCheckId)
                    .HasName("PK__healthch__27355EBF14972C18");

                entity.ToTable("healthcheck_tracker");

                entity.Property(e => e.ConfigId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.EndTime).HasPrecision(6);

                entity.Property(e => e.IncidentId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Source)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.StartTime).HasPrecision(6);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.Config)
                    .WithMany(p => p.HealthcheckTrackers)
                    .HasForeignKey(d => d.ConfigId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__healthche__Confi__7D439ABD");
            });

            modelBuilder.Entity<JobExecution>(entity =>
            {
                entity.HasKey(e => e.ExecutionId)
                    .HasName("PK__job_exec__473088C51EAAF65F");

                entity.ToTable("job_execution");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.EndTime).HasPrecision(6);

                entity.Property(e => e.JobStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.StartTime).HasPrecision(6);

                entity.Property(e => e.StatusMessage).IsUnicode(false);

                entity.HasOne(d => d.Job)
                    .WithMany(p => p.JobExecutions)
                    .HasForeignKey(d => d.JobId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__job_execu__JobId__7E37BEF6");
            });

            modelBuilder.Entity<JobMaster>(entity =>
            {
                entity.HasKey(e => e.JobId)
                    .HasName("PK__job_mast__056690C24FD018B9");

                entity.ToTable("job_master");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.Description).IsUnicode(false);

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.JobName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);
            });

            modelBuilder.Entity<JobScheduler>(entity =>
            {
                entity.HasKey(e => e.JobId)
                    .HasName("PK__job_sche__056690C29AA58B6D");

                entity.ToTable("job_scheduler");

                entity.Property(e => e.JobId).ValueGeneratedNever();

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.HasOne(d => d.Job)
                    .WithOne(p => p.JobScheduler)
                    .HasForeignKey<JobScheduler>(d => d.JobId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__job_sched__JobId__7F2BE32F");
            });

            modelBuilder.Entity<NotificationConfiguration>(entity =>
            {
                entity.HasKey(e => e.NotifcationConfigId)
                    .HasName("PK__notifica__488F8B405623CFC9");

                entity.ToTable("notification_configuration");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ReferenceKey).IsRequired();

                entity.Property(e => e.ReferenceType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ReferenceValue).IsRequired();
            });

            modelBuilder.Entity<Observable>(entity =>
            {
                entity.ToTable("observable");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DataType)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ObservableName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UnitOfMeasure)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);
            });

            modelBuilder.Entity<ObservableResourceMap>(entity =>
            {
                entity.HasKey(e => new { e.ObservableId, e.ResourceId })
                    .HasName("PK__observab__6F6FEF6A030E6F6B");

                entity.ToTable("observable_resource_map");

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LowerThreshold)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.OperatorId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpperThreshold)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);

                entity.HasOne(d => d.Observable)
                    .WithMany(p => p.ObservableResourceMaps)
                    .HasForeignKey(d => d.ObservableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObservableID");

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.ObservableResourceMaps)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResourceID");
            });

            modelBuilder.Entity<Observation>(entity =>
            {
                entity.ToTable("observations");

                entity.Property(e => e.Application)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ConfigId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.EventType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IncidentId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.IsNotified)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.NotifiedTime).HasPrecision(6);

                entity.Property(e => e.ObservableName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ObservationStatus)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ObservationTime).HasPrecision(6);

                entity.Property(e => e.PortfolioId)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.RemediationStatus)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Source)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SourceIp)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.State)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TransactionId)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Operator>(entity =>
            {
                entity.ToTable("operator");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.Operator1)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("Operator");

                entity.Property(e => e.Rule)
                    .HasMaxLength(300)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Platform>(entity =>
            {
                entity.ToTable("platforms");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExecutionMode)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.PlatformName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PlatformType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RuntimeDetails)
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RecipientConfiguration>(entity =>
            {
                entity.HasKey(e => e.RecipientConfigId)
                    .HasName("PK__recipien__5ADE3879E8C67DB7");

                entity.ToTable("recipient_configuration");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.RecipientName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ReferenceKey).IsRequired();

                entity.Property(e => e.ReferenceType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ReferenceValue).IsRequired();

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RemdiationPlanActionParamMap>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("remdiation_plan_action_param_map");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ProvidedValue)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ProvidedValueType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Param)
                    .WithMany()
                    .HasForeignKey(d => d.ParamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ParamID");
            });

            modelBuilder.Entity<RemediationPlan>(entity =>
            {
                entity.ToTable("remediation_plan");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.IsDeleted).HasColumnName("isDeleted");

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.RemediationPlanDescription)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.RemediationPlanName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RemediationPlanActionMap>(entity =>
            {
                entity.HasKey(e => e.RemediationPlanActionId)
                    .HasName("remediation_plan_action_map_pkey");

                entity.ToTable("remediation_plan_action_map");

                entity.Property(e => e.ActionStageId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.HasOne(d => d.Action)
                    .WithMany(p => p.RemediationPlanActionMaps)
                    .HasForeignKey(d => d.ActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActionId_1");

                entity.HasOne(d => d.RemediationPlan)
                    .WithMany(p => p.RemediationPlanActionMaps)
                    .HasForeignKey(d => d.RemediationPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RemediationPlanId");
            });

            modelBuilder.Entity<RemediationPlanExecution>(entity =>
            {
                entity.HasKey(e => e.RemediationPlanExecId)
                    .HasName("remediation_plan_executions_pkey");

                entity.ToTable("remediation_plan_executions");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ExecutedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ExecutionEndDateTime).HasPrecision(6);

                entity.Property(e => e.ExecutionStartDateTime).HasPrecision(6);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.NodeDetails)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<RemediationPlanExecutionAction>(entity =>
            {
                entity.HasKey(e => e.RemediationPlanExecActionId)
                    .HasName("remediation_plan_execution_actions_pkey");

                entity.ToTable("remediation_plan_execution_actions");

                entity.Property(e => e.CorrelationId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Logdata)
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.OrchestratorDetails)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.Output)
                    .HasMaxLength(2000)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.HasOne(d => d.RemediationPlanExec)
                    .WithMany(p => p.RemediationPlanExecutionActions)
                    .HasForeignKey(d => d.RemediationPlanExecId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RemediationPlanExecId");
            });

            modelBuilder.Entity<Resource>(entity =>
            {
                entity.ToTable("resource");

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Comments)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ResourceName)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceRef)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Source)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);

                entity.Property(e => e.VersionNumber)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ResourceAttribute>(entity =>
            {
                entity.HasKey(e => new { e.ResourceId, e.AttributeName })
                    .HasName("PK__resource__B5DF3C9D3430E9ED");

                entity.ToTable("resource_attributes");

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeValue)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.VersionNumber)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.ResourceAttributes)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResourceID_2");
            });

            modelBuilder.Entity<ResourceDependencyMap>(entity =>
            {
                entity.HasKey(e => new { e.ResourceId, e.DependencyResourceId, e.PortfolioId });

                entity.ToTable("resource_dependency_map");

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DependencyResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PortfolioId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DependencyType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.ResourceDependencyMaps)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResourceId_1");
            });

            modelBuilder.Entity<ResourceObservableActionMap>(entity =>
            {
                entity.HasKey(e => new { e.ResourceId, e.ObservableId, e.ActionId })
                    .HasName("PK__resource__4E964DECC469954B");

                entity.ToTable("resource_observable_action_map");

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);

                entity.HasOne(d => d.Action)
                    .WithMany(p => p.ResourceObservableActionMaps)
                    .HasForeignKey(d => d.ActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActionId_2");

                entity.HasOne(d => d.Observable)
                    .WithMany(p => p.ResourceObservableActionMaps)
                    .HasForeignKey(d => d.ObservableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObservableId_5");

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.ResourceObservableActionMaps)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResourceId_4");
            });

            modelBuilder.Entity<ResourceObservableRemediationPlanMap>(entity =>
            {
                entity.HasKey(e => new { e.ResourceId, e.ObservableId, e.RemediationPlanId })
                    .HasName("PK__resource__060F25F40A34CD62");

                entity.ToTable("resource_observable_remediation_plan_map");

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);

                entity.HasOne(d => d.Observable)
                    .WithMany(p => p.ResourceObservableRemediationPlanMaps)
                    .HasForeignKey(d => d.ObservableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObservableId_3");

                entity.HasOne(d => d.RemediationPlan)
                    .WithMany(p => p.ResourceObservableRemediationPlanMaps)
                    .HasForeignKey(d => d.RemediationPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RemediationPlanId_2");

                entity.HasOne(d => d.Resource)
                    .WithMany(p => p.ResourceObservableRemediationPlanMaps)
                    .HasForeignKey(d => d.ResourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResourceId_3");
            });

            modelBuilder.Entity<Resourcetype>(entity =>
            {
                entity.ToTable("resourcetype");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.PlatfromType)
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceTypeName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);
            });

            modelBuilder.Entity<ResourcetypeDependencyMap>(entity =>
            {
                entity.HasKey(e => new { e.ResourcetypeId, e.DependencyResourceTypeId })
                    .HasName("PK__resource__4530BFD0F1C2E9BB");

                entity.ToTable("resourcetype_dependency_map");

                entity.Property(e => e.DependencyResourceTypeId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DependencyType)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsFixedLength(true);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.PortfolioId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);
            });

            modelBuilder.Entity<ResourcetypeMetadatum>(entity =>
            {
                entity.HasKey(e => new { e.ResourceTypeId, e.AttributeName })
                    .HasName("PK__resource__CA17F26B8CACE111");

                entity.ToTable("resourcetype_metadata");

                entity.Property(e => e.AttributeName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeType)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DefaultValue)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.HasOne(d => d.ResourceType)
                    .WithMany(p => p.ResourcetypeMetadata)
                    .HasForeignKey(d => d.ResourceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResourceTypeId_4");
            });

            modelBuilder.Entity<ResourcetypeObservableActionMap>(entity =>
            {
                entity.HasKey(e => new { e.ResourceTypeId, e.ObservableId, e.ActionId })
                    .HasName("PK__resource__315E831A7EFC3872");

                entity.ToTable("resourcetype_observable_action_map");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);

                entity.HasOne(d => d.Action)
                    .WithMany(p => p.ResourcetypeObservableActionMaps)
                    .HasForeignKey(d => d.ActionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ActionId");

                entity.HasOne(d => d.Observable)
                    .WithMany(p => p.ResourcetypeObservableActionMaps)
                    .HasForeignKey(d => d.ObservableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObservableId_1");

                entity.HasOne(d => d.ResourceType)
                    .WithMany(p => p.ResourcetypeObservableActionMaps)
                    .HasForeignKey(d => d.ResourceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResourceTypeId");
            });

            modelBuilder.Entity<ResourcetypeObservableMap>(entity =>
            {
                entity.HasKey(e => new { e.ObservableId, e.ResourceTypeId })
                    .HasName("PK__resource__089363854C68ECC1");

                entity.ToTable("resourcetype_observable_map");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LowerThreshold)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.OperatorId)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpperThreshold)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);

                entity.HasOne(d => d.Observable)
                    .WithMany(p => p.ResourcetypeObservableMaps)
                    .HasForeignKey(d => d.ObservableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObservableID3");

                entity.HasOne(d => d.ResourceType)
                    .WithMany(p => p.ResourcetypeObservableMaps)
                    .HasForeignKey(d => d.ResourceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResourceTypeID4");
            });

            modelBuilder.Entity<ResourcetypeObservableRemediationPlanMap>(entity =>
            {
                entity.HasKey(e => new { e.ResourceTypeId, e.ObservableId })
                    .HasName("PK_1");

                entity.ToTable("resourcetype_observable_remediation_plan_map");

                entity.Property(e => e.CreateDate).HasPrecision(6);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ValidityEnd).HasPrecision(6);

                entity.Property(e => e.ValidityStart).HasPrecision(6);

                entity.HasOne(d => d.Observable)
                    .WithMany(p => p.ResourcetypeObservableRemediationPlanMaps)
                    .HasForeignKey(d => d.ObservableId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ObservableId_2");

                entity.HasOne(d => d.RemediationPlan)
                    .WithMany(p => p.ResourcetypeObservableRemediationPlanMaps)
                    .HasForeignKey(d => d.RemediationPlanId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RemediationPlanId_1");

                entity.HasOne(d => d.ResourceType)
                    .WithMany(p => p.ResourcetypeObservableRemediationPlanMaps)
                    .HasForeignKey(d => d.ResourceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ResourceTypeId_1");
            });

            modelBuilder.Entity<ResourcetypeServiceDetail>(entity =>
            {
                entity.HasKey(e => e.ServiceId)
                    .HasName("PK__resource__C51BB00A59040197");

                entity.ToTable("resourcetype_service_details");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ServiceName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.ToTable("tenant");

                entity.Property(e => e.BaseUrl).IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasPrecision(6);

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasPrecision(6);

                entity.Property(e => e.Name)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.ToTable("tickets");

                entity.Property(e => e.Id)
                    .ValueGeneratedNever()
                    .HasColumnName("id");

                entity.Property(e => e.Autoresolutionattempted).HasColumnName("autoresolutionattempted");

                entity.Property(e => e.Comments)
                    .IsRequired()
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("comments");

                entity.Property(e => e.Createdate)
                    .HasColumnType("date")
                    .HasColumnName("createdate");

                entity.Property(e => e.Resolutiondate)
                    .HasColumnType("date")
                    .HasColumnName("resolutiondate");

                entity.Property(e => e.Resolvingparty)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("resolvingparty");

                entity.Property(e => e.Shortdesc)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("shortdesc");

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("status");

                entity.Property(e => e.Ticketid)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ticketid");

                entity.Property(e => e.Ticketingsystemname)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("ticketingsystemname");
            });

            modelBuilder.Entity<UnhealthyReportRemediation>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("Unhealthy_report_Remediation");

                entity.Property(e => e.ObservationTime).HasPrecision(6);

                entity.Property(e => e.RemediationStatus)
                    .IsRequired()
                    .HasMaxLength(9)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
