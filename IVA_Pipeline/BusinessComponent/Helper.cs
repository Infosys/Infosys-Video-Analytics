/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿/*
 *© 2019 Infosys Limited, Bangalore, India. All Rights Reserved. Infosys believes the information in this document is accurate as of its publication date; such information is subject to change without notice. Infosys acknowledges the proprietary rights of other companies to the trademarks, product names and such other intellectual property rights mentioned in this document. Except as expressly permitted, neither this document nor any part of it may be reproduced, stored in a retrieval system, or transmitted in any form or by any means, electronic, mechanical, printing, photocopying, recording or otherwise, without the prior permission of Infosys Limited and/or any named intellectual property rights holders under this document.   
 * 
 * © 2019 INFOSYS LIMITED. CONFIDENTIAL AND PROPRIETARY 
 */

using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess.Document;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using MediaToolkit;
using MediaToolkit.Model;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Channels;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using SC = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ServiceClientLibrary;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using ST = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Document;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    public class Helper
    {
        public static ST.Workflow DownloadBlob(string DeviceId, string FrameId, string TenantId, string StorageBaseURL, string fileExtension)
        {
            WorkflowDS wf = new WorkflowDS();
#if DEBUG
            using (LogHandler.TraceOperations("Helper:DownloadBlobImage", LogHandler.Layer.Business, Guid.NewGuid(), null))
            {

                LogHandler.LogUsage(String.Format("The DownloadBlob method of Helper class is getting executed with parameters :  DeviceId={0}; FrameId={1}; TenantId={2}; at {3}", DeviceId, FrameId, TenantId, DateTime.UtcNow.ToLongTimeString()), null);
#endif

                DE.Document.Workflow blob = wf.Download(new DE.Document.Workflow()
                {
                    DeviceId = DeviceId,
                    FrameId = FrameId + fileExtension,
                    TenantId = Convert.ToInt32(TenantId),
                    StorageBaseURL = StorageBaseURL
                });
                if (blob.StatusCode == 0 && blob.File?.Length > 0)
                {
                    blob.File.Position = 0;
                }
                else
                {
                    return null;
                }





#if DEBUG
                LogHandler.LogUsage(String.Format("The DownloadBlob method of Helper class finished execution with parameters :  DeviceId={0}; FrameId={1}; TenantId={2}; at {3}", DeviceId, FrameId, TenantId, DateTime.UtcNow.ToLongTimeString()), null);
#endif
                return blob;
#if DEBUG
            }
#endif
        }

        public static (MediaMetaDataMsg, string) ExtractVideoMetaData(string videoSource, int tenantId)
        {
            MediaMetaDataMsg mediaMetaDataMsgReq = new MediaMetaDataMsg();
            MediaMetadataDetail mediaMetadataDetails = new MediaMetadataDetail();
            mediaMetadataDetails.CreatedBy = "";
            mediaMetadataDetails.CreatedDate = DateTime.UtcNow;
            
            mediaMetadataDetails.TenantId = tenantId;
            var inputFile = new MediaFile { Filename = videoSource };
            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);
            }
            SE.Data.Media_Metadata mediaMetadata = new SE.Data.Media_Metadata();
            var fileMetaData = inputFile.Metadata.VideoData;
            if (fileMetaData.BitRateKbs != null)
            {
                mediaMetadata.BitRateKbs = (int)fileMetaData.BitRateKbs;
            }
            mediaMetadata.ColorModel = fileMetaData.ColorModel;
            mediaMetadata.FileExtension = Path.GetExtension(videoSource); ;
            mediaMetadata.FileSize = new System.IO.FileInfo(videoSource).Length;
            mediaMetadata.VideoDuration = inputFile.Metadata.Duration.TotalSeconds;
            mediaMetadata.Format = fileMetaData.Format;
            mediaMetadata.Fps = fileMetaData.Fps;
            mediaMetadata.FrameSize = fileMetaData.FrameSize;
            mediaMetadataDetails.MetaData = JsonConvert.SerializeObject(mediaMetadata);
            mediaMetaDataMsgReq.MediaMetadataDetail = mediaMetadataDetails;
            return (mediaMetaDataMsgReq, mediaMetadata.Format);
        }


        


        public static DeviceDetails AssignConfigValues(AttributeDetailsResMsg objSE)
        {
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "AssignConfigValues", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);

            DeviceDetails retObj = new DeviceDetails();
       

            foreach (var obj in objSE.Attributes)
            {
                if (obj != null)
                {
                    switch (obj.AttributeName)
                    {
                        case "CAMERA_URL":
                            retObj.CameraURl = obj.AttributeValue;
                            break;
                        case "STORAGE_BASE_URL":
                            retObj.StorageBaseUrl = obj.AttributeValue;
                            break;
                        case "FRAMETOPREDICT":
                            retObj.FrameToPredict = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "PREDICTION_MODEL":
                            retObj.ModelName = obj.AttributeValue;
                            break;
                        case "VIDEO_FEED_TYPE":
                            retObj.VideoFeedType = obj.AttributeValue;
                            break;
                        case "OFFLINE_VIDEO_DIRECTORY":
                            retObj.OfflineVideoDirectory = obj.AttributeValue;
                            break;
                        case "MIL_LIBRARYNAME":
                            retObj.MILLibraryName = obj.AttributeValue;
                            break;
                        case "ARCHIVE_LOCATION":
                            retObj.ArchiveDirectory = obj.AttributeValue;
                            break;
                        case "ARCHIVE_ENABLED":
                            retObj.ArchiveEnabled = obj.AttributeValue.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "VIEWER_IP_ADDRESS":
                            retObj.IpAddress = obj.AttributeValue;
                            break;
                        case "VIEWER_PORT":
                            retObj.Port = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "CONFIDENCE_THRESHOLD":
                            retObj.ConfidenceThreshold = Convert.ToSingle(obj.AttributeValue);
                            break;
                        case "OVERLAP_THRESHOLD":
                            retObj.OverlapThreshold = Convert.ToSingle(obj.AttributeValue);
                            break;
                        case "ENABLELOTS":
                            retObj.EnableLots = obj.AttributeValue.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "BOX_COLOR":
                            string boxColor = obj.AttributeValue.ToLower();
                            retObj.BoxColor = boxColor;
                            break;
                        case "PEN_THICKNESS":
                            retObj.PenThickness = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "LABEL_FONT_SIZE":
                            retObj.LabelFontSize = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "LABEL_FONT_STYLE":
                            string labelFontStyle = obj.AttributeValue.ToLower();
                            labelFontStyle = Char.ToUpper(labelFontStyle[0]) + labelFontStyle.Substring(1);
                            retObj.LabelFontStyle = labelFontStyle;
                            break;
                        case "LABEL_HEIGHT":
                            retObj.LabelHeight = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "LABEL_FONT_COLOR":
                            string labelColor = obj.AttributeValue.ToLower();
                            labelColor = Char.ToUpper(labelColor[0]) + labelColor.Substring(1);
                            retObj.LabelFontColor = labelColor;
                            break;
                        case "FTP_PERSECOND":
                            retObj.FTPPerSeconds = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "PREVIOUS_FRAME_COUNT":
                            retObj.PreviousFrameCount = obj.AttributeValue;
                            break;
                        case "SIMILARITY_THRESHOLD":
                            retObj.SimilarityThreshold = obj.AttributeValue;
                            break;
                        case "UNIQUEPERSONTRACKING_ENABLED":
                            retObj.UniquePersonTrackingEnabled = obj.AttributeValue.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "UNIQUE_PERSON_MODEL":
                            retObj.UPModelName = obj.AttributeValue;
                            break;
                        case "METRIC_TYPE":
                            retObj.MetricType = obj.AttributeValue;
                            break;
                        case "EMAIL_NOTIFICATION_DESCRIPTION":
                            retObj.EmailNoticationDescription = obj.AttributeValue;
                            break;
                        case "TASKS_ROUTE":
                            retObj.TasksRoute = obj.AttributeValue;
                            break;
                        case "TRANSPORT_REGION_CODES":
                            retObj.TransportRegionCodes = obj.AttributeValue;
                            break;
                        
                        case "PERSONCOUNT_OVERLAP_THRESHOLD":
                            if (float.TryParse(obj.AttributeValue, out float upOverlapThreshold))
                                retObj.UniquePersonOverlapThreshold = upOverlapThreshold;
                            else
                                LogHandler.LogError($"The PERSONCOUNT_OVERLAP_THRESHOLD value is not in correct (float) format. value {obj.AttributeValue}", LogHandler.Layer.Business, null);

                            break;
                        case "VIDEO_STREAMING_OPTION":
                            retObj.VideoStreamingOption = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "STREAMING_PATH":
                            retObj.StreamingPath = obj.AttributeValue;
                            break;
                        case "STREAMING_PATH_RAW":
                            retObj.StreamingPathRaw=obj.AttributeValue;
                            break;
                        case "FFMPEG_ARGUMENTS":
                            retObj.FfmpegArguments = obj.AttributeValue;
                            break;
                        case "FFMPEG_ARGUMENTS_RAW_INPUT":
                            retObj.FfmpegArgumentsRawInput=obj.AttributeValue;
                            break;
                        case "DISPLAY_ALL_FRAMES":
                            retObj.DisplayAllFrames = obj.AttributeValue.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;

                        case "ENFORCE_FRAME_SEQUENCING":
                            retObj.EnforceFrameSequencing = obj.AttributeValue.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "MAX_SEQUENCE_NUMBER":
                            retObj.MaxSequenceNumber = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "INITIAL_COLLECTION_BUFFERING_SIZE": // INITIAL_COLLECTION_BUFFERING_SIZE , TRANSPORT_SEQUENCING_BUFFERING_SIZE
                            retObj.InitialCollectionBufferingSize = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "DELETE_FRAMES_FROM_BLOB":
                            retObj.DeleteFramesFromBlob = obj.AttributeValue.Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "FRAME_SEQUENCING_MESSAGE_STUCK_DURATION_MSEC":
                            retObj.FrameSequencingMessageStuckDuration = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "FRAME_SEQUENCING_MESSAGE_RETRY":
                            retObj.FrameSequencingMessageRetry = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "TRANSPORT_SEQUENCING_BUFFERING_SIZE":
                            retObj.TransportSequencingBufferingSize = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "DEVICE_ID":
                            retObj.DeviceId = obj.AttributeValue;
                            break;
                        case "TENANT_ID":
                            retObj.TenantId = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "MSG_VERSION":
                            retObj.MsgVersion = obj.AttributeValue;
                            break;
                        case "INF_VERSION":
                            retObj.InfVersion = obj.AttributeValue;
                            break;
                        case "POSE_POINT_RENDERING":
                            retObj.PosePointRendering = obj.AttributeValue;
                            break;
                        case "SEGMENT_RENDERING":
                            retObj.SegmentRendering = obj.AttributeValue;
                            break;
                        case "PREDICT_CART":
                            retObj.PredictCart = obj.AttributeValue;
                            break;
                        case "TRACKING":
                            retObj.Tracking =   obj.AttributeValue;
                            break;
                        case "CROWD_COUNTING":
                            retObj.CrowdCounting = obj.AttributeValue;
                            break;
                        case "RENDERER_FONT_SCALE":
                            retObj.RendererFontScale =  Convert.ToDouble( obj.AttributeValue);
                            break;
                        case "RENDERER_FONT_THICKNESS":
                            retObj.RendererFontThickness = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "MPLUG":
                            retObj.Mplug = obj.AttributeValue;
                            break;
                        case "HEATMAP":
                            retObj.HeatMap = obj.AttributeValue;
                            break;
                        case "INDEXNAME":
                            retObj.IndexName = obj.AttributeValue;
                            break;
                        case "SPEED_DETECTION":
                            retObj.SpeedDetection = obj.AttributeValue;
                            break;
                        case "SEGMENTCOLORS":
                            retObj.SegmentColors = obj.AttributeValue;
                            break;
                        case "PYTHONVIRTUALPATH":
                            retObj.PythonVirtualPath = obj.AttributeValue;
                            break;
                        case "PANOPTICSEGMENTATION":
                            retObj.PanopticSegmentation = obj.AttributeValue;
                            break;
                        case "LABEL_COLOR":
                            retObj.LabelColor = obj.AttributeValue;
                            break;
                        case "PYTHONVERSION":
                            retObj.PythonVersion = obj.AttributeValue;
                            break;
                        case "BACKGROUND_COLOR":
                            retObj.BackgroundColor = obj.AttributeValue;
                            break;
                        case "RENDERER_RECTANGLE_POINT_X":
                            retObj.RendererRectanglePointX = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_RECTANGLE_POINT_Y":
                            retObj.RendererRectanglePointY = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_LABEL_POINT_X":
                            retObj.RendererLabelPointX = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_LABEL_POINT_Y":
                            retObj.RendererLabelPointY = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_RECTANGLE_HEIGHT":
                            retObj.RendererRectangleHeight = Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "RENDERER_PREDICTCART_LIST_BACKGROUNDCOLOR":
                            retObj.RendererPredictCartListBackgroundColor = obj.AttributeValue;
                            break;
                        case "BACKGROUND_CHANGE":
                            retObj.BackgroundChange = obj.AttributeValue;
                            break;
                        case "FFMPEG_BACKGROUNDCHANGE":
                            retObj.FfmpegforBackgroundChange = obj.AttributeValue;
                            break;
                        case "PROMPT_INPUT_DIRECTORY":
                            retObj.PromptInputDirectory = obj.AttributeValue;
                            break;
                        case "MASK_IMAGE_DIRECTORY":
                            retObj.MaskImageDirectory = obj.AttributeValue;
                            break;
                        case "REPLACE_IMAGE_DIRECTORY":
                            retObj.ReplaceImageDirectory = obj.AttributeValue;
                            break;
                        case "ENABLE_ELASTICSTORE":
                            retObj.EnableElasticStore = obj.AttributeValue;
                            break;
                        case "OUTPUT_IMAGE":
                            retObj.OutputImage = obj.AttributeValue;
                            break;
                        case "PCD_DIRECTORY":
                            retObj.PcdDirectory = obj.AttributeValue;
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
                        case "MAX_EMPTY_FRAME_COUNT":
                            retObj.MaxEmptyFrameCount=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "CALCULATE_FRAME_GRABBER_FPR":
                            retObj.CalculateFrameGrabberFPR=obj.AttributeValue;
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
                        case "VIDEO_FORMATS_ALLOWED":
                            retObj.VideoFormatsAllowed=obj.AttributeValue;
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
                        case "RENDERER_BACKGROUND_TRANSPARENCY":
                            retObj.RendererBackgroundTransparency=Convert.ToDouble(obj.AttributeValue);
                            break;
                        case "ROI_COORDINATES":
                            retObj.RoiCoordinates=obj.AttributeValue;
                            break;
                        case "ROI_TYPE":
                            retObj.RoiType=Convert.ToInt32(obj.AttributeValue);
                            break;
                        case "INPUT_DEBUG_IMAGE_FILE_PATH":
                            retObj.InputDebugImageFilePath=obj.AttributeValue;
                            break;
                        case "ROI_DEBUG_IMAGE_FILE_PATH":
                            retObj.RoiDebugImageFilePath=obj.AttributeValue;
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
                        case "HYPERPARAMETERS":
                            retObj.HyperParameters = obj.AttributeValue;
                            break;
                        case "OBJECTDETECTION_RENDERING":
                            retObj.ObjectDetectionRendering = obj.AttributeValue;
                            break;
                        case "DBPROVIDER":
                            retObj.DBProvider = obj.AttributeValue;
                            break;
                        case "DATACOLLECTOR_FILENAME":
                            retObj.DataCollectorFileName = obj.AttributeValue;
                            break;
                        case "SPLIT_SCREEN_RENDERING":
                            retObj.SplitScreenRendering = obj.AttributeValue;
                            break;
                        case "SPLIT_SCREEN_GRID":
                            retObj.SplitScreenGrid = obj.AttributeValue;
                            break;
                        case "DISPLAY_PREDICTION_INFO":
                            retObj.DisplayPredictionInfo = obj.AttributeValue == "yes" ? true : false;
                            break;
                        case "ENVIRONMENT_ADAPTER_RETRY_LIMIT":
                            retObj.EnvironmentAdapterRetryLimit = Convert.ToInt32(obj.AttributeValue);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    
                    LogHandler.LogError("Configuration Value of {0} is Missing for Tenant ID: {1}, Device ID :{2}", LogHandler.Layer.FrameGrabber, obj.AttributeName, retObj.TenantId, retObj.DeviceId);

                    FaceMaskDetectionInvalidConfigException exception = new FaceMaskDetectionInvalidConfigException(String.Format("Configuration Value of {0} is Missing for Tenant ID: {1}, Device ID :{2}", obj.AttributeName, retObj.TenantId, retObj.DeviceId));
                    throw exception;
                }
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "AssignConfigValues", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            return retObj;
        }

        public static void UpdateFeedRequestStatus(int feedId,string status,DeviceDetails deviceDetails) {
            if(!deviceDetails.DBEnabled) {
                return;
            }
            SC.MaskDetector maskDetector = new SC.MaskDetector();
          
            SE.Message.FeedRequestReqMsg feedRequestReqMsg = new SE.Message.FeedRequestReqMsg();
            feedRequestReqMsg.FeedProcessorMasterId = feedId;
            feedRequestReqMsg.Status = status;
            
            FeedRequestReqMsg data = new FeedRequestReqMsg {
                FeedProcessorMasterId = feedId,
                Status = status
            };

            var uri = String.Format($"{Config.AppSettings.ConfigWebApi}FrameDetails/UpdateFeedRequestDetails");
            var apiResponse = ServiceCaller.ServiceCall(data, uri, "PUT");
            var response = JsonConvert.DeserializeObject<SE.Message.UpdateResourceAttributeResMsg>(apiResponse);
        }


        public static void UpdateCompletedFeedRequestDetails(int masterId,string status,DeviceDetails deviceDetails) {
#if DEBUG
            

            using (LogHandler.TraceOperations("UpdateFeedRequestDetails:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                if(!deviceDetails.DBEnabled) {
                    return;
                }
                try
                {
                    FeedRequestDS feedRequestDS = new FeedRequestDS();
                    FeedRequest feedRequest = feedRequestDS.GetOneWithMasterId(masterId);
                    feedRequest.Status = status;
                    feedRequestDS.Update(feedRequest);
                }
                catch (Exception ex)
                {
                    
                }
#if DEBUG
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "UpdateFeedDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The UpdateFeedDetails Method finished Executing for Master ID :{0} at {1}.", LogHandler.Layer.FrameGrabber, masterId, DateTime.UtcNow.ToLongTimeString());
#endif
            return;
        }


        public static bool getClientStatus(string deviceId,string tenantId,DeviceDetails deviceDetails) {
#if DEBUG
            using (LogHandler.TraceOperations("FrameRendererProcess:getClientStatus", LogHandler.Layer.Business, Guid.NewGuid(), null))
            {
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "getClientStatus", "FrameRendererProcess"), LogHandler.Layer.Business, null);
#endif
                
                bool clientStatus = true;
                if(deviceDetails.DBEnabled) {
                    var uri = String.Format($"{Config.AppSettings.ConfigWebApi}Configuration/GetClientStatus?tid={tenantId}&did={deviceId}");
                    var apiResponse = ServiceCaller.ServiceCall(null, uri, "GET");
                    clientStatus = Convert.ToBoolean(apiResponse);
                }
#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "getClientStatus", "FrameRendererProcess"), LogHandler.Layer.Business, null);
#endif
                return clientStatus;
#if DEBUG
            }
#endif
        }

        public static void DeleteBlob(string DeviceId, string FrameId, string TenantId, string StorageBaseURL, string fileExtension)
        {
            WorkflowDS wf = new WorkflowDS();
#if DEBUG
            using (LogHandler.TraceOperations("Helper:DeleteBlob", LogHandler.Layer.Business, Guid.NewGuid(), null))
            {

                LogHandler.LogUsage(String.Format("The DeleteBlob method of Helper class is getting executed with parameters :  DeviceId={0}; FrameId={1}; TenantId={2}; at {3}", DeviceId, FrameId, TenantId, DateTime.UtcNow.ToLongTimeString()), null);
#endif

                wf.Delete(new DE.Document.Workflow()
                {
                    DeviceId = DeviceId,
                    FrameId = FrameId + fileExtension,
                    TenantId = Convert.ToInt32(TenantId),
                    StorageBaseURL = StorageBaseURL
                });



#if DEBUG
                LogHandler.LogUsage(String.Format("The DeleteBlob method of Helper class finished execution with parameters :  DeviceId={0}; FrameId={1}; TenantId={2}; at {3}", DeviceId, FrameId, TenantId, DateTime.UtcNow.ToLongTimeString()), null);
#endif

#if DEBUG
            }
#endif
        }

        public static DeviceDetails UpdateConfigValues(Dictionary<string, string?> environmentValues, DeviceDetails deviceDetails)
        {
            //LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UpdateConfigValues"), LogHandler.Layer.FrameGrabber, null);
            try
            {
                foreach (string key in environmentValues.Keys)
                {
                    switch (key)
                    {
                        case "CAMERA_URL":
                            deviceDetails.CameraURl = environmentValues[key];
                            break;
                        case "STORAGE_BASE_URL":
                            deviceDetails.BaseUrl=deviceDetails.StorageBaseUrl=environmentValues[key];
                            break;
                        case "FRAMETOPREDICT":
                            deviceDetails.FrameToPredict = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "PREDICTION_MODEL":
                            deviceDetails.ModelName = environmentValues[key];
                            break;
                        case "VIDEO_FEED_TYPE":
                            deviceDetails.VideoFeedType = environmentValues[key];
                            break;
                        case "OFFLINE_VIDEO_DIRECTORY":
                            deviceDetails.OfflineVideoDirectory = environmentValues[key];
                            break;
                        case "MIL_LIBRARYNAME":
                            deviceDetails.MILLibraryName = environmentValues[key];
                            break;
                        case "ARCHIVE_LOCATION":
                            deviceDetails.ArchiveDirectory = environmentValues[key];
                            break;
                        case "ARCHIVE_ENABLED":
                            deviceDetails.ArchiveEnabled = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "VIEWER_IP_ADDRESS":
                            deviceDetails.IpAddress = environmentValues[key];
                            break;
                        case "VIEWER_PORT":
                            deviceDetails.Port = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "CONFIDENCE_THRESHOLD":
                            deviceDetails.ConfidenceThreshold = Convert.ToSingle(environmentValues[key]);
                            break;
                        case "OVERLAP_THRESHOLD":
                            deviceDetails.OverlapThreshold = Convert.ToSingle(environmentValues[key]);
                            break;
                        case "ENABLELOTS":
                            deviceDetails.EnableLots = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "BOX_COLOR":
                            string boxColor = environmentValues[key].ToLower();
                            deviceDetails.BoxColor = boxColor;
                            break;
                        case "PEN_THICKNESS":
                            deviceDetails.PenThickness = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "LABEL_FONT_SIZE":
                            deviceDetails.LabelFontSize = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "LABEL_FONT_STYLE":
                            string labelFontStyle = environmentValues[key].ToLower();
                            labelFontStyle = Char.ToUpper(labelFontStyle[0]) + labelFontStyle.Substring(1);
                            deviceDetails.LabelFontStyle = labelFontStyle;
                            break;
                        case "LABEL_HEIGHT":
                            deviceDetails.LabelHeight = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "LABEL_FONT_COLOR":
                            string labelColor = environmentValues[key].ToLower();
                            labelColor = Char.ToUpper(labelColor[0]) + labelColor.Substring(1);
                            deviceDetails.LabelFontColor = labelColor;
                            break;
                        case "FTP_PERSECOND":
                            deviceDetails.FTPPerSeconds = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "PREVIOUS_FRAME_COUNT":
                            deviceDetails.PreviousFrameCount = environmentValues[key];
                            break;
                        case "SIMILARITY_THRESHOLD":
                            deviceDetails.SimilarityThreshold = environmentValues[key];
                            break;
                        case "UNIQUEPERSONTRACKING_ENABLED":
                            deviceDetails.UniquePersonTrackingEnabled = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "UNIQUE_PERSON_MODEL":
                            deviceDetails.UPModelName = environmentValues[key];
                            break;
                        case "METRIC_TYPE":
                            deviceDetails.MetricType = environmentValues[key];
                            break;
                        case "EMAIL_NOTIFICATION_DESCRIPTION":
                            deviceDetails.EmailNoticationDescription = environmentValues[key];
                            break;
                        case "TASKS_ROUTE":
                            deviceDetails.TasksRoute = environmentValues[key];
                            break;
                        case "TRANSPORT_REGION_CODES":
                            deviceDetails.TransportRegionCodes = environmentValues[key];
                            break;
                        case "PERSONCOUNT_OVERLAP_THRESHOLD":
                            if (float.TryParse(environmentValues[key], out float upOverlapThreshold))
                                deviceDetails.UniquePersonOverlapThreshold = upOverlapThreshold;
                            else
                                LogHandler.LogError($"The PERSONCOUNT_OVERLAP_THRESHOLD value is not in correct (float) format. Value: {environmentValues[key]}", LogHandler.Layer.Business, null);
                            break;
                        case "ENFORCE_FRAME_SEQUENCING":
                            deviceDetails.EnforceFrameSequencing = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "MAX_SEQUENCE_NUMBER":
                            deviceDetails.MaxSequenceNumber = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "INITIAL_COLLECTION_BUFFERING_SIZE":
                            deviceDetails.InitialCollectionBufferingSize = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "DELETE_FRAMES_FROM_BLOB":
                            deviceDetails.DeleteFramesFromBlob = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "FRAME_SEQUENCING_MESSAGE_STUCK_DURATION_MSEC":
                            deviceDetails.FrameSequencingMessageStuckDuration = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "FRAME_SEQUENCING_MESSAGE_RETRY":
                            deviceDetails.FrameSequencingMessageRetry = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "TRANSPORT_SEQUENCING_BUFFERING_SIZE":
                            deviceDetails.TransportSequencingBufferingSize = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "STREAMING_PATH":
                            deviceDetails.StreamingPath = environmentValues[key];
                            break;
                        case "STREAMING_PATH_RAW":
                            deviceDetails.StreamingPathRaw = environmentValues[key];
                            break;
                        case "DISPLAY_ALL_FRAMES":
                            deviceDetails.DisplayAllFrames = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "CLEAN_UP_STREAMING_FOLDER":
                            deviceDetails.CleanUpStreamingFolder = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "FFMPEG_ARGUMENTS":
                            deviceDetails.FfmpegArguments = environmentValues[key];
                            break;
                        case "FFMPEG_ARGUMENTS_RAW_INPUT":
                            deviceDetails.FfmpegArgumentsRawInput = environmentValues[key];
                            break;
                        case "VIDEO_STREAMING_OPTION":
                            deviceDetails.VideoStreamingOption = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "VIDEO_FORMATS_ALLOWED":
                            deviceDetails.VideoFormatsAllowed = environmentValues[key];
                            break;
                        case "SHARED_BLOB_STORAGE":
                            deviceDetails.SharedBlobStorage = environmentValues[key].ToLower().Equals("yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;

                        case "CVPREDICT_FIND_CONTROL_IN_MULTIPLE_CONTROL_STATES":
                            deviceDetails.TemplateMatching.FindControlInMultipleControlStates = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "CVPREDICT_IMAGE_RECOGNITION_TIMEOUT":
                            deviceDetails.TemplateMatching.ImageRecognitionTimeout = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "CVPREDICT_USE_TRUE_COLOR_TEMPLATE_MATCHING":
                            deviceDetails.TemplateMatching.UseTrueColorTemplateMatching = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "CVPREDICT_IMAGE_MATCH_CONFIDENCE_THRESHOLD":
                            deviceDetails.TemplateMatching.ImageMatchConfidenceThreshold = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "CVPREDICT_MULTIPLE_SCALE_TEMPLATE_MATCHING":
                            deviceDetails.TemplateMatching.MultipleScaleTemplateMatching = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "CVPREDICT_IMAGE_MATCH_MAX_SCALE_STEP_COUNT":
                            deviceDetails.TemplateMatching.ImageMatchMaxScaleStepCount = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "CVPREDICT_IMAGE_MATCH_SCALE_STEP_SIZE":
                            deviceDetails.TemplateMatching.ImageMatchScaleStepSize = Convert.ToDouble(environmentValues[key]);
                            break;
                        case "CVPREDICT_ENABLE_TEMPLATE_MATCHING_MAPPING":
                            deviceDetails.TemplateMatching.EnableTemplateMatchMapping = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "CVPREDICT_WAIT_FOREVER":
                            deviceDetails.TemplateMatching.WaitForever = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "ENABLE_ELASTICSTORE":
                            deviceDetails.EnableElasticStore = environmentValues[key];
                            break;
                        case "CVPREDICT_TEMPLATE_MATCH_MAPPING_BORDER_THICKNESS":
                            deviceDetails.TemplateMatching.TemplateMatchMappingBorderThickness = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "DEVICE_ID":
                            deviceDetails.DeviceId = environmentValues[key];
                            break;
                        case "TENANT_ID":
                            deviceDetails.TenantId = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "MSG_VERSION":
                            deviceDetails.MsgVersion = environmentValues[key];
                            break;
                        case "INF_VERSION":
                            deviceDetails.InfVersion = environmentValues[key];
                            break;
                        case "POSE_POINT_RENDERING":
                            deviceDetails.PosePointRendering = environmentValues[key];
                            break;
                        case "SEGMENT_RENDERING":
                            deviceDetails.SegmentRendering = environmentValues[key];
                            break;
                        case "CLASSIFICATION_RENDERING":
                            deviceDetails.ClassificationRendering = environmentValues[key];
                            break;
                        case "PREDICT_CART":
                            deviceDetails.PredictCart = environmentValues[key];
                            break;
                        case "TRACKING":
                            deviceDetails.Tracking = environmentValues[key];
                            break;
                        case "CROWD_COUNTING":
                            deviceDetails.CrowdCounting = environmentValues[key];
                            break;
                        case "RENDERER_FONT_SCALE":
                            deviceDetails.RendererFontScale = Convert.ToDouble(environmentValues[key]);
                            break;
                        case "RENDERER_FONT_THICKNESS":
                            deviceDetails.RendererFontThickness = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "MPLUG":
                            deviceDetails.Mplug = environmentValues[key];
                            break;
                        case "HEATMAP":
                            deviceDetails.HeatMap = environmentValues[key];
                            break;
                        case "INDEXNAME":
                            deviceDetails.IndexName = environmentValues[key];
                            break;
                        case "SPEED_DETECTION":
                            deviceDetails.SpeedDetection = environmentValues[key];
                            break;
                        case "SEGMENTCOLORS":
                            deviceDetails.SegmentColors = environmentValues[key];
                            break;
                        case "PYTHONVIRTUALPATH":
                            deviceDetails.PythonVirtualPath = environmentValues[key];
                            break;
                        case "PANOPTICSEGMENTATION":
                            deviceDetails.PanopticSegmentation = environmentValues[key];
                            break;
                        case "LABEL_COLOR":
                            deviceDetails.LabelColor = environmentValues[key];
                            break;
                        case "PYTHONVERSION":
                            deviceDetails.PythonVersion = environmentValues[key];
                            break;
                        case "BACKGROUND_COLOR":
                            deviceDetails.BackgroundColor = environmentValues[key];
                            break;
                        case "RENDERER_RECTANGLE_POINT_X":
                            deviceDetails.RendererRectanglePointX = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "RENDERER_RECTANGLE_POINT_Y":
                            deviceDetails.RendererRectanglePointY = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "RENDERER_LABEL_POINT_X":
                            deviceDetails.RendererLabelPointX = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "RENDERER_LABEL_POINT_Y":
                            deviceDetails.RendererLabelPointY = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "RENDERER_RECTANGLE_HEIGHT":
                            deviceDetails.RendererRectangleHeight = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "RENDERER_PREDICTCART_LIST_BACKGROUNDCOLOR":
                            deviceDetails.RendererPredictCartListBackgroundColor = environmentValues[key];
                            break;
                        case "BACKGROUND_CHANGE":
                            deviceDetails.BackgroundChange = environmentValues[key];
                            break;
                        case "FFMPEG_BACKGROUNDCHANGE":
                            deviceDetails.FfmpegforBackgroundChange = environmentValues[key];
                            break;
                        case "PROMPT_INPUT_DIRECTORY":
                            deviceDetails.PromptInputDirectory = environmentValues[key];
                            break;
                        case "MASK_IMAGE_DIRECTORY":
                            deviceDetails.MaskImageDirectory = environmentValues[key];
                            break;
                        case "REPLACE_IMAGE_DIRECTORY":
                            deviceDetails.ReplaceImageDirectory = environmentValues[key];
                            break;
                        case "OUTPUT_IMAGE":
                            deviceDetails.OutputImage = environmentValues[key];
                            break;
                        case "PCD_DIRECTORY":
                            deviceDetails.PcdDirectory = environmentValues[key];
                            break;
                        case "RENDERER_BACKGROUND_TRANSPARENCY":
                            deviceDetails.RendererBackgroundTransparency = Convert.ToDouble(environmentValues[key]);
                            break;
                        case "CVPREDICT_MULTI_ROTATION_TEMPLATE_MATCHING":
                            deviceDetails.TemplateMatching.MultiRotationTemplateMatching = environmentValues[key].Equals("Yes", StringComparison.InvariantCultureIgnoreCase) ? true : false;
                            break;
                        case "CVPREDICT_IMAGE_MATCH_ROTATION_STEP_ANGLE":
                            deviceDetails.TemplateMatching.ImageMatchRotationStepAngle = Convert.ToDouble(environmentValues[key]);
                            break;
                        case "XAI_API_VERSION":
                            deviceDetails.XaiApiVersion = environmentValues[key];
                            break;
                        case "XAI_TO_RUN":
                            deviceDetails.XaiToRun = environmentValues[key];
                            break;
                        case "XAI_MODEL":
                            deviceDetails.XaiModel = environmentValues[key];
                            break;
                        case "XAI_BATCH_SIZE":
                            deviceDetails.XaiBatchSize = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "XAI_TEMPLATE_NAME":
                            deviceDetails.XaiTemplateName = environmentValues[key];
                            break;
                        case "HYPERPARAMETERS":
                            deviceDetails.HyperParameters = environmentValues[key];
                            break;
                        case "OBJECTDETECTION_RENDERING":
                            deviceDetails.ObjectDetectionRendering = environmentValues[key];
                            break;
                        case "RENDER_IMAGE_FILE_PATH":
                            deviceDetails.RenderImageFilePath = environmentValues[key];
                            break;
                        case "RENDER_IMAGE_ENABLED":
                            deviceDetails.RenderImageEnabled = environmentValues[key];
                            break;
                        case "DEBUG_IMAGE_FILE_PATH":
                            deviceDetails.DebugImageFilePath = environmentValues[key];
                            break;
                        case "IMAGE_DEBUG_ENABLED":
                            deviceDetails.ImageDebugEnabled = environmentValues[key];
                            break;
                        case "ENABLE_PING":
                            deviceDetails.EnablePing = Convert.ToBoolean(environmentValues[key]);
                            break;
                        case "CLIENT_CONNECTION_RETRY_COUNT":
                            deviceDetails.ClientConnectionRetryCount = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "FRAME_RENDERER_WAIT_TIME_FOR_TRANSPORT_MS":
                            deviceDetails.FrameRenderer_WaitTimeForTransportms = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "FRAME_RENDERER_EOF_COUNT":
                            deviceDetails.FrameRenderer_EOF_Count = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "FRAME_RENDERER_EOF_FILE_PATH":
                            deviceDetails.FrameRenderer_EOF_File_Path = environmentValues[key];
                            break;
                        case "FRAME_GRAB_RATE_THROTTLING_SLEEP_FRAME_COUNT":
                            deviceDetails.FrameGrabRateThrottlingSleepFrameCount = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "FRAME_GRAB_RATE_THROTTLING_SLEEP_DURATION_MSEC":
                            deviceDetails.FrameGrabRateThrottlingSleepDurationMsec = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "FFMPEG_EXE_FILE":
                            deviceDetails.FfmpegExeFile = environmentValues[key];
                            break;
                        case "CALCULATE_FRAME_GRABBER_FPR":
                            deviceDetails.CalculateFrameGrabberFPR = environmentValues[key];
                            break;
                        case "MAX_EMPTY_FRAME_COUNT":
                            deviceDetails.MaxEmptyFrameCount = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "EMPTY_FRAME_PROCESS_INTERVAL":
                            deviceDetails.EmptyFrameProcessInterval = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "FTP_CYCLE":
                            deviceDetails.FTPCycle = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "ELASTIC_STORE_INDEX_NAME":
                            deviceDetails.ElasticStoreIndexName = environmentValues[key];
                            break;
                        case "PROMPT_TEMPLATES_DIRECTORY":
                            deviceDetails.PromptTemplatesDirectory = environmentValues[key];
                            break;
                        case "REDUCE_FRAME_QUALITY_TO":
                            deviceDetails.ReduceFrameQualityTo = Convert.ToDouble(environmentValues[key]);
                            break;
                        case "MIN_THREAD_ON_POOL":
                            deviceDetails.MinThreadOnPool = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "MAX_THREAD_ON_POOL":
                            deviceDetails.MaxThreadOnPool = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "MAX_FAIL_COUNT":
                            deviceDetails.MaxFailCount = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "IMAGE_FORMATS_TO_USE":
                            deviceDetails.ImageFormatsToUse = environmentValues[key];
                            break;
                        case "OFFLINE_PROCESS_INTERVAL":
                            deviceDetails.OfflineProcessInterval = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "DATA_STREAM_TIME_OUT":
                            deviceDetails.DataStreamTimeOut = environmentValues[key];
                            break;
                        case "CLIENT_CONNECTION_WAITING_TIME":
                            deviceDetails.ClientConnectionWaitingTime = Convert.ToInt32(environmentValues[key]);
                            break;
                        case "PROCESS_LOADER_TRACE_FILE":
                            deviceDetails.ProcessLoaderTraceFile = environmentValues[key];
                            break;
                        case "PREDICTION_TYPE":
                            deviceDetails.PredictionType = environmentValues[key];
                            break;
                        case "SPLIT_SCREEN_RENDERING":
                            deviceDetails.SplitScreenRendering = environmentValues[key];
                            break;
                        case "SPLIT_SCREEN_GRID":
                            deviceDetails.SplitScreenGrid = environmentValues[key];
                            break;
                        case "DISPLAY_PREDICTION_INFO":
                            deviceDetails.DisplayPredictionInfo = environmentValues[key] == "yes" ? true : false;
                            break;
                        case "ENVIRONMENT_ADAPTER_RETRY_LIMIT":
                            deviceDetails.EnvironmentAdapterRetryLimit = Convert.ToInt32(environmentValues[key]);
                            break;
                        default:
                            break;
                    }
                }
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "AssignConfigValues", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception while assigning updated values to device attributes, exception: {0}, inner exception: {1}, stack trace: {2}", LogHandler.Layer.Resource, ex.Message, ex.InnerException, ex.StackTrace);
            }
            return deviceDetails;
        }
    }
}
