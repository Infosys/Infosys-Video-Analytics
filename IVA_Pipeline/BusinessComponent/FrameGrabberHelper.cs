/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using DA = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System.IO.Compression;
using System.Diagnostics;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using TR = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Security;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using System.Net.Http;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using Nest;
using System.Threading.Channels;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data;
using System.Reflection;
using System.IO.MemoryMappedFiles;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using OpenCvSharp;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    public static class FrameGrabberHelper
    {
        #region Attributes

        public static List<Task> taskList = new List<Task>();

        public static double imageTask = 0;
        public static double lotTask = 0;
        public static double blobImageTime = 0;
        public static double blobZipTime = 0;
        public static double pushQTime = 0;
        public static double preprocessingTime = 0;
        public static double configTime = 0;
        public static double compressTime = 0;
        public static int totalLotCount = 0;
        public static int totalImgCount = 0;

        
        public static AppSettings appSettings = Config.AppSettings;
        public static readonly int MaxFailureCount = 0;
        public static readonly int IntervalWaitTime = 0;
        public static readonly bool StopAfterMaxFail;
        public static int FrameCount = 0;
        public static int TotalFramesGrabbed = 0;
        public static int TotalFramesDequeued = 0;
        public static int TotalMessageSendForPrediction = 0;
        public static int lastFrameNumberSendForPredict = 0;
        public static readonly int tenantId = appSettings.TenantID;
        public static readonly string deviceId = appSettings.DeviceID;
        public static readonly int FrameCompressPercent = 0;
        public static readonly int ZipCompressPercent = Convert.ToInt32(ConfigurationManager.AppSettings["ZipCompressPercent"]);
        public static int FrameGrabFailureCount = 0;
        public static int BlobStoreFailureCount = 0;
        public static int PushMessageFailureCount = 0;
        public static int OtherExceptionCount = 0;
        public static readonly int lotSize;
        public static readonly int frameToPredict;
        public static int lotSizeTemp = 0;
        public static readonly string storageBaseUrl;
        public static readonly string cameraURL;
        public static string modelName;
        public static readonly string UPmodelName;
        public static readonly string userName = UserDetails.userName;
        public static readonly string videoFeedType;
        public static readonly string offlineVideoBaseDirectory;
        private static readonly string millibraryname;
        public static readonly string pcdBaseDirectory;

        public static readonly string offlinePromptDirectory;
        public static readonly string maskImageDirectory;
        public static List<string> msk_img = new List<string>();
        public static string replaceImageDirectory;
        public static List<string> rep_img = new List<string>();

        public static readonly string archiveLocation;
        public static readonly bool archiveEnabled;
        public static readonly bool lotsEnabled;
        public static readonly bool uniquePersonTrackingEnabled;
        public static readonly float uniquePersonOverlapThreshold;
        public static int currentVideoTotalFrameCount = 0;
        public static int videoStreamingOption = 0;
        public static readonly float confidenceThreshold;
        public static readonly float overlapThreshold;
        public static readonly int FramesToPredictPerSecond;
        public static readonly TR.TaskRoute taskRouter = new TR.TaskRoute();
        public static double FPS = 0;
        
        public static int IN_PROGRESS = 1;
        
        public static int CLOSED = 2;
        
        public static int MARKED_CLOSED = 3;
        public static bool displayAllFrames;
        public static int MasterId;
        public static bool clientStatus = true;
        public static readonly int clientStatusUpdateTime = Convert.ToInt32(ConfigurationManager.AppSettings["ClientStatusUpdateTime"]);
        public static readonly string instanceName = string.Concat(tenantId, "_", deviceId);
        public static readonly string debugImageFilePath = appSettings.FgDebugImageFilePath;
        public static readonly string enableDebugImage;
        public static readonly string videoFormatsAllowed;
        public static readonly string streamingPath;
        public static readonly bool enforceFrameSequencing;
        public static readonly int maxSequenceNumber;
        public static bool cleanUpStreamingFolder = false;
        private static Dictionary<int, string> framesNotSendForRendering = new Dictionary<int, string>();
        public static DeviceDetails deviceDetails = ConfigHelper.SetDeviceDetails(tenantId.ToString(), deviceId, CacheConstants.FrameGrabberCode);
        public static readonly int MaxThreadOnPool = deviceDetails.MaxThreadOnPool;
        #endregion

        static FrameGrabberHelper()
        {
            try
            {
                if (MaxThreadOnPool > 0)
                    ThreadPool.SetMaxThreads(MaxThreadOnPool, MaxThreadOnPool); /* Limits the maximum number of active threads on thread pool */
               
                DateTime apiCallST = DateTime.UtcNow;
#if DEBUG
                LogHandler.LogDebug("The GetDeviceAttributes service is called to get config details for Tenant Id: {0} and Device Id: {1} at {2}.", LogHandler.Layer.FrameGrabber, tenantId, deviceId, DateTime.UtcNow.ToLongTimeString());
#endif
               
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate {
                    return true;
                });
                
                DeviceDetails response = ConfigHelper.SetDeviceDetails(tenantId.ToString(), deviceId, CacheConstants.FrameGrabberCode);
                if (response == null)
                    throw new FaceMaskDetectionCriticalException("Failed to get device configuration from services. Response is null.");
#if DEBUG
                LogHandler.LogDebug("The GetDeviceAttributes service is executed successfully to get config details for Tenant Id: {0} and Device Id: {1} at {2}.", LogHandler.Layer.FrameGrabber, tenantId, deviceId, DateTime.UtcNow.ToLongTimeString());
#endif
                configTime = DateTime.UtcNow.Subtract(apiCallST).TotalSeconds;
                configTime = DateTime.UtcNow.Subtract(apiCallST).TotalSeconds;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }
                else
                {
                    userName = " ";
                }
                lotSize = response.FrameToPredict;
                cameraURL = response.CameraURl;
                modelName = response.ModelName;
                storageBaseUrl = response.StorageBaseUrl;
                videoFeedType = response.VideoFeedType;
                offlineVideoBaseDirectory = response.OfflineVideoDirectory;
                millibraryname = response.MILLibraryName;
                pcdBaseDirectory = response.PcdDirectory;
                archiveLocation = response.ArchiveDirectory;
                archiveEnabled = response.ArchiveEnabled;
                confidenceThreshold = response.ConfidenceThreshold;
                overlapThreshold = response.OverlapThreshold;
                lotsEnabled = response.EnableLots;
                FramesToPredictPerSecond = response.FTPPerSeconds;
                uniquePersonTrackingEnabled = response.UniquePersonTrackingEnabled;
                UPmodelName = response.UPModelName;
                uniquePersonOverlapThreshold = response.UniquePersonOverlapThreshold;
                videoStreamingOption = response.VideoStreamingOption;
                displayAllFrames = response.DisplayAllFrames;
                videoFormatsAllowed = response.VideoFormatsAllowed;
                cleanUpStreamingFolder = response.CleanUpStreamingFolder;
                streamingPath = response.StreamingPath;
                maxSequenceNumber = response.MaxSequenceNumber;
                enforceFrameSequencing = response.EnforceFrameSequencing;
                offlinePromptDirectory = response.PromptInputDirectory;
                maskImageDirectory = response.MaskImageDirectory;
                replaceImageDirectory = response.ReplaceImageDirectory;
                enableDebugImage = response.ImageDebugEnabled;
                FrameCompressPercent = Convert.ToInt32(response.ReduceFrameQualityTo);
                MaxFailureCount = response.MaxFailCount;
                IntervalWaitTime = response.OfflineProcessInterval;
                int.TryParse(response.PreviousFrameCount, out int previousframecount);
                bool.TryParse(ConfigurationManager.AppSettings["StopAfterMaxFailure"], out StopAfterMaxFail);
                
            }
            catch (Exception ex)
            {
                #region Exception handling
                LogHandler.LogError("FrameGrabber threw an exception for Tenant: {0}, Device: {1}. Exception Message: {2}, Trace: {3}",
                LogHandler.Layer.FrameGrabber, tenantId, deviceId, ex.Message, ex.StackTrace);
               
                bool failureLogged = false;
                try
                {
                    Exception tempEx = new Exception();
                    bool rethrow = ExceptionHandler.HandleException(ex, ApplicationConstants.WORKER_EXCEPTION_HANDLING_POLICY, out tempEx);
                    failureLogged = true;
                    if (rethrow)
                    {
                        throw tempEx;
                    }
                    else
                    {
                        UpdateFeedDetails(MasterId, DateTime.UtcNow.Ticks);
                        Environment.Exit(0);
                    }
                }
                catch (Exception innerEx)
                {
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "Main", "FrameGrabber"),
                    LogHandler.Layer.Business, null);
                   
                    if (!failureLogged)
                    {
                        LogHandler.LogDebug(String.Format("Exception occured while handling an exception. Error Message: {0}", innerEx.Message), LogHandler.Layer.Business, null);
                    }
                    UpdateFeedDetails(MasterId, DateTime.UtcNow.Ticks);
                    Environment.Exit(0);
                }
                #endregion
            }
        }

        public static bool IsDeviceInitiated(string fileName)
        {
            SE.Message.FeedProcessorMasterMsg feedProcessorMasterMsg = GetFeedMasterWithVideoName(fileName);
            
            if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail?.Status == 0)
            {
                return true;
            }
            return false;
        }

        public static bool UpdateResourceAttribute(int tenantId, string deviceId, string attributeName, string attributeValue)
        {
            SE.Message.ResourceAttributeMsg resourceAttributeMsg = new SE.Message.ResourceAttributeMsg();
            resourceAttributeMsg.TenantId = tenantId;
            resourceAttributeMsg.ResourceId = deviceId;
            resourceAttributeMsg.AttributeName = attributeName;
            resourceAttributeMsg.AttributeName = attributeValue;
            
            var uri = String.Format($"{Config.AppSettings.ConfigWebApi}Configuration​/UpdateResourceAttribute");
            var req = JsonConvert.SerializeObject(resourceAttributeMsg);
            var apiResponse = ServiceCaller.ServiceCall(req, uri, "PUT");
            var response = JsonConvert.DeserializeObject<SE.Message.UpdateFeedDetailsResMsg>(apiResponse);
            return true;
        }
        public static void getLatestclientStatus(string tid, string deviceId)
        {
            while (true)
            {
                if (deviceDetails.DBEnabled)
                {
                    var uri = String.Format($"{Config.AppSettings.ConfigWebApi}Configuration/GetClientStatus?tid={tid}&did={deviceId}");
                    var apiResponse = ServiceCaller.ServiceCall(null, uri, "GET");
                    clientStatus = Convert.ToBoolean(apiResponse);
                }

                
                Thread.Sleep(clientStatusUpdateTime);
            }
        }

        public static IEnumerable<string> GetOfflineFileLocations(string baseDiretory)
        {
            string[] exts = videoFormatsAllowed.Split(',').ToArray();
            if (Directory.Exists(baseDiretory))
            {
                var fileNames = Directory.EnumerateFiles(baseDiretory, "*.*")
                    .Where(f => exts.Any(x => f.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                return fileNames;
            }
            else
            {
                
                LogHandler.LogError($"Could not find the base directory <{baseDiretory}> for offline video files..", LogHandler.Layer.FrameGrabber, null);
               
                return null;
            }
        }


        public static int GenerateMaterId()
        {
            bool secondHalfYear = false;
            DateTime datetime = DateTime.UtcNow;
           
            int month = datetime.Month;
            if (month > 6)
            {
                datetime = datetime.AddMonths(-6);
                secondHalfYear = true;
            }
            string datetimestr = datetime.ToString("ddMyyHHmm");
            int masterId = Convert.ToInt32(datetimestr);
            if (secondHalfYear)
            {
                masterId = -masterId;
            }
            return masterId;
        }

        public static int InsertFeedDetails(string videoSourceURL, long startTimeTick)
        {
            int masterID = 0;
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "InsertFeedDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The InsertFeedDetails Method started Executing for Video :{0} at {1}.", LogHandler.Layer.FrameGrabber, videoSourceURL, DateTime.UtcNow.ToLongTimeString());

            using (LogHandler.TraceOperations("InsertFeedDetails:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                try
                {
                    SE.Data.Feed_Master data = new SE.Data.Feed_Master()
                    {
                        ResourceId = deviceId,
                        FileName = videoSourceURL, 
                        FeedURI = videoSourceURL,
                        ProcessingStartTimeTicks = startTimeTick,
                        CreatedBy = userName,
                        CreatedDate = DateTime.UtcNow,
                        TenantId = tenantId,
                        Status = IN_PROGRESS,
                        MachineName = System.Environment.MachineName,
                    };
                    SE.Message.InsertFeedDetailsReqMsg reqMsg = new SE.Message.InsertFeedDetailsReqMsg()
                    {
                        FeedMaster = data
                    };

                    
                    if (deviceDetails.DBEnabled)
                    {
                        
                        var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/InsertFeedDetails");
                        var req = JsonConvert.SerializeObject(reqMsg);
                        var apiResponse = ServiceCaller.ServiceCall(req, uri, "POST");
                        string st = JsonConvert.SerializeObject(reqMsg);
                        var response = JsonConvert.DeserializeObject<SE.Message.InsertFeedDetailsResMsg>(apiResponse);
                        
                        if (response != null)
                            masterID = response.MasterId;
                        else
                        {
                            LogHandler.LogError("Exception occured while inserting data into feed processor master table for file: {0}, Device ID :{1}, Tenant ID :{2}.", LogHandler.Layer.FrameGrabber, videoSourceURL, deviceId, tenantId);
                            
                        }
                    }

                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured while inserting data into feed processor master table for file: {0}, Device ID :{1}, Tenant ID :{2}. Exception message :{3}",
                        LogHandler.Layer.FrameGrabber, videoSourceURL, deviceId, tenantId, ex.Message);
                   
                }
#if DEBUG
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "InsertFeedDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The InsertFeedDetails Method finished Executing for Video :{0} at {1}.", LogHandler.Layer.FrameGrabber, videoSourceURL, DateTime.UtcNow.ToLongTimeString());
#endif
            return masterID;
        }

        public static SE.Message.FeedProcessorMasterMsg GetFeedMasterWithVideoName(string videoName)
        {
            SE.Message.FeedProcessorMasterMsg response = null;
            if (deviceDetails.DBEnabled)
            {
                
                string uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/GetFeedProcessorMasterFromVideoName?videoName={videoName}");

                var apiResponse = ServiceCaller.ServiceCall(null, uri, "GET");
                response = JsonConvert.DeserializeObject<SE.Message.FeedProcessorMasterMsg>(apiResponse);
            }


            
            return response;

        }

        public static SE.Message.FeedProcessorMasterMsg GetFeedMasterWithDeviceId(string deviceId)
        {
            SE.Message.FeedProcessorMasterMsg response = null;
            if (deviceDetails.DBEnabled)
            {
                string uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/GetFeedProcessorMasterWithDeviceId?deviceId={deviceId}");

                var apiResponse = ServiceCaller.ServiceCall(null, uri, "GET");
                response = JsonConvert.DeserializeObject<SE.Message.FeedProcessorMasterMsg>(apiResponse);
            }
            return response;
        }

        public static SE.Message.FeedProcessorMasterMsg GetFeedProcessorMasterWithMasterId(int feedMasterId)
        {
            var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/GetFeedDetails?masterId={feedMasterId}");

            var apiResponse = ServiceCaller.ServiceCall(null, uri, "GET");
            var response = JsonConvert.DeserializeObject<SE.Message.FeedProcessorMasterMsg>(apiResponse);
            return response;

        }

        public static SE.Message.FeedRequestReqMsg GetFeedRequestWithMasterId(int masterId)
        {
            SE.Message.FeedRequestReqMsg retObj = new SE.Message.FeedRequestReqMsg();
            
            var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/GetFeedRequestWithMasterId?masterId={masterId}");

            var apiResponse = ServiceCaller.ServiceCall(null, uri, "GET");
            var inpObj = JsonConvert.DeserializeObject<SE.Message.FeedRequestReqMsg>(apiResponse);

            if (inpObj != null)
            {
                retObj.CreatedBy = inpObj.CreatedBy;
                retObj.CreatedDate = inpObj.CreatedDate;
                retObj.FeedProcessorMasterId = inpObj.FeedProcessorMasterId;
                retObj.LastFrameGrabbedTime = inpObj.LastFrameGrabbedTime;
                retObj.LastFrameId = inpObj.LastFrameId;
                retObj.LastFrameProcessedTime = inpObj.LastFrameProcessedTime;
                retObj.ModifiedBy = inpObj.ModifiedBy;
                retObj.ModifiedDate = inpObj.ModifiedDate;
                retObj.RequestId = inpObj.RequestId;
                retObj.ResourceId = inpObj.ResourceId;
                retObj.Status = inpObj.Status;
                retObj.TenantId = inpObj.TenantId;
                retObj.VideoName = inpObj.VideoName;
                retObj.StartFrameProcessedTime = inpObj.StartFrameProcessedTime;
                retObj.Model = inpObj.Model;
            }

            return retObj;

        }

        public static SE.Message.FeedRequestReqMsg GetFeedRequestWithRequestId(string requestId)
        {
            SE.Message.FeedRequestReqMsg retObj = new SE.Message.FeedRequestReqMsg();
            var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/GetFeedRequestWithRequestId?requestId={requestId}");

            var apiResponse = ServiceCaller.ServiceCall(null, uri, "GET");
            var inpObj = JsonConvert.DeserializeObject<SE.Message.FeedRequestReqMsg>(apiResponse);
           
            if (inpObj != null)
            {
                retObj.CreatedBy = inpObj.CreatedBy;
                retObj.CreatedDate = inpObj.CreatedDate;
                retObj.FeedProcessorMasterId = inpObj.FeedProcessorMasterId;
                retObj.LastFrameGrabbedTime = inpObj.LastFrameGrabbedTime;
                retObj.LastFrameId = inpObj.LastFrameId;
                retObj.LastFrameProcessedTime = inpObj.LastFrameProcessedTime;
                retObj.ModifiedBy = inpObj.ModifiedBy;
                retObj.ModifiedDate = inpObj.ModifiedDate;
                retObj.RequestId = inpObj.RequestId;
                retObj.ResourceId = inpObj.ResourceId;
                retObj.Status = inpObj.Status;
                retObj.TenantId = inpObj.TenantId;
                retObj.VideoName = inpObj.VideoName;
                retObj.StartFrameProcessedTime = inpObj.StartFrameProcessedTime;
                retObj.Model = inpObj.Model;
            }

            return retObj;

        }

        public static SE.Message.FeedRequestReqMsg GetFeedRequestWithDeviceId(string deviceId)
        {
            SE.Message.FeedRequestReqMsg retObj = new SE.Message.FeedRequestReqMsg();
            
            var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/GetFeedRequestWithDeviceId?requestId={deviceId}");

            var apiResponse = ServiceCaller.ServiceCall(null, uri, "GET");
            var inpObj = JsonConvert.DeserializeObject<SE.Message.FeedRequestReqMsg>(apiResponse);
            
            if (inpObj != null)
            {
                retObj.CreatedBy = inpObj.CreatedBy;
                retObj.CreatedDate = inpObj.CreatedDate;
                retObj.FeedProcessorMasterId = inpObj.FeedProcessorMasterId;
                retObj.LastFrameGrabbedTime = inpObj.LastFrameGrabbedTime;
                retObj.LastFrameId = inpObj.LastFrameId;
                retObj.LastFrameProcessedTime = inpObj.LastFrameProcessedTime;
                retObj.ModifiedBy = inpObj.ModifiedBy;
                retObj.ModifiedDate = inpObj.ModifiedDate;
                retObj.RequestId = inpObj.RequestId;
                retObj.ResourceId = inpObj.ResourceId;
                retObj.Status = inpObj.Status;
                retObj.TenantId = inpObj.TenantId;
                retObj.VideoName = inpObj.VideoName;
                retObj.StartFrameProcessedTime = inpObj.StartFrameProcessedTime;
                retObj.Model = inpObj.Model;
            }

            return retObj;

        }

        public static bool UpdateFeedRequestDetails(SE.Message.FeedRequestReqMsg feedRequestReqMsg)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UpdateFeedRequestDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The UpdateFeedRequestDetails Method started Executing for feedMasterId :{0} at {1}.", LogHandler.Layer.FrameGrabber, feedRequestReqMsg.FeedProcessorMasterId, DateTime.UtcNow.ToLongTimeString());

            using (LogHandler.TraceOperations("UpdateFeedRequestDetails:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                try
                {
                    
                    var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/UpdateFeedRequestDetails");
                    var req = JsonConvert.SerializeObject(feedRequestReqMsg);
                    var apiResponse = ServiceCaller.ServiceCall(req, uri, "PUT");
                    var response = JsonConvert.DeserializeObject<SE.Message.UpdateFeedRequestResMsg>(apiResponse);
                    
                    if (response != null)
                        return response.Status;
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured in method UpdateFeedRequestDetails while Updating data into feed processor master table for Master ID: {0}, Device ID :{1}, Tenant ID :{2}. Exception message :{3}",
                        LogHandler.Layer.FrameGrabber, feedRequestReqMsg.FeedProcessorMasterId, deviceId, tenantId, ex.Message);
                    
                }
#if DEBUG
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "UpdateFeedRequestDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The UpdateFeedRequestDetails Method finished Executing for feedMasterId :{0} at {1}.", LogHandler.Layer.FrameGrabber, feedRequestReqMsg.FeedProcessorMasterId, DateTime.UtcNow.ToLongTimeString());
#endif
            return false;
        }


        public static int UpdateMediaMetaData(SE.Message.Media_MetaData_Msg_Req mediaMetaDataMsgReq)
        {
            int mediaId = -1;
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UpdateMediaMetaData", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The UpdateMediaMetaData Method started Executing for feedMasterId :{0} at {1}.", LogHandler.Layer.FrameGrabber, mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId, DateTime.UtcNow.ToLongTimeString());

            using (LogHandler.TraceOperations("UpdateMediaMetaData:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                try
                {
                    var uri = Config.AppSettings.ConfigWebApi + "configuration/UpdateMediaMetaData";
                    
                    var req = JsonConvert.SerializeObject(mediaMetaDataMsgReq);
                    var apiResponse = ServiceCaller.ServiceCall(req, uri, "PUT");
                    var response = JsonConvert.DeserializeObject<SE.Message.Media_MetaData_Msg_Res>(apiResponse);
                    if (response != null)
                        return response.MediaId;
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured in method UpdateMediaMetaData while Updating data into MediaMetaData table for Master ID: {0}, Device ID :{1}, Tenant ID :{2}. Exception message :{3}",
                        LogHandler.Layer.FrameGrabber, mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId, deviceId, tenantId, ex.Message);
                  
                }
#if DEBUG
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "UpdateMediaMetaData", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The UpdateMediaMetaData Method finished Executing for feedMasterId :{0} at {1}.", LogHandler.Layer.FrameGrabber, mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId, DateTime.UtcNow.ToLongTimeString());
#endif
            return mediaId;
        }

        public static int InsertMediaMetaData(MediaMetaDataMsg mediaMetaDataMsgReq)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UpdateMediaMetaData", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The InsertMediaMetaData Method started Executing for feedMasterId :{0} at {1}.", LogHandler.Layer.FrameGrabber, mediaMetaDataMsgReq.MediaMetadataDetail.FeedProcessorMasterId, DateTime.UtcNow.ToLongTimeString());

            using (LogHandler.TraceOperations("InsertMediaMetaData:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                try
                {
                   
                    var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/InsertMediaMetaData");
                    var req = JsonConvert.SerializeObject(mediaMetaDataMsgReq);
                    var apiResponse = ServiceCaller.ServiceCall(req, uri, "POST");
                    var response = JsonConvert.DeserializeObject<SE.Message.Media_MetaData_Msg_Res>(apiResponse);
                 
                    if (response != null)
                        return response.MediaId;
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured while Inserting data into MediaMetaData table for Master ID: {0}, Device ID :{1}, Tenant ID :{2}. Exception message :{3}",
                        LogHandler.Layer.FrameGrabber, deviceId, tenantId, ex.Message);
                    
                }
#if DEBUG
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "InsertMediaMetaData", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The InsertMediaMetaData Method finished Executing for feedMasterId :{0} at {1}.", LogHandler.Layer.FrameGrabber, mediaMetaDataMsgReq.MediaMetadataDetail.FeedProcessorMasterId, DateTime.UtcNow.ToLongTimeString());
#endif
            return -1;
        }

        public static bool UpdateFeedDetails(int masterId, long endTimeTick, int status = 2)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UpdateFeedDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The UpdateFeedDetails Method started Executing for Master ID :{0} at {1}.", LogHandler.Layer.FrameGrabber, masterId, DateTime.UtcNow.ToLongTimeString());

            using (LogHandler.TraceOperations("UpdateFeedDetails:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif

                try
                {
                    SE.Data.Feed_Master data = new SE.Data.Feed_Master()
                    {
                        FeedProcessorMasterId = masterId,
                        ProcessingEndTimeTicks = endTimeTick,
                        ModifiedBy = userName,
                        ModifiedDate = DateTime.UtcNow,
                        TenantId = tenantId,
                        Status = status
                    };
                    SE.Message.UpdateFeedDetailsReqMsg reqMsg = new SE.Message.UpdateFeedDetailsReqMsg()
                    {
                        FeedMaster = data
                    };
                    if (deviceDetails.DBEnabled)
                    {
                        
                        var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/UpdateFeedDetails");
                        var req = JsonConvert.SerializeObject(reqMsg);
                        var apiResponse = ServiceCaller.ServiceCall(req, uri, "PUT");
                        var response = JsonConvert.DeserializeObject<SE.Message.UpdateFeedDetailsResMsg>(apiResponse);
                    
                        if (response != null)
                            return response.Status;
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured in method UpdateFeedDetails while Updating data into feed processor master table for Master ID: {0}, Device ID :{1}, Tenant ID :{2}. Exception message :{3}",
                        LogHandler.Layer.FrameGrabber, masterId, deviceId, tenantId, ex.Message);
                    
                }
#if DEBUG
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "UpdateFeedDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The UpdateFeedDetails Method finished Executing for Master ID :{0} at {1}.", LogHandler.Layer.FrameGrabber, masterId, DateTime.UtcNow.ToLongTimeString());
#endif
            return false;
        }


        public static bool UpdateAllFeedDetails(SE.Message.FeedProcessorMasterMsg reqMsg)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UpdateFeedDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The UpdateFeedDetails Method started Executing for Master ID :{0} at {1}.", LogHandler.Layer.FrameGrabber, reqMsg.FeedProcessorMasterDetail.FeedProcessorMasterId, DateTime.UtcNow.ToLongTimeString());

            using (LogHandler.TraceOperations("UpdateFeedDetails:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                try
                {
                    
                    var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/UpdateFeedDetails2");
                    var req = JsonConvert.SerializeObject(reqMsg);
                    var apiResponse = ServiceCaller.ServiceCall(req, uri, "PUT");
                    var response = JsonConvert.DeserializeObject<SE.Message.UpdateFeedDetailsResMsg>(apiResponse);
                   
                    if (response != null)
                        return response.Status;

                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured in method UpdateAllFeedDetails while Updating data into feed processor master table for Master ID: {0}, Device ID :{1}, Tenant ID :{2}. Exception message :{3}",
                        LogHandler.Layer.FrameGrabber, reqMsg.FeedProcessorMasterDetail.FeedProcessorMasterId, deviceId, tenantId, ex.Message);
                   
                }
#if DEBUG
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "UpdateFeedDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The UpdateFeedDetails Method finished Executing for Master ID :{0} at {1}.", LogHandler.Layer.FrameGrabber, reqMsg.FeedProcessorMasterDetail.FeedProcessorMasterId, DateTime.UtcNow.ToLongTimeString());
#endif
            return false;
        }

        
        public static void ProcessLotAsync(List<Mat> lotFramesList, List<string> grabberTimeList, string fileName, int sequenceNumber, int frameNumber)
        {
            
            List<byte[]> imageArr = new List<byte[]>();
            List<string> grabberTimeArr = new List<string>(grabberTimeList);
            for (int i = 0; i < lotFramesList.Count; i++)
            {
                Mat frameObj = lotFramesList[i];
                if (frameObj != null)
                {
                    imageArr.Add(frameObj.ImEncode(".jpg"));
                }
            }
#if DEBUG
            LogHandler.LogDebug("Executing method {0}. Size of lotFramesArr  : {1} , size of grabberTimeArr : {2}", LogHandler.Layer.FrameGrabber,
                "ProcessLotAsync", imageArr.Count, grabberTimeArr.Count);
#endif
            taskList.Add(Task.Run(() =>
            {
                try
                {
                    DateTime st = DateTime.UtcNow;
                    Stopwatch sw = Stopwatch.StartNew();
                    string lotName = fileName + ApplicationConstants.FileExtensions.zip;
#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "ProcessLotAsync", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);



                    using (LogHandler.TraceOperations("ProcessLotAsync:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
                    {
                        
#endif

                        using (MemoryStream zipFile = new MemoryStream())
                        {
                           
                            ZipLotImagesNewAsync(imageArr, fileName, zipFile, grabberTimeArr);

                            if (zipFile?.Length > 0)
                            {
                                bool status = false;
                                if (UploadLotToBlob(zipFile, lotName))
                                {
                                    
                                    var taskList = taskRouter.GetTaskRouteDetails(tenantId.ToString(), deviceId, TaskRouteConstants.FrameGrabberLotCode)[TaskRouteConstants.FrameGrabberLotCode];
                                    foreach (var task in taskList)
                                    {
                                        byte[] pcdBytes = Array.Empty<byte>();
                                        PushToQueues(pcdBytes, fileName, task, null, sequenceNumber, frameNumber, DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt"), "Grabber", DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt"), "", "", "", ""); /* Added last three parameters as static as per new iva request: Yoges Govindaraj */
                                    }
                                    sw.Stop();
                                    

                                }
                                else
                                {
                                    LogHandler.LogError("The UploadToBlobAsync Method Failed for Lot :{0} at {1}", LogHandler.Layer.FrameGrabber, lotName, DateTime.UtcNow);
                                    
                                }
                                zipFile.Dispose();

#if DEBUG
#endif
                            }
                            else
                                LogHandler.LogDebug("The ZipLotImagesNewAsync Method returned NULL for Lot :{0}.", LogHandler.Layer.FrameGrabber, DateTime.UtcNow.ToLongTimeString(), lotName);

                         

                        }

#if DEBUG
                    }
#endif
                    LogHandler.LogDebug("The ProcessLotAsync Method Finished Executing at {0} for Lot :{1}.", LogHandler.Layer.FrameGrabber, DateTime.UtcNow.ToLongTimeString(), lotName);
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "ProcessLotAsync", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
                  
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured in ProcessLotAsync fileName: {0}. Exception message :{1}",
                        LogHandler.Layer.FrameGrabber, fileName, ex.Message);
                    
                    if (imageArr != null)
                    {
                        imageArr.Clear();
                        imageArr = null;
                    }
                    if (grabberTimeArr != null)
                    {
                        grabberTimeArr.Clear();
                        grabberTimeArr = null;
                    }

                }
                finally
                {
                    if (imageArr != null)
                    {
                        imageArr.Clear();
                        imageArr = null;
                    }
                    if (grabberTimeArr != null)
                    {
                        grabberTimeArr.Clear();
                        grabberTimeArr = null;
                    }
                }

            }));
        }

        public static void ProcessAllFileAsync(List<Mat> lotFramesList, List<string> grabberTimeList, string fileName, int sequenceNumber, int frameNumber)
        {
            List<Mat> lotFramesArr = new List<Mat>(lotFramesList);
            List<string> grabberTimeArr = new List<string>(grabberTimeList);
#if DEBUG
            LogHandler.LogDebug("Executing method {0}.  size of grabberTimeArr : {1}", LogHandler.Layer.FrameGrabber, "ProcessAllFileAsync", grabberTimeArr.Count);
#endif
            
            try
            {
                DateTime st = DateTime.UtcNow;
                Stopwatch sw = Stopwatch.StartNew();
#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "ProcessAllFileAsync", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
                using (LogHandler.TraceOperations("ProcessAllFileAsync:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
                {
#endif
                    for (var i = 0; i < lotFramesArr.Count; i++)
                    {

                        using (Mat frame = lotFramesArr[i])
                        {
                            if (frame != null)
                            {
                                byte[] file = frame.ImEncode(".jpg");
                                FrameGrabberHelper.UploadImage(file, grabberTimeArr[i]);
                            }
                            frame.Dispose();
                        }

                    }

                   

                    var taskList = taskRouter.GetTaskRouteDetails(tenantId.ToString(), deviceId, FrameGrabber._taskCode)[FrameGrabber._taskCode];
                    foreach (var task in taskList)
                    {
                        byte[] pcdBytes = Array.Empty<byte>();
                        PushToQueues(pcdBytes, fileName, task, null, sequenceNumber, frameNumber, DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt"), "Grabber", DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt"), "", "", "", ""); /* Added last three parameters as static as per new iva request: Yoges Govindaraj */
                    }
#if DEBUG
                }
#endif
                LogHandler.LogDebug("The ProcessAllFileAsync Method Finished Executing at {0} for Lot :{1}.", LogHandler.Layer.FrameGrabber, DateTime.UtcNow.ToLongTimeString());
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "ProcessAllFileAsync", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            }
            finally
            {
                lotFramesArr = null;
                grabberTimeArr = null;
            }
       
        }

      
        public static Stream ZipLotImagesNewAsync(List<byte[]> lotFramesArr, string fileName, MemoryStream returnStream, List<string> grabberTimeList)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "ZipLotImagesNewAsync", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The ZipLotImagesNewAsync Method started Executing for Lot :{0}.", LogHandler.Layer.FrameGrabber, fileName);

            LogHandler.LogDebug("Executing method {0}. Size of lotFramesArr  : {1} , size of grabberTimeList : {2}", LogHandler.Layer.FrameGrabber, "ZipLotImagesNewAsync", lotFramesArr.Count, grabberTimeList.Count);

            using (LogHandler.TraceOperations("ZipLotImagesNewAsync:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                try
                {
                    using (var archive = new ZipArchive(returnStream, ZipArchiveMode.Create, true))
                    {

                        for (int i = 0; i < lotFramesArr.Count; i++)
                        {
                            
                            byte[] file = lotFramesArr[i];
                            if (file != null)
                            {
                               
                                if (file != null && file.Length > 0)
                                {
                                    string fName = grabberTimeList[i] + ApplicationConstants.FileExtensions.jpg;
                                    var fileInArchive = archive.CreateEntry(fName, CompressionLevel.Optimal);
                                    using (var entryStream = fileInArchive.Open())
                                    {

                                        using (MemoryStream ms = new MemoryStream())
                                        {
                                            CompressImage(file, ms, FrameCompressPercent).CopyTo(ms);
                                            ms.Position = 0;
                                            ms.CopyTo(entryStream);
                                        }
                                    }
                                }
                                Array.Clear(file, 0, file.Length);
                                file = null;

                            }
                           
                        }
                    }
#if DEBUG
                    if (returnStream != null && returnStream.Length > 0)
                        LogHandler.LogDebug("The ZipLotImagesNewAsync Method finished Successfully for Lot :{0}.", LogHandler.Layer.FrameGrabber, fileName);
                    else
                    {
                        LogHandler.LogError("The ZipLotImagesNewAsync Method Failed for Lot :{0}.", LogHandler.Layer.FrameGrabber, fileName);
                       
                    }

#endif
                    returnStream.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                   
                    lotFramesArr.Clear();
                    lotFramesArr = null;
                    returnStream = null;

                    LogHandler.LogError("Exception occured while zipping the frames", LogHandler.Layer.FrameGrabber, null);
                  
                    if (BlobStoreFailureCount > MaxFailureCount)
                    {
                        
                        if (StopAfterMaxFail)
                        {
                            UpdateFeedDetails(MasterId, DateTime.UtcNow.Ticks);
                            Environment.Exit(0);
                        }
                        FaceMaskDetectionCriticalException exception = new FaceMaskDetectionCriticalException(String.Format("Exception occured while zipping the frames"), ex);

                        throw exception;
                    }

                    Interlocked.Increment(ref BlobStoreFailureCount);
                }
#if DEBUG
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "ZipLotImagesNewAsync", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
            return returnStream;
        }

       
        public static bool UploadLotToBlob(Stream file, string fileName)
        {
            bool status = true;
            try
            {
                
#if DEBUG
                DateTime st = DateTime.UtcNow;
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UploadLotToBlob", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
                LogHandler.LogDebug("The UploadLotToBlob Method started Executing for Lot :{0} at {1}.", LogHandler.Layer.FrameGrabber, fileName, DateTime.UtcNow.ToLongTimeString());

                using (LogHandler.TraceOperations("UploadLotToBlob:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
                {
#endif
                    DA.Document.WorkflowDS workflowDS = new DA.Document.WorkflowDS();
                    DE.Document.Workflow response = workflowDS.Upload(new DE.Document.Workflow
                    {
                        TenantId = tenantId,
                        File = file,
                        FrameId = fileName,
                        StorageBaseURL = storageBaseUrl,
                        UploadedBy = userName,
                        DeviceId = deviceId
                    });

                    if (response?.StatusCode == 0)
                    {
                        Interlocked.Increment(ref totalLotCount);
                       
#if DEBUG
                        LogHandler.LogDebug("The Lot :{0} is successfully uploaded at {1}.", LogHandler.Layer.FrameGrabber, fileName, DateTime.UtcNow.ToLongTimeString());
#endif
                    }
                    else
                    {
                        if (BlobStoreFailureCount > MaxFailureCount)
                        {
                           
                            if (StopAfterMaxFail)
                            {
                                UpdateFeedDetails(MasterId, DateTime.UtcNow.Ticks);
                                Environment.Exit(0);
                            }
                            LogHandler.LogError("Failed to Upload zipped data into Blob Storage. Reached Maximum failure count. Terminating the process", LogHandler.Layer.FrameGrabber, null);
                            throw new FaceMaskDetectionCriticalException(String.Format("Failed to Upload Zipped data into Blob Storage. Reached Maximum Failure Count {0}", MaxFailureCount));

                        }
                        LogHandler.LogError("Failed to Upload zipped data into Blob Storage for Lot {0}", LogHandler.Layer.FrameGrabber, fileName);
                       
                        Interlocked.Increment(ref BlobStoreFailureCount);
                        status = false;
                    }

#if DEBUG
                }
                LogHandler.LogDebug("The UploadLotToBlob Method Finished Executing for Lot :{0} at {1}.", LogHandler.Layer.FrameGrabber, fileName, DateTime.UtcNow.ToLongTimeString());
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "UploadToBlobAsync", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
                LogHandler.LogDebug($"Blob insertion for lot {fileName} took {DateTime.UtcNow.Subtract(st).TotalMilliseconds} miliseconds", LogHandler.Layer.FrameGrabber, null);
#endif
            }
            catch (Exception ex)
            {
                status = false;
                LogHandler.LogError("Exception thrown while Upload zipped data into Blob Storage for Lot {0}. Exception Message: {1}", LogHandler.Layer.FrameGrabber, fileName, ex.Message);
                Interlocked.Increment(ref BlobStoreFailureCount);

            }

            return status;
        }

       


        public static bool PushToQueues(byte[] pcdBytes, string frameId, string moduleCode, List<string> grabberTimeList, int sequenceNumber, int frameNumber, string Starttime, string Source, string Endtime, string Ffp, string Ltsize, string Lfp, string videoFileName)
        {

            bool status = true;
           
            Endtime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");

#if DEBUG
            DateTime st = DateTime.UtcNow; 
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "PushToQueues", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The PushToQueues Method started Executing for Frame :{0} at {1}.", LogHandler.Layer.FrameGrabber, frameId, DateTime.UtcNow.ToLongTimeString());

            using (LogHandler.TraceOperations("PushToQueues:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                if (frameId == "")
                {
                    frameId = DateTime.UtcNow.Ticks.ToString();
                }
                DE.Queue.FrameGrabberMetaData queueEntity = new DE.Queue.FrameGrabberMetaData()
                {
                    Fid = frameId,
                    Did = deviceId,
                    Sbu = storageBaseUrl,
                    Tid = tenantId.ToString(),
                    Mod = modelName,
                    TE = taskRouter.GetTaskRouteDetails(tenantId.ToString(), deviceId, moduleCode),
                    FeedId = MasterId.ToString(),
                    Fp = FrameCount.ToString(),
                    Fids = grabberTimeList,
                    SequenceNumber = sequenceNumber.ToString(),
                    FrameNumber = frameNumber.ToString(),
                    Stime = Starttime,
                    Src = Source,
                    Etime = Endtime,
                    Msk_img = msk_img, 
                    Rep_img = rep_img,  
                    Ffp = Ffp,
                    Ltsize = Ltsize,
                    Lfp = Lfp,
                    videoFileName = videoFileName,
                    Pcd = pcdBytes,
                    Hp = deviceDetails.HyperParameters

                };
                
                if (!clientStatus)
                {
                    framesNotSendForRendering.Add(sequenceNumber, frameId);
                    
                }
               

                if (TaskRouteConstants.UniquePersonCode == moduleCode)
                {
                    queueEntity.Mod = UPmodelName;
                }
                
                string response = taskRouter.SendMessageToQueue(tenantId.ToString(), deviceId, moduleCode, queueEntity);
                if (string.IsNullOrEmpty(response))
                {
                    if (PushMessageFailureCount > MaxFailureCount)
                    {
                       
                        if (StopAfterMaxFail)
                        {
                            UpdateFeedDetails(MasterId, DateTime.UtcNow.Ticks);
                            Environment.Exit(0);
                        }
                        throw new FaceMaskDetectionCriticalException(String.Format("Failed to Send Message into Queue. Reached Maximum Failure Count {0}", MaxFailureCount));
                    }
                    LogHandler.LogError("Failed to Send Message into PushToFQueues.", LogHandler.Layer.FrameGrabber, null);
                    
                    Interlocked.Increment(ref PushMessageFailureCount);
                    status = false;
                    
                }
#if DEBUG
                else
                    LogHandler.LogDebug("Successfully sent the message to PushToQueues for :{0} at {1}", LogHandler.Layer.FrameGrabber, frameId, DateTime.UtcNow);
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "PushToQueues", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            pushQTime += DateTime.UtcNow.Subtract(st).TotalMilliseconds;
            LogHandler.LogDebug($"Pushing to Queue for FrameId:{frameId} took {DateTime.UtcNow.Subtract(st).TotalMilliseconds} milliseconds", LogHandler.Layer.FrameGrabber, null);
#endif
            return status;
        }

        
        public static bool UploadToBlob(Stream file, string fileName)
        {
            bool status = true;
            try
            {
                DateTime st = DateTime.UtcNow;
                Stopwatch sw = Stopwatch.StartNew();
                file.Position = 0;
                
#if DEBUG

                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UploadToBlob", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
                LogHandler.LogDebug("The UploadToBlob Method started Executing for Frame :{0} with Parameters : tenantId {1}, File Name:{2}, storageBaseUrl: {3}, user : {4}, deviceId : {5}.",
                    LogHandler.Layer.FrameGrabber, fileName, tenantId, fileName, storageBaseUrl, userName, deviceId);
                if (enableDebugImage != null && enableDebugImage.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (debugImageFilePath != null && Directory.Exists(debugImageFilePath))
                    {
                        Image image = Image.FromStream(file);
                        image.Save(debugImageFilePath + fileName);
                    }
                }

                using (LogHandler.TraceOperations("UploadToBlob:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
                {
#endif
                    DA.Document.WorkflowDS workflowDS = new DA.Document.WorkflowDS();
                    var response = workflowDS.Upload(new DE.Document.Workflow
                    {
                        TenantId = tenantId,
                        File = file,
                        FrameId = fileName,
                        StorageBaseURL = storageBaseUrl,
                        UploadedBy = userName,
                        DeviceId = deviceId
                    });
#if DEBUG
                    LogHandler.LogDebug("Upload response for frame:{0} is {1}",
                        LogHandler.Layer.FrameGrabber, fileName, response?.StatusCode);
#endif
                    if (response?.StatusCode == 0)
                    {
                        Interlocked.Increment(ref totalImgCount);
                        sw.Stop();
                        
#if DEBUG
                        LogHandler.LogDebug("The Image is uploaded to Blob Successfully. Frame ID: {0} ", LogHandler.Layer.FrameGrabber, fileName);

#endif
                    }
                    else
                    {
                        if (BlobStoreFailureCount > MaxFailureCount)
                        {
                            
                            if (StopAfterMaxFail)
                            {
                                UpdateFeedDetails(MasterId, DateTime.UtcNow.Ticks);
                                Environment.Exit(0);
                            }
                            LogHandler.LogError("Failed to Upload data into Blob Storage. Reached Maximum failure count. Terminating the process", LogHandler.Layer.FrameGrabber, null);
                            throw new FaceMaskDetectionCriticalException(String.Format("Failed to Upload data into Blob Storage. Reached Maximum Failure Count {0}", MaxFailureCount));

                        }
                        LogHandler.LogError("Failed to Upload data into Blob Storage.", LogHandler.Layer.FrameGrabber, null);
                        
                        Interlocked.Increment(ref BlobStoreFailureCount);
                        status = false;
                    }

#if DEBUG
                }

                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "UploadToBlob", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
                LogHandler.LogDebug($"Blob insertion for Image {fileName} took {DateTime.UtcNow.Subtract(st).TotalMilliseconds} milliseconds", LogHandler.Layer.FrameGrabber, null);
#endif
            }
            catch (Exception ex)
            {
              
                status = false;
                Interlocked.Increment(ref BlobStoreFailureCount);
                LogHandler.LogError("Exception thrown while uploading Frame {0}. Exception Message: {1}", LogHandler.Layer.FrameGrabber, fileName, ex.Message);
            }

            return status;

        }

        public static void PostVideoProcess(string file)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "PostVideoProcess", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("The PostVideoProcess Method started Executing for file :{0} ", LogHandler.Layer.FrameGrabber, file);
#endif
            if (archiveEnabled)
            {
                string archiveLocationTemp = Path.Combine(archiveLocation, deviceId);
                Directory.CreateDirectory(archiveLocationTemp);
                string[] fileNamewithExt = Path.GetFileName(file).Split('.');
                File.Move(file, Path.Combine(archiveLocationTemp, fileNamewithExt[0] + DateTime.UtcNow.Ticks.ToString() + "." + fileNamewithExt[1]));
#if DEBUG
                LogHandler.LogDebug("The Video File : {0} is Archived to {1}", LogHandler.Layer.FrameGrabber, file, archiveLocationTemp);
#endif
               
            }
            else
            {
                File.Delete(file);
#if DEBUG
                LogHandler.LogDebug("The Video File : {0} is Deleted", LogHandler.Layer.FrameGrabber, file);
#endif
                
            }
           
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "PostVideoProcess", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
        }

        
        public static MemoryStream CompressImage(byte[] frame, MemoryStream imgMS, int quality)
        {
            using (MemoryStream ms = new MemoryStream(frame))
            {
#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "CompressImage", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
                if (quality < 0 || quality > 100)
                    throw new ArgumentOutOfRangeException("quality must be between 0 and 100.");

                
                EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
             
                ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                EncoderParameters encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = qualityParam;
                using (Bitmap bm = new Bitmap(ms))
                {
                    bm.Save(imgMS, jpegCodec, encoderParams);
                }
#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "CompressImage", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
            }

            return imgMS;
        }

        
        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }

        public static void ProcessImageAsync(Mat frame, string fileName, int sequenceNumber, int frameNumber, string Stime, string Source, string Ffp, string Ltsize, string Lfp, string videoFileName)//Added Additional properties for new iva request
        {
            

            using (LogHandler.TraceOperations("FrameGrabberHealper:ProcessImageAsync", LogHandler.Layer.Business, Guid.NewGuid(), null))
            {

                byte[] file = null;

                try
                {
                    if (frame.Data != null)
                    {

                        file = frame.ImEncode(".jpg");

                        DateTime st = DateTime.UtcNow;
                        Stopwatch sw = Stopwatch.StartNew();
                        string Etime;
                        Etime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
#if DEBUG
                        LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "ProcessImageAsync", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
                        LogHandler.LogDebug("The ProcessImageAsync Method started Executing for file :{0} ", LogHandler.Layer.FrameGrabber, fileName);

                        
#endif
                        bool status = false;
                        using (MemoryStream img = new MemoryStream())
                        {
                            
                            if (FrameCompressPercent == 0 || FrameCompressPercent == 100)
                            {
                                img.Write(file, 0, file.Length);
                            }
                            else
                            {
                                CompressImage(file, img, FrameCompressPercent).CopyTo(img);
                            }

                            if (UploadToBlob(img, fileName + ApplicationConstants.FileExtensions.jpg))
                            {
                                bool memdoc = TaskRouteDS.IsMemoryDoc();
                                if (!(FrameGrabberHelper.displayAllFrames && TaskRouteDS.IsMemoryDoc() && FrameGrabberHelper.lotSizeTemp > 1))
                                {

                                    var taskList = taskRouter.GetTaskRouteDetails(tenantId.ToString(), deviceId, FrameGrabber._taskCode)[FrameGrabber._taskCode];
                                    foreach (var task in taskList)
                                    {
                                        byte[] pcdBytes = Array.Empty<byte>();
                                        PushToQueues(pcdBytes, fileName, task, null, sequenceNumber, frameNumber, Stime, "Grabber", Etime, Ffp, Ltsize, Lfp, videoFileName); /* Added last three parameters as static as per new iva request: Yoges Govindaraj */
                                    }
                                }
                                DateTime et = DateTime.UtcNow;
                                string ElapseTimePerFG = et.Subtract(st).TotalSeconds.ToString();
                                sw.Stop();
                                status = true;
                                                       
                            }
                            else
                                status = false;
                            img.Dispose();
                          
                            file = null;

                        }
                        var s = $"FrameId: {fileName} Response time: {sw.Elapsed}";

#if DEBUG
                        LogHandler.LogInfo("The ProcessMethod:ProcessImageAsync. ClassName :FrameGrabber. FrameIdFGB :{0}. TimeElapsed :{1} ", LogHandler.Layer.FrameGrabber, fileName, sw.Elapsed);
#endif
                      
                    }
                }
                catch (Exception ex)
                {
                    file = null;
                    LogHandler.LogError("The ProcessImageAsync Method threw an Exception for file :{0}. Exception message : {1} ", LogHandler.Layer.FrameGrabber, fileName, ex.Message);
                }
                finally
                {
                   
                }
            }
        }


        public static void UploadImage(byte[] file, string fileName)
        {
            taskList.Add(Task.Run(() =>
            {
                try
                {

#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "UploadImages", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
                    LogHandler.LogDebug("The UploadImages Method started Executing for file :{0} ", LogHandler.Layer.FrameGrabber, fileName);

#endif
                    bool status = false;
                    using (MemoryStream img = new MemoryStream())
                    {
                        if (FrameCompressPercent == 0 || FrameCompressPercent == 100)
                        {
                            img.Write(file, 0, file.Length);
                        }
                        else
                        {
                            CompressImage(file, img, FrameCompressPercent).CopyTo(img);
                        }

                        status = UploadToBlob(img, fileName + ApplicationConstants.FileExtensions.jpg);

                        img.Dispose();
                        file = null;
                    }
#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "UploadImages", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
                }
                catch (Exception ex)
                {
                    file = null;
                    LogHandler.LogError("The UploadImages Method threw an Exception for file :{0}. Exception message : {1} ", LogHandler.Layer.FrameGrabber, fileName, ex.Message);
                }
            }));
        }

        
        public static FeedProcessorMasterDetails GetInCompletedFramGrabberDetails(int tenantId, string deviceId)
        {
            FeedProcessorMasterDetails feedProcessorDetails = null;
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "GetInCompletedFramGrabberDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);

            using (LogHandler.TraceOperations("GetInCompletedFramGrabberDetails:FrameGrabberHelper", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                try
                {
                    if (deviceDetails.DBEnabled)
                    {
                        var uri = String.Format($"{Config.AppSettings.ConfigWebApi}configuration/GetInCompleteFramGrabberDetails?tid{tenantId}&did={deviceId}");

                        var apiResponse = ServiceCaller.ServiceCall(null, uri, "GET");
                        var response = JsonConvert.DeserializeObject<SE.Message.FeedMasterResMsg>(apiResponse);

                        
                        feedProcessorDetails = MapFeedProcessorMasterSEtoBE(response);
                    }



                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception occured while getting InCompleted Frame Grabber details, Device ID :{1}, Tenant ID :{2}. Exception message :{3}",
                        LogHandler.Layer.FrameGrabber, deviceId, tenantId, ex.Message);

                }
#if DEBUG
            }

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "GetInCompletedFramGrabberDetails", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);

#endif
            return feedProcessorDetails;
        }

        private static FeedProcessorMasterDetails MapFeedProcessorMasterSEtoBE(SE.Message.FeedMasterResMsg response)
        {
            FeedProcessorMasterDetails retObj = null;
            try
            {
                if (response != null)
                {
                    var responseObj = response.FeedMaster;
                    if (responseObj != null)
                    {
                        string jsonString = JsonConvert.SerializeObject(responseObj);
                        retObj = JsonConvert.DeserializeObject<FeedProcessorMasterDetails>(jsonString);
                    }
                }


            }
            catch (Exception ex)
            {
                throw ex;
            }
            return retObj;
        }

        public static void sendEventMessage(string eventType, int totalFrameCount, int frameNumberSendForPredict, int totalMessageSend)
        {
            LogHandler.LogDebug($"Sending Event message of event type {eventType} for the master id: {MasterId.ToString()}", LogHandler.Layer.FrameGrabber);
            DE.Queue.MaintenanceMetaData queueEntity = new DE.Queue.MaintenanceMetaData();
            queueEntity.Did = deviceId;
            queueEntity.Tid = tenantId.ToString();
            queueEntity.MessageType = ProcessingStatus.EventHandling;
            queueEntity.Timestamp = DateTime.UtcNow;
            queueEntity.ResourceId = deviceId;
            queueEntity.EventType = eventType;
            DE.Queue.FrameInformation frameInformation = new DE.Queue.FrameInformation();
            frameInformation.TID = tenantId.ToString();
            frameInformation.DID = deviceId;
            frameInformation.TotalFrameCount = totalFrameCount.ToString();
            frameInformation.LastFrameNumberSendForPrediction = frameNumberSendForPredict.ToString();
            frameInformation.TotalMessageSendForPrediction = totalMessageSend.ToString();
            frameInformation.FeedId = MasterId.ToString();
            frameInformation.FramesNotSendForRendering = framesNotSendForRendering;
            frameInformation.Model = modelName;
            queueEntity.Data = JsonConvert.SerializeObject(frameInformation);

            var taskList = taskRouter.GetTaskRouteDetails(FrameGrabberHelper.tenantId.ToString(),
                FrameGrabberHelper.deviceId, FrameGrabber._taskCode)[FrameGrabber._taskCode];

            foreach (string moduleCode in taskList)
            {
                taskRouter.SendMessageToQueue(tenantId.ToString(), deviceId, moduleCode, queueEntity);
            }
        }

        public static IEnumerable<string> GetImageFileLocations(string baseDiretory)
        {
            string[] exts = deviceDetails.ImageFormatsToUse.Split(',').ToArray();
            if (Directory.Exists(baseDiretory))
            {
                var fileNames = Directory.EnumerateFiles(baseDiretory, "*.*")
                    .Where(f => exts.Any(x => f.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
                return fileNames;
            }
            else
            {
                LogHandler.LogError($"Could not find the base directory <{baseDiretory}> for offline video files..", LogHandler.Layer.FrameGrabber, null);
                
                return null;
            }
        }

        public static void ImageAsync(Mat frame, string fileName, int sequenceNumber, int frameNumber, string Stime, string Source, string Ffp, string Ltsize, string Lfp, string videoFileName)//Added Additional properties for new iva request
        {
            

            using (LogHandler.TraceOperations("FrameGrabberHealper:ProcessImageAsync", LogHandler.Layer.Business, Guid.NewGuid(), null))
            {

                byte[] file = null;

                try
                {
                    if (frame.Data != null)
                    {

                        file = frame.ImEncode(".jpg");

                        DateTime st = DateTime.UtcNow;
                        Stopwatch sw = Stopwatch.StartNew();
                        string Etime;
                        Etime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
#if DEBUG
                        LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "ProcessImageAsync", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
                        LogHandler.LogDebug("The ProcessImageAsync Method started Executing for file :{0} ", LogHandler.Layer.FrameGrabber, fileName);

#endif
                        bool status = false;
                        using (MemoryStream img = new MemoryStream())
                        {
                           
                            if (FrameCompressPercent == 0 || FrameCompressPercent == 100)
                            {
                                img.Write(file, 0, file.Length);
                            }
                            else
                            {
                                CompressImage(file, img, FrameCompressPercent).CopyTo(img);
                            }

                            if (UploadToBlob(img, fileName + ApplicationConstants.FileExtensions.jpg))
                            {
                                bool memdoc = TaskRouteDS.IsMemoryDoc();
                               

                                var taskList = taskRouter.GetTaskRouteDetails(tenantId.ToString(), deviceId, FrameGrabber._taskCode)[FrameGrabber._taskCode];
                                foreach (var task in taskList)
                                {
                                    byte[] pcdBytes = Array.Empty<byte>();
                                    PushToQueues(pcdBytes, fileName, task, null, sequenceNumber, frameNumber, Stime, "Grabber", Etime, Ffp, Ltsize, Lfp, videoFileName); /* Added last three parameters as static as per new iva request: Yoges Govindaraj */
                                }
                             
                                DateTime et = DateTime.UtcNow;
                                string ElapseTimePerFG = et.Subtract(st).TotalSeconds.ToString();
                                sw.Stop();
                                
                                status = true;
                                                       
                            }
                            else
                                status = false;
                            img.Dispose();
                          
                            file = null;

                        }
                        var s = $"FrameId: {fileName} Response time: {sw.Elapsed}";

#if DEBUG
                        LogHandler.LogInfo("The ProcessMethod:ProcessImageAsync. ClassName :FrameGrabber. FrameIdFGB :{0}. TimeElapsed :{1} ", LogHandler.Layer.FrameGrabber, fileName, sw.Elapsed);
#endif
                        
                    }
                }
                catch (Exception ex)
                {
                    file = null;
                    LogHandler.LogError("The ProcessImageAsync Method threw an Exception for file :{0}. Exception message : {1} ", LogHandler.Layer.FrameGrabber, fileName, ex.Message);
                }
                finally
                {
                    
                }
            }
        }

       

        public static void GetMaskInputImages(string directory)
        {
            if (Directory.Exists(directory))
            {
                string[] images = Directory.GetFiles(directory);
                bool isMemoryDoc = TaskRouteDS.IsMemoryDoc();
                if (!isMemoryDoc)
                {
                    string fileName = DateTime.Now.Ticks.ToString();
                    List<byte[]> imageArr = new List<byte[]>();
                    List<string> grabberTimeArr = new List<string>();
                    foreach (string image in images)
                    {
                        grabberTimeArr.Add(DateTime.Now.Ticks.ToString());
                        byte[] imageData = File.ReadAllBytes(image);
                        imageArr.Add(imageData);
                        File.Delete(image);
                    }
                    if (imageArr.Count > 0)
                    {
                        using (MemoryStream zipFile = new MemoryStream())
                        {
                            ZipLotImagesNewAsync(imageArr, fileName, zipFile, grabberTimeArr);
                            if (zipFile?.Length > 0)
                                if (UploadLotToBlob(zipFile, fileName + ApplicationConstants.FileExtensions.zip))
                                {
                                    msk_img.Add(fileName + ApplicationConstants.FileExtensions.zip);
                                    
                                }
                        }
                    }
                }
                else
                {
                    foreach (string image in images)
                    {
                        msk_img.Add(Convert.ToBase64String(File.ReadAllBytes(image)));
                        File.Delete(image);
                    }
                }
                
            }
        }

        public static void GetReplaceInputImages(string directory)
        {
            if (Directory.Exists(directory))
            {
                string[] images = Directory.GetFiles(directory);
                string imagesUploaded = "";
                bool isMemoryDoc = TaskRouteDS.IsMemoryDoc();
                if (!isMemoryDoc)
                {
                    string fileName = DateTime.Now.Ticks.ToString();
                    List<byte[]> imageArr = new List<byte[]>();
                    List<string> grabberTimeArr = new List<string>();
                    foreach (string image in images)
                    {
                        grabberTimeArr.Add(DateTime.Now.Ticks.ToString());
                        byte[] imageData = File.ReadAllBytes(image);
                        imageArr.Add(imageData);
                        File.Delete(image);
                    }
                    if (imageArr.Count > 0)
                    {
                        using (MemoryStream zipFile = new MemoryStream())
                        {
                            ZipLotImagesNewAsync(imageArr, fileName, zipFile, grabberTimeArr);
                            if (zipFile?.Length > 0)
                                if (UploadLotToBlob(zipFile, fileName + ApplicationConstants.FileExtensions.zip))
                                {
                                    rep_img.Add(fileName + ApplicationConstants.FileExtensions.zip);
                                    
                                }
                        }
                    }
                }
                else
                {
                    foreach (string image in images)
                    {
                        rep_img.Add(Convert.ToBase64String(File.ReadAllBytes(image)));
                        File.Delete(image);
                    }
                }
                
            }
        }

    }
}
