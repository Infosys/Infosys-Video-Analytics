/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;

using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute
{
    public class TaskRoute
    {

        ObjectCache cache = MemoryCache.Default;
        CacheItemPolicy policy = new CacheItemPolicy();
        public static TaskRouteMetadata _taskRouteMetaData;
        public static TaskRouteMetadata TaskRouteMetaData(DeviceDetails deviceDetails) {
            if(_taskRouteMetaData==null) {
                _taskRouteMetaData=new TaskRoute().GetTaskRouteConfig(Config.AppSettings.TenantID.ToString(),Config.AppSettings.DeviceID,deviceDetails);
            }
            return _taskRouteMetaData;
        }

        
        public Boolean AllowTaskRouting(string tenantId,string deviceId,List<string> moduleList,DeviceDetails deviceDetails) {
            Boolean res = false;
            TaskRouteMetadata taskRouteMetadata=GetTaskRouteConfig(tenantId,deviceId,deviceDetails);
            foreach (string module in moduleList)
            {
                if (taskRouteMetadata.TasksRoute?.SelectToken(module) != null)
                {
                    res = true;

                }
            }

            return res;
        }

        
        public Dictionary<string,List<string>> GetTaskRouteDetails(string tenantId,string deviceId,string module,DeviceDetails deviceDetails) {
            TaskRouteMetadata taskRouteMetadata=GetTaskRouteConfig(tenantId,deviceId,deviceDetails);
            Dictionary<string, List<string>> te = new Dictionary<string, List<string>>();
            List<string> taskRouteDetails = null;
            if (taskRouteMetadata.TasksRoute?.SelectToken(module) != null)
            {
                taskRouteDetails = new List<string>();
                taskRouteDetails = JsonConvert.DeserializeObject<List<string>>(taskRouteMetadata.TasksRoute.SelectToken(module).ToString());
            }
            if (taskRouteDetails != null)
            {
                te.Add(module, taskRouteDetails);
                return te;
            }
            return null;
        }

       
        public string SendMessageToQueue<T>(string tenantId,string deviceId,string module,T message,DeviceDetails deviceDetails) {
            TaskRouteDS taskRouteDS = new TaskRouteDS();
            string msgResponse = string.Empty;
            
#if DEBUG
            using (LogHandler.TraceOperations("TaskRouterHelper:SendMessageToQueue", LogHandler.Layer.Business, Guid.NewGuid()))
            {
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "SendMessageToQueue", "TaskRouterHelper"), LogHandler.Layer.Business, null);
#endif
                TaskRouteMetadata taskRouteMetadata=GetTaskRouteConfig(tenantId,deviceId,deviceDetails);
               
                JObject transportRegionCodes = taskRouteMetadata.TransportRegionCodes;
                var transportRegion = transportRegionCodes?.SelectToken(module);
                if (transportRegion != null)
                {
                    msgResponse += taskRouteDS.Send(message, transportRegion.ToString());
                }
                else
                {
                    
                    throw new TaskRouteNotFoundException(String.Format("Transport Region config is not found for {0}", module));
                }
                


#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "SendMessageToQueue", "TaskRouterHelper"), LogHandler.Layer.Business, null);
            }
#endif
            return msgResponse;
        }

        public string SendMessageToQueueWithTask<T>(TaskRouteMetadata taskRouteMetadata, string module, T message, string task)
        {
            TaskRouteDS taskRouteDS = new TaskRouteDS();
            string msgResponse = "";
           

#if DEBUG
            using (LogHandler.TraceOperations("TaskRouterHelper:SendMessageToQueue", LogHandler.Layer.Business, Guid.NewGuid(), null))
            {
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "SendMessageToQueue", "TaskRouterHelper"), LogHandler.Layer.Business, null);
#endif
                if (task != null)
                {
                  

                    JObject transportRegionCodes = taskRouteMetadata.TransportRegionCodes;
                    var transportRegion = transportRegionCodes?.SelectToken(task);
                    if (transportRegion != null)
                    {
                        msgResponse += taskRouteDS.Send(message, transportRegion.ToString());
                    }
                    else
                    {
                        
                        throw new TaskRouteNotFoundException(String.Format("Transport Region config is not found for {0}", task));
                    }

                }


#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "SendMessageToQueue", "TaskRouterHelper"), LogHandler.Layer.Business, null);
            }
#endif
            return msgResponse;
        }


       

        public TaskRouteMetadata GetTaskRouteConfig(string tenantId,string deviceId,DeviceDetails deviceDetails) {
            TaskRouteMetadata taskRouteMetadata;
            string taskRouteKey = string.Format(TaskRouteConstants.TaskRouteKey, tenantId, deviceId);
            taskRouteMetadata = (TaskRouteMetadata)cache[taskRouteKey];
#if DEBUG
            using (LogHandler.TraceOperations("TaskRouterHelper:GetTaskRoteConfig", LogHandler.Layer.Business, Guid.NewGuid(), null))
            {
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "GetTaskRoteConfig", "TaskRouterHelper"), LogHandler.Layer.Business, null);
#endif
                if (taskRouteMetadata == null)
                {
                    taskRouteMetadata = new TaskRouteMetadata();
                    deviceDetails = UpdateEnvironmentVariables(deviceDetails);
                    string taskRoutesString = deviceDetails.TasksRoute;
                    if (deviceDetails.TasksRoute == null)
                    {

                    }
                    if (deviceDetails.TransportRegionCodes == null)
                    {
                        
                    }
                    taskRouteMetadata.TasksRoute = JObject.Parse(deviceDetails.TasksRoute);
                    taskRouteMetadata.TransportRegionCodes = JObject.Parse(deviceDetails.TransportRegionCodes);
                    cache.Set(taskRouteKey, taskRouteMetadata, policy);
                }

#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "GetTaskRoteConfig", "TaskRouterHelper"), LogHandler.Layer.Business, null);
            }
#endif
            return taskRouteMetadata;
        }

        public static string GetTaskCode(string taskCode,Dictionary<string,string> arguments) {
            AppSettings appSettings=Config.AppSettings;
            DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,CacheConstants.TasksRoute,arguments);
            if(arguments!=null && arguments.Count>0) {
                string type=arguments[arguments.Keys.First()];
                if(type.ToLower()=="values") {
                    deviceDetails=UpdateConfigValues(arguments,deviceDetails);
                }
            }
            Dictionary<string,string> taskRoute=JsonConvert.DeserializeObject<Dictionary<string,string>>(JsonConvert.SerializeObject(TaskRouteMetaData(deviceDetails).TransportRegionCodes));
            foreach(string key in taskRoute.Keys)
            {
                if(taskRoute.GetValueOrDefault(key) == taskCode)
                {
                    return key;
                }
            }
            return "";
        }

        public static DeviceDetails UpdateEnvironmentVariables(DeviceDetails deviceDetails)
        {
            LIFAdapter adapter = new LIFAdapter();
            Dictionary<string, string?> environmentValues = new Dictionary<string, string?>();
            int retry = deviceDetails.EnvironmentAdapterRetryLimit;
            while (retry > 0)
            {
                try
                {
                    adapter.GetEnvironmentVariables(ApplicationConstants.ENVIRONMENT_REGION, out environmentValues);
                    break;
                }
                catch (Exception ex)
                {
                    retry--;
                    LogHandler.LogError("Error while assigning environment variables, exception: {0}, inner exception: {1}, stack trace: {2}", LogHandler.Layer.Infrastructure, ex.Message, ex.InnerException, ex.StackTrace);
                    if (retry == 0)
                    {
                        LogHandler.LogError("Exception in environment adapter, exception: {0}, inner exception: {1}, stack trace: {2}", LogHandler.Layer.Infrastructure, ex.Message, ex.InnerException, ex.StackTrace);
                    }
                }
            }
            deviceDetails = UpdateConfigValues(environmentValues, deviceDetails);
            return deviceDetails;
        }

        public static DeviceDetails UpdateConfigValues(Dictionary<string, string?> environmentValues, DeviceDetails deviceDetails)
        {
            //LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UpdateConfigValues"), LogHandler.Layer.FrameGrabber, null);

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
            return deviceDetails;
        }
    }
}
