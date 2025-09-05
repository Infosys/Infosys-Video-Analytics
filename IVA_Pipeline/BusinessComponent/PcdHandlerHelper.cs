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
using SE=Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using DA=Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using DE=Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System.IO.Compression;
using System.Diagnostics;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using TR=Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
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

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent {
    public static class PcdHandlerHelper {
        #region Attributes
        public static List<Task> taskList=new List<Task>();
        public static double pushQTime=0;
        public static double configTime=0;
        public static AppSettings appSettings=Config.AppSettings;
        public static readonly int MaxFailureCount=0;
        public static readonly int IntervalWaitTime=0;
        public static readonly bool StopAfterMaxFail;
        public static int FrameCount=0;
        public static int TotalFramesGrabbed=0;
        public static int TotalMessageSendForPrediction=0;
        public static int lastFrameNumberSendForPredict=0;
        public static readonly int tenantId=appSettings.TenantID;
        public static readonly string deviceId=appSettings.DeviceID;
        public static int PushMessageFailureCount=0;
        public static int OtherExceptionCount=0;
        public static readonly int lotSize;
        public static int lotSizeTemp=0;
        public static readonly string storageBaseUrl;
        public static readonly string cameraURL;
        public static string modelName;
        public static readonly string UPmodelName;
        public static readonly string userName=UserDetails.userName;
        public static readonly string videoFeedType;
        public static readonly string offlineVideoBaseDirectory;
        public static readonly string pcdBaseDirectory;    
        public static readonly string offlinePromptDirectory;
        public static readonly string maskImageDirectory;
        public static List<string> msk_img=new List<string>();
        public static string replaceImageDirectory;
        public static List<string> rep_img=new List<string>();
        public static readonly string archiveLocation;
        public static readonly bool archiveEnabled;       
        public static readonly bool lotsEnabled;
        public static readonly bool uniquePersonTrackingEnabled;
        public static readonly float uniquePersonOverlapThreshold;       
        public static int currentVideoTotalFrameCount=0;
        public static int videoStreamingOption=0;
        public static readonly float confidenceThreshold;
        public static readonly float overlapThreshold;
        public static readonly int FramesToPredictPerSecond;
        public static readonly TR.TaskRoute taskRouter=new TR.TaskRoute();
        public static double FPS=0;
        /* 1 – In Progress */
        public static int IN_PROGRESS=1;        
        /* 2 – Closed */
        public static int CLOSED=2;              
        /* 3 – Marked Closed */
        public static int MARKED_CLOSED=3;
        public static bool displayAllFrames;
        public static int MasterId;
        public static bool clientStatus=true;       
        public static readonly string streamingPath;
        public static readonly bool enforceFrameSequencing;
        public static readonly int maxSequenceNumber;
        public static bool cleanUpStreamingFolder=false;
        private static Dictionary<int,string> framesNotSendForRendering=new Dictionary<int,string>();
        public static DeviceDetails deviceDetails=ConfigHelper.SetDeviceDetails(tenantId.ToString(),deviceId,CacheConstants.PcdHandlerCode);
        public static readonly int MaxThreadOnPool=deviceDetails.MaxThreadOnPool;
        #endregion

        static PcdHandlerHelper() {
            try {
                if(MaxThreadOnPool>0)
                    ThreadPool.SetMaxThreads(MaxThreadOnPool,MaxThreadOnPool); /* Limits the maximum number of active threads on thread pool */
                /* if(!LogHandler.InitializeRaw(instanceName))
                    LogHandler.LogError("Initializing raw values for performance monitor failed. The data might be inaccurate for some counters.",LogHandler.Layer.FrameGrabber,null);
                maskDetector=new SC.MaskDetector();
                channel=maskDetector.ServiceChannel;
                Console.WriteLine($"Getting config details for Tenant Id: {tenantId} and Device Id: {deviceId}");
                Getting config details */                
                DateTime apiCallST=DateTime.UtcNow;
                #if DEBUG
                LogHandler.LogDebug("The GetDeviceAttributes service is called to get config details for Tenant Id: {0} and Device Id: {1} at {2}",LogHandler.Layer.PcdHandler,tenantId,deviceId,DateTime.UtcNow.ToLongTimeString());
                #endif
                /* Change SSL checks so that all checks pass */
                ServicePointManager.ServerCertificateValidationCallback=new RemoteCertificateValidationCallback(delegate {
                    return true;
                });
                /* var uri=String.Format($"{Config.AppSettings.ConfigWebApi}Configuration/GetDeviceAttributes?tid={tenantId}&did={deviceId}");
                var apiResponse=ServiceCaller.ApiCaller(null,uri,"GET"); */
                DeviceDetails response=ConfigHelper.SetDeviceDetails(tenantId.ToString(),deviceId,CacheConstants.PcdHandlerCode);
                if(response==null)
                    throw new FaceMaskDetectionCriticalException("Failed to get device configuration from services. Response is null.");
                #if DEBUG
                LogHandler.LogDebug("The GetDeviceAttributes service is executed successfully to get config details for Tenant Id: {0} and Device Id: {1} at {2}",LogHandler.Layer.PcdHandler,tenantId,deviceId,DateTime.UtcNow.ToLongTimeString());
                #endif
                configTime=DateTime.UtcNow.Subtract(apiCallST).TotalSeconds;
                if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                    userName=System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                }
                else {
                    userName=" ";
                }
                lotSize=response.FrameToPredict;
                cameraURL=response.CameraURl;
                modelName=response.ModelName;
                storageBaseUrl=response.StorageBaseUrl;
                videoFeedType=response.VideoFeedType;
                offlineVideoBaseDirectory=response.OfflineVideoDirectory;
                pcdBaseDirectory=response.PcdDirectory;
                archiveLocation=response.ArchiveDirectory;
                archiveEnabled=response.ArchiveEnabled;
                confidenceThreshold=response.ConfidenceThreshold;
                overlapThreshold=response.OverlapThreshold;
                lotsEnabled=response.EnableLots;
                FramesToPredictPerSecond=response.FTPPerSeconds;
                uniquePersonTrackingEnabled=response.UniquePersonTrackingEnabled;
                UPmodelName=response.UPModelName;
                uniquePersonOverlapThreshold=response.UniquePersonOverlapThreshold;
                videoStreamingOption=response.VideoStreamingOption;
                displayAllFrames=response.DisplayAllFrames;
                cleanUpStreamingFolder=response.CleanUpStreamingFolder;
                streamingPath=response.StreamingPath;
                maxSequenceNumber=response.MaxSequenceNumber;
                enforceFrameSequencing=response.EnforceFrameSequencing;
                offlinePromptDirectory=response.PromptInputDirectory;
                maskImageDirectory=response.MaskImageDirectory;
                replaceImageDirectory=response.ReplaceImageDirectory;
                MaxFailureCount=response.MaxFailCount;
                IntervalWaitTime=response.OfflineProcessInterval;
                int.TryParse(response.PreviousFrameCount,out int previousframecount);
                bool.TryParse(ConfigurationManager.AppSettings["StopAfterMaxFailure"],out StopAfterMaxFail);
                /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.FramesToPredictPerSec,
                instanceName,FramesToPredictPerSecond,true,false); */
                if(deviceDetails.DBEnabled) {
                    ResourceAttributesDS resourceAttributesDS=new ResourceAttributesDS();
                    offlineVideoBaseDirectory=resourceAttributesDS.GetOfflineVideoDirectory(tenantId,deviceId);
                    if(Directory.Exists(offlineVideoBaseDirectory)) {
                        string fileName=Directory.GetFiles(offlineVideoBaseDirectory).FirstOrDefault();
                        if(fileName!=null) {
                            string file=Path.GetFileNameWithoutExtension(fileName);
                            FeedRequest feedRequest=new FeedRequestDS().GetOneWithRequestId(file);
                            MasterId=Convert.ToInt32(feedRequest.FeedProcessorMasterId);
                            modelName=feedRequest.Model;
                        }
                    }                    
                }
            }
            catch(Exception ex) {
                #region Exception handling
                LogHandler.LogError("PcdHandlerHelper threw an exception for Tenant: {0}, Device: {1}. Exception Message: {2}, Trace: {3}",
                LogHandler.Layer.PcdHandler,tenantId,deviceId,ex.Message,ex.StackTrace);
                /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.ErrorCount,
                instanceName,0,false,false); */
                bool failureLogged=false;
                try {
                    Exception tempEx=new Exception();
                    bool rethrow=ExceptionHandler.HandleException(ex,ApplicationConstants.WORKER_EXCEPTION_HANDLING_POLICY,out tempEx);
                    failureLogged=true;
                    if(rethrow) {
                        throw tempEx;
                    }
                    else {
                        UpdateFeedDetails(MasterId,DateTime.UtcNow.Ticks);
                        Environment.Exit(0);
                    }
                }
                catch(Exception innerEx) {
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed,"Main","PcdHandlerHelper"),
                    LogHandler.Layer.Business,null);
                    /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.ErrorCount,
                    instanceName,0,false,false); */
                    if(!failureLogged) {
                        LogHandler.LogDebug(String.Format("Exception occurred while handling an exception. Error Message: {0}",innerEx.Message),LogHandler.Layer.Business,null);
                    }
                    UpdateFeedDetails(MasterId,DateTime.UtcNow.Ticks);
                    Environment.Exit(0);
                }
                #endregion
            }
        }

        public static int InsertFeedDetails(string pcdSourceURL,long startTimeTick) {
            int masterID=0;
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"InsertFeedDetails","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The InsertFeedDetails method started executing for Pcd File: {0} at {1}",LogHandler.Layer.PcdHandler,pcdSourceURL,DateTime.UtcNow.ToLongTimeString());
            using(LogHandler.TraceOperations("InsertFeedDetails:PcdHandlerHelper",LogHandler.Layer.PcdHandler,Guid.NewGuid(),null)) {
                #endif
                try {
                    SE.Data.Feed_Master data=new SE.Data.Feed_Master() {
                        ResourceId=deviceId, 
                        FileName=pcdSourceURL, /* Offline pcd filename can be extracted */
                        FeedURI=pcdSourceURL,
                        ProcessingStartTimeTicks=startTimeTick,
                        CreatedBy=userName,
                        CreatedDate=DateTime.UtcNow,
                        TenantId=tenantId,
                        Status=IN_PROGRESS,
                        MachineName=System.Environment.MachineName,
                    };
                    SE.Message.InsertFeedDetailsReqMsg reqMsg=new SE.Message.InsertFeedDetailsReqMsg() {
                        FeedMaster=data
                    };
                    /* Call API caller method */
                    if(deviceDetails.DBEnabled) {
                        /* var uri=String.Format($"{Config.AppSettings.ConfigWebApi}configuration.svc/InsertFeedDetails"); */
                        var uri=String.Format($"{Config.AppSettings.ConfigWebApi}configuration/InsertFeedDetails");
                        var req=JsonConvert.SerializeObject(reqMsg);
                        var apiResponse=ServiceCaller.ServiceCall(req,uri,"POST");
                        string st=JsonConvert.SerializeObject(reqMsg);
                        var response=JsonConvert.DeserializeObject<SE.Message.InsertFeedDetailsResMsg>(apiResponse);
                        /* var response=channel.InsertFeedDetails(reqMsg); */
                        if(response!=null)
                            masterID=response.MasterId;
                        else {
                            LogHandler.LogError("Exception occurred while inserting data into feed processor master table for File: {0}, Device Id: {1}, Tenant Id: {2}",LogHandler.Layer.PcdHandler,pcdSourceURL,deviceId,tenantId);
                            /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.ErrorCount,
                            instanceName,0,false,false); */
                        }
                    }
                }
                catch(Exception ex) {
                    LogHandler.LogError("Exception occurred while inserting data into feed processor master table for File: {0}, Device Id: {1}, Tenant Id: {2}. Exception message: {3}",
                    LogHandler.Layer.PcdHandler,pcdSourceURL,deviceId,tenantId,ex.Message);
                    /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.ErrorCount,
                    instanceName,0,false,false);
                    TBD whether to throw exception or not */
                }
                #if DEBUG
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"InsertFeedDetails","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The InsertFeedDetails method finished executing for Pcd File: {0} at {1}",LogHandler.Layer.PcdHandler,pcdSourceURL,DateTime.UtcNow.ToLongTimeString());
            #endif
            return masterID;
        }

        public static bool UpdateFeedRequestDetails(SE.Message.FeedRequestReqMsg feedRequestReqMsg) {
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"UpdateFeedRequestDetails","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The UpdateFeedRequestDetails method started executing for Feed Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,feedRequestReqMsg.FeedProcessorMasterId,DateTime.UtcNow.ToLongTimeString());
            using (LogHandler.TraceOperations("UpdateFeedRequestDetails:PcdHandlerHelper",LogHandler.Layer.PcdHandler,Guid.NewGuid(),null)) {
                #endif
                try {
                    
                    var uri=String.Format($"{Config.AppSettings.ConfigWebApi}configuration/UpdateFeedRequestDetails");
                    var req=JsonConvert.SerializeObject(feedRequestReqMsg);
                    var apiResponse=ServiceCaller.ServiceCall(req,uri,"PUT");
                    var response=JsonConvert.DeserializeObject<SE.Message.UpdateFeedRequestResMsg>(apiResponse);
                    
                    if(response!=null)
                        return response.Status;
                }
                catch(Exception ex) {
                    LogHandler.LogError("Exception occurred in method UpdateFeedRequestDetails while updating data into feed processor master table for Master Id: {0}, Device Id: {1}, Tenant Id: {2}. Exception message: {3}",
                    LogHandler.Layer.PcdHandler,feedRequestReqMsg.FeedProcessorMasterId,deviceId,tenantId,ex.Message);
                    
                }
                #if DEBUG
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"UpdateFeedRequestDetails","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The UpdateFeedRequestDetails method finished executing for Feed Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,feedRequestReqMsg.FeedProcessorMasterId,DateTime.UtcNow.ToLongTimeString());
            #endif
            return false;
        }


        public static int UpdateMediaMetaData(SE.Message.Media_MetaData_Msg_Req mediaMetaDataMsgReq) {
            int mediaId=-1;
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"UpdateMediaMetaData","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The UpdateMediaMetaData method started executing for Feed Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId,DateTime.UtcNow.ToLongTimeString());
            using(LogHandler.TraceOperations("UpdateMediaMetaData:PcdHandlerHelper",LogHandler.Layer.PcdHandler,Guid.NewGuid(),null)) {
                #endif
                try {
                    var uri=Config.AppSettings.ConfigWebApi+"configuration/UpdateMediaMetaData";
                    
                    var req=JsonConvert.SerializeObject(mediaMetaDataMsgReq);
                    var apiResponse=ServiceCaller.ServiceCall(req,uri,"PUT");
                    var response=JsonConvert.DeserializeObject<SE.Message.Media_MetaData_Msg_Res>(apiResponse);
                    if(response!=null)
                        return response.MediaId;
                }
                catch(Exception ex) {
                    LogHandler.LogError("Exception occurred in method UpdateMediaMetaData while updating data into media metadata table for Master Id: {0}, Device Id: {1}, Tenant Id: {2}. Exception message: {3}",
                    LogHandler.Layer.PcdHandler,mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId,deviceId,tenantId,ex.Message);
                    
                }
                #if DEBUG
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"UpdateMediaMetaData","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The UpdateMediaMetaData method finished executing for Feed Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId,DateTime.UtcNow.ToLongTimeString());
            #endif
            return mediaId;
        }

        public static int InsertMediaMetaData(MediaMetaDataMsg mediaMetaDataMsgReq) {
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"InsertMediaMetaData","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The InsertMediaMetaData method started executing for Feed Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,mediaMetaDataMsgReq.MediaMetadataDetail.FeedProcessorMasterId,DateTime.UtcNow.ToLongTimeString());
            using (LogHandler.TraceOperations("InsertMediaMetaData:PcdHandlerHelper",LogHandler.Layer.PcdHandler,Guid.NewGuid(),null)) {
                #endif
                try {
                    
                    var uri=String.Format($"{Config.AppSettings.ConfigWebApi}configuration/InsertMediaMetaData");
                    var req=JsonConvert.SerializeObject(mediaMetaDataMsgReq);
                    var apiResponse=ServiceCaller.ServiceCall(req,uri,"POST");
                    var response=JsonConvert.DeserializeObject<SE.Message.Media_MetaData_Msg_Res>(apiResponse);
                    
                    if(response!=null)
                        return response.MediaId;
                }
                catch(Exception ex) {
                    LogHandler.LogError("Exception occurred while inserting data into media metadata table for Master Id: {0}, Device Id: {1}, Tenant Id: {2}. Exception message: {3}",
                    LogHandler.Layer.PcdHandler,deviceId,tenantId,ex.Message);
                    
                }
                #if DEBUG
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"InsertMediaMetaData","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The InsertMediaMetaData method finished executing for Feed Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,mediaMetaDataMsgReq.MediaMetadataDetail.FeedProcessorMasterId,DateTime.UtcNow.ToLongTimeString());
            #endif
            return -1;
        }

        public static bool UpdateFeedDetails(int masterId,long endTimeTick,int status=2) {
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"UpdateFeedDetails","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The UpdateFeedDetails method started executing for Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,masterId,DateTime.UtcNow.ToLongTimeString());
            using(LogHandler.TraceOperations("UpdateFeedDetails:PcdHandlerHelper",LogHandler.Layer.PcdHandler,Guid.NewGuid(),null)) {
                #endif
                try {
                    SE.Data.Feed_Master data=new SE.Data.Feed_Master() {
                        FeedProcessorMasterId=masterId,
                        ProcessingEndTimeTicks=endTimeTick,
                        ModifiedBy=userName,
                        ModifiedDate=DateTime.UtcNow,
                        TenantId=tenantId,
                        Status=status
                    };
                    SE.Message.UpdateFeedDetailsReqMsg reqMsg=new SE.Message.UpdateFeedDetailsReqMsg() {
                        FeedMaster=data
                    };
                    if(deviceDetails.DBEnabled) {
                        
                        var uri=String.Format($"{Config.AppSettings.ConfigWebApi}configuration/UpdateFeedDetails");
                        var req=JsonConvert.SerializeObject(reqMsg);
                        var apiResponse=ServiceCaller.ServiceCall(req,uri,"PUT");
                        var response=JsonConvert.DeserializeObject<SE.Message.UpdateFeedDetailsResMsg>(apiResponse);
                        
                        if(response!=null)
                            return response.Status;
                    }
                }
                catch(Exception ex) {
                    LogHandler.LogError("Exception occurred in method UpdateFeedDetails while updating data into feed processor master table for Master Id: {0}, Device Id: {1}, Tenant Id: {2}. Exception message: {3}",
                    LogHandler.Layer.PcdHandler,masterId,deviceId,tenantId,ex.Message);
                    
                }
                #if DEBUG
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"UpdateFeedDetails","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The UpdateFeedDetails method finished executing for Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,masterId,DateTime.UtcNow.ToLongTimeString());
            #endif
            return false;
        }

        public static bool UpdateAllFeedDetails(SE.Message.FeedProcessorMasterMsg reqMsg) {
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"UpdateAllFeedDetails","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The UpdateAllFeedDetails method started executing for Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,reqMsg.FeedProcessorMasterDetail.FeedProcessorMasterId,DateTime.UtcNow.ToLongTimeString());
            using(LogHandler.TraceOperations("UpdateAllFeedDetails:PcdHandlerHelper",LogHandler.Layer.PcdHandler,Guid.NewGuid(),null)) {
                #endif
                try {
                    
                    var uri=String.Format($"{Config.AppSettings.ConfigWebApi}configuration/UpdateFeedDetails2");
                    var req=JsonConvert.SerializeObject(reqMsg);
                    var apiResponse=ServiceCaller.ServiceCall(req,uri,"PUT");
                    var response=JsonConvert.DeserializeObject<SE.Message.UpdateFeedDetailsResMsg>(apiResponse);
                    
                    if(response!=null)
                        return response.Status;
                }
                catch(Exception ex) {
                    LogHandler.LogError("Exception occurred in method UpdateAllFeedDetails while Updating data into feed processor master table for Master Id: {0}, Device Id: {1}, Tenant Id: {2}. Exception message: {3}",
                    LogHandler.Layer.PcdHandler,reqMsg.FeedProcessorMasterDetail.FeedProcessorMasterId,deviceId,tenantId,ex.Message);
                    
                }
                #if DEBUG
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"UpdateAllFeedDetails","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The UpdateAllFeedDetails method finished executing for Master Id: {0} at {1}",LogHandler.Layer.PcdHandler,reqMsg.FeedProcessorMasterDetail.FeedProcessorMasterId,DateTime.UtcNow.ToLongTimeString());
            #endif
            return false;
        }

        
        public static bool PushToQueues(byte[] pcdBytes,string frameId,string moduleCode,List<string> grabberTimeList,int sequenceNumber,int frameNumber,string Starttime,string Source,string Endtime,string Ffp,string Ltsize,string Lfp,string videoFileName) {        
            bool status=true;
            
            Endtime=DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
            #if DEBUG
            DateTime st=DateTime.UtcNow; 
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"PushToQueues","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The PushToQueues method started executing for frame: {0} at {1}",LogHandler.Layer.PcdHandler,frameId,DateTime.UtcNow.ToLongTimeString());            
            using(LogHandler.TraceOperations("PushToQueues:PcdHandlerHelper",LogHandler.Layer.PcdHandler,Guid.NewGuid(),null)) {
                #endif
                if(frameId=="") {
                    frameId=DateTime.UtcNow.Ticks.ToString();
                }
                DE.Queue.PcdHandlerMetaData queueEntity=new DE.Queue.PcdHandlerMetaData() {
                    Fid=frameId,
                    Did=deviceId,
                    Sbu=storageBaseUrl,
                    Tid=tenantId.ToString(),
                    Mod=modelName,
                    TE=taskRouter.GetTaskRouteDetails(tenantId.ToString(),deviceId,moduleCode),
                    FeedId=MasterId.ToString(),
                    Fp=FrameCount.ToString(),
                    Fids=grabberTimeList,
                    SequenceNumber=sequenceNumber.ToString(),
                    FrameNumber=frameNumber.ToString(),
                    Stime=Starttime,
                    Src=Source,
                    Etime=Endtime,
                    Msk_img=msk_img, /* Added Msk_img field as per the new schema */
                    Rep_img=rep_img, /* Added Rep_img field as per the new schema */
                    Ffp=Ffp,
                    Ltsize=Ltsize,
                    Lfp=Lfp,
                    videoFileName=videoFileName,
                    Pcd=pcdBytes
                };
                
                        if(!clientStatus) {
                            framesNotSendForRendering.Add(sequenceNumber,frameId);
                            
                        }
                
                if(TaskRouteConstants.UniquePersonCode==moduleCode) {
                    queueEntity.Mod=UPmodelName;
                }
                string response=taskRouter.SendMessageToQueue(tenantId.ToString(),deviceId,moduleCode,queueEntity);
                if(string.IsNullOrEmpty(response)) {
                    if(PushMessageFailureCount>MaxFailureCount) {
                        
                        if(StopAfterMaxFail) {
                            UpdateFeedDetails(MasterId,DateTime.UtcNow.Ticks);
                            Environment.Exit(0);
                        }
                        throw new FaceMaskDetectionCriticalException(String.Format("Failed to send message into queue. Reached maximum failure count: {0}",MaxFailureCount));
                    }
                    LogHandler.LogError("Failed to send message into PushToQueues",LogHandler.Layer.PcdHandler,null);
                    
                    Interlocked.Increment(ref PushMessageFailureCount);
                    status=false;
                }
                #if DEBUG
                else
                    LogHandler.LogDebug("Successfully sent the message to PushToQueues for {0} at {1}",LogHandler.Layer.PcdHandler,frameId,DateTime.UtcNow);
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"PushToQueues","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            pushQTime+=DateTime.UtcNow.Subtract(st).TotalMilliseconds; /* To remove */
            LogHandler.LogDebug($"Pushing to queue for frameId: {frameId} took {DateTime.UtcNow.Subtract(st).TotalMilliseconds} milliseconds",LogHandler.Layer.PcdHandler,null);
            #endif
            return status;
        }

       
        public static FeedProcessorMasterDetails GetInCompletedPcdHandlerDetails(int tenantId,string deviceId) {
            FeedProcessorMasterDetails feedProcessorDetails=null;
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"GetInCompletedPcdHandlerDetails","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            using(LogHandler.TraceOperations("GetInCompletedPcdHandlerDetails:PcdHandlerHelper",LogHandler.Layer.PcdHandler,Guid.NewGuid(),null)) {
                #endif
                try {
                    if(deviceDetails.DBEnabled) {
                        
                        var uri=String.Format($"{Config.AppSettings.ConfigWebApi}configuration/GetInCompleteFramGrabberDetails?tid{tenantId}&did={deviceId}");                        
                        var apiResponse=ServiceCaller.ServiceCall(null,uri,"GET");
                        var response=JsonConvert.DeserializeObject<SE.Message.FeedMasterResMsg>(apiResponse);
                        
                        feedProcessorDetails=MapFeedProcessorMasterSEtoBE(response);
                    }
                }
                catch(Exception ex) {
                    LogHandler.LogError("Exception occurred while getting incompleted Pcd Handler details. Device Id: {1}, Tenant Id: {2}, Exception Message: {3}",
                    LogHandler.Layer.PcdHandler,deviceId,tenantId,ex.Message);
                }
                #if DEBUG
            }
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"GetInCompletedPcdHandlerDetails","FrameGrabber"),LogHandler.Layer.PcdHandler,null);
            #endif
            return feedProcessorDetails;
        }

        private static FeedProcessorMasterDetails MapFeedProcessorMasterSEtoBE(SE.Message.FeedMasterResMsg response) {
            FeedProcessorMasterDetails retObj=null;
            try {
                if(response!=null) {
                    var responseObj=response.FeedMaster;
                    if(responseObj!=null) {
                        string jsonString=JsonConvert.SerializeObject(responseObj);
                        retObj=JsonConvert.DeserializeObject<FeedProcessorMasterDetails>(jsonString);
                    }
                }
            }
            catch(Exception ex) {
                throw ex;
            }
            return retObj;
        }

        public static void sendEventMessage(string eventType,int totalFrameCount,int frameNumberSendForPredict,int totalMessageSend) {
            DE.Queue.MaintenanceMetaData queueEntity=new DE.Queue.MaintenanceMetaData();
            queueEntity.Did=deviceId;
            queueEntity.Tid=tenantId.ToString();
            queueEntity.MessageType=ProcessingStatus.EventHandling;
            queueEntity.Timestamp=DateTime.UtcNow;
            queueEntity.ResourceId=deviceId;
            queueEntity.EventType=eventType;
            DE.Queue.FrameInformation frameInformation=new DE.Queue.FrameInformation();
            frameInformation.TID=tenantId.ToString();
            frameInformation.DID=deviceId;
            frameInformation.TotalFrameCount=totalFrameCount.ToString();
            frameInformation.LastFrameNumberSendForPrediction=frameNumberSendForPredict.ToString();
            frameInformation.TotalMessageSendForPrediction=totalMessageSend.ToString();
            frameInformation.FeedId=MasterId.ToString();
            frameInformation.FramesNotSendForRendering=framesNotSendForRendering;
            queueEntity.Data=JsonConvert.SerializeObject(frameInformation);
            var taskList=taskRouter.GetTaskRouteDetails(PcdHandlerHelper.tenantId.ToString(),
            PcdHandlerHelper.deviceId,TaskRouteConstants.PcdHandlerCode)[TaskRouteConstants.PcdHandlerCode];
            foreach(string moduleCode in taskList) {
                taskRouter.SendMessageToQueue(tenantId.ToString(),deviceId,moduleCode,queueEntity);
            }
        }

        public static void PostVideoProcess(string file) {
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"PostVideoProcess","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The PostVideoProcess method started executing for file: {0}",LogHandler.Layer.PcdHandler,file);
            #endif
            if(archiveEnabled) {
                string archiveLocationTemp=Path.Combine(archiveLocation,deviceId);
                Directory.CreateDirectory(archiveLocationTemp);
                string[] fileNamewithExt=Path.GetFileName(file).Split('.');
                File.Move(file,Path.Combine(archiveLocationTemp,fileNamewithExt[0]+DateTime.UtcNow.Ticks.ToString()+"."+fileNamewithExt[1]));
                #if DEBUG
                LogHandler.LogDebug("The pcd file: {0} is archived to {1}",LogHandler.Layer.PcdHandler,file,archiveLocationTemp);
                #endif
                
            }
            else {
                File.Delete(file);
                #if DEBUG
                LogHandler.LogDebug("The pcd file: {0} is deleted",LogHandler.Layer.PcdHandler,file);
                #endif
                
            }
            
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"PostVideoProcess","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            #endif
        }
        
        public static IEnumerable<string> GetPcdFileLocations(string baseDiretory) {
            if(Directory.Exists(baseDiretory)) {
                var fileNames=Directory.EnumerateFiles(baseDiretory,"*.*");
                return fileNames;
            }
            else {
                LogHandler.LogError($"Could not find the base directory - {baseDiretory} for offline pcd files",LogHandler.Layer.PcdHandler,null);
                return null;
            }
        }

        public static void ProcessPcdAsync(string pcdSource,string fileName,int sequenceNumber,int frameNumber,string Stime,string Source,string Ffp,string Ltsize,string Lfp,string videoFileName) {
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"ProcessPcdAsync","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("The ProcessPcdAsync method started executing for file: {0}",LogHandler.Layer.PcdHandler,fileName);
            #endif
            using(LogHandler.TraceOperations("PcdHandlerHelper:ProcessPcdAsync",LogHandler.Layer.Business,Guid.NewGuid(),null)) {
                byte[] pcdBytes=null;
                try {
                    if(File.Exists(pcdSource)) {
                        DateTime starttime=DateTime.UtcNow;
                        Stopwatch stopwatch=Stopwatch.StartNew();
                        pcdBytes=File.ReadAllBytes(pcdSource);
                        string Etime;
                        Etime=DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                        var taskList=taskRouter.GetTaskRouteDetails(tenantId.ToString(),deviceId,TaskRouteConstants.PcdHandlerCode)[TaskRouteConstants.PcdHandlerCode];
                        foreach(var task in taskList) {
                            PushToQueues(pcdBytes,fileName,task,null,sequenceNumber,frameNumber,Stime,Source,Etime,Ffp,Ltsize,Lfp,videoFileName);
                        }
                        DateTime endtime=DateTime.UtcNow;
                        string ElapseTimePerFG=endtime.Subtract(starttime).TotalSeconds.ToString();
                        stopwatch.Stop();
                        pcdBytes=null;
                        #if DEBUG
                        LogHandler.LogInfo("The ProcessMethod:ProcessPcdAsync, ClassName:PcdHandlerHelper, PcdFilePCH: {0}, TimeElapsed: {1}",
                        LogHandler.Layer.PcdHandler,fileName,stopwatch.Elapsed);
                        LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"ProcessPcdAsync","PcdHandlerHelper"),LogHandler.Layer.PcdHandler,null);
                        #endif
                    }
                }
                catch(Exception ex) {
                    pcdBytes=null;
                    LogHandler.LogError("The ProcessPcdAsync method threw an exception for file: {0}. Exception message: {1}",
                    LogHandler.Layer.PcdHandler,fileName,ex.Message);
                }
            }
        }
    }
}
