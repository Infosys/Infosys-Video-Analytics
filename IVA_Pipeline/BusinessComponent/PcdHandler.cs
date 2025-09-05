/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using System.Drawing;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using TR=Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Cryptography;
using OpenCvSharp;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent {
    public static class PcdHandler {
        static double totalTime=0;
        static double preProcessTime=0;
        static DateTime ppST=DateTime.UtcNow;
        static VideoCapture reader;
        static bool isclosing=false;
        static bool isUpdated;
        static bool isDBEnabled=true;
        static Boolean isDisposed=false;
        static string LastFrameId=null;
        static DateTime LastFrameGrabbedTime;
        static int sequenceNumber=0;
        static bool isMemoryDoc=false;
        /* Added new variables to calculate MTP values in PcdHandler */
        static string Stime;
        static string Src="Pcd Handler";
        /* Added to check the frame is first or last one */
        static string Ffp=null;
        static string Lfp=null;
        static string LtSize=null;

        public static void ReadFromConfig() {
            AppSettings appSettings=Config.AppSettings;
            DeviceDetails deviceDetails=TR.ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,CacheConstants.PcdHandlerCode);
            if(deviceDetails.DBEnabled!=null) {
                isDBEnabled=deviceDetails.DBEnabled;
            }
        }

        public static void PcdHandlerProcess(bool isConsoleMode) {
            TR.TaskRoute taskRouter=new TR.TaskRoute();
            isMemoryDoc=TaskRouteDS.IsMemoryDoc();
            List<string> moduleList=new List<string>{TaskRouteConstants.PcdHandlerCode};
            if(taskRouter.AllowTaskRouting(PcdHandlerHelper.tenantId.ToString(),PcdHandlerHelper.deviceId,moduleList)) {
                #if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"Main","PcdHandler"),
                LogHandler.Layer.PcdHandler,null);
                LogHandler.LogDebug("Pcd Handler application started for Tenant: {0}, Device: {1}, Camera: {2}, Model: {3}, Storage Location: {4}",
                LogHandler.Layer.PcdHandler,PcdHandlerHelper.tenantId,PcdHandlerHelper.deviceId,PcdHandlerHelper.cameraURL,PcdHandlerHelper.modelName,PcdHandlerHelper.storageBaseUrl);
                #endif
                ReadFromConfig();           
                if(isConsoleMode) {
                    if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                        SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck),true);
                    }
                    /* Process any duplicate running instance */
                    ProcessIncompletedPcdHandlerInstance();
                }
                while(!isclosing) {
                    try {
                        switch(PcdHandlerHelper.videoFeedType) {
                            case "PCD":
                                var pcdFilesToProcess=PcdHandlerHelper.GetPcdFileLocations(PcdHandlerHelper.pcdBaseDirectory);
                                if(pcdFilesToProcess.Count()!=0) {
                                    foreach(string file in pcdFilesToProcess) {
                                        #region re-initialize variable
                                        if(!File.Exists(file)) {
                                            break;
                                        }
                                        PcdHandlerHelper.currentVideoTotalFrameCount=1;
                                        PcdHandlerHelper.FPS=0;
                                        #endregion
                                        #if DEBUG
                                        using(LogHandler.TraceOperations("PcdHandler: Main for pcd file - {0}",LogHandler.Layer.PcdHandler,Guid.NewGuid(),file)) {
                                            #endif
                                            PcdHandlerHelper.TotalFramesGrabbed=0;
                                            sequenceNumber=0;
                                            PcdHandlerHelper.lastFrameNumberSendForPredict=0;
                                            PcdHandlerHelper.TotalMessageSendForPrediction=0;
                                            SetFTP();
                                            if(!File.Exists(file)) {
                                                break;
                                            }
                                            sequenceNumber++;
                                            preProcessTime+=DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
                                            StartProcessPcd(file); /* Starts the process for this pcd file */
                                            /* Pcd file completed processing */
                                            {
                                                LastFrameGrabbedTime=DateTime.UtcNow;
                                                DateTime LastProcessedTime=DateTime.UtcNow;
                                                /* Updating feed request table */
                                                if(isDBEnabled) {
                                                    var feedRequest=FrameGrabberHelper.GetFeedRequestWithMasterId(PcdHandlerHelper.MasterId);
                                                    if(feedRequest!=null && feedRequest.RequestId!=null) {
                                                        feedRequest.LastFrameGrabbedTime=LastFrameGrabbedTime;
                                                        feedRequest.LastFrameId=LastFrameId;
                                                        PcdHandlerHelper.UpdateFeedRequestDetails(feedRequest);
                                                    }
                                                    FeedProcessorMasterMsg feedProcessorMaster=FrameGrabberHelper.GetFeedProcessorMasterWithMasterId(PcdHandlerHelper.MasterId);
                                                    feedProcessorMaster.FeedProcessorMasterDetail.Status=ProcessingStatus.feedCompletedStatus;
                                                    feedProcessorMaster.FeedProcessorMasterDetail.ProcessingEndTimeTicks=DateTime.UtcNow.Ticks;
                                                    if(!isUpdated && PcdHandlerHelper.UpdateAllFeedDetails(feedProcessorMaster))
                                                        isUpdated=true;
                                                }
                                                PcdHandlerHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.EndOfFile,
                                                PcdHandlerHelper.TotalFramesGrabbed,PcdHandlerHelper.lastFrameNumberSendForPredict,PcdHandlerHelper.TotalMessageSendForPrediction);
                                                totalTime+=DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
                                                LogHandler.LogInfo($"The pcd file: {file} is processed successfully. Total Time: {totalTime}, Pre Process Time: {preProcessTime}, Queue Insertion Time: {PcdHandlerHelper.pushQTime}, Total Frames In Pcd File: {PcdHandlerHelper.currentVideoTotalFrameCount}, Total Frames Grabbed: {PcdHandlerHelper.TotalFramesGrabbed}, Total Frames Processed: {PcdHandlerHelper.FrameCount}",LogHandler.Layer.PcdHandler,null);
                                            }
                                            PcdHandlerHelper.taskList.Clear();
                                            PcdHandlerHelper.taskList.TrimExcess();
                                            PcdHandlerHelper.PostVideoProcess(file);
                                            #if DEBUG
                                        }
                                        #endif
                                    }
                                }
                                Thread.Sleep(PcdHandlerHelper.IntervalWaitTime*1000);
                                break;
                        }
                    }
                    catch(Exception ex) {
                        if(!isUpdated && PcdHandlerHelper.UpdateFeedDetails(PcdHandlerHelper.MasterId,DateTime.UtcNow.Ticks))
                            isUpdated=true;
                        /* Console.WriteLine($"Frame Grabber grabbed {0} frames and sent it to queue",FrameGrabberHelper.FrameCount); */
                        LogHandler.LogError("Pcd Handler application threw an Exception: {0}, StackTrace: {1}",
                        LogHandler.Layer.PcdHandler,ex.Message,ex.StackTrace);
                        /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.ErrorCount,
                        FrameGrabberHelper.instanceName,0,false,false); */
                        LogHandler.LogInfo("Pcd Handler application stopped for Tenant: {0}, Device: {1}, Camera: {2}, Model: {3}, Storage Location: {4}",
                        LogHandler.Layer.PcdHandler,PcdHandlerHelper.tenantId,PcdHandlerHelper.deviceId,PcdHandlerHelper.cameraURL,PcdHandlerHelper.modelName,PcdHandlerHelper.storageBaseUrl);
                        bool failureLogged=false;
                        try {
                            Exception tempEx=new Exception();
                            bool rethrow=ExceptionHandler.HandleException(ex,ApplicationConstants.WORKER_EXCEPTION_HANDLING_POLICY,out tempEx);
                            failureLogged=true;
                            if(rethrow) {
                                throw tempEx;
                            }
                            else {
                                if(isConsoleMode) {
                                    Environment.Exit(0);
                                }
                            }
                        }
                        catch(Exception innerEx) {
                            LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed,"Main","PcdHandler"),
                            LogHandler.Layer.Business,null);
                            /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.ErrorCount,
                            FrameGrabberHelper.instanceName,0,false,false); */
                            if(!failureLogged) {
                                LogHandler.LogDebug(String.Format("Exception occurred while handling an exception. Error message: {0}",innerEx.Message),LogHandler.Layer.PcdHandler,null);
                            }
                            if(isConsoleMode) {
                                Environment.Exit(0);
                            }
                        }
                    }
                }
                if(isclosing) {
                    Dispose();
                }
            }
            else {
                LogHandler.LogInfo("There is no task route for PcdHandler",LogHandler.Layer.PcdHandler);
            }
        }

        /* This method checks any incomplete PcdHandler instance exits. If yes it asks user to select the option.
        Based on the user action system will update incompleted Pcd Handler status to 3 – Marked Closed.
        Statuses are 
        1 – In Progress
        2 – Closed
        3 – Marked Closed */
        private static void ProcessIncompletedPcdHandlerInstance() {
            string strMode=null;
            int tenantId=PcdHandlerHelper.tenantId;
            string deviceId=PcdHandlerHelper.deviceId;
            FeedProcessorMasterDetails feedProcessorDetails=PcdHandlerHelper.GetInCompletedPcdHandlerDetails(tenantId,deviceId);
            if(feedProcessorDetails!=null) {
                /* Set the foreground color to red */
                Console.ForegroundColor=ConsoleColor.Red;
                /* Console.WriteLine(String.Format("Duplicate running instance detected for device [{0}]",deviceId));
                Restore original colors */ 
                Console.ResetColor();
                strMode=Console.ReadLine();
                switch(strMode) {
                    case "1":
                        /* Console.WriteLine("Option 1 is selected"); */
                        PcdHandlerHelper.UpdateFeedDetails(feedProcessorDetails.FeedProcessorMasterId,DateTime.UtcNow.Ticks,PcdHandlerHelper.MARKED_CLOSED);
                        /* Console.WriteLine("Marked the duplicate running instance as closed and exit. Please close the duplicate running instance."); */
                        Environment.Exit(0);
                        break;
                    case "2":
                        /* Console.WriteLine("Option 2 is selected and user has to close the duplicated running instance manually."); */
                        Environment.Exit(0);
                        break;
                    case "3":
                        /* Console.WriteLine("Option 3 is selected and no action is taken."); */
                        Environment.Exit(0);
                        break;
                    default:
                        /* Console.WriteLine("Please select valid mode."); */
                        break;
                }
            }
        } 

        /* Set lot size based on the calculated FPS and FramesToPredictPerSecond which is configured in database (FTP_PERSECOND).
        If FramesToPredictPerSecond is not configured in database or FPS is not calcualted then lot size will be taken from
        FRAMETOPREDICT (from the database). */
        private static void SetFTP() {
            if(PcdHandlerHelper.FramesToPredictPerSecond>0 && PcdHandlerHelper.FPS>0)
                PcdHandlerHelper.lotSizeTemp=(int)(PcdHandlerHelper.FPS/PcdHandlerHelper.FramesToPredictPerSecond);
            else
                PcdHandlerHelper.lotSizeTemp=PcdHandlerHelper.lotSize;
            /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.FramesToPredict,
            FrameGrabberHelper.instanceName,FrameGrabberHelper.lotSizeTemp,true,false);
            LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.FrameCountInLot,
            FrameGrabberHelper.instanceName,FrameGrabberHelper.lotSizeTemp,true,false);
            LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.FPS,
            FrameGrabberHelper.instanceName,(long)FrameGrabberHelper.FPS,true,false);
            Console.WriteLine($"The FPS is {FrameGrabberHelper.FPS}, lotSizeTemp is {FrameGrabberHelper.lotSizeTemp}, FrameCount is {FrameGrabberHelper.FrameCount}"); */
        }

        public static void Dispose() {
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"Dispose","PcdHandler"),LogHandler.Layer.PcdHandler,null);
            #endif
            if(!isDisposed) {
                if(reader!=null) {
                    reader.Dispose();
                    reader=null;
                }
                isDisposed=true;
            }
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End,"Dispose","PcdHandler"),LogHandler.Layer.PcdHandler,null);
            #endif 
        }

        static void StartProcessPcd(string pcdSource) {
            #if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start,"StartProcessPcd","PcdHandler"),
            LogHandler.Layer.PcdHandler,null);
            LogHandler.LogDebug("StartProcessPcd method of PcdHandler started for pcd file: {0}",
            LogHandler.Layer.PcdHandler,pcdSource);
            #endif
            string videoFileName=Path.GetFileName(pcdSource);
            string fileType=Path.GetExtension(pcdSource);
            string requestId=Path.GetFileNameWithoutExtension(pcdSource);
            if(isDBEnabled) {
                /* FeedProcessorMasterMsg feedProcessorMasterMsg=FrameGrabberHelper.GetFeedMasterWithVideoName(imageSource); */
                FeedProcessorMasterMsg feedProcessorMasterMsg=FrameGrabberHelper.GetFeedMasterWithVideoName(requestId);
                /* To check if video is uploaded from Demo Portal */
                if(feedProcessorMasterMsg!=null && feedProcessorMasterMsg.FeedProcessorMasterDetail?.Status==0) {
                    /* Commented because the GetFeedMasterWithVideoName was called twice
                    if(FrameGrabberHelper.IsDeviceInitiated(videoSource))
                        FeedProcessorMasterMsg feedProcessorMasterMsg=FrameGrabberHelper.GetFeedMasterWithVideoName(videoSource); */
                    if(feedProcessorMasterMsg!=null && feedProcessorMasterMsg.FeedProcessorMasterDetail!=null && feedProcessorMasterMsg.FeedProcessorMasterDetail.FeedProcessorMasterId!=0) {
                        var feedProcessorMasterDetail=feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail.Status=PcdHandlerHelper.IN_PROGRESS;
                        feedProcessorMasterDetail.FrameProcessedRate=PcdHandlerHelper.lotSize;
                        PcdHandlerHelper.MasterId=feedProcessorMasterDetail.FeedProcessorMasterId;
                        feedProcessorMasterMsg.FeedProcessorMasterDetail=feedProcessorMasterDetail;
                        PcdHandlerHelper.UpdateAllFeedDetails(feedProcessorMasterMsg);
                        var feedRequest=FrameGrabberHelper.GetFeedRequestWithRequestId(requestId);
                        feedRequest.FeedProcessorMasterId=PcdHandlerHelper.MasterId;
                        feedRequest.Status=ProcessingStatus.inProgressStatus;
                        feedRequest.StartFrameProcessedTime=DateTime.UtcNow;
                        feedRequest.ResourceId=PcdHandlerHelper.deviceId;
                        var status=PcdHandlerHelper.UpdateFeedRequestDetails(feedRequest);
                        PcdHandlerHelper.modelName=feedRequest.Model;
                        Media_MetaData_Msg_Req mediaMetaDataMsgReq=new Media_MetaData_Msg_Req();
                        mediaMetaDataMsgReq.MediaMetadataDetails=new Media_Metadata_Details();
                        mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId=PcdHandlerHelper.MasterId;
                        mediaMetaDataMsgReq.MediaMetadataDetails.RequestId=requestId;
                        PcdHandlerHelper.UpdateMediaMetaData(mediaMetaDataMsgReq);
                    }
                    else {
                        PcdHandlerHelper.MasterId=PcdHandlerHelper.InsertFeedDetails(pcdSource,DateTime.UtcNow.Ticks);
                        LogHandler.LogError("MasterId: {0}",LogHandler.Layer.Business,PcdHandlerHelper.MasterId);
                        MediaMetaDataMsg mediaMetaDataMsgReq;
                        (mediaMetaDataMsgReq,_)=Helper.ExtractVideoMetaData(pcdSource,PcdHandlerHelper.tenantId);
                        PcdHandlerHelper.InsertMediaMetaData(mediaMetaDataMsgReq);
                    }
                }
                else {
                    PcdHandlerHelper.MasterId=PcdHandlerHelper.InsertFeedDetails(pcdSource,DateTime.UtcNow.Ticks);
                }
            }
            else {
                PcdHandlerHelper.MasterId=FrameGrabberHelper.GenerateMaterId();
            }
            PcdHandlerHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.StartOfFile,0,0,0);
            /* In case of vaapi_filename update status as in progress in feed detail and feed request table */
            isUpdated=false;
            PcdHandlerHelper.FrameCount=0;
            string fileName=string.Empty;
            PcdHandlerHelper.TotalFramesGrabbed++;
            int frameNumber=PcdHandlerHelper.TotalFramesGrabbed;
            LtSize="";
            #if DEBUG
            LogHandler.LogDebug("The PcdHandler started processing the pcd file {0} with FTP as {1}",LogHandler.Layer.PcdHandler,pcdSource,PcdHandlerHelper.lotSizeTemp);
            using(LogHandler.TraceOperations("PcdHandler:StartProcessPcd",LogHandler.Layer.PcdHandler,Guid.NewGuid(),null)) {
                #endif
                try {
                    Stime=DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                    Ffp="";
                    Lfp="";
                    PcdHandlerHelper.FrameCount++;
                    if(File.Exists(pcdSource)) {
                        fileName=DateTime.UtcNow.Ticks.ToString();
                        PcdHandlerHelper.TotalMessageSendForPrediction++;
                        PcdHandlerHelper.lastFrameNumberSendForPredict=frameNumber;
                        PcdHandlerHelper.ProcessPcdAsync(pcdSource,fileName,sequenceNumber,frameNumber,Stime,Src,Ffp,LtSize,Lfp,videoFileName);
                    }
                }
                catch(FaceMaskDetectionCriticalException criticalEx) {
                    /* Should crash the application */
                    LogHandler.LogError("Exception thrown in StartProcessPcd method of PcdHandler for pcd file: {0}. Exception message: {1}",
                    LogHandler.Layer.PcdHandler,pcdSource,criticalEx.Message);
                    throw criticalEx;
                }
                catch(Exception ex) {
                    /* Other exceptions */
                    LogHandler.LogError("Exception thrown in StartProcessPcd method of PcdHandler for pcd file: {0}. Exception message: {1}",
                    LogHandler.Layer.PcdHandler,pcdSource,ex.Message);
                    if(PcdHandlerHelper.OtherExceptionCount>PcdHandlerHelper.MaxFailureCount) {
                        /* Breached error limit */
                        throw ex;
                    }
                    PcdHandlerHelper.OtherExceptionCount++;
                }
#if DEBUG
            }
#endif
        }

        #region Program closing event handler code
        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType) {
            /* Console.WriteLine("Inside ConsoleCtrlCheck: "+ctrlType); */
            if(!isUpdated) {
                if(!PcdHandlerHelper.UpdateFeedDetails(PcdHandlerHelper.MasterId,DateTime.UtcNow.Ticks)) {
                    LogHandler.LogError("Failed to update feed details into database on completion of execution at {0} for Device Id: {1} and Tenant Id: {2}",LogHandler.Layer.PcdHandler,DateTime.UtcNow.ToString(),PcdHandlerHelper.deviceId,PcdHandlerHelper.tenantId);
                    /* LogHandler.CollectPerformanceMetric(ApplicationConstants.FGPerfMonCategories.FrameGrabber,ApplicationConstants.FGPerfMonCounters.ErrorCount,
                    FrameGrabberHelper.instanceName,0,false,false); */
                }
            }
            /* LogHandler.InitializeRaw(FrameGrabberHelper.instanceName); */
            switch(ctrlType) {
                case CtrlTypes.CTRL_C_EVENT:
                    isclosing=true;
                    Dispose();
                    /* Console.WriteLine("Inside CTRL_C_EVENT"); */
                    Environment.Exit(0);
                    break;
                case CtrlTypes.CTRL_CLOSE_EVENT:
                    isclosing=true;
                    Dispose();
                    /* Console.WriteLine("Inside CTRL_CLOSE_EVENT"); */
                    Environment.Exit(0);
                    break;
            }
            totalTime+=DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
            #if DEBUG
            LogHandler.LogInfo($"Master Id: {PcdHandlerHelper.MasterId}, Total Time: {totalTime}, Pre Process Time: {preProcessTime}, Queue Insertion Time: {PcdHandlerHelper.pushQTime}, Total Frames In Pcd File: {PcdHandlerHelper.currentVideoTotalFrameCount}, Total Frames Grabbed: {PcdHandlerHelper.TotalFramesGrabbed}, Total Frames Processed: {PcdHandlerHelper.FrameCount}",LogHandler.Layer.PcdHandler,null);
            #endif
            isclosing=true;
            return true;
        }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler,bool Add);

        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        public enum CtrlTypes {
            CTRL_C_EVENT=0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT=5,
            CTRL_SHUTDOWN_EVENT
        }
        #endregion
	}
}
