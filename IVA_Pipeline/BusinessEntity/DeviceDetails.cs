/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
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
        public string MetricType { get; set; }
        public string EmailNoticationDescription { get; set; }
        public string TasksRoute { get; set; }
        public string TransportRegionCodes { get; set; }
        public int VideoStreamingOption { get; set; }
        public string FfmpegArguments { get; set; }
        public string FfmpegArgumentsRawInput {get;set;}
        public bool DisplayAllFrames { get; set; }

        public string BaseUrl { get; set; }

        public string PredictionModel { get; set; }
        public int FrameToPredict { get; set; }

        public bool DownLoadLot { get; set; }

        public string VideoFormatsAllowed { get; set; }
        public bool CleanUpStreamingFolder { get; set; }
        public string StreamingPath { get; set; }
        public string StreamingPathRaw {get;set;}
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
        public string MILLibraryName { get; set; }
        public string IndexName { get; set; }
        public string SpeedDetection { get; set; }
        public string SegmentColors { get; set; }
        public string PanopticSegmentation { get; set; }
        public string PythonVirtualPath { get; set; }
        public string LabelColor { get; set; }

        public string PythonVersion { get; set; }

        public string BackgroundColor { get; set; }

        public int RendererRectanglePointX { get; set; }

        public int RendererRectanglePointY { get; set; }

        public int RendererLabelPointX { get; set; }

        public int RendererLabelPointY { get; set; }

        public int RendererRectangleHeight { get; set; }

        public string RendererPredictCartListBackgroundColor { get; set; }
        public string BackgroundChange { get; set; }
        public string FfmpegforBackgroundChange { get; set; }
        public string PromptInputDirectory { get; set; }
        public string MaskImageDirectory { get; set; }
        public string ReplaceImageDirectory { get; set; }
        public string OutputImage { get; set; }
        public string NumberOfInferenceSteps { get; set; }
        public string OutputImageWidth { get; set; }
        public string OutputImageHeight { get; set; }
        public string NumberOfOutputImages { get; set; }
        public string GuidanceScale { get; set; }
        public string LcmOriginSteps { get; set; }
        public Double RendererFontScale { get; set; }

        public string EnableElasticStore { get; set; }
        
        public string XaiApiVersion { get; set; }
        public string XaiToRun { get; set; }
        public string XaiModel { get; set; }

        public int XaiBatchSize { get; set; }


        
        public string XaiTemplateName { get; set; }
        public string HyperParameters { get; set; }
        public string ObjectDetectionRendering { get; set; }
        public string RenderImageFilePath {get;set;}
        public string RenderImageEnabled {get;set;}
        public string DebugImageFilePath {get;set;}
        public string ImageDebugEnabled {get;set;}
        public bool EnablePing {get;set;}
        public int ClientConnectionRetryCount {get;set;}
        public int FrameRenderer_WaitTimeForTransportms {get;set;}
        public int FrameRenderer_EOF_Count {get;set;}
        public string FrameRenderer_EOF_File_Path {get;set;}
        public int FrameGrabRateThrottlingSleepFrameCount {get;set;}
        public int FrameGrabRateThrottlingSleepDurationMsec {get;set;}
        public string FfmpegExeFile {get;set;}
        public string CalculateFrameGrabberFPR {get;set;}
        public int MaxEmptyFrameCount {get;set;}
        public int EmptyFrameProcessInterval {get;set;}
        public int FTPCycle {get;set;}
        public string ElasticStoreIndexName {get;set;}
        public string PromptTemplatesDirectory {get;set;}
        public double ReduceFrameQualityTo {get;set;}
        public int MinThreadOnPool {get;set;}
        public int MaxThreadOnPool {get;set;}
        public int MaxFailCount {get;set;}
        public string ImageFormatsToUse {get;set;}
        public int OfflineProcessInterval {get;set;}
        public string DataStreamTimeOut {get;set;}
        public int ClientConnectionWaitingTime {get;set;}
        public string ProcessLoaderTraceFile {get;set;}
        public string PredictionType {get;set;}
        public string AnalyticsPredictionType {get;set;}
        public bool DBEnabled {get;set;}
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
