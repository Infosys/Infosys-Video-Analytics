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

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail
{
    public partial class framedetailsNewContext : DbContext
    {
        public framedetailsNewContext()
        {
        }

        public framedetailsNewContext(DbContextOptions<framedetailsNewContext> options)
            : base(options)
        {
        }

        public virtual DbSet<FeedProcessorMaster> FeedProcessorMasters { get; set; }
        public virtual DbSet<FeedRequest> FeedRequests { get; set; }
        public virtual DbSet<FrameMaster> FrameMasters { get; set; }
        public virtual DbSet<FrameMetadatum> FrameMetadata { get; set; }
        public virtual DbSet<FramePredictedClassDetail> FramePredictedClassDetails { get; set; }
        public virtual DbSet<MediaMetadatum> MediaMetadata { get; set; }
        public virtual DbSet<ObjectTrackingDetail> ObjectTrackingDetails { get; set; }

        public virtual DbSet<TemplateDetails> TemplateDetails { get; set; }

        public virtual DbSet<Template> Template { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                AppSettings appSettings = Config.AppSettings;
                switch(appSettings.DBProvider)
                {
                    case "postgres":
                        optionsBuilder.UseNpgsql(appSettings.FrameDetailStore);
                        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                        break;
                    default:
                        optionsBuilder.UseSqlServer(appSettings.FrameDetailStore);
                        break;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

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

            modelBuilder.Entity<FeedRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId)
                    .HasName("PK__feed_request_id");

                entity.ToTable("Feed_Request");

                entity.Property(e => e.RequestId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.LastFrameGrabbedTime).HasPrecision(6);

                entity.Property(e => e.LastFrameId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastFrameProcessedTime).HasPrecision(6);

                entity.Property(e => e.Model)
                    .HasMaxLength(350)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.VideoName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FrameMaster>(entity =>
            {
                entity.HasKey(e => new { e.ResourceId, e.FrameId })
                    .HasName("PK__frame_ma__5BB26551BB6EC923");

                entity.ToTable("frame_master");

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FrameId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.FrameGrabTime).HasPrecision(6);

                entity.Property(e => e.FrameMasterId).ValueGeneratedOnAdd();

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Mtp).HasPrecision(6);
            });

            modelBuilder.Entity<FrameMetadatum>(entity =>
            {
                entity.HasKey(e => new { e.ResourceId, e.FrameId, e.TenantId, e.PredictionType })
                    .IsClustered(false);

                entity.ToTable("frame_metadata");

                entity.Property(e => e.ResourceId)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FrameId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PredictionType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.FrameGrabTime).HasPrecision(6);

                entity.Property(e => e.MachineName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MetaData).IsRequired();

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.SequenceId).ValueGeneratedOnAdd();

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FramePredictedClassDetail>(entity =>
            {
                entity.ToTable("frame_predicted_class_details");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.FrameGrabTime).HasPrecision(6);

                entity.Property(e => e.FrameId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FrameProcessedDateTime).HasPrecision(6);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.PredictedClass)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PredictionType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Region)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ResourceId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.FrameMaster)
                    .WithMany(p => p.FramePredictedClassDetails)
                    .HasForeignKey(d => new { d.ResourceId, d.FrameId })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__frame_predicted___4D94879B");
            });

            modelBuilder.Entity<MediaMetadatum>(entity =>
            {
                entity.HasKey(e => e.MediaId)
                    .HasName("PK__media_pro__940A63F35323FD36");

                entity.ToTable("media_metadata  ");

                entity.Property(e => e.CreatedBy)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedDate).HasPrecision(6);

                entity.Property(e => e.MetaData).IsRequired();

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);

                entity.Property(e => e.RequestId)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ObjectTrackingDetail>(entity =>
            {
                entity.ToTable("object_tracking_details");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.DeviceId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FrameId)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedDate).HasPrecision(6);
            });

            modelBuilder.Entity<TemplateDetails>(entity =>
            {
                entity.HasNoKey();
                entity.ToTable("TemplateDetails");
                entity.Property(e => e.TemplateId)
            .IsRequired()
            .HasMaxLength(50)

            .IsUnicode(false);
                entity.Property(e => e.TemplateName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeValue)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeComparison)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.AttributeApplicability).HasPrecision(6);

                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedBy)
             .IsRequired()
             .HasMaxLength(50)
             .IsUnicode(false);
            });

            modelBuilder.Entity<Template>(entity =>
            {
                entity.ToTable("Template");

                entity.Property(e => e.TemplateId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TemplateName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);



                entity.Property(e => e.ModifiedBy)
                    .HasMaxLength(50)
                    .IsUnicode(false);
                entity.Property(e => e.CreatedBy)
             .IsRequired()
             .HasMaxLength(50)
             .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
