/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
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
                    /* try {
                        var rs=JsonConvert.DeserializeObject<AttributeDetailsResMsg>(responseString);
                    }
                    catch(Exception e) {
                        throw e;
                    } */
                    var res=JsonConvert.DeserializeObject<AttributeDetailsResMsg>(responseString);
                    var response=AssignConfigValues(JsonConvert.DeserializeObject<AttributeDetailsResMsg>(responseString));
                    /* var response=AssignConfigValues(channel.GetDeviceAttributes(int.Parse(tId),deviceId)); */                     
                    if(response==null)
                        throw new FaceMaskDetectionCriticalException("Failed to get device configuration from services. Response is null.");
                    deviceDetails=new DeviceDetails();
                    deviceDetails.AllIpAddress=response.AllIpAddress;
                    deviceDetails.ArchiveDirectory=response.ArchiveDirectory;
                    deviceDetails.ArchiveEnabled=response.ArchiveEnabled;
                    deviceDetails.BaseUrl=response.BaseUrl;
                    deviceDetails.BoxColor=response.BoxColor;
                    deviceDetails.CameraURl=response.CameraURl;
                    deviceDetails.ComplianceLowerThreshold=response.ComplianceLowerThreshold;
                    deviceDetails.ComplianceUpperThreshold=response.ComplianceUpperThreshold;
                    deviceDetails.ConfidenceThreshold=response.ConfidenceThreshold;
                    deviceDetails.DefaultPenColor=response.DefaultPenColor;
                    deviceDetails.DeviceId=response.DeviceId;
                    deviceDetails.DisplayAllFrames=response.DisplayAllFrames;
                    deviceDetails.DownLoadLot=response.DownLoadLot;
                    deviceDetails.EmailNoticationDescription=response.EmailNoticationDescription;
                    deviceDetails.EnableLots=response.EnableLots;
                    deviceDetails.FfmpegArguments=response.FfmpegArguments;
                    deviceDetails.FrameToPredict=response.FrameToPredict;
                    deviceDetails.FTPPerSeconds=response.FTPPerSeconds;
                    deviceDetails.IpAddress=response.IpAddress;
                    deviceDetails.IsClientActive=response.IsClientActive;
                    deviceDetails.LabelFontColor=response.LabelFontColor;
                    deviceDetails.LabelFontSize=response.LabelFontSize;
                    deviceDetails.LabelFontStyle=response.LabelFontStyle;
                    deviceDetails.LabelHeight=response.LabelHeight;
                    deviceDetails.LotSize=response.LotSize;
                    deviceDetails.MaskLabel=response.MaskLabel;
                    deviceDetails.MaskPenColor=response.MaskPenColor;
                    deviceDetails.MetricType=response.MetricType;
                    deviceDetails.MlModelUrl=response.MlModelUrl;
                    deviceDetails.ModelName=response.ModelName;
                    deviceDetails.NoMaskLabel=response.NoMaskLabel;
                    deviceDetails.NoMaskLowerThreshold=response.NoMaskLowerThreshold;
                    deviceDetails.NoMaskPenColor=response.NoMaskPenColor;
                    deviceDetails.NoMaskUpperThreshold=response.NoMaskUpperThreshold;
                    deviceDetails.OfflineVideoDirectory=response.OfflineVideoDirectory;
                    deviceDetails.OverlapThreshold=response.OverlapThreshold;
                    deviceDetails.PenThickness=response.PenThickness;
                    deviceDetails.Port=response.Port;
                    deviceDetails.PredictionClassType=response.PredictionClassType;
                    deviceDetails.PredictionModel=response.PredictionModel;
                    deviceDetails.PreviousFrameCount=response.PreviousFrameCount;
                    deviceDetails.QueueName=response.QueueName;
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
                    /* Fixing name mismatch attributes */
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
                    deviceDetails.BackGroundColor=response.BackGroundColor;
                    deviceDetails.RendererRectanglePointX=response.RendererRectanglePointX;
                    deviceDetails.RendererRectanglePointY=response.RendererRectanglePointY;
                    deviceDetails.RendererLabelPointX=response.RendererLabelPointX;
                    deviceDetails.RendererLabelPointY=response.RendererLabelPointY;
                    deviceDetails.RendererRectangleHeight=response.RendererRectangleHeight;    
                    deviceDetails.RendererPredictCartListBackgroundColor=response.RendererPredictCartListBackgroundColor;
                    deviceDetails.BackgroundChange=response.BackgroundChange;
                    deviceDetails.FfmpegforBackgroundChange=response.FfmpegforBackgroundChange;
                    deviceDetails.EnablePrompt=response.EnablePrompt;
                    deviceDetails.PromptInputDirectory=response.PromptInputDirectory;
                    deviceDetails.MaskImageInput=response.MaskImageInput;
                    deviceDetails.MaskImageDirectory=response.MaskImageDirectory;
                    deviceDetails.ReplaceImageInput=response.ReplaceImageInput;
                    deviceDetails.ReplaceImageDirectory=response.ReplaceImageDirectory;
                    deviceDetails.BlobforGenerativeAI=response.BlobforGenerativeAI;
                    deviceDetails.GENAI=response.GENAI;
                    deviceDetails.OutputImage=response.OutputImage;
                    deviceDetails.PcdDirectory=response.PcdDirectory;
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
                    if(response.LotSize>1 && response.EnableLots) {
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
                        case "ARCHIVE_LOCATION":
                            retObj.ArchiveDirectory=obj.AttributeValue;
                            break;
                        case "ARCHIVE_ENABLED":
                            retObj.ArchiveEnabled=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "RENDERER_Q":
                            retObj.QueueName=obj.AttributeValue;
                            break;
                        case "MASK_DETECTOR_VIEWER_IP_ADDRESS":
                            retObj.IpAddress=obj.AttributeValue;
                            break;
                        case "MASK_DETECTOR_VIEWER_PORT":
                            retObj.Port=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "ALL_IP_ADDRESS":
                            retObj.AllIpAddress=obj.AttributeValue;
                            break;
                        case "COMPLIANCE_UPPER_THRESHOLD":
                            retObj.ComplianceUpperThreshold=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "COMPLIANCE_LOWER_THRESHOLD":
                            retObj.ComplianceLowerThreshold=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "NOMASK_UPPER_THRESHOLD":
                            retObj.NoMaskUpperThreshold=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "NOMASK_LOWER_THRESHOLD":
                            retObj.NoMaskLowerThreshold=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "MASK_LABEL":
                            retObj.MaskLabel=obj.AttributeValue;
                            break;
                        case "NOMASK_LABEL":
                            retObj.NoMaskLabel=obj.AttributeValue;
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
                        case "NOMASK_PEN_COLOR":
                            string noMaskcolor=obj.AttributeValue.ToLower();
                            noMaskcolor=Char.ToUpper(noMaskcolor[0])+noMaskcolor.Substring(1);
                            retObj.NoMaskPenColor=noMaskcolor;
                            break;
                        case "MASK_PEN_COLOR":
                            string maskColor=obj.AttributeValue.ToLower();
                            maskColor=Char.ToUpper(maskColor[0])+maskColor.Substring(1);
                            retObj.MaskPenColor=maskColor;
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
                        case "PREDICTION_CLASS_TYPE":
                            retObj.PredictionClassType=obj.AttributeValue;
                            break;
                        case "ML_MODEL_URL":
                            retObj.MlModelUrl=obj.AttributeValue;
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
                        case "INITIAL_COLLECTION_BUFFERING_SIZE": /* INITIAL_COLLECTION_BUFFERING_SIZE, TRANSPORT_SEQUENCING_BUFFERING_SIZE */
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
                        case "DISPLAY_ALL_FRAMES":
                            retObj.DisplayAllFrames=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "CLEAN_UP_STREAMING_FOLDER":
                            retObj.CleanUpStreamingFolder=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
                            break;
                        case "FFMPEG_ARGUMENTS":
                            retObj.FfmpegArguments=obj.AttributeValue;
                            break;
                        case "IS_CLIENT_ACTIVE":
                            retObj.IsClientActive=obj.AttributeValue.Equals("Yes",StringComparison.InvariantCultureIgnoreCase)?true:false;
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
                        /* case "KPSkeleton":
                            retObj.KPSkeleton=obj.AttributeValue;
                            break; */
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
                            retObj.BackGroundColor=obj.AttributeValue;
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
                        case "ENABLE_PROMPT":
                            retObj.EnablePrompt=obj.AttributeValue;
                            break;
                        case "PROMPT_INPUT_DIRECTORY":
                            retObj.PromptInputDirectory=obj.AttributeValue;
                            break;
                        case "MASK_IMAGE_INPUT":
                            retObj.MaskImageInput=obj.AttributeValue;
                            break;
                        case "MASK_IMAGE_DIRECTORY":
                            retObj.MaskImageDirectory=obj.AttributeValue;
                            break;
                        case "REPLACE_IMAGE_INPUT":
                            retObj.ReplaceImageInput=obj.AttributeValue;
                            break;
                        case "REPLACE_IMAGE_DIRECTORY":
                            retObj.ReplaceImageDirectory=obj.AttributeValue;
                            break;
                        case "BLOB_GENAI":
                            retObj.BlobforGenerativeAI=obj.AttributeValue;
                            break;
						case "GENAI":
							retObj.GENAI=obj.AttributeValue;
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
