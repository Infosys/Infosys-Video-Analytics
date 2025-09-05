/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using System;


namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common
{
    public struct ApplicationConstants
    {
        public const string FRAMERENDERER_HANDLING_POLICY = "VideoAnalytics.FrameRender";
        public const string FRAMEVIEWER_HANDLING_POLICY="VideoAnalytics.FrameViewer";
        public const string WORKER_EXCEPTION_HANDLING_POLICY = "VideoAnalytics.Worker";
        public const string SERVICE_EXCEPTIONHANDLING_POLICY = "VideoAnalytics.Worker";

        #region Blob Constants
        public const string APP_NAME = "WEM";
        public const string DOCUMENTSTORE_KEY = "FrameRepository"; 
        public const string SCRIPT_STORE_KEY = "ScriptRepository";
        public const string TWOFIELD_KEY_FORMAT = "{0}_{1}";
        public const string SCRIPT_REPO_SERVICEINTERFACE = "/iapwemservices/WEMScriptService.svc";
        public const string COMMON_SERVICEINTERFACE = "/iapwemservices/WEMCommonService.svc";
        public const string SECURE_PASSCODE = "IAP2GO_SEC!URE";
        public const string ROLE_MANAGER = "Manager";
        public const string ROLE_ANALYST = "Analyst";
        public const string ROLE_AGENT = "Agent";
        public const string ROLE_GUEST = "Guest";
        public const string DOMAIN_USERS = "domain users";
        #endregion

        public struct FileExtensions
        {
            public const string jpg = ".jpg";
            public const string zip = ".zip";
            public const string mp4 = ".mp4";
            public const string txt = ".txt";
        }
        public struct R2wStatus
        {
            public const string successStatus = "Completed";
            public const string failureStatus = "Failure";
            public const string failureStatusMessage = "Couldn't  Post to R2W";

        }

        public struct UserDetails
        {
            public const string userName = "VA_UNIX_ADMIN";
            public const string machineName = "UNIX_VM";
        }



        public struct ProcessingStatus
        {
            public const string videoFilePrefix = "va_";
            public const string initiatedStatus = "Initiated";
            public const string inProgressStatus = "In Progress";
            public const string RequestedStatus = "Requested";
            public const string completedStatus = "Completed";
            public const string FrameRendererCompletedStatus = "Completed";
            public const int feedCompletedStatus = 2;
            public const int feedInprogressStatus = 2;
            public const string EndOfFile = "End of file";
            public const string PredictionModel = "PredictionModel";
            public const string FileNotFound = "File Not Found";
            public const string StreamingUrlAttributeName = "MEDIA_STREAMING_URL";
            public const string PredictionModelDb = "PREDICTION_MODEL";
            public const string SupportedFormat = "h264";
            public const string VideoFormatNotSupportedMsg = "Video format is not supported";
            public const string AllResourcesAreBusy = "No pipeline are available to serve your request. Please try after sometime";
            public const int initiatedStatusCode = 0;
            public const int inProgressStatusCode = 1;
            public const int completedStatusCode = 2;
            public const string FailedToPredict = "Failed To Predict";
            public const string StartOfFile = "Start of file";
            public const string EventHandling = "Event Handling";
            public const string Maintenance = "Maintenance";
            public const int CLIENT_VIEWER = 0;
            public const int FFMPEG = 1;
            public const int IMAGE = 2;
        }

        public struct FrameGrabberConstants
        {

            public const string configuartionServiceUrl = "Configuration/GetClientStatus?tid={0}&did={1}";
        }


        public struct FrameRendererKey
        {

            public const string clientActivationAttribute = "IS_CLIENT_ACTIVE";
            public static string maskLabel = "Mask";
            public static string noMaskLabel = "No Mask";
            public static string processIdPrefix = "FR_";
           public const string clientInactive = "no"; 
            public const string clientActive = "yes";
			public static string previousFrameNumber = "FR_PFN_";
            public static string UnderScore = "_";


        }

        public struct PreloaderrKey
        {
            public static string processIdPrefix = "PLR_";
        }
        public struct DataCollectionStatus
        {
            public const string frameCollector = "FrameCollector";
            public const string frameInserted = "FrameInserted";
            public const string successStatus = "Data Collection-Completed";
            public const string failureStatus = "Data Collection-Failed";
        }
        public struct JobHelperStatus
        {
            public const string successStatus = "Data Collection-Initiated";

        }
        public struct JsonTokenConstants
        {
            public const string frameId = "frameid";
            public const string predictions = "Fs";
            public const string DM = "Dm";
            public const string CS = "Cs";
            public const string LB = "Lb";
            public const string predictedclasssequenceId = "id";
            public const string PTS = "Pts";


        }
        public struct FrameDetailsProcessConstants
        {
            public const string maskClassType = "Mask";
            public const string nomaskClassType = "NoMask";
            public const string successStatus = "FrameMaster Data Collection-success";
            public const string failureStatus = "FrameMaster Data Collection-Failed";
        }

        public struct FacadeClient
        {
            public const string baseURI = "BaseURI";
            public const string operation = "Operation";
            public const string port = "Port";
            public const string endPoint = "EndPoint";
            public const string body = "Body";
            public const string R2W = "R2W";
            public const string apiAdapter = "Infosys.Ainauto.Adapters.Interfaces.IApiAdapter";
            public const string post = "POST";
            public const string postMethod = "Post";

        }

        public struct MetricIngestorFacadeClient
        {
            public const string baseURI = "BaseURI";
            public const string operation = "Operation";
            public const string port = "Port";
            public const string endPoint = "EndPoint";
            public const string body = "Body";
            public const string MetricIngestor = "SuperBotIngestor";
            public const string apiAdapter = "Infosys.Ainauto.Adapters.Interfaces.IApiAdapter";
            public const string post = "POST";
            public const string postMethod = "Post";
            public const string metricIngestorConfigKey = "SuperBotConfigData";
            public const string fileUrlFormat = "{0}//{1}//{2}//{3}";

        }
        public struct BlobCleanupConstants
        {
            public const string dateFormat = "dd-MMM-yyyy";
            public const string dateTimeFormat = "dd-MMM-yyyy-hhmmss";
            public const string pathConstant = "/";
            public const string getAllFilesConstant = ".*";
            public const string CleanupUpdateStatus = "Cleanup Completed";
            public const string NotFoundStatus = "Not found";

        }
        public struct FGPerfMonCategories
        {
            public const string FrameGrabber = @"FaceMaskDetection-FrameGrabber";
        }
        public struct FGPerfMonCounters
        {
            public const string FPS = @"Computed Frame Capture Rate/Sec";
            public const string FGFPS = @"Frame Grabber Capture Rate/Sec";
            public const string RawFPS = @"Raw Frame Capture Rate (FPS)";
            public const string FramesToPredictPerSec = @"# Of Frames to Predict/Sec";
            public const string FramesToPredict = @"Frame at Position To Predict";
            public const string FrameCountInLot = @"# of Frames in Lot";
            public const string ErrorCount = @"# of Errors";
            public const string FrameBlobUploadTime = @"Frame Post Wait Time(ms)";
            
            public const string TotalFramesProcessed = @"# of Frames Processed";
            public const string NumOfFramesProcessedPerSec = @"# of Frames Processed/Sec";
            public const string NumOfPredictionFramesProcessedPerSec = @"# of Prediction Frames Processed/Sec";
            public const string NumOfLotFramesProcessedPerSec = @"# of Lot Frames Processed/Sec";
            public const string FrameProcessingTime = @"Prediction Frame Process Time(ms)";
            
            public const string LotProcessingTime = @"Lot Process Time (ms)";
           
            public const string FrameFileSizeInBytes = @"Frame File Size in bytes";
            public const string LotFileSizeInBytes = @"Lot File Size in bytes";
            public const string TotalFramesUploaded = @"# of Frames Uploaded";
            public const string TotalLotsUploaded = @"# of Lots Uploaded";
        }

        public struct FRPerfMonCategories
        {
            public const string CategoryName = @"FaceMaskDetection-FrameRenderer";
        }

        public struct FRPerfMonCounters
        {

            public const string FramesRejectedCount = @"# of Predicted Frames Rejected";
            public const string PredictedFramesProcessedCount = @"# of Frames Processed (Predicted)";
            public const string LotFramesProcessedCount = @"# of Frames Processed (Lot)";
            public const string TotalFramesProcessed = @"# of Frames Processed (All)";
            public const string PredictedFrameProcessingTime = @"Prediction Frame Process Time (ms)";
            public const string LotFrameProcessingTime = @"Lot Frame Process Time (ms)";
            public const string FramesProcessedPerSecond = @"# of Frames Processed/Sec";
            public const string ByteSent = @"# of Bytes Sent/Request";
            public const string TotalErrors = @"# of Errors";
            public const string TransportingTimeforFrame = @"Time Taken to Transport Frame (ms)";
            public const string TotalObjectDetected = @"# of Object Detections (All)";
            public const string Mask1ObjectDetected = @"# of Object Detections (Mask1)";
            public const string Mask2ObjectDetected = @"# of Object Detections (Mask2)";
            public const string NoMaskObjectDetected = @"# of Object Detections (NoMask)";
            public const string CounterTemplate = @"# of Object Detections ({0})";
            public const string OverallFrameProcessingTime = @"Frame Dispatch Time (ms)";
            public const string PostFrameProcessingTime = @"Frame Post Processing Time (ms)";
            public const string TransportFrameProcessingTime = @"Transport Frame Processing Time (ms)";
            public const string FramesProcessedInPostProcessPerSecond = @"# of Frames Processed in Post Process/Sec";
            public const string FramesProcessedInTransPortPerSecond = @"# of Frames Processed in Transport/Sec";
            public const string TotalFramesDownloadedFromBlob = @"# of Frames Downloaded from Blob";
            public const string TotalFramesTransferred = @"# of Frames Transferred (All)";
            public const string FramesReceivedCount = @"# of Frames Received";            

        }

        public struct DCPerfMonCategories
        {
            public const string CategoryName = @"FaceMaskDetection-DataCollector";
        }

        public struct DCPerfMonCounters
        {

            public const string TotalFramesProcessed = @"# of Frames Processed (All)";
            public const string FramesProcessedPerSecond = @"# of Frames Processed/Sec";
            public const string TotalErrors = @"# of Errors";
            public const string OverallFrameProcessingTime = @"Overall Frame Process Time (ms)";
            public const string FramesReceivedCount = @"# of Frames Received";

        }
        public struct MetricIngestorPerfMonCategories
        {
            public const string CategoryName = @"FaceMaskDetection-MetricIngestor";
        }

        public struct MetricIngestorPerfMonCounters
        {

            public const string TotalFramesProcessed = @"# of Frames Processed (All)";
            public const string FramesProcessedPerSecond = @"# of Frames Processed/Sec";
            public const string TotalErrors = @"# of Errors";
            public const string OverallFrameProcessingTime = @"Overall Frame Process Time (ms)";

        }
        public struct PredictorPerfMonCategories
        {
            public const string CategoryName = @"FaceMaskDetection-Predictor";
        }

        public struct PredictorPerfMonCounters
        {
            public const string TotalFramesProcessed = @"# of Frames Processed (All)";
            public const string FramePredictionTime = @"Prediction Frame Process Time (ms)";
            public const string FramesProcessedPerSecond = @"# of Frames Processed/Sec";
            public const string TotalErrors = @"# of Errors";
            public const string TotalObjectDetected = @"# of Object Detections (All)";
            public const string Mask1ObjectDetected = @"# of Object Detections (Mask1)";
            public const string Mask2ObjectDetected = @"# of Object Detections (Mask2)";
            public const string NoMaskObjectDetected = @"# of Object Detections (NoMask)";
            public const string FramesWthNoObjectDetected = @"# of Object Not Detected";
            public const string OverallFrameProcessingTime = @"Overall Frame Process Time (ms)";
            public const string TotalFrameWithBadFormat = @"# of Frames - Bad Format";
            public const string FramesReceivedCount = @"# of Frames Received";

        }

        

        public struct IndexDetails
        {
            public const string FrameMetaDataActionStagingIndex = "framemetadata_staging_video4";

        }
    }

    public struct TaskRouteConstants
    {
        public const string TaskRouteKey = @"{0}_{1}_TaskRoute";
        public const string FrameGrabberCode = "FGR";
        public const string PreLoaderCode = "PRL";
        public const string FrameProcessorCode = "OPR";
        public const string UniquePersonCode = "TPR";
        public const string MetricIngestorCode = "MIN";
        public const string FrameRendererCode = "REN";
        public const string FrameCollectorCode = "DCO";
        public const string AnalyticsCode = "ANA";
        public const string FrameGrabberLotCode = "FGL";
        public const string FrameGrabberUniquePersonCode = "FGU";
        public const string FrameRepositoryRegion = "FrameRepository";
        public const string MemoryDoc = "MemoryDoc";
        public const string FrameElasticSearch = "FES";
        public const string FrameExplainer = "XEN";
        public const string PromptHandler = "PRH";
        public const string PromptInjector = "PRI";
        public const string FrameExplainerNode = "FEN";
        public const string SensorDataCollectorProcess = "SEN";
        public const string FrameExplainerDataCollector = "XDCO";
        public const string PcdHandlerCode="PCH";
        public const string DataAggregatorCode = "AGR";
        public const string FrameViewerCode="FVI";
        public const string ExplainerModelPredictor = "EMR";
        
    }

    public struct CacheConstants
    {
        public const string CacheKeyFormat = "{0}_{1}_{2}";
        public const string CacheKeyFormatForToken = "AICloudAuthToken";
        public const string TaskRouteKey = "TR";
        public const string FrameProcessorCode = "FP";
        public const string AnalyticsCode = "ANA";
        public const string FrameRendererCode = "REN";
        public const string FrameGrabberCode = "FGR";
        public const string UniquePersonCode = "TPR";
        public const string FrameCollectorCode = "DCO";
        public const string FramePreloaderCode = "PRL";
        public const string MetricIngestorCode = "MIG";
        public const string FrameRendererEOF = "FrameRendererEOF";
        public const string CacheKeyFormatForFfmpegIntialise = "FFMPEGIntialise";
        public const string FrameElasticSearch = "FES";
        public const string PromptHandler = "PRH";
        public const string PromptInjector = "PRI";
        public const string SensorDataCollectorProcess = "SEN";
        public const string FrameExplainerDataCollector = "XDCO";
        public const string PcdHandlerCode="PCH";
        public const string FrameViewerCode="FVI";
        public const string FrameExplainer="XEN";
        public const string ObjectDetectorAnalytics="ODA";
        public const string HelperCode="HEL";
        public const string ClientConnectHost="CCH";
        public const string ClientTCPConnect="CTC";
        public const string DataAggregatorCode = "AGR";
        public const string ExplainerModelPredictor = "EMR";
        
    }


    public enum NotificationType
    {
        None = 0,
        AnomalyDetected,
        RemediationFailure,
        ThresholdBreach,
        NoRemediation,
        EnvironmentScan,
        RemediationSuccess,
        Summary,
        EnvironmentScanConsolidated
    }

    public enum NotificationChannel
    {
        Email = 1,
        SMS = 2,
        Push = 4,
    }
    public enum XaiConstantsAttributes
    {
        Ad,
        Class,
        Did,
        FrameNumber,
        Cs,
        Lb,
        AttributeComparison,
        Yes,
        No,
        Explainer_Metadata
    }
    public static class ExtentionMethodsClass
    {
        public static string RemoveSpaces(this string str)
        {
            
            return str.Replace(" ", "");


        }
        public struct CacheConstants
        {
            public const string CacheKeyFormat = "{0}_{1}_{2}";
            public const string CacheKeyFormatForToken = "AICloudAuthToken";
            public const string TaskRouteKey = "TR";
            public const string FrameProcessorCode = "FP";
            public const string AnalyticsCode = "ANA";
            public const string FrameRendererCode = "REN";
            public const string UniquePersonCode = "TPR";
            public const string FrameCollectorCode = "DCO";
            public const string FramePreloaderCode = "PRL";
            public const string PromptHandler = "PRH";
            public const string PromptInjector = "PRI";
            public const string FrameExplainer = "FEN";
            public const string FrameElasticSearch = "FES";
            
            public const string SensorDataCollectorProcess = "SEN";
            public const string FrameExplainerDataCollector = "XDCO";
            public const string ExplainerModelPredictor = "EMR";    
        }

        public struct TaskRouteConstants
        {
            public const string TaskRouteKey = @"{0}_{1}_TaskRoute";
            public const string FrameGrabberCode = "FGR";
            public const string PreLoaderCode = "PRL";
            public const string FrameProcessorCode = "OPR";
            public const string UniquePersonCode = "TPR";
            public const string MetricIngestorCode = "MIN";
            public const string FrameRendererCode = "REN";
            public const string FrameCollectorCode = "DCO";
            public const string AnalyticsCode = "ANA";
            public const string FrameGrabberLotCode = "FGL";
            public const string FrameGrabberUniquePersonCode = "FGU";
            public const string PromptHandler = "PRH";
            public const string PromptInjector = "PRI";
            public const string FrameExplainer = "XEN";
            public const string FrameElasticSearch = "FES";
            public const string FrameExplainerNode = "XEN";
            public const string SensorDataCollectorProcess = "SEN";
            public const string FrameExplainerDataCollector = "XDCO";
            public const string ExplainerModelPredictor = "EMR";
        }
        public static double ConvertToDouble(this string str)
        {
            double d;
            if (Double.TryParse(str, out d))
                return d;
            else
                return 0;
        }
        public static DateTime ConvertToDate(this string str)
        {
            DateTime d;
            if (DateTime.TryParse(str, out d))
                return d;
            else
                return new DateTime();
        }
        public enum XaiConstantsAttributes
        {
            Ad,
            Class,
            Did,
            FrameNumber,
            Cs,
            Lb
        }

    }

}

