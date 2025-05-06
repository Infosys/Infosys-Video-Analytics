/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity
{
    public class ConfigDetails
    {
        public string StorageBaseUrl { get; set; }
        public string CameraURl { get; set; }
        public string VideoFeedType { get; set; }
        public string OfflineVideoDirectory { get; set; }
        public string ArchiveDirectory { get; set; }
        public bool ArchiveEnabled { get; set; }
        public int LotSize { get; set; }
        public string ModelName { get; set; }
        public string UPModelName { get; set; }
        public string QueueName { get; set; }
        public string DeviceId { get; set; }
        public int TenantId { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }

        
        public float ConfidenceThreshold { get; set; }
        public float OverlapThreshold { get; set; }
        public bool EnableLots { get; set; }
        public int PenThickness { get; set; }
        public int LabelFontSize { get; set; }
        public string LabelFontStyle { get; set; }
        public int LabelHeight { get; set; }
        public string LabelFontColor { get; set; }

        public string DefaultPenColor { get; set; }
        public int FTPPerSeconds { get; set; }

        public string PreviousFrameCount { get; set; }
        public string SimilarityThreshold { get; set; }

        public bool UniquePersonTrackingEnabled { get; set; }
        public float UniquePersonOverlapThreshold { get; set; }
        public string BoxColor { get; set; }
        public string PredictionClassType { get; set; }
        public string MetricType { get; set; }
        public string EmailNoticationDescription { get; set; }
        public string TasksRoute { get; set; }
        public string TransportRegionCodes { get; set; }
        public int VideoStreamingOption { get; set; }
        public string FfmpegArguments { get; set; }
        public bool DisplayAllFrames { get; set; }
        public string VideoFormatsAllowed { get; set; }
        public bool CleanUpStreamingFolder { get; set; }
        public string StreamingPath { get; set; }
        public bool EnforceFrameSequencing { get; set; }
        public int MaxSequenceNumber { get; set; }
        public int InitialCollectionBufferingSize { get; set; }
        public bool DeleteFramesFromBlob { get; set; }
        public int FrameSequencingMessageStuckDuration { get; set; }
        public int FrameSequencingMessageRetry { get; set; }
        public int TransportSequencingBufferingSize { get; set; }
        public bool SegmentRendering { get; set; }

    }

    public class TaskRouteMetadata
    {
        public JObject TasksRoute { get; set; }
        public JObject TransportRegionCodes { get; set; }
    }


    public class HistoryDetails
    {
        public string RequestId { get; set; }
        public string ResourceId { get; set; }
        public Nullable<int> FeedProcessorMasterId { get; set; }
        public string VideoName { get; set; }
        public string Status { get; set; }
        public int TenantId { get; set; }
        public string LastFrameId { get; set; }
        public string LastFrameGrabbedTime { get; set; }
        public string LastFrameProcessedTime { get; set; }
        public string StartFrameProcessedTime { get; set; }
        public string FileName { get; set; }
        public string FeedURI { get; set; }
        public long ProcessingStartTimeTicks { get; set; }
        public Nullable<long> ProcessingEndTimeTicks { get; set; }
        public string MachineName { get; set; }
        public string ModelName { get; set; }
        public long FileSize { get; set; }
        public int VideoDuration { get; set; }
        public double Fps { get; set; }
        public int TotalFrameProcessed { get; set; }
        public double TimeTaken { get; set; }
        public double FrameProcessedRate { get; set; }

        public MediaMetadata VideoMetaData { get; set; }
    }

    public class MediaMetadata
    {

        public long FileSize { get; set; }
        public double VideoDuration { get; set; }
        public double Fps { get; set; }
        public string FileExtension { get; set; }
        public int BitRateKbs { get; set; }
        public string ColorModel { get; set; }
        public string Format { get; set; }
        public string FrameSize { get; set; }
    }


    public class MediaMetaDataMsg
    { 
        public MediaMetadataDetail MediaMetadataDetail { get; set; }
    }

    public class MediaMetadataDetail
    {
       
        public int MediaId { get; set; }
       
        public int FeedProcessorMasterId { get; set; }
       
        public string RequestId { get; set; }
        
       
        public System.DateTime CreatedDate { get; set; }
       
        public string CreatedBy { get; set; }
       
        public string ModifiedBy { get; set; }
       
        public Nullable<System.DateTime> ModifiedDate { get; set; }
       
        public int TenantId { get; set; }
       
        public string MetaData { get; set; }

    }

    public class AttributeDetailsResMsg
    {
     
        public List<AttributeDetails> Attributes { get; set; }
    

      
       public Dictionary<int, List<int>> KpSkeleton { get; set; }

  
    }

    public class AttributeDetails
    {
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
    }

    public class FeedStatusDetails
    {
        public string VideoName { get; set; }
        public string FeedId { get; set; }
        public string Status { get; set; }
        public long ProcessingStartTime { get; set; }
        public long ProcessingEndTime { get; set; }
        public long LastPredictorTime { get; set; }
        public string StreamingUrl { get; set; }

    }

    public class PredictCartDetails 
    {
        public string Ad { get; set; }
    }

}
