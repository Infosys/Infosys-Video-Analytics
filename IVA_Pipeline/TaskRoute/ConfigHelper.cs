/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess.Document;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using System.Runtime.Caching;
using Newtonsoft.Json;
using System.Configuration;
using System.IO;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute
{
    public class ConfigHelper
    {
        static AppSettings appSettings = Config.AppSettings;
        public static ObjectCache Cache;
        public static string DeviceDetailsCacheKey;

        public static DeviceDetails SetDeviceDetails(string tId,string deviceId,string keyPrefix) {
            DeviceDetails deviceDetails=null;
            Cache=MemoryCache.Default;
            CacheItemPolicy policy=new CacheItemPolicy();
            SC.MaskDetector maskDetector=new SC.MaskDetector();
            string configSource=appSettings.ConfigSource;
            #if DEBUG
            using(LogHandler.TraceOperations("Helper:SetDeviceDetails",LogHandler.Layer.Business,Guid.NewGuid(),null)) {
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"SetDeviceDetails","Helper"),LogHandler.Layer.Business,null);
                #endif
                
                DeviceDetailsCacheKey=string.Format(CacheConstants.CacheKeyFormat,keyPrefix,tId,deviceId);
                
                deviceDetails=(DeviceDetails)ConfigHelper.Cache[ConfigHelper.DeviceDetailsCacheKey];
                string responseString=string.Empty;
                if(deviceDetails==null) {
                    if(configSource=="file") {
                        string configFilePath=appSettings.ConfigFilePath;
                        
                        using(StreamReader r=new StreamReader(configFilePath)) {
                            responseString=r.ReadToEnd();
                            
                        }
                    }
                    else {
                        var uri=String.Format($"{Config.AppSettings.ConfigWebApi}Configuration/GetDeviceAttributes?tid={int.Parse(tId)}&did={deviceId}");
                        responseString=ServiceCaller.ServiceCall(null,uri,"GET");
                    }
                    
                    var res=JsonConvert.DeserializeObject<AttributeDetailsResMsg>(responseString);
                    var response=AssignConfigValues(JsonConvert.DeserializeObject<AttributeDetailsResMsg>(responseString));
                                        
                    if(response==null)
                        throw new FaceMaskDetectionCriticalException("Failed to get device configuration from services. Response is null.");
                    deviceDetails=new DeviceDetails();
                    deviceDetails.ArchiveDirectory=response.ArchiveDirectory;
                    deviceDetails.ArchiveEnabled=response.ArchiveEnabled;
                    deviceDetails.BaseUrl=response.BaseUrl;
                    deviceDetails.BoxColor=response.BoxColor;
                    deviceDetails.CameraURl=response.CameraURl;
                    deviceDetails.ConfidenceThreshold=response.ConfidenceThreshold;
                    deviceDetails.DefaultPenColor=response.DefaultPenColor;
                    deviceDetails.DeviceId=response.DeviceId;
                    deviceDetails.DisplayAllFrames=response.DisplayAllFrames;
                    deviceDetails.DownLoadLot=response.DownLoadLot;
                    deviceDetails.EmailNoticationDescription=response.EmailNoticationDescription;
                    deviceDetails.EnableLots=response.EnableLots;
                    deviceDetails.FfmpegArguments=response.FfmpegArguments;
                    deviceDetails.FfmpegArgumentsRawInput=response.FfmpegArgumentsRawInput;
                    deviceDetails.FrameToPredict=response.FrameToPredict;
                    deviceDetails.FTPPerSeconds=response.FTPPerSeconds;
                    deviceDetails.IpAddress=response.IpAddress;
                    deviceDetails.LabelFontColor=response.LabelFontColor;
                    deviceDetails.LabelFontSize=response.LabelFontSize;
                    deviceDetails.LabelFontStyle=response.LabelFontStyle;
                    deviceDetails.LabelHeight=response.LabelHeight;
                    deviceDetails.LotSize=response.LotSize;
                    deviceDetails.MetricType=response.MetricType;
                    deviceDetails.ModelName=response.ModelName;
                    deviceDetails.OfflineVideoDirectory=response.OfflineVideoDirectory;
                    deviceDetails.MILLibraryName= response.MILLibraryName;
                    deviceDetails.OverlapThreshold=response.OverlapThreshold;
                    deviceDetails.PenThickness=response.PenThickness;
                    deviceDetails.Port=response.Port;
                    deviceDetails.PredictionModel=response.PredictionModel;
                    deviceDetails.PreviousFrameCount=response.PreviousFrameCount;
                    deviceDetails.SimilarityThreshold=response.SimilarityThreshold;
                    deviceDetails.StorageBaseUrl=response.StorageBaseUrl;
                    deviceDetails.TasksRoute=response.TasksRoute;
                    deviceDetails.TenantId=response.TenantId;
                    deviceDetails.TransportRegionCodes=response.TransportRegionCodes;
                    deviceDetails.UniquePersonOverlapThreshold=response.UniquePersonOverlapThreshold;
                    deviceDetails.UniquePersonTrackingEnabled=response.UniquePersonTrackingEnabled;
                    deviceDetails.UPModelName=response.UPModelName;
                    deviceDetails.VideoFeedType=response.VideoFeedType;
                    deviceDetails.VideoStreamingOption=response.VideoStreamingOption;
                    
                    deviceDetails.BaseUrl=response.StorageBaseUrl;
                    deviceDetails.PredictionModel=response.ModelName;
                    deviceDetails.FrameToPredict=response.FrameToPredict;
                    deviceDetails.FrameSequencingMessageStuckDuration=response.FrameSequencingMessageStuckDuration;
                    deviceDetails.FrameSequencingMessageRetry=response.FrameSequencingMessageRetry;
                    deviceDetails.MaxSequenceNumber=response.MaxSequenceNumber;
                    deviceDetails.EnforceFrameSequencing=response.EnforceFrameSequencing;
                    deviceDetails.InitialCollectionBufferingSize=response.InitialCollectionBufferingSize;
                    deviceDetails.DeleteFramesFromBlob=response.DeleteFramesFromBlob;
                    deviceDetails.TransportSequencingBufferingSize=response.TransportSequencingBufferingSize;
                    deviceDetails.StreamingPath=response.StreamingPath;
                    deviceDetails.StreamingPathRaw=response.StreamingPathRaw;
                    deviceDetails.SharedBlobStorage=response.SharedBlobStorage;
                    deviceDetails.MsgVersion=response.MsgVersion;
                    deviceDetails.InfVersion=response.InfVersion;
                    deviceDetails.KpSkeleton=response.KpSkeleton;
                    deviceDetails.PosePointRendering=response.PosePointRendering;
                    deviceDetails.SegmentRendering=response.SegmentRendering;
                    deviceDetails.ClassificationRendering=response.ClassificationRendering;
                    deviceDetails.PredictCart=response.PredictCart;
                    deviceDetails.Tracking=response.Tracking;
                    deviceDetails.CrowdCounting=response.CrowdCounting;
                    deviceDetails.RendererFontScale=response.RendererFontScale;
                    deviceDetails.RendererFontThickness=response.RendererFontThickness;
                    deviceDetails.Mplug=response.Mplug;
                    deviceDetails.HeatMap=response.HeatMap;
                    deviceDetails.IndexName=response.IndexName;
                    deviceDetails.SpeedDetection=response.SpeedDetection;
                    deviceDetails.SegmentColors=response.SegmentColors;
                    deviceDetails.PythonVirtualPath=response.PythonVirtualPath;
                    deviceDetails.PanopticSegmentation=response.PanopticSegmentation;
                    deviceDetails.LabelColor=response.LabelColor;
                    deviceDetails.PythonVersion=response.PythonVersion;
                    deviceDetails.BackgroundColor=response.BackgroundColor;
                    deviceDetails.RendererRectanglePointX=response.RendererRectanglePointX;
                    deviceDetails.RendererRectanglePointY=response.RendererRectanglePointY;
                    deviceDetails.RendererLabelPointX=response.RendererLabelPointX;
                    deviceDetails.RendererLabelPointY=response.RendererLabelPointY;
                    deviceDetails.RendererRectangleHeight=response.RendererRectangleHeight;    
                    deviceDetails.RendererPredictCartListBackgroundColor=response.RendererPredictCartListBackgroundColor;
                    deviceDetails.BackgroundChange=response.BackgroundChange;
                    deviceDetails.FfmpegforBackgroundChange=response.FfmpegforBackgroundChange;
                    deviceDetails.PromptInputDirectory=response.PromptInputDirectory;
                    deviceDetails.MaskImageDirectory=response.MaskImageDirectory;
                    deviceDetails.ReplaceImageDirectory=response.ReplaceImageDirectory;
                    deviceDetails.OutputImage=response.OutputImage;
                    deviceDetails.PcdDirectory=response.PcdDirectory;
                    deviceDetails.RenderImageFilePath=response.RenderImageFilePath;
                    deviceDetails.RenderImageEnabled=response.RenderImageEnabled;
                    deviceDetails.DebugImageFilePath=response.DebugImageFilePath;
                    deviceDetails.ImageDebugEnabled=response.ImageDebugEnabled;
                    deviceDetails.EnablePing=response.EnablePing;
                    deviceDetails.ClientConnectionRetryCount=response.ClientConnectionRetryCount;
                    deviceDetails.FrameRenderer_WaitTimeForTransportms=response.FrameRenderer_WaitTimeForTransportms;
                    deviceDetails.FrameRenderer_EOF_Count=response.FrameRenderer_EOF_Count;
                    deviceDetails.FrameRenderer_EOF_File_Path=response.FrameRenderer_EOF_File_Path;
                    deviceDetails.FrameGrabRateThrottlingSleepFrameCount=response.FrameGrabRateThrottlingSleepFrameCount;
                    deviceDetails.FrameGrabRateThrottlingSleepDurationMsec=response.FrameGrabRateThrottlingSleepDurationMsec;
                    deviceDetails.FfmpegExeFile=response.FfmpegExeFile;
                    deviceDetails.CalculateFrameGrabberFPR=response.CalculateFrameGrabberFPR;
                    deviceDetails.MaxEmptyFrameCount=response.MaxEmptyFrameCount;
                    deviceDetails.EmptyFrameProcessInterval=response.EmptyFrameProcessInterval;
                    deviceDetails.FTPCycle=response.FTPCycle;
                    deviceDetails.ElasticStoreIndexName=response.ElasticStoreIndexName;
                    deviceDetails.PromptTemplatesDirectory=response.PromptTemplatesDirectory;
                    deviceDetails.ReduceFrameQualityTo=response.ReduceFrameQualityTo;
                    deviceDetails.MinThreadOnPool=response.MinThreadOnPool;
                    deviceDetails.MaxThreadOnPool=response.MaxThreadOnPool;
                    deviceDetails.MaxFailCount=response.MaxFailCount;
                    deviceDetails.VideoFormatsAllowed=response.VideoFormatsAllowed;
                    deviceDetails.ImageFormatsToUse=response.ImageFormatsToUse;
                    deviceDetails.OfflineProcessInterval=response.OfflineProcessInterval;
                    deviceDetails.DataStreamTimeOut=response.DataStreamTimeOut;
                    deviceDetails.ClientConnectionWaitingTime=response.ClientConnectionWaitingTime;
                    deviceDetails.ProcessLoaderTraceFile=response.ProcessLoaderTraceFile;
                    deviceDetails.PredictionType=response.PredictionType;
                    deviceDetails.AnalyticsPredictionType=response.AnalyticsPredictionType;
                    deviceDetails.DBEnabled=response.DBEnabled;
                    deviceDetails.TemplateMatching=new TemplateMatching();
                    deviceDetails.TemplateMatching.FindControlInMultipleControlStates=response.TemplateMatching.FindControlInMultipleControlStates;
                    deviceDetails.TemplateMatching.ImageRecognitionTimeout=response.TemplateMatching.ImageRecognitionTimeout;
                    deviceDetails.TemplateMatching.UseTrueColorTemplateMatching=response.TemplateMatching.UseTrueColorTemplateMatching;
                    deviceDetails.TemplateMatching.ImageMatchConfidenceThreshold=response.TemplateMatching.ImageMatchConfidenceThreshold;
                    deviceDetails.TemplateMatching.MultipleScaleTemplateMatching=response.TemplateMatching.MultipleScaleTemplateMatching;
                    deviceDetails.TemplateMatching.ImageMatchMaxScaleStepCount=response.TemplateMatching.ImageMatchMaxScaleStepCount;
                    deviceDetails.TemplateMatching.ImageMatchScaleStepSize=response.TemplateMatching.ImageMatchScaleStepSize;
                    deviceDetails.TemplateMatching.EnableTemplateMatchMapping=response.TemplateMatching.EnableTemplateMatchMapping;
                    deviceDetails.TemplateMatching.WaitForever=response.TemplateMatching.WaitForever;
                    deviceDetails.TemplateMatching.TemplateMatchMappingBorderThickness=response.TemplateMatching.TemplateMatchMappingBorderThickness;
                    deviceDetails.TemplateMatching.MultiRotationTemplateMatching=response.TemplateMatching.MultiRotationTemplateMatching;
                    deviceDetails.TemplateMatching.ImageMatchRotationStepAngle=response.TemplateMatching.ImageMatchRotationStepAngle;
                    deviceDetails.XaiApiVersion = response.XaiApiVersion;
                    deviceDetails.EnableElasticStore= response.EnableElasticStore;
                    deviceDetails.XaiToRun = response.XaiToRun;
                    deviceDetails.XaiModel = response.XaiModel;
                    deviceDetails.XaiBatchSize = response.XaiBatchSize;
                    deviceDetails.XaiTemplateName = response.XaiTemplateName;
                    deviceDetails.HyperParameters = response.HyperParameters;
                    deviceDetails.ObjectDetectionRendering = response.ObjectDetectionRendering;
                    if (response.LotSize>1 && response.EnableLots) {
                        deviceDetails.DownLoadLot=true;
                    }
                }
                Cache.Set(DeviceDetailsCacheKey,deviceDetails,policy);
                #if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"SetDeviceDetails","Helper"),LogHandler.Layer.Business,null);
            }
            #endif
            return deviceDetails;
        }

        public static DeviceDetails AssignConfigValues(AttributeDetailsResMsg objSE) {
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"AssignConfigValues","FrameGrabber"),LogHandler.Layer.FrameGrabber,null);
            DeviceDetails retObj=new DeviceDetails();
            
            retObj.KpSkeleton=objSE.KpSkeleton;
            
            retObj.TemplateMatching=new TemplateMatching();
            foreach(var obj in objSE.Attributes) {
                if(obj!=null) {
                    switch(obj.AttributeName) {
                        case "CAMERA_URL":
                            retObj.CameraURl=obj.AttributeValue;
                            break;
                        case "STORAGE_BASE_URL":
                            retObj.StorageBaseUrl=obj.AttributeValue;
                            break;
                        case "FRAMETOPREDICT":
                            retObj.FrameToPredict=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "PREDICTION_MODEL":
                            retObj.ModelName=obj.AttributeValue;
                            break;
                        case "VIDEO_FEED_TYPE":
                            retObj.VideoFeedType=obj.AttributeValue;
                            break;
                        case "OFFLINE_VIDEO_DIRECTORY":
                            retObj.OfflineVideoDirectory=obj.AttributeValue;
                            break;
                        case "MIL_LIBRARYNAME":
                            retObj.MILLibraryName = obj.AttributeValue;
                            break;
                        case "ARCHIVE_LOCATION":
                            retObj.ArchiveDirectory=obj.AttributeValue;
                            break;
                        case "ARCHIVE_ENABLED":
                            retObj.ArchiveEnabled=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "VIEWER_IP_ADDRESS":
                            retObj.IpAddress=obj.AttributeValue;
                            break;
                        case "VIEWER_PORT":
                            retObj.Port=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "CONFIDENCE_THRESHOLD":
                            retObj.ConfidenceThreshold=Convert.ToSingle(obj.AttributeValue);
                            break;
                        case "OVERLAP_THRESHOLD":
                            retObj.OverlapThreshold=Convert.ToSingle(obj.AttributeValue);
                            break;
                        case "ENABLELOTS":
                            retObj.EnableLots=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "BOX_COLOR":
                            string boxColor=obj.AttributeValue.ToLower();
                            retObj.BoxColor=boxColor;
                            break;
                        case "PEN_THICKNESS":
                            retObj.PenThickness=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "LABEL_FONT_SIZE":
                            retObj.LabelFontSize=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "LABEL_FONT_STYLE":
                            string labelFontStyle=obj.AttributeValue.ToLower();
                            labelFontStyle=Char.ToUpper(labelFontStyle[0])+labelFontStyle.Substring(1);
                            retObj.LabelFontStyle=labelFontStyle;
                            break;
                        case "LABEL_HEIGHT":
                            retObj.LabelHeight=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "LABEL_FONT_COLOR":
                            string labelColor=obj.AttributeValue.ToLower();
                            labelColor=Char.ToUpper(labelColor[0])+labelColor.Substring(1);
                            retObj.LabelFontColor=labelColor;
                            break;
                        case "FTP_PERSECOND":
                            retObj.FTPPerSeconds=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "PREVIOUS_FRAME_COUNT":
                            retObj.PreviousFrameCount=obj.AttributeValue;
                            break;
                        case "SIMILARITY_THRESHOLD":
                            retObj.SimilarityThreshold=obj.AttributeValue;
                            break;
                        case "UNIQUEPERSONTRACKING_ENABLED":
                            retObj.UniquePersonTrackingEnabled=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "UNIQUE_PERSON_MODEL":
                            retObj.UPModelName=obj.AttributeValue;
                            break;
                        case "METRIC_TYPE":
                            retObj.MetricType=obj.AttributeValue;
                            break;
                        case "EMAIL_NOTIFICATION_DESCRIPTION":
                            retObj.EmailNoticationDescription=obj.AttributeValue;
                            break;
                        case "TASKS_ROUTE":
                            retObj.TasksRoute=obj.AttributeValue;
                            break;
                        case "TRANSPORT_REGION_CODES":
                            retObj.TransportRegionCodes=obj.AttributeValue;
                            break;
                        case "PERSONCOUNT_OVERLAP_THRESHOLD":
                            if(float.TryParse(obj.AttributeValue,out float upOverlapThreshold))
                                retObj.UniquePersonOverlapThreshold=upOverlapThreshold;
                            else
                                LogHandler.LogError($"The PERSONCOUNT_OVERLAP_THRESHOLD value is not in correct (float) format. Value: {obj.AttributeValue}",LogHandler.Layer.Business,null);
                            break;
                        case "ENFORCE_FRAME_SEQUENCING":
                            retObj.EnforceFrameSequencing=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "MAX_SEQUENCE_NUMBER":
                            retObj.MaxSequenceNumber=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "INITIAL_COLLECTION_BUFFERING_SIZE": 
                            retObj.InitialCollectionBufferingSize=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "DELETE_FRAMES_FROM_BLOB":
                            retObj.DeleteFramesFromBlob=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "FRAME_SEQUENCING_MESSAGE_STUCK_DURATION_MSEC":
                            retObj.FrameSequencingMessageStuckDuration=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "FRAME_SEQUENCING_MESSAGE_RETRY":
                            retObj.FrameSequencingMessageRetry=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "TRANSPORT_SEQUENCING_BUFFERING_SIZE":
                            retObj.TransportSequencingBufferingSize=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "STREAMING_PATH":
                            retObj.StreamingPath=obj.AttributeValue;
                            break;
                        case "STREAMING_PATH_RAW":
                            retObj.StreamingPathRaw=obj.AttributeValue;
                            break;
                        case "DISPLAY_ALL_FRAMES":
                            retObj.DisplayAllFrames=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "CLEAN_UP_STREAMING_FOLDER":
                            retObj.CleanUpStreamingFolder=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "FFMPEG_ARGUMENTS":
                            retObj.FfmpegArguments=obj.AttributeValue;
                            break;
                        case "FFMPEG_ARGUMENTS_RAW_INPUT":
                            retObj.FfmpegArgumentsRawInput=obj.AttributeValue;
                            break;
                        case "VIDEO_STREAMING_OPTION":
                            retObj.VideoStreamingOption=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "VIDEO_FORMATS_ALLOWED":
                            retObj.VideoFormatsAllowed=obj.AttributeValue;
                            break;
                        case "SHARED_BLOB_STORAGE":
                            retObj.SharedBlobStorage=obj.AttributeValue.ToLower().Equals("yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        
                        case "CVPREDICT_FIND_CONTROL_IN_MULTIPLE_CONTROL_STATES":
                            retObj.TemplateMatching.FindControlInMultipleControlStates=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "CVPREDICT_IMAGE_RECOGNITION_TIMEOUT":
                            retObj.TemplateMatching.ImageRecognitionTimeout=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "CVPREDICT_USE_TRUE_COLOR_TEMPLATE_MATCHING":
                            retObj.TemplateMatching.UseTrueColorTemplateMatching=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "CVPREDICT_IMAGE_MATCH_CONFIDENCE_THRESHOLD":
                            retObj.TemplateMatching.ImageMatchConfidenceThreshold=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "CVPREDICT_MULTIPLE_SCALE_TEMPLATE_MATCHING":
                            retObj.TemplateMatching.MultipleScaleTemplateMatching=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "CVPREDICT_IMAGE_MATCH_MAX_SCALE_STEP_COUNT":
                            retObj.TemplateMatching.ImageMatchMaxScaleStepCount=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "CVPREDICT_IMAGE_MATCH_SCALE_STEP_SIZE":
                            retObj.TemplateMatching.ImageMatchScaleStepSize=Convert.ToDouble(obj.AttributeValue);
                            break;
                        case "CVPREDICT_ENABLE_TEMPLATE_MATCHING_MAPPING":
                            retObj.TemplateMatching.EnableTemplateMatchMapping=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "CVPREDICT_WAIT_FOREVER":
                            retObj.TemplateMatching.WaitForever=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "ENABLE_ELASTICSTORE":
                            retObj.EnableElasticStore = obj.AttributeValue;
                            break;
                        case "CVPREDICT_TEMPLATE_MATCH_MAPPING_BORDER_THICKNESS":
                            retObj.TemplateMatching.TemplateMatchMappingBorderThickness=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "DEVICE_ID":
                            retObj.DeviceId=obj.AttributeValue;
                            break;
                        case "TENANT_ID":
                            retObj.TenantId=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "MSG_VERSION":
                            retObj.MsgVersion=obj.AttributeValue;
                            break;
                        case "INF_VERSION":
                            retObj.InfVersion=obj.AttributeValue;
                            break;
                        case "POSE_POINT_RENDERING":
                            retObj.PosePointRendering=obj.AttributeValue;
                            break;
                        case "SEGMENT_RENDERING":
                            retObj.SegmentRendering=obj.AttributeValue;
                            break;
                        case "CLASSIFICATION_RENDERING":
                            retObj.ClassificationRendering=obj.AttributeValue;
                            break;
                        case "PREDICT_CART":
                            retObj.PredictCart=obj.AttributeValue;
                            break;
                        case "TRACKING":
                            retObj.Tracking=obj.AttributeValue;
                            break;
                        case "CROWD_COUNTING":
                            retObj.CrowdCounting=obj.AttributeValue;
                            break;
                        case "RENDERER_FONT_SCALE":
                            retObj.RendererFontScale=Convert.ToDouble(obj.AttributeValue);
                            break;
                        case "RENDERER_FONT_THICKNESS":
                            retObj.RendererFontThickness=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "MPLUG":
                            retObj.Mplug=obj.AttributeValue;
                            break;
                        case "HEATMAP":
                            retObj.HeatMap=obj.AttributeValue;
                            break;
                        case "INDEXNAME":
                            retObj.IndexName=obj.AttributeValue;
                            break;
                        case "SPEED_DETECTION":
                            retObj.SpeedDetection=obj.AttributeValue;
                            break;
                        case "SEGMENTCOLORS":
                            retObj.SegmentColors=obj.AttributeValue;
                            break;
                        case "PYTHONVIRTUALPATH":
                            retObj.PythonVirtualPath=obj.AttributeValue;
                            break;
                        case "PANOPTICSEGMENTATION":
                            retObj.PanopticSegmentation=obj.AttributeValue;
                            break;
                        case "LABEL_COLOR":
                            retObj.LabelColor=obj.AttributeValue;
                            break;
                        case "PYTHONVERSION":
                            retObj.PythonVersion=obj.AttributeValue;
                            break;
                        case "BACKGROUND_COLOR":
                            retObj.BackgroundColor=obj.AttributeValue;
                            break;
                        case "RENDERER_RECTANGLE_POINT_X":
                            retObj.RendererRectanglePointX=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_RECTANGLE_POINT_Y":
                            retObj.RendererRectanglePointY=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_LABEL_POINT_X":
                            retObj.RendererLabelPointX=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_LABEL_POINT_Y":
                            retObj.RendererLabelPointY=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_RECTANGLE_HEIGHT":
                            retObj.RendererRectangleHeight=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_PREDICTCART_LIST_BACKGROUNDCOLOR":
                            retObj.RendererPredictCartListBackgroundColor=obj.AttributeValue;
                            break;
                        case "BACKGROUND_CHANGE":
                            retObj.BackgroundChange=obj.AttributeValue;
                            break;
                        case "FFMPEG_BACKGROUNDCHANGE":
                            retObj.FfmpegforBackgroundChange=obj.AttributeValue;
                            break;
                        case "PROMPT_INPUT_DIRECTORY":
                            retObj.PromptInputDirectory=obj.AttributeValue;
                            break;
                        case "MASK_IMAGE_DIRECTORY":
                            retObj.MaskImageDirectory=obj.AttributeValue;
                            break;
                        case "REPLACE_IMAGE_DIRECTORY":
                            retObj.ReplaceImageDirectory=obj.AttributeValue;
                            break;
                        case "OUTPUT_IMAGE":
                            retObj.OutputImage=obj.AttributeValue;
                            break;
                        case "PCD_DIRECTORY":
                            retObj.PcdDirectory=obj.AttributeValue;
                            break;
                        case "CVPREDICT_MULTI_ROTATION_TEMPLATE_MATCHING":
                            retObj.TemplateMatching.MultiRotationTemplateMatching=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "CVPREDICT_IMAGE_MATCH_ROTATION_STEP_ANGLE":
                            retObj.TemplateMatching.ImageMatchRotationStepAngle=Convert.ToDouble(obj.AttributeValue);
                            break;
                        case "XAI_API_VERSION":
                            retObj.XaiApiVersion = obj.AttributeValue;
                            break;
                        case "XAI_TO_RUN":
                            retObj.XaiToRun = obj.AttributeValue;
                            break;
                        case "XAI_MODEL":
                            retObj.XaiModel = obj.AttributeValue;
                            break;
                        case "XAI_BATCH_SIZE":
                            retObj.XaiBatchSize = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "XAI_TEMPLATE_NAME":
                            retObj.XaiTemplateName = obj.AttributeValue;
                            break;
                        case "HYPERPARAMETERS":
                            retObj.HyperParameters = obj.AttributeValue;
                            break;
                        case "OBJECTDETECTION_RENDERING":
                            retObj.ObjectDetectionRendering = obj.AttributeValue;
                            break;
                        case "RENDER_IMAGE_FILE_PATH":
                            retObj.RenderImageFilePath=obj.AttributeValue;
                            break;
                        case "RENDER_IMAGE_ENABLED":
                            retObj.RenderImageEnabled=obj.AttributeValue;
                            break;
                        case "DEBUG_IMAGE_FILE_PATH":
                            retObj.DebugImageFilePath=obj.AttributeValue;
                            break;
                        case "IMAGE_DEBUG_ENABLED":
                            retObj.ImageDebugEnabled=obj.AttributeValue;
                            break;
                        case "ENABLE_PING":
                            retObj.EnablePing=Convert.ToBoolean(obj.AttributeValue);
                            break;
                        case "CLIENT_CONNECTION_RETRY_COUNT":
                            retObj.ClientConnectionRetryCount=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "FRAME_RENDERER_WAIT_TIME_FOR_TRANSPORT_MS":
                            retObj.FrameRenderer_WaitTimeForTransportms=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "FRAME_RENDERER_EOF_COUNT":
                            retObj.FrameRenderer_EOF_Count=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "FRAME_RENDERER_EOF_FILE_PATH":
                            retObj.FrameRenderer_EOF_File_Path=obj.AttributeValue;
                            break;
                        case "FRAME_GRAB_RATE_THROTTLING_SLEEP_FRAME_COUNT":
                            retObj.FrameGrabRateThrottlingSleepFrameCount=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "FRAME_GRAB_RATE_THROTTLING_SLEEP_DURATION_MSEC":
                            retObj.FrameGrabRateThrottlingSleepDurationMsec=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "FFMPEG_EXE_FILE":
                            retObj.FfmpegExeFile=obj.AttributeValue;
                            break;
                        case "CALCULATE_FRAME_GRABBER_FPR":
                            retObj.CalculateFrameGrabberFPR=obj.AttributeValue;
                            break;
                        case "MAX_EMPTY_FRAME_COUNT":
                            retObj.MaxEmptyFrameCount=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "EMPTY_FRAME_PROCESS_INTERVAL":
                            retObj.EmptyFrameProcessInterval=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "FTP_CYCLE":
                            retObj.FTPCycle=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "ELASTIC_STORE_INDEX_NAME":
                            retObj.ElasticStoreIndexName=obj.AttributeValue;
                            break;
                        case "PROMPT_TEMPLATES_DIRECTORY":
                            retObj.PromptTemplatesDirectory=obj.AttributeValue;
                            break;
                        case "REDUCE_FRAME_QUALITY_TO":
                            retObj.ReduceFrameQualityTo=Convert.ToDouble(obj.AttributeValue);
                            break;
                        case "MIN_THREAD_ON_POOL":
                            retObj.MinThreadOnPool=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "MAX_THREAD_ON_POOL":
                            retObj.MaxThreadOnPool=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "MAX_FAIL_COUNT":
                            retObj.MaxFailCount=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "IMAGE_FORMATS_TO_USE":
                            retObj.ImageFormatsToUse=obj.AttributeValue;
                            break;
                        case "OFFLINE_PROCESS_INTERVAL":
                            retObj.OfflineProcessInterval=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "DATA_STREAM_TIME_OUT":
                            retObj.DataStreamTimeOut=obj.AttributeValue;
                            break;
                        case "CLIENT_CONNECTION_WAITING_TIME":
                            retObj.ClientConnectionWaitingTime=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "PROCESS_LOADER_TRACE_FILE":
                            retObj.ProcessLoaderTraceFile=obj.AttributeValue;
                            break;
                        case "PREDICTION_TYPE":
                            retObj.PredictionType=obj.AttributeValue;
                            break;
                        case "ANALYTICS_PREDICTION_TYPE":
                            retObj.AnalyticsPredictionType=obj.AttributeValue;
                            break;
                        case "DB_ENABLED":
                            retObj.DBEnabled=Convert.ToBoolean(obj.AttributeValue);
                            break;
                        
                        default:
                            break;
                    }
                }
                else {
                    
                    LogHandler.LogError("Configuration value of {0} is missing for Tenant Id: {1}, Device Id: {2}",LogHandler.Layer.FrameGrabber,obj.AttributeName,retObj.TenantId,retObj.DeviceId);
                    FaceMaskDetectionInvalidConfigException exception=new FaceMaskDetectionInvalidConfigException(String.Format("Configuration value of {0} is missing for Tenant Id: {1}, Device Id: {2}",obj.AttributeName,retObj.TenantId,retObj.DeviceId));
                    throw exception;
                }
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"AssignConfigValues","FrameGrabber"),LogHandler.Layer.FrameGrabber,null);
            return retObj;
        }
    }
}
