/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity
{
    public class DeviceDetails
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

        public int ComplianceUpperThreshold { get; set; }
        public int ComplianceLowerThreshold { get; set; }
        public int NoMaskUpperThreshold { get; set; }
        public int NoMaskLowerThreshold { get; set; }
        public string MaskLabel { get; set; }
        public string NoMaskLabel { get; set; }
        public string AllIpAddress { get; set; }

        public float ConfidenceThreshold { get; set; }
        public float OverlapThreshold { get; set; }
        public bool EnableLots { get; set; }
        public string NoMaskPenColor { get; set; }
        public string MaskPenColor { get; set; }
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
        public string MlModelUrl { get; set; }
        public string MetricType { get; set; }
        public string EmailNoticationDescription { get; set; }
        public string TasksRoute { get; set; }
        public string TransportRegionCodes { get; set; }
        public bool IsClientActive { get; set; }
        public int VideoStreamingOption { get; set; }
        public string FfmpegArguments { get; set; }
        public bool DisplayAllFrames { get; set; }

        public string BaseUrl { get; set; }

        public string PredictionModel { get; set; }
        public int FrameToPredict { get; set; }

        public bool DownLoadLot { get; set; }

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
        public string MediaStreamingUrl { get; set; }

        public bool SharedBlobStorage { get; set; }

        public TemplateMatching TemplateMatching { get; set; }

        public string MsgVersion { get; set; }

        public string InfVersion { get; set; }

        public Dictionary<int, List<float>> Kp { get; set; }

        public Dictionary<int, List<int>> KpSkeleton { get; set; }

        public string PosePointRendering { get; set; }

        public string  SegmentRendering { get; set; }

        public string ClassificationRendering { get; set; }

        public string PredictCart { get; set; }

        public string Tracking { get; set; }

        public string CrowdCounting { get; set; }

        public string PcdDirectory {get;set;}
        public int RendererFontThickness{ get; set; }

        public string Mplug { get; set; }

        public string HeatMap { get; set; }

        public string IndexName { get; set; }
        public string SpeedDetection { get; set; }
        public string SegmentColors { get; set; }
        public string PanopticSegmentation { get; set; }
        public string PythonVirtualPath { get; set; }
        public string LabelColor { get; set; }

        public string PythonVersion { get; set; }

        public string BackGroundColor { get; set; }

        public int RendererRectanglePointX { get; set; }

        public int RendererRectanglePointY { get; set; }

        public int RendererLabelPointX { get; set; }

        public int RendererLabelPointY { get; set; }

        public int RendererRectangleHeight { get; set; }

        public string RendererPredictCartListBackgroundColor { get; set; }
        public string BackgroundChange { get; set; }
        public string FfmpegforBackgroundChange { get; set; }
        public string EnablePrompt { get; set; }
        public string PromptInputDirectory { get; set; }
        public string MaskImageInput { get; set; }
        public string MaskImageDirectory { get; set; }
        public string ReplaceImageInput { get; set; }
        public string ReplaceImageDirectory { get; set; }
        public string BlobforGenerativeAI { get; set; }
        public string GENAI { get; set; }
        public string OutputImage { get; set; }
        public string NumberOfInferenceSteps { get; set; }
        public string OutputImageWidth { get; set; }
        public string OutputImageHeight { get; set; }
        public string NumberOfOutputImages { get; set; }
        public string GuidanceScale { get; set; }
        public string LcmOriginSteps { get; set; }
        public Double RendererFontScale { get; set; }

        public string EnableElasticStore { get; set; }
        public string ElasticStoreLabel { get; set; }
    }
    
    public class TemplateMatching {
        public bool FindControlInMultipleControlStates {get;set;}
        public int ImageRecognitionTimeout {get;set;}
        public bool UseTrueColorTemplateMatching {get;set;}
        public int ImageMatchConfidenceThreshold {get;set;}
        public bool MultipleScaleTemplateMatching {get;set;}
        public int ImageMatchMaxScaleStepCount {get;set;}
        public double ImageMatchScaleStepSize {get;set;}
        public bool EnableTemplateMatchMapping {get;set;}
        public bool WaitForever {get;set;}
        public int TemplateMatchMappingBorderThickness {get;set;}
        public bool MultiRotationTemplateMatching {get;set;}
        public double ImageMatchRotationStepAngle {get;set;}
    }
}
