/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Emgu.CV;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using System.Drawing;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Configuration;
using System.Collections;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using TR = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;

using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Cryptography;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    public static class FrameGrabber
    {
        static double totalTime = 0;
        static double preProcessTime = 0;
        static double grabTotalTime = 0;
        static double grabCycleTotalTime = 0;
        static double grabinMethodTime = 0;
        static DateTime ppST = DateTime.UtcNow;
        static bool videoCompleted;
        static bool videoGrabCompleted;
        static bool promptsCompleted;

        static VideoCapture reader;
        static VideoCapture liveReader;
        static bool isclosing = false;
        static bool isFirstFrame = true;
        static bool isUpdated;
        static DateTime newTime = new DateTime();
        static DateTime prevTime = new DateTime();
        static double timeDiffToIgnore = 0;
        static int FTPCycle = 0;
        static int previousFTPCycle = (FTPCycle - 1);
        static string calculateFrameGrabberFPR = null;
        static int maxEmptyFrameCount = 10;
        static int emptyFrameWaitTime = 1000;
        static bool isDBEnabled = true;
        static DateTime firstFrameTime = new DateTime();
        static DateTime secondFrameTime = new DateTime();
        static int frameCount = 0;
        static int emptyFrameCount = 0;
        static Boolean isDisposed = false;
        static int nullFrameCount = 0;
        static int nullQueryFrameCount = 0;
        static string LastFrameId = null;
        static DateTime LastFrameGrabbedTime;

        static Queue frameQueue = new Queue();
        static bool canCalculateFrameGrabberFPR = false;
        static int frameGrabSleepTime = 0;
        static int currentGrabberFrame = 0;
        static int sequenceNumber = 0;
        static bool isMemoryDoc = false;
        static int frameGrabRateThrottlingSleepFrameCount = 0;
        static long previousFid = 0;
        static string Stime;
        static string Src = "Grabber";
        static string Ffp = null;
        static string Lfp = null;
        static string LtSize = null;

        static int Fposition;
        static int FirstFrame ;
        static int LastFrame ;
        static string Lotsize;
        static int LFrame;
        static int nextframe = 0;
        static bool imageCompleted;
        static bool imageGrabCompleted;
        public static void ReadFromConfig()
        {
            AppSettings appSettings = Config.AppSettings;
            if (appSettings.EmptyFrameProcessInterval > 0)
            {
                emptyFrameWaitTime = appSettings.EmptyFrameProcessInterval;
            }
            if (appSettings.MaxEmptyFrameCount > 0)
            {
                maxEmptyFrameCount = appSettings.MaxEmptyFrameCount;
            }

            if (appSettings.CalculateFrameGrabberFPR != null)
            {
                calculateFrameGrabberFPR = appSettings.CalculateFrameGrabberFPR;
            }

            if (calculateFrameGrabberFPR != null && calculateFrameGrabberFPR.Equals("yes", StringComparison.InvariantCultureIgnoreCase))
            {
                canCalculateFrameGrabberFPR = true;
            }

            if (appSettings.FrameTimeDifferenceIgnoreThreshold != null)
            {
                timeDiffToIgnore = appSettings.FrameTimeDifferenceIgnoreThreshold;
            }

            if (appSettings.FTPCycle != null)
            {
                FTPCycle = appSettings.FTPCycle;
                previousFTPCycle = (FTPCycle - 1);
            }

            if (appSettings.FrameGrabRateThrottlingSleepDurationMsec != null)
            {
                frameGrabSleepTime = appSettings.FrameGrabRateThrottlingSleepDurationMsec;
            }

            if (appSettings.FrameGrabRateThrottlingSleepFrameCount != null)
            {
                frameGrabRateThrottlingSleepFrameCount = appSettings.FrameGrabRateThrottlingSleepFrameCount;
            }
            if(appSettings.DBEnabled != null)
            {
                isDBEnabled = appSettings.DBEnabled;
            }


        }


        public static void FrameGrabberProcess(bool isConsoleMode)
        {
            TR.TaskRoute taskRouter = new TR.TaskRoute();
            isMemoryDoc = TaskRouteDS.IsMemoryDoc();
            List<string> moduleList = new List<string> { TaskRouteConstants.FrameGrabberCode, TaskRouteConstants.FrameGrabberLotCode, TaskRouteConstants.FrameGrabberUniquePersonCode };
            if (FrameGrabberHelper.displayAllFrames && FrameGrabberHelper.lotsEnabled && TaskRouteDS.IsMemoryDoc())
            {
                throw new FaceMaskDetectionInvalidConfigException("Both DisplayAllFrames and LotsEnable can't be enabled");
            }
            
            if (taskRouter.AllowTaskRouting(FrameGrabberHelper.tenantId.ToString(), FrameGrabberHelper.deviceId, moduleList))
            {
#if DEBUG
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Main", "FrameGrabber"),
                LogHandler.Layer.FrameGrabber, null);
                LogHandler.LogDebug("Frame Grabber Application started for Tenant : {0},\nDevice : {1},\nCamera : {2},\nModel : {3},\nStorage Location : {4}",
                    LogHandler.Layer.FrameGrabber, FrameGrabberHelper.tenantId, FrameGrabberHelper.deviceId, FrameGrabberHelper.cameraURL, FrameGrabberHelper.modelName, FrameGrabberHelper.storageBaseUrl);
#endif

                ReadFromConfig();           
                if (isConsoleMode)
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);
                    }
                    
                    ProcessIncompletedFrameGrabberInstance();
                }

                while (!isclosing)
                {
                    try
                    {
                        switch (FrameGrabberHelper.videoFeedType)
                        {
                            case "OFFLINE":
                                var filesToProcess = FrameGrabberHelper.GetOfflineFileLocations(FrameGrabberHelper.offlineVideoBaseDirectory);
                                foreach (string file in filesToProcess)
                                {
                                    #region re-initialize variable

                                    if (!File.Exists(file))
                                    {
                                      
                                        break;
                                    }
                                    reader = new VideoCapture(file);

                                    FrameGrabberHelper.currentVideoTotalFrameCount  = Convert.ToInt32(reader.Get(Emgu.CV.CvEnum.CapProp.FrameCount));
                                    FrameGrabberHelper.FPS = Convert.ToInt32(reader.Get(Emgu.CV.CvEnum.CapProp.Fps));

                                    #endregion
#if DEBUG  
                                    using (LogHandler.TraceOperations("FrameGrabber:Main for Offline video {0}", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), file))
                                    {
#endif
                                        currentGrabberFrame = 0;
                                        FrameGrabberHelper.TotalFramesGrabbed = 0;
                                        sequenceNumber = 0;
                                        FrameGrabberHelper.lastFrameNumberSendForPredict = 0;
                                        FrameGrabberHelper.TotalMessageSendForPrediction = 0;
                                        SetFTP();
                                        videoGrabCompleted = false;
                                        if (!File.Exists(file))
                                        {
                                            break;
                                        }
                                        CancellationTokenSource cancellationTokenSource = GetFrames();
                                        
                                        preProcessTime += DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
                                        if (!File.Exists(file))
                                        {
                                            cancellationTokenSource.Cancel();
                                            break;
                                        }
                                        if (FrameGrabberHelper.GENAI.ToLower() == "yes")
                                        {
                                           
                                            FrameGrabberHelper.GetPromptsFromFile();
                                            FrameGrabberHelper.GetMaskInputImages(FrameGrabberHelper.maskImageInput, FrameGrabberHelper.maskImageDirectory);
                                            FrameGrabberHelper.GetReplaceInputImages(FrameGrabberHelper.replaceImageInput, FrameGrabberHelper.replaceImageDirectory);
                                        }
                                        StartProcess(file);   

                                        {
                                            LastFrameGrabbedTime = DateTime.UtcNow;
                                            DateTime LastProcessedTime = DateTime.UtcNow;
                                        
                                            if (isDBEnabled)
                                            {
                                                LogHandler.LogDebug("MasterId : {0}", LogHandler.Layer.FrameGrabber, FrameGrabberHelper.MasterId);
                                                var feedRequest = FrameGrabberHelper.GetFeedRequestWithMasterId(FrameGrabberHelper.MasterId);
                                                LogHandler.LogDebug("feed request details : {0}", LogHandler.Layer.FrameGrabber, JsonConvert.SerializeObject(feedRequest));
                                                if (feedRequest != null && feedRequest.RequestId != null)
                                                {
                                                    feedRequest.LastFrameGrabbedTime = LastFrameGrabbedTime;
                                                    feedRequest.LastFrameId = LastFrameId;
                                                    FrameGrabberHelper.UpdateFeedRequestDetails(feedRequest);
                                                }
                                                FeedProcessorMasterMsg feedProcessorMaster = FrameGrabberHelper.GetFeedProcessorMasterWithMasterId(FrameGrabberHelper.MasterId);
                                                feedProcessorMaster.FeedProcessorMasterDetail.Status = ProcessingStatus.feedCompletedStatus;

                                                feedProcessorMaster.FeedProcessorMasterDetail.ProcessingEndTimeTicks = DateTime.UtcNow.Ticks;
                                                if (!isUpdated && FrameGrabberHelper.UpdateAllFeedDetails(feedProcessorMaster))
                                                    isUpdated = true;
                                            }

                                            FrameGrabberHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.EndOfFile,
                                                FrameGrabberHelper.TotalFramesGrabbed, FrameGrabberHelper.lastFrameNumberSendForPredict, FrameGrabberHelper.TotalMessageSendForPrediction);
                                            totalTime += DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
                                            LogHandler.LogInfo($"The Video File : {file} is Processed successfully.\nTotal Time :{totalTime}\nPre Process: {preProcessTime}\nTotal Frame Grab Time: {grabTotalTime}\n Frame Grab method : {grabinMethodTime}\nFrame Grab Cycle Time : {grabCycleTotalTime}\nBlob Insertion Time for Image :{FrameGrabberHelper.blobImageTime}\nBlob processing & Insertion Time for Lot :{FrameGrabberHelper.blobZipTime}\nQ Insertion Time:{FrameGrabberHelper.pushQTime}\nCompress Time:{FrameGrabberHelper.compressTime}\nTotal Frames in video :{FrameGrabberHelper.currentVideoTotalFrameCount}\nTotal Frames Grabbed: {FrameGrabberHelper.TotalFramesGrabbed}\nTotal Frames Processed: {FrameGrabberHelper.FrameCount}\nTotal Lots Processed: {FrameGrabberHelper.totalLotCount}\nTotal Images Processed: {FrameGrabberHelper.totalImgCount}\nTotal time to process Image Task: {FrameGrabberHelper.imageTask}\nTotal time to process Lot tasks: {FrameGrabberHelper.lotTask}", LogHandler.Layer.FrameGrabber, null);
                                        }
                                        reader.Dispose();
                                        FrameGrabberHelper.taskList.Clear();
                                        FrameGrabberHelper.taskList.TrimExcess();
                                        FrameGrabberHelper.PostVideoProcess(file);
#if DEBUG
                                    }
#endif
                                }
                                Thread.Sleep(FrameGrabberHelper.IntervalWaitTime * 1000);
                                break;
                            case "LIVE":
                                if (FrameGrabberHelper.cameraURL.All(char.IsDigit))
                                {
                                    int camera_id = Int32.Parse(FrameGrabberHelper.cameraURL);
                                    reader = new VideoCapture(camera_id);
                                }
                                else
                                {
                                    reader = new VideoCapture(FrameGrabberHelper.cameraURL);
                                }
                                
                                currentGrabberFrame = 0;
                                FrameGrabberHelper.TotalFramesGrabbed = 0;
                                sequenceNumber = 0;
                                FrameGrabberHelper.lastFrameNumberSendForPredict = 0;
                                FrameGrabberHelper.TotalMessageSendForPrediction = 0;
                                GetFrames();
                                if (FrameGrabberHelper.GENAI.ToLower() == "yes")
                                {
                                    FrameGrabberHelper.GetPromptsFromFile();
                                    FrameGrabberHelper.GetMaskInputImages(FrameGrabberHelper.maskImageInput, FrameGrabberHelper.maskImageDirectory);
                                    FrameGrabberHelper.GetReplaceInputImages(FrameGrabberHelper.replaceImageInput, FrameGrabberHelper.replaceImageDirectory);
                                }
                                StartProcess(FrameGrabberHelper.cameraURL); 
                               
                                #region postProcess
                                
                                #endregion
                                break;
                            case "IMAGE":
                                var imageFilesToProcess = FrameGrabberHelper.GetImageFileLocations(FrameGrabberHelper.offlineVideoBaseDirectory);
                                if (imageFilesToProcess.Count() != 0)
                                {
                                    foreach (string file in imageFilesToProcess)
                                    {
                                        #region re-initialize variable

                                        if (!File.Exists(file))
                                        {
                                            break;
                                        }
                                        reader = new VideoCapture(file);

                                        
                                        

                                        FrameGrabberHelper.currentVideoTotalFrameCount = Convert.ToInt32(reader.Get(Emgu.CV.CvEnum.CapProp.FrameCount));
                                        FrameGrabberHelper.FPS = Convert.ToInt32(reader.Get(Emgu.CV.CvEnum.CapProp.Fps));

                                       
                                        #endregion
#if DEBUG
                                        using (LogHandler.TraceOperations("FrameGrabber:Main for Offline video {0}", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), file))
                                        {
#endif
                                            currentGrabberFrame = 0;
                                            FrameGrabberHelper.TotalFramesGrabbed = 0;
                                            sequenceNumber = 0;
                                            FrameGrabberHelper.lastFrameNumberSendForPredict = 0;
                                            FrameGrabberHelper.TotalMessageSendForPrediction = 0;
                                            SetFTP();
                                            videoGrabCompleted = false;
                                            if (!File.Exists(file))
                                            {
                                                break;
                                            }
                                            
                                            try
                                            {

                                                GetFramesImage();
                                            }
                                            catch (Exception ex)
                                            {
                                                throw ex;
                                            }
                                            preProcessTime += DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
                                            if (!File.Exists(file))
                                            {
                                                
                                                break;
                                            }
                                            if (FrameGrabberHelper.GENAI.ToLower() == "yes")
                                            {
                                                
                                                FrameGrabberHelper.GetPromptsFromFile();
                                                FrameGrabberHelper.GetMaskInputImages(FrameGrabberHelper.maskImageInput, FrameGrabberHelper.maskImageDirectory);
                                                FrameGrabberHelper.GetReplaceInputImages(FrameGrabberHelper.replaceImageInput, FrameGrabberHelper.replaceImageDirectory);
                                            }
                                            StartProcessImage(file);  

                                            
                                            {
                                                LastFrameGrabbedTime = DateTime.UtcNow;
                                                DateTime LastProcessedTime = DateTime.UtcNow;
                                                
                                                if (isDBEnabled)
                                                {
                                                    var feedRequest = FrameGrabberHelper.GetFeedRequestWithMasterId(FrameGrabberHelper.MasterId);
                                                    if (feedRequest != null && feedRequest.RequestId != null)
                                                    {
                                                        feedRequest.LastFrameGrabbedTime = LastFrameGrabbedTime;
                                                        feedRequest.LastFrameId = LastFrameId;
                                                        FrameGrabberHelper.UpdateFeedRequestDetails(feedRequest);
                                                    }
                                                    FeedProcessorMasterMsg feedProcessorMaster = FrameGrabberHelper.GetFeedProcessorMasterWithMasterId(FrameGrabberHelper.MasterId);
                                                    feedProcessorMaster.FeedProcessorMasterDetail.Status = ProcessingStatus.feedCompletedStatus;

                                                    feedProcessorMaster.FeedProcessorMasterDetail.ProcessingEndTimeTicks = DateTime.UtcNow.Ticks;
                                                    if (!isUpdated && FrameGrabberHelper.UpdateAllFeedDetails(feedProcessorMaster))
                                                        isUpdated = true;
                                                }

                                                FrameGrabberHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.EndOfFile,
                                                    FrameGrabberHelper.TotalFramesGrabbed, FrameGrabberHelper.lastFrameNumberSendForPredict, FrameGrabberHelper.TotalMessageSendForPrediction);
                                                totalTime += DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
                                                LogHandler.LogInfo($"The Video File : {file} is Processed successfully.\nTotal Time :{totalTime}\nPre Process: {preProcessTime}\nTotal Frame Grab Time: {grabTotalTime}\n Frame Grab method : {grabinMethodTime}\nFrame Grab Cycle Time : {grabCycleTotalTime}\nBlob Insertion Time for Image :{FrameGrabberHelper.blobImageTime}\nBlob processing & Insertion Time for Lot :{FrameGrabberHelper.blobZipTime}\nQ Insertion Time:{FrameGrabberHelper.pushQTime}\nCompress Time:{FrameGrabberHelper.compressTime}\nTotal Frames in video :{FrameGrabberHelper.currentVideoTotalFrameCount}\nTotal Frames Grabbed: {FrameGrabberHelper.TotalFramesGrabbed}\nTotal Frames Processed: {FrameGrabberHelper.FrameCount}\nTotal Lots Processed: {FrameGrabberHelper.totalLotCount}\nTotal Images Processed: {FrameGrabberHelper.totalImgCount}\nTotal time to process Image Task: {FrameGrabberHelper.imageTask}\nTotal time to process Lot tasks: {FrameGrabberHelper.lotTask}", LogHandler.Layer.FrameGrabber, null);
                                            }
                                           
                                            
                                          
                                            
                                            reader.Dispose();
                                            FrameGrabberHelper.taskList.Clear();
                                            FrameGrabberHelper.taskList.TrimExcess();
                                            FrameGrabberHelper.PostVideoProcess(file);
#if DEBUG
                                        }
#endif
                                    }
                                }
                                Thread.Sleep(FrameGrabberHelper.IntervalWaitTime * 1000);
                                break;

                            case "PCD":
                                var pcdFilesToProcess=FrameGrabberHelper.GetPcdFileLocations(FrameGrabberHelper.pcdBaseDirectory);
                                if(pcdFilesToProcess.Count()!=0) {
                                    foreach(string file in pcdFilesToProcess) {
                                        #region re-initialize variable
                                        if(!File.Exists(file)) {
                                            break;
                                        }
                                        FrameGrabberHelper.currentVideoTotalFrameCount=1;
                                        FrameGrabberHelper.FPS=0;
                                        #endregion

                                        #if DEBUG
                                        using(LogHandler.TraceOperations("FrameGrabber: Main for offline video - {0}",LogHandler.Layer.FrameGrabber,Guid.NewGuid(),file)) {
                                            #endif
                                            currentGrabberFrame=0;
                                            FrameGrabberHelper.TotalFramesGrabbed=0;
                                            sequenceNumber=0;
                                            FrameGrabberHelper.lastFrameNumberSendForPredict=0;
                                            FrameGrabberHelper.TotalMessageSendForPrediction=0;
                                            SetFTP();
                                            if(!File.Exists(file)) {
                                                break;
                                            }
                                            preProcessTime+=DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
                                            StartProcessPcd(file); 
                                            
                                            {
                                                LastFrameGrabbedTime=DateTime.UtcNow;
                                                DateTime LastProcessedTime=DateTime.UtcNow;
                                                
                                                if(isDBEnabled) {
                                                    var feedRequest=FrameGrabberHelper.GetFeedRequestWithMasterId(FrameGrabberHelper.MasterId);
                                                    if(feedRequest!=null && feedRequest.RequestId!=null) {
                                                        feedRequest.LastFrameGrabbedTime=LastFrameGrabbedTime;
                                                        feedRequest.LastFrameId=LastFrameId;
                                                        FrameGrabberHelper.UpdateFeedRequestDetails(feedRequest);
                                                    }
                                                    FeedProcessorMasterMsg feedProcessorMaster=FrameGrabberHelper.GetFeedProcessorMasterWithMasterId(FrameGrabberHelper.MasterId);
                                                    feedProcessorMaster.FeedProcessorMasterDetail.Status=ProcessingStatus.feedCompletedStatus;
                                                    feedProcessorMaster.FeedProcessorMasterDetail.ProcessingEndTimeTicks=DateTime.UtcNow.Ticks;
                                                    if(!isUpdated && FrameGrabberHelper.UpdateAllFeedDetails(feedProcessorMaster))
                                                        isUpdated=true;
                                                }
                                                FrameGrabberHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.EndOfFile,
                                                FrameGrabberHelper.TotalFramesGrabbed,FrameGrabberHelper.lastFrameNumberSendForPredict,FrameGrabberHelper.TotalMessageSendForPrediction);
                                                totalTime+=DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
                                                LogHandler.LogInfo($"The pcd file: {file} is processed successfully. Total Time: {totalTime}, Pre Process Time: {preProcessTime}, Total Frame Grab Time: {grabTotalTime}, Frame Grab Method Time: {grabinMethodTime}, Frame Grab Cycle Time: {grabCycleTotalTime}, Blob Image Insertion Time: {FrameGrabberHelper.blobImageTime}, Blob Processing And Insertion Time For Lot: {FrameGrabberHelper.blobZipTime}, Q Insertion Time: {FrameGrabberHelper.pushQTime}, Compress Time: {FrameGrabberHelper.compressTime}, Total Frames In Video: {FrameGrabberHelper.currentVideoTotalFrameCount}, Total Frames Grabbed: {FrameGrabberHelper.TotalFramesGrabbed}, Total Frames Processed: {FrameGrabberHelper.FrameCount}, Total Lots Processed: {FrameGrabberHelper.totalLotCount}, Total Images Processed: {FrameGrabberHelper.totalImgCount}, Total Time To Process Image Task: {FrameGrabberHelper.imageTask}, Total Time To Process Lot Tasks: {FrameGrabberHelper.lotTask}",LogHandler.Layer.FrameGrabber,null);
                                            }
                                            FrameGrabberHelper.taskList.Clear();
                                            FrameGrabberHelper.taskList.TrimExcess();
                                            FrameGrabberHelper.PostVideoProcess(file);
                                            #if DEBUG
                                        }
                                        #endif
                                    }
                                }
                                Thread.Sleep(FrameGrabberHelper.IntervalWaitTime*1000);
                                break;


                            case "PROMPT":
                                
                                StartProcessPrompt(FrameGrabberHelper.offlinePromptDirectory);
                                FrameGrabberHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.EndOfFile,
                                                FrameGrabberHelper.TotalFramesGrabbed, FrameGrabberHelper.lastFrameNumberSendForPredict, FrameGrabberHelper.TotalMessageSendForPrediction);
                                break;

                        }

                    }
                    catch (Exception ex)
                    {
                        if (!isUpdated && FrameGrabberHelper.UpdateFeedDetails(FrameGrabberHelper.MasterId, DateTime.UtcNow.Ticks))
                            isUpdated = true;
                       
                        LogHandler.LogError("Frame Grabber Application Threw an Exception : {0} ,StackTrace : {1}",
                            LogHandler.Layer.FrameGrabber, ex.Message, ex.StackTrace);
                        
                        LogHandler.LogInfo("Frame Grabber Application stopped for Tenant : {0},\nDevice : {1},\nCamera : {2},\nModel : {3},\nStorage Location : {4}",
                            LogHandler.Layer.FrameGrabber, FrameGrabberHelper.tenantId, FrameGrabberHelper.deviceId, FrameGrabberHelper.cameraURL, FrameGrabberHelper.modelName, FrameGrabberHelper.storageBaseUrl);
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
                                if (isConsoleMode)
                                {
                                    Environment.Exit(0);
                                }

                            }
                        }
                        catch (Exception innerEx)
                        {
                            LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "Main", "FrameGrabber"),
                                    LogHandler.Layer.Business, null);
                            
                            if (!failureLogged)
                            {
                                LogHandler.LogDebug(String.Format("Exception Occured while handling an exception. error message: {0}", innerEx.Message), LogHandler.Layer.FrameGrabber, null);
                            }
                            if (isConsoleMode)
                            {
                                Environment.Exit(0);
                            }
                        }

                    }
                }
                if (isclosing)
                {
                    Dispose();
                }
            }
            else
            {
                
                LogHandler.LogError("There is no Task Route for FrameGrabber", LogHandler.Layer.FrameGrabber);
            }
        }

        static void StartProcess(string videoSource)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "StartProcess", "FrameGrabber"),
                LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("StartProcess Method of Frame Grabber started for video {0}",
                LogHandler.Layer.FrameGrabber, videoSource);
#endif
            
            string videoFileName = Path.GetFileName(videoSource);
            string fileType = Path.GetExtension(videoSource);
            string requestId = Path.GetFileNameWithoutExtension(videoSource);
            if (isDBEnabled)
            {
                FeedProcessorMasterMsg feedProcessorMasterMsg = FrameGrabberHelper.GetFeedMasterWithVideoName(videoSource);
                LogHandler.LogDebug("Videosource : {0}", LogHandler.Layer.FrameGrabber, videoSource);
                LogHandler.LogDebug("Feed processor master details for the videosource : {0}", LogHandler.Layer.FrameGrabber, JsonConvert.SerializeObject(feedProcessorMasterMsg));
              
                if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail?.Status == 0)
               
                {
                    
                    if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail != null && feedProcessorMasterMsg.FeedProcessorMasterDetail.FeedProcessorMasterId != 0)
                    {
                        var feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail.Status = FrameGrabberHelper.IN_PROGRESS;
                        feedProcessorMasterDetail.FrameProcessedRate = FrameGrabberHelper.lotSize;
                        FrameGrabberHelper.MasterId = feedProcessorMasterDetail.FeedProcessorMasterId;
                        feedProcessorMasterMsg.FeedProcessorMasterDetail = feedProcessorMasterDetail;
                        FrameGrabberHelper.UpdateAllFeedDetails(feedProcessorMasterMsg);

                        var feedRequest = FrameGrabberHelper.GetFeedRequestWithRequestId(requestId);
                        feedRequest.FeedProcessorMasterId = FrameGrabberHelper.MasterId;
                        feedRequest.Status = ProcessingStatus.inProgressStatus;
                        feedRequest.StartFrameProcessedTime = DateTime.UtcNow;
                        feedRequest.ResourceId = FrameGrabberHelper.deviceId;
                        var status = FrameGrabberHelper.UpdateFeedRequestDetails(feedRequest);
                        FrameGrabberHelper.modelName = feedRequest.Model;
                        Media_MetaData_Msg_Req mediaMetaDataMsgReq = new Media_MetaData_Msg_Req();
                        mediaMetaDataMsgReq.MediaMetadataDetails = new Media_Metadata_Details();
                        mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId = FrameGrabberHelper.MasterId;
                        mediaMetaDataMsgReq.MediaMetadataDetails.RequestId = requestId;
                        FrameGrabberHelper.UpdateMediaMetaData(mediaMetaDataMsgReq);
                    }
                    else
                    {
                        FrameGrabberHelper.MasterId = FrameGrabberHelper.InsertFeedDetails(videoSource, DateTime.UtcNow.Ticks);
                        LogHandler.LogError("MasterId : {0}", LogHandler.Layer.Business, FrameGrabberHelper.MasterId);
                        MediaMetaDataMsg mediaMetaDataMsgReq;
                        (mediaMetaDataMsgReq, _) = Helper.ExtractVideoMetaData(videoSource, FrameGrabberHelper.tenantId);
                        FrameGrabberHelper.InsertMediaMetaData(mediaMetaDataMsgReq);

                    }

                }
                else
                {
                    FrameGrabberHelper.MasterId = FrameGrabberHelper.InsertFeedDetails(videoSource, DateTime.UtcNow.Ticks);
                }
            }
            else
            {
                FrameGrabberHelper.MasterId = FrameGrabberHelper.GenerateMaterId();
            }

            FrameGrabberHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.StartOfFile,0, 0, 0);

            
            isUpdated = false;

            SetFTP(); 

            FrameGrabberHelper.BlobStoreFailureCount = 0;
            FrameGrabberHelper.FrameGrabFailureCount = 0;
            FrameGrabberHelper.PushMessageFailureCount = 0;
            videoCompleted = false;
            videoGrabCompleted = false;
            isFirstFrame = true;
            FrameGrabberHelper.FrameCount = 0;
            int lotNumber = 0;
            bool canProcess = false;

            List<Image<Bgr, Byte>> lotFrames = null;
            List<String> grabberTimeList = null;

            string fileName = string.Empty;
            int seqNumber = 0;
            int currentFrameNumber = 0;
            LtSize = Convert.ToString(FrameGrabberHelper.lotSize);
            if (FrameGrabberHelper.lotSize == 1)
            {
                FirstFrame = 1;
                LastFrame = FrameGrabberHelper.currentVideoTotalFrameCount;
            }
            else if (FrameGrabberHelper.lotSize > 1)
            {
                FirstFrame = 1;
                Fposition = FrameGrabberHelper.lotSize;
                for (int i=1;i< FrameGrabberHelper.currentVideoTotalFrameCount;)
                {

                    if(Fposition < FrameGrabberHelper.currentVideoTotalFrameCount)
                    {
                        nextframe = Fposition + FirstFrame;
                        Fposition += FrameGrabberHelper.lotSize;
                        LFrame = nextframe;
                        i = Fposition;

                        if (Fposition > FrameGrabberHelper.currentVideoTotalFrameCount)
                        {
                            LastFrame = LFrame - FrameGrabberHelper.lotSize;
                        }
                        else if (Fposition == FrameGrabberHelper.currentVideoTotalFrameCount)
                        {
                            LastFrame = LFrame;
                        }
                    }

                }

            }
#if DEBUG
            LogHandler.LogDebug("The Frame Grabber started processing the video {0} with FTP as {1}", LogHandler.Layer.FrameGrabber, videoSource, FrameGrabberHelper.lotSizeTemp);

            using (LogHandler.TraceOperations("FrameGrabber:StartProcess", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif  
                while (!videoCompleted)
                {
                    try
                    {
                        DateTime frameGrabTime;
                        Image<Bgr, Byte> frame = null;

                        FrameMetaData frameMetaData = null;
                        long Fid = 0;

                        
                       
                       

                        if (frameQueue.Count > 0)
                        {
                            frameMetaData = (FrameMetaData)frameQueue.Dequeue();
                            frame = (Image<Bgr, Byte>)frameMetaData.Frame;
                            frameGrabTime = frameMetaData.FrameGrabberTime;
                            Fid = frameMetaData.Fid;
                            LastFrameId = Fid.ToString();
                            canProcess = true;


                            Stime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                            
                        }
                        else if (videoGrabCompleted)
                        {
                            
                            videoCompleted = true;
                        }
                        if (canProcess && frame != null)
                        {
                            if (lotFrames != null)
                            {
                                
                                grabberTimeList.Add(Fid.ToString());
                                lotFrames.Add(frame);
                                lotNumber++;
                            }
                            else
                            {
                                isFirstFrame = true; 
                                fileName = Fid.ToString();
                                currentFrameNumber = frameMetaData.FrameNumber;
                                seqNumber = frameMetaData.SequenceNumber;
                               
                                Ffp = "";
                                Lfp = "";

                                if(FrameGrabberHelper.lotSize ==1 && frameMetaData.FrameNumber == 1)
                                {
                                    Ffp = "1";
                                }
                                else if (FrameGrabberHelper.lotSize == 1 && frameMetaData.FrameNumber == LastFrame)
                                {
                                    Lfp = "1";
                                }
                                
                                if (FrameGrabberHelper.lotSize >1  && frameMetaData.FrameNumber == 1)
                                {
                                    Ffp = "1";
                                }
                                else if (FrameGrabberHelper.lotSize > 1 && frameMetaData.FrameNumber == LastFrame)
                                {
                                    Lfp = "1";
                                }
                                if (FrameGrabberHelper.lotSizeTemp > 1 && !canSkipFrame())
                                {
                                   
                                    lotFrames = new List<Image<Bgr, Byte>>();
                                    grabberTimeList = new List<string>();
                                }
                            }

                            FrameGrabberHelper.FrameCount++;
#if DEBUG
                            LogHandler.LogDebug("StartProcess Method of Frame Grabber and lotSizeTemp value {0}", LogHandler.Layer.FrameGrabber, FrameGrabberHelper.lotSizeTemp);
#endif                            
                            switch (isFirstFrame)
                            {
                                case true:
#if DEBUG
                                    LogHandler.LogDebug("StartProcess Method of Frame Grabber and inside isFirstFrame true and Current Frame {0}", LogHandler.Layer.FrameGrabber, FrameGrabberHelper.TotalFramesGrabbed);
#endif
                                    
                                    fileName = Fid.ToString();
                                    currentFrameNumber = frameMetaData.FrameNumber;
                                    seqNumber = frameMetaData.SequenceNumber;
                                    FrameGrabberHelper.TotalMessageSendForPrediction++;
                                    FrameGrabberHelper.lastFrameNumberSendForPredict = currentFrameNumber;
                                    
                                    FrameGrabberHelper.ProcessImageAsync(frame, fileName, seqNumber, currentFrameNumber, Stime, Src,Ffp, LtSize, Lfp, videoFileName); 
                                    isFirstFrame = false;
                                    break;
                                case false:
#if DEBUG
                                    LogHandler.LogDebug("StartProcess Method of Frame Grabber and inside isFirstFrame false , " +
                                        "value of lotNumber {0} and value of videoCompleted {1}, lotSizeTemp {2} and Current Frame {3}", LogHandler.Layer.FrameGrabber,
                                        lotNumber, videoCompleted, FrameGrabberHelper.lotSizeTemp, FrameGrabberHelper.TotalFramesGrabbed);
#endif
                                    

#if DEBUG
                                    LogHandler.LogDebug("StartProcess Method of Frame Grabber and inside lotNumber check and " +
                                        "value of lotsEnabled {0} and  count of lotFrames {1}",
                                        LogHandler.Layer.FrameGrabber, FrameGrabberHelper.lotsEnabled, lotFrames.Count);
#endif


                                    
                                    if (FrameGrabberHelper.displayAllFrames && TaskRouteDS.IsMemoryDoc())
                                    {

                                        
                                        FrameGrabberHelper.ProcessImageAsync(frame, Fid.ToString(), -1, -1, Stime, Src, Ffp, LtSize, Lfp, videoFileName);


                                    }
                                   
                                    if (lotNumber >= (FrameGrabberHelper.lotSizeTemp - 1) || videoCompleted)
                                    {
#if DEBUG
                                        LogHandler.LogDebug("StartProcess Method of Frame Grabber and inside lotNumber check and " +
                                            "value of lotsEnabled {0} and  count of lotFrames {1}",
                                            LogHandler.Layer.FrameGrabber, FrameGrabberHelper.lotsEnabled, lotFrames.Count);
#endif


                                        if (FrameGrabberHelper.displayAllFrames && TaskRouteDS.IsMemoryDoc())
                                        {
                                            
                                            var taskList = FrameGrabberHelper.taskRouter.GetTaskRouteDetails(FrameGrabberHelper.tenantId.ToString(), FrameGrabberHelper.deviceId, TaskRouteConstants.FrameGrabberCode)[TaskRouteConstants.FrameGrabberCode];
                                            foreach (var task in taskList)
                                            {
                                                var formattedNow = DateTime.UtcNow.ToString("yyyy-MM-dd,HH:mm:ss.fff tt");
                                                byte[] pcdBytes = Array.Empty<byte>();
                                                FrameGrabberHelper.PushToQueues(pcdBytes, fileName, task, grabberTimeList, seqNumber, currentFrameNumber, formattedNow, "Grabber", formattedNow, Ffp, LtSize, Lfp, videoFileName); /* Added datetime.now as static value for pushqueue to check: Yoges Govindaraj */
                                            }

                                        }
                                        else if (FrameGrabberHelper.lotsEnabled)
                                        {
                                            FrameGrabberHelper.ProcessLotAsync(lotFrames, grabberTimeList, fileName, seqNumber, currentFrameNumber);
                                        }
                                      
                                        for (int i = 0; i < lotFrames.Count; i++)
                                        {
                                            Image<Bgr, Byte> frameObj = lotFrames[i];

                                            if (frameObj != null && !(FrameGrabberHelper.displayAllFrames && TaskRouteDS.IsMemoryDoc()))
                                            {
                                                frameObj.Dispose();
                                            }
                                        }
                                        lotNumber = 0;
                                        lotFrames.Clear();
                                        lotFrames.TrimExcess();
                                        lotFrames = null;
                                        grabberTimeList.Clear();
                                        grabberTimeList.TrimExcess();
                                        grabberTimeList = null;
                                        isFirstFrame = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }

                    }
                    catch (FaceMaskDetectionCriticalException criticalEx)
                    {
                        
                        LogHandler.LogError("Exception thrown in StartProcess Method of Frame Grabber for video {0}. Exception message: {1}",
                            LogHandler.Layer.FrameGrabber, videoSource, criticalEx.Message);
                       
                        throw criticalEx;
                    }
                    catch (Exception ex)
                    {
                        
                        LogHandler.LogError("Exception thrown in StartProcess Method of Frame Grabber for video {0}. Exception message: {1}",
                            LogHandler.Layer.FrameGrabber, videoSource, ex.Message);
                        
                        if (FrameGrabberHelper.OtherExceptionCount > FrameGrabberHelper.MaxFailureCount)
                        {
                            
                            throw ex;
                        }
                        FrameGrabberHelper.OtherExceptionCount++;
                        continue;
                    }

                }
#if DEBUG
            }

#endif

        }

       

        private static void ProcessIncompletedFrameGrabberInstance()
        {
            string strMode = null;
            int tenantId = FrameGrabberHelper.tenantId;
            string deviceId = FrameGrabberHelper.deviceId;
            FeedProcessorMasterDetails feedProcessorDetails = FrameGrabberHelper.GetInCompletedFramGrabberDetails(tenantId, deviceId);
            if (feedProcessorDetails != null)
            {
                
                Console.ForegroundColor = ConsoleColor.Red;
                 
                Console.ResetColor();
                
                strMode = Console.ReadLine();
                switch (strMode)
                {
                    case "1":
                        FrameGrabberHelper.UpdateFeedDetails(feedProcessorDetails.FeedProcessorMasterId, DateTime.UtcNow.Ticks, FrameGrabberHelper.MARKED_CLOSED);
                        Environment.Exit(0);
                        break;
                    case "2":
                        Environment.Exit(0);
                        break;
                    case "3":
                        Environment.Exit(0);
                        break;
                    default:
                        break;
                }
            }

        }


        private static CancellationTokenSource GetFrames()
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "getFrames", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            Task frameGrabTask =Task.Run(() =>
            {
                int count = 0;
                int errorCount = 0;
                while (!videoGrabCompleted)
                {
                    try
                    {

#if DEBUG
                        DateTime startTime = DateTime.Now;
#endif

                        FrameMetaData frameMetaData = GrabFrame(out bool status);

#if DEBUG
                        LogHandler.LogDebug("GetFrames Method of TimeTaken for GrabFrame {0} in msec, current frame count {1}",
                            LogHandler.Layer.FrameGrabber, DateTime.Now.Subtract(startTime).TotalMilliseconds, count);
#endif

                        if (status & frameMetaData != null)
                        {

#if DEBUG
                            DateTime processStartTime = DateTime.Now;
#endif
                            frameQueue.Enqueue(frameMetaData);
#if DEBUG
                            LogHandler.LogDebug("GetFrames Method of TimeTaken for Enqueue {0} in msec, time taken till now {1} in msec",
                                LogHandler.Layer.FrameGrabber, DateTime.Now.Subtract(processStartTime).TotalMilliseconds,
                                DateTime.Now.Subtract(startTime).TotalMilliseconds);
#endif
#if DEBUG
                            processStartTime = DateTime.Now;
#endif

                            if (FrameGrabberHelper.videoFeedType == "LIVE")
                            {

                                

                                if (canCalculateFrameGrabberFPR && count == previousFTPCycle)
                                {
#if DEBUG
                                    LogHandler.LogDebug("GetFrames Method and previousFTPCycle FrameCount value {0}",
                                        LogHandler.Layer.FrameGrabber, count);
#endif
                                    count = 0;
                                }

                                switch (count)
                                {
                                    case 0:
                                        prevTime = DateTime.UtcNow;
                                        break;
                                    case 1:
                                        CalculateFPS(count);
                                        break;
                                }

                            }
                            count++;
#if DEBUG

                            LogHandler.LogDebug("GetFrames Method of TimeTaken for FPS calculation {0} in msec, TimeTaken for overall {1} in msec, frame count {2}",
                               LogHandler.Layer.FrameGrabber, DateTime.Now.Subtract(processStartTime).TotalMilliseconds,
                               DateTime.Now.Subtract(startTime).TotalMilliseconds, count);
#endif

                        }
                        if (errorCount > 0)
                        {
                            errorCount = 0;
                        }

                    }
                    catch (Exception ex)
                    {
                        LogHandler.LogError("Exception Occured in GetFrames method error message: {0}, exception trace {1} ",
                            LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                        Thread.Sleep(emptyFrameWaitTime);
                        errorCount++;
                        if (errorCount >= FrameGrabberHelper.MaxFailureCount)
                        {
                            throw ex;
                        }
                    }
                }


            }, tokenSource.Token);
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "GetFrames", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif  
            return tokenSource;
        }
         
        static FrameMetaData GrabFrame(out bool status)
        {
#if DEBUG            
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "GrabFrame", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
            status = true;
            Mat mat = null;


            FrameMetaData frameMetaData = new FrameMetaData();
            try
            {
#if DEBUG
                LogHandler.LogDebug("GrabFrame Method Previous TotalFramesGrabbed {0}",
                    LogHandler.Layer.FrameGrabber, FrameGrabberHelper.TotalFramesGrabbed);
#endif
               
                    mat = reader.QueryFrame();
                
                if (mat != null && !mat.IsEmpty)
                {
                    currentGrabberFrame++;
                    FrameGrabberHelper.TotalFramesGrabbed++;
                    frameMetaData.Frame = mat.ToImage<Bgr, Byte>();
                    mat.Dispose();
                    if (FrameGrabberHelper.maxSequenceNumber > 0 && sequenceNumber == FrameGrabberHelper.maxSequenceNumber)
                    {
                        sequenceNumber = 0;
                    }
                    sequenceNumber++;
                    if (frameGrabSleepTime > 0 && frameGrabRateThrottlingSleepFrameCount > 0 &&
                        (FrameGrabberHelper.TotalFramesGrabbed % frameGrabRateThrottlingSleepFrameCount == 0))
                    {
                        Thread.Sleep(frameGrabSleepTime);
                    }

                    if (currentGrabberFrame == FrameGrabberHelper.lotSizeTemp)
                    {
                        currentGrabberFrame = 0;
                    }
#if DEBUG
#endif
                    if (canSkipFrame() && currentGrabberFrame != 1)
                    {
                        status = false;
                        return null;
                    }
                    LogHandler.LogInfo($"{frameMetaData.FrameNumber} not skipped",LogHandler.Layer.FrameGrabber);


                    frameMetaData.FrameGrabberTime = DateTime.UtcNow;
                    while (previousFid == frameMetaData.FrameGrabberTime.Ticks)
                    {
                        frameMetaData.FrameGrabberTime = DateTime.UtcNow;
                    }
                    frameMetaData.Fid = frameMetaData.FrameGrabberTime.Ticks;
                    frameMetaData.FrameNumber = FrameGrabberHelper.TotalFramesGrabbed;
                    frameMetaData.SequenceNumber = sequenceNumber;
                    previousFid = frameMetaData.Fid;




                    nullFrameCount = 0;
#if DEBUG
                    LogHandler.LogDebug("GrabFrame Method TotalFramesGrabbed {0} and  currentVideoTotalFrameCount {1}",
                        LogHandler.Layer.FrameGrabber, FrameGrabberHelper.TotalFramesGrabbed, FrameGrabberHelper.currentVideoTotalFrameCount);

#endif
                    if (FrameGrabberHelper.videoFeedType == "OFFLINE" && FrameGrabberHelper.TotalFramesGrabbed >= FrameGrabberHelper.currentVideoTotalFrameCount)
                    {
                        videoGrabCompleted = true;
#if DEBUG
                        LogHandler.LogDebug("GrabFrame Method Video is Completed {0} and current Frame Queue Count {1} ",
                            LogHandler.Layer.FrameGrabber, videoCompleted, frameQueue.Count);
#endif

                    }
                   
#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "GrabFrame", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif                    
                    return frameMetaData;
                }

                else
                {
                    status = false;
                    nullFrameCount++;
                   
                    if (maxEmptyFrameCount == nullFrameCount)
                    {
                        nullFrameCount = 0;
                        if (FrameGrabberHelper.videoFeedType == "OFFLINE")
                        {
                            LogHandler.LogError("Frame Grabber grabbed a NULL frame,Empty frame wait time {0}, Max Empty Frame Count {1}, Current Empty Frame count {2} ",
                        LogHandler.Layer.FrameGrabber, emptyFrameWaitTime, maxEmptyFrameCount, nullFrameCount);
                            
                            videoGrabCompleted = true;
                        }
                        else
                        {
                            Thread.Sleep(emptyFrameWaitTime);
                        }

                    }


                }
            }
            catch (FaceMaskDetectionCriticalException ex)
            {
                if (FrameGrabberHelper.FrameGrabFailureCount > FrameGrabberHelper.MaxFailureCount)
                {
                    LogHandler.LogError("Failed to grab the frame. Frame Data is null. Reached Maximum Failure Count {0}", LogHandler.Layer.FrameGrabber, FrameGrabberHelper.MaxFailureCount);
                   
                    FaceMaskDetectionCriticalException exception = new FaceMaskDetectionCriticalException(String.Format("Failed to grab the frame. Frame Data is null. Reached Maximum Failure Count {0}", FrameGrabberHelper.MaxFailureCount), ex);
                    throw exception;
                }
                LogHandler.LogError("Failed to grab the frame. Frame Data is null.", LogHandler.Layer.FrameGrabber, null);
                
                status = false;
                FrameGrabberHelper.FrameGrabFailureCount++;
            }
            catch (Exception ex)
            {
                LogHandler.LogError("Exception Occured in GetFrames method error message: {0}, exception trace {1} ",
                    LogHandler.Layer.Business, ex.Message, ex.StackTrace);
#if DEBUG
#endif
                status = false;
                throw ex;
            }
            finally
            {
                if (mat != null)
                {
                    mat.Dispose();
                    mat = null;
                }
            }
            return null;
        }

        private static void CalculateFPS(int frameNumber)
        {
            newTime = DateTime.UtcNow;
            if (frameNumber != 0)
            {
                var time_diff = newTime.Subtract(prevTime).TotalSeconds;

                var fpsPerFrame = 1 / time_diff;
                
                FrameGrabberHelper.FPS = fpsPerFrame;
                
               


                SetFTP();
#if DEBUG
                LogHandler.LogDebug("CalculateFPS Method of Frame Grabber and  time_diff value {0} and FPS value {1}, Current lot size {2},", LogHandler.Layer.FrameGrabber,
                    time_diff, FrameGrabberHelper.FPS, FrameGrabberHelper.lotSizeTemp);
#endif
            }
            prevTime = newTime;
            

        }

      
        private static void SetFTP()
        {
            if (FrameGrabberHelper.FramesToPredictPerSecond > 0 && FrameGrabberHelper.FPS > 0)
                FrameGrabberHelper.lotSizeTemp = (int)(FrameGrabberHelper.FPS / FrameGrabberHelper.FramesToPredictPerSecond);
            else
                FrameGrabberHelper.lotSizeTemp = FrameGrabberHelper.lotSize;
           
        }


        static byte[] QueryFrame(out bool status)
        {
#if DEBUG            
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "QueryFrame", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
            status = true;
            try
            {
                Mat mat = liveReader.QueryFrame();
                double FPS = Convert.ToInt32(liveReader.Get(Emgu.CV.CvEnum.CapProp.Fps));

#if DEBUG
                LogHandler.LogDebug("QueryFrame Method of Frame Grabber and  RAW Frame Rate {0} ", LogHandler.Layer.FrameGrabber,
                    (long)FPS);
#endif

                if (mat != null && !mat.IsEmpty)
                {
                    byte[] image = mat.ToImage<Bgr, Byte>().ToJpegData();
                    nullQueryFrameCount = 0;

#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "QueryFrame", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif                    
                    return image;
                }
                else
                {
                    status = false;
                    nullQueryFrameCount++;
                    if (maxEmptyFrameCount == nullQueryFrameCount)
                    {
                        nullQueryFrameCount = 0;
                        Thread.Sleep(emptyFrameWaitTime);
                    }
                }
            }
            catch (FaceMaskDetectionCriticalException ex)
            {
                LogHandler.LogError("Failed to Query the frame. Exception {0} and StackTrace {1}", LogHandler.Layer.FrameGrabber, ex.Message, ex.StackTrace);
                status = false;
            }
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "QueryFrame", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif 
            return null;
        }


        public static void Dispose()
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Dispose", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
            if (!isDisposed)
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }

                if (liveReader != null)
                {
                    liveReader.Dispose();
                    liveReader = null;
                }
                frameQueue.Clear();
                isDisposed = true;
            }
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "Dispose", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif  

        }

        private static bool canSkipFrame()
        {
            bool canSkipFrame = false;

            switch (isMemoryDoc)
            {
                
                case true:
                    if (FrameGrabberHelper.lotSizeTemp > 1 && !FrameGrabberHelper.displayAllFrames)
                    {
                        canSkipFrame = true;
                    }
                    break;
                case false:
                    if (FrameGrabberHelper.lotSizeTemp > 1 && !FrameGrabberHelper.lotsEnabled)
                    {
                        canSkipFrame = true;
                    }
                    break;
            }
            return canSkipFrame;
        }

        static void StartProcessPcd(string pcdSource) {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "StartProcess", "FrameGrabber"),
            LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("StartProcess method of FrameGrabber started for image: {0}",
            LogHandler.Layer.FrameGrabber, pcdSource);
#endif
            string videoFileName = Path.GetFileName(pcdSource);
            string fileType = Path.GetExtension(pcdSource);
            string requestId = Path.GetFileNameWithoutExtension(pcdSource);
            if (isDBEnabled)
            {
                /* FeedProcessorMasterMsg feedProcessorMasterMsg=FrameGrabberHelper.GetFeedMasterWithVideoName(imageSource); */
                FeedProcessorMasterMsg feedProcessorMasterMsg = FrameGrabberHelper.GetFeedMasterWithVideoName(requestId);
                /* To check if video is uploaded from Demo Portal */
                if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail?.Status == 0)
                {
                    /* Commented because the GetFeedMasterWithVideoName was called twice
                    if(FrameGrabberHelper.IsDeviceInitiated(videoSource))
                        FeedProcessorMasterMsg feedProcessorMasterMsg=FrameGrabberHelper.GetFeedMasterWithVideoName(videoSource); */
                    if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail != null && feedProcessorMasterMsg.FeedProcessorMasterDetail.FeedProcessorMasterId != 0)
                    {
                        var feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail.Status = FrameGrabberHelper.IN_PROGRESS;
                        feedProcessorMasterDetail.FrameProcessedRate = FrameGrabberHelper.lotSize;
                        FrameGrabberHelper.MasterId = feedProcessorMasterDetail.FeedProcessorMasterId;
                        feedProcessorMasterMsg.FeedProcessorMasterDetail = feedProcessorMasterDetail;
                        FrameGrabberHelper.UpdateAllFeedDetails(feedProcessorMasterMsg);
                        var feedRequest = FrameGrabberHelper.GetFeedRequestWithRequestId(requestId);
                        feedRequest.FeedProcessorMasterId = FrameGrabberHelper.MasterId;
                        feedRequest.Status = ProcessingStatus.inProgressStatus;
                        feedRequest.StartFrameProcessedTime = DateTime.UtcNow;
                        feedRequest.ResourceId = FrameGrabberHelper.deviceId;
                        var status = FrameGrabberHelper.UpdateFeedRequestDetails(feedRequest);
                        FrameGrabberHelper.modelName = feedRequest.Model;
                        Media_MetaData_Msg_Req mediaMetaDataMsgReq = new Media_MetaData_Msg_Req();
                        mediaMetaDataMsgReq.MediaMetadataDetails = new Media_Metadata_Details();
                        mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId = FrameGrabberHelper.MasterId;
                        mediaMetaDataMsgReq.MediaMetadataDetails.RequestId = requestId;
                        FrameGrabberHelper.UpdateMediaMetaData(mediaMetaDataMsgReq);
                    }
                    else
                    {
                        FrameGrabberHelper.MasterId = FrameGrabberHelper.InsertFeedDetails(pcdSource, DateTime.UtcNow.Ticks);
                        LogHandler.LogError("MasterId: {0}", LogHandler.Layer.Business, FrameGrabberHelper.MasterId);
                        MediaMetaDataMsg mediaMetaDataMsgReq;
                        (mediaMetaDataMsgReq, _) = Helper.ExtractVideoMetaData(pcdSource, FrameGrabberHelper.tenantId);
                        FrameGrabberHelper.InsertMediaMetaData(mediaMetaDataMsgReq);

                    }
                }
                else
                {
                    FrameGrabberHelper.MasterId = FrameGrabberHelper.InsertFeedDetails(pcdSource, DateTime.UtcNow.Ticks);
                }
            }
            else {
                FrameGrabberHelper.MasterId=FrameGrabberHelper.GenerateMaterId();
            }
            FrameGrabberHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.StartOfFile, 0, 0, 0);
            /* In case of vaapi_filename update status as in progress in feed detail and feed request table */
            isUpdated=false;
            string fileName=string.Empty;
            int sequenceNumber=0;
            FrameGrabberHelper.TotalFramesGrabbed++;
            int frameNumber=FrameGrabberHelper.TotalFramesGrabbed;
            LtSize="";
#if DEBUG
            LogHandler.LogDebug("The FrameGrabber started processing the video {0} with FTP as {1}", LogHandler.Layer.FrameGrabber, pcdSource, FrameGrabberHelper.lotSizeTemp);
            using (LogHandler.TraceOperations("FrameGrabber:StartProcess", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif
                try {
                    Stime=DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                    Ffp="";
                    Lfp="";
                    if(File.Exists(pcdSource)) {
                        FrameGrabberHelper.TotalMessageSendForPrediction++;
                        FrameGrabberHelper.lastFrameNumberSendForPredict=frameNumber;
                        FrameGrabberHelper.ProcessPcdAsync(pcdSource,fileName,sequenceNumber,frameNumber,Stime,Src,Ffp,LtSize,Lfp,videoFileName);
                    }
                }
                catch(FaceMaskDetectionCriticalException criticalEx) {
                    /* Should crash the application */
                    LogHandler.LogError("Exception thrown in StartProcess method of FrameGrabber for video: {0}. Exception message: {1}",
                    LogHandler.Layer.FrameGrabber,pcdSource,criticalEx.Message);
                    throw criticalEx;
                }
                catch(Exception ex) {
                    /* Other exceptions */
                    LogHandler.LogError("Exception thrown in StartProcess method of FrameGrabber for video: {0}. Exception message: {1}",
                    LogHandler.Layer.FrameGrabber,pcdSource,ex.Message);
                    if(FrameGrabberHelper.OtherExceptionCount>FrameGrabberHelper.MaxFailureCount) {
                        /* Breached error limit */
                        throw ex;
                    }
                    FrameGrabberHelper.OtherExceptionCount++;
                }
            }
        }

        static void StartProcessImage(string imageSource)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "StartProcess", "FrameGrabber"),
                LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("StartProcess Method of Frame Grabber started for image {0}",
                LogHandler.Layer.FrameGrabber, imageSource);
#endif
            
            
            string videoFileName = Path.GetFileName(imageSource);
            string fileType = Path.GetExtension(imageSource);
            string requestId = Path.GetFileNameWithoutExtension(imageSource);
            if (isDBEnabled)
            {
                FeedProcessorMasterMsg feedProcessorMasterMsg = FrameGrabberHelper.GetFeedMasterWithVideoName(requestId);

                
                if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail?.Status == 0)
                
                {
                    if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail != null && feedProcessorMasterMsg.FeedProcessorMasterDetail.FeedProcessorMasterId != 0)
                    {
                        var feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail.Status = FrameGrabberHelper.IN_PROGRESS;
                        feedProcessorMasterDetail.FrameProcessedRate = FrameGrabberHelper.lotSize;
                        FrameGrabberHelper.MasterId = feedProcessorMasterDetail.FeedProcessorMasterId;
                        feedProcessorMasterMsg.FeedProcessorMasterDetail = feedProcessorMasterDetail;
                        FrameGrabberHelper.UpdateAllFeedDetails(feedProcessorMasterMsg);

                        var feedRequest = FrameGrabberHelper.GetFeedRequestWithRequestId(requestId);
                        feedRequest.FeedProcessorMasterId = FrameGrabberHelper.MasterId;
                        feedRequest.Status = ProcessingStatus.inProgressStatus;
                        feedRequest.StartFrameProcessedTime = DateTime.UtcNow;
                        feedRequest.ResourceId = FrameGrabberHelper.deviceId;
                        var status = FrameGrabberHelper.UpdateFeedRequestDetails(feedRequest);
                        FrameGrabberHelper.modelName = feedRequest.Model;
                        Media_MetaData_Msg_Req mediaMetaDataMsgReq = new Media_MetaData_Msg_Req();
                        mediaMetaDataMsgReq.MediaMetadataDetails = new Media_Metadata_Details();
                        mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId = FrameGrabberHelper.MasterId;
                        mediaMetaDataMsgReq.MediaMetadataDetails.RequestId = requestId;
                        FrameGrabberHelper.UpdateMediaMetaData(mediaMetaDataMsgReq);
                    }
                    else
                    {
                        FrameGrabberHelper.MasterId = FrameGrabberHelper.InsertFeedDetails(imageSource, DateTime.UtcNow.Ticks);
                        LogHandler.LogError("MasterId : {0}", LogHandler.Layer.Business, FrameGrabberHelper.MasterId);
                        MediaMetaDataMsg mediaMetaDataMsgReq;
                        (mediaMetaDataMsgReq, _) = Helper.ExtractVideoMetaData(imageSource, FrameGrabberHelper.tenantId);
                        FrameGrabberHelper.InsertMediaMetaData(mediaMetaDataMsgReq);

                    }

                }
                else
                {
                    FrameGrabberHelper.MasterId = FrameGrabberHelper.InsertFeedDetails(imageSource, DateTime.UtcNow.Ticks);
                }
            }
            else
            {
                FrameGrabberHelper.MasterId = FrameGrabberHelper.GenerateMaterId();
            }

            FrameGrabberHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.StartOfFile, 0, 0, 0);

            isUpdated = false;

            SetFTP(); 

          
            FrameGrabberHelper.BlobStoreFailureCount = 0;
            FrameGrabberHelper.FrameGrabFailureCount = 0;
            FrameGrabberHelper.PushMessageFailureCount = 0;
            videoCompleted = false;
            videoGrabCompleted = false;
            isFirstFrame = true;
            FrameGrabberHelper.FrameCount = 0;
            int lotNumber = 0;
            bool canProcess = false;


            string fileName = string.Empty;
            int seqNumber = 0;
            int currentFrameNumber = 0;
            LtSize = "";
           
            
#if DEBUG
            LogHandler.LogDebug("The Frame Grabber started processing the video {0} with FTP as {1}", LogHandler.Layer.FrameGrabber, imageSource, FrameGrabberHelper.lotSizeTemp);

            using (LogHandler.TraceOperations("FrameGrabber:StartProcess", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif  
                while (!videoCompleted)
                {
                    try
                    {
                        DateTime frameGrabTime;
                        Image<Bgr, Byte> frame = null;

                        FrameMetaData frameMetaData = null;
                        long Fid = 0;

                  

                        if (frameQueue.Count > 0)
                        {
                            frameMetaData = (FrameMetaData)frameQueue.Dequeue();
                            frame = (Image<Bgr, Byte>)frameMetaData.Frame;
                            frameGrabTime = frameMetaData.FrameGrabberTime;
                            Fid = frameMetaData.Fid;
                            LastFrameId = Fid.ToString();
                            canProcess = true;


                            Stime = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                           
                        }
                        else if (videoCompleted)
                        {
                          
                            videoCompleted = true;
                       
                        }
                        if (canProcess && frame != null)
                        {
                           
                                
                                fileName = Fid.ToString();
                                currentFrameNumber = frameMetaData.FrameNumber;
                                seqNumber = frameMetaData.SequenceNumber;
                                Ffp = "";
                                Lfp = "";

                               

                            FrameGrabberHelper.FrameCount++;
#if DEBUG
                            LogHandler.LogDebug("StartProcess Method of Frame Grabber and inside isFirstFrame true and Current Frame {0}", LogHandler.Layer.FrameGrabber, FrameGrabberHelper.TotalFramesGrabbed);
#endif
                           
#if DEBUG
                            
#endif
                            switch (isFirstFrame)
                            {
                                case true:
#if DEBUG
                                    LogHandler.LogDebug("StartProcess Method of Frame Grabber and inside isFirstFrame true and Current Frame {0}", LogHandler.Layer.FrameGrabber, FrameGrabberHelper.TotalFramesGrabbed);
#endif
                                    
                                    fileName = Fid.ToString();
                                    currentFrameNumber = frameMetaData.FrameNumber;
                                    seqNumber = frameMetaData.SequenceNumber;
                                    FrameGrabberHelper.TotalMessageSendForPrediction++;
                                    FrameGrabberHelper.lastFrameNumberSendForPredict = currentFrameNumber;
                                    
                                    FrameGrabberHelper.ImageAsync(frame, fileName, seqNumber, currentFrameNumber, Stime, Src, Ffp, LtSize, Lfp, videoFileName); 
                                    isFirstFrame = true;
                                    videoCompleted = true;
                                    break;
#if DEBUG
#endif
                                    

#if DEBUG
                                    
#endif
 

                                default:
                                    break;
                            }
                        }

                    }
                    catch (FaceMaskDetectionCriticalException criticalEx)
                    {
                        LogHandler.LogError("Exception thrown in StartProcess Method of Frame Grabber for video {0}. Exception message: {1}",
                            LogHandler.Layer.FrameGrabber, imageSource, criticalEx.Message);
                      
                        throw criticalEx;
                    }
                    catch (Exception ex)
                    {
                        LogHandler.LogError("Exception thrown in StartProcess Method of Frame Grabber for video {0}. Exception message: {1}",
                            LogHandler.Layer.FrameGrabber, imageSource, ex.Message);
                      
                        if (FrameGrabberHelper.OtherExceptionCount > FrameGrabberHelper.MaxFailureCount)
                        {
                            throw ex;
                        }
                        FrameGrabberHelper.OtherExceptionCount++;
                        continue;
                    }

                }
#if DEBUG
            }

#endif

        }

        static void StartProcessPrompt(string promptLocation)
        {
            string promptFile = Directory.GetFiles(promptLocation).FirstOrDefault();
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "StartProcess", "FrameGrabber"),
                LogHandler.Layer.FrameGrabber, null);
            LogHandler.LogDebug("StartProcess Method of Frame Grabber started for Prompt {0}",
                LogHandler.Layer.FrameGrabber, promptFile);
#endif
            
            string requestId = Path.GetFileNameWithoutExtension(promptFile);
           
            if (isDBEnabled)
            {
                FeedProcessorMasterMsg feedProcessorMasterMsg = FrameGrabberHelper.GetFeedMasterWithVideoName(promptFile);
                
                if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail?.Status == 0)
                
                {
                    if (feedProcessorMasterMsg != null && feedProcessorMasterMsg.FeedProcessorMasterDetail != null && feedProcessorMasterMsg.FeedProcessorMasterDetail.FeedProcessorMasterId != 0)
                    {
                        var feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail = feedProcessorMasterMsg.FeedProcessorMasterDetail;
                        feedProcessorMasterDetail.Status = FrameGrabberHelper.IN_PROGRESS;
                        feedProcessorMasterDetail.FrameProcessedRate = FrameGrabberHelper.lotSize;
                        FrameGrabberHelper.MasterId = feedProcessorMasterDetail.FeedProcessorMasterId;
                        feedProcessorMasterMsg.FeedProcessorMasterDetail = feedProcessorMasterDetail;
                        FrameGrabberHelper.UpdateAllFeedDetails(feedProcessorMasterMsg);

                        var feedRequest = FrameGrabberHelper.GetFeedRequestWithRequestId(requestId);
                        feedRequest.FeedProcessorMasterId = FrameGrabberHelper.MasterId;
                        feedRequest.Status = ProcessingStatus.inProgressStatus;
                        feedRequest.StartFrameProcessedTime = DateTime.UtcNow;
                        feedRequest.ResourceId = FrameGrabberHelper.deviceId;
                        var status = FrameGrabberHelper.UpdateFeedRequestDetails(feedRequest);
                        FrameGrabberHelper.modelName = feedRequest.Model;
                        Media_MetaData_Msg_Req mediaMetaDataMsgReq = new Media_MetaData_Msg_Req();
                        mediaMetaDataMsgReq.MediaMetadataDetails = new Media_Metadata_Details();
                        mediaMetaDataMsgReq.MediaMetadataDetails.FeedProcessorMasterId = FrameGrabberHelper.MasterId;
                        mediaMetaDataMsgReq.MediaMetadataDetails.RequestId = requestId;
                        FrameGrabberHelper.UpdateMediaMetaData(mediaMetaDataMsgReq);
                    }
                    else
                    {
                        FrameGrabberHelper.MasterId = FrameGrabberHelper.InsertFeedDetails(promptFile, DateTime.UtcNow.Ticks);
                        LogHandler.LogError("MasterId : {0}", LogHandler.Layer.Business, FrameGrabberHelper.MasterId);
                        MediaMetaDataMsg mediaMetaDataMsgReq;
                        (mediaMetaDataMsgReq, _) = Helper.ExtractVideoMetaData(promptFile, FrameGrabberHelper.tenantId);
                        FrameGrabberHelper.InsertMediaMetaData(mediaMetaDataMsgReq);
                    }

                }
                else
                {
                    FrameGrabberHelper.MasterId = FrameGrabberHelper.InsertFeedDetails(promptFile, DateTime.UtcNow.Ticks);
                }
            }
            else
            {
                FrameGrabberHelper.MasterId = FrameGrabberHelper.GenerateMaterId();
            }

            FrameGrabberHelper.sendEventMessage(ApplicationConstants.ProcessingStatus.StartOfFile, 0, 0, 0);

            isUpdated = false;

            FrameGrabberHelper.BlobStoreFailureCount = 0;
            FrameGrabberHelper.FrameGrabFailureCount = 0;
            FrameGrabberHelper.PushMessageFailureCount = 0;
            promptsCompleted = false;
            videoGrabCompleted = false;
            FrameGrabberHelper.FrameCount = 0;
            int lotNumber = 0;
            bool canProcess = false;
            string fileName = string.Empty;
            int seqNumber = 0;
            int currentFrameNumber = 0;
#if DEBUG
            LogHandler.LogDebug("The Frame Grabber started processing the video {0} with FTP as {1}", LogHandler.Layer.FrameGrabber, "", FrameGrabberHelper.lotSizeTemp);

            using (LogHandler.TraceOperations("FrameGrabber:StartProcess", LogHandler.Layer.FrameGrabber, Guid.NewGuid(), null))
            {
#endif  
                while (!promptsCompleted)
                {
                    try
                    {
                        promptFile = Directory.GetFiles(promptLocation).FirstOrDefault();
                        if(!string.IsNullOrEmpty(promptFile))
                        {
                            FrameGrabberHelper.GetPromptsFromFile();
                            byte[] pcdBytes = Array.Empty<byte>();
                            FrameGrabberHelper.PushToQueues(pcdBytes,"", TaskRouteConstants.GenerativeAI, null, sequenceNumber, 0, DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt"), "Grabber", DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt"), "", "", "", "");
                            break;
                        }
                        
                    }
                    catch (FaceMaskDetectionCriticalException criticalEx)
                    {
                        throw criticalEx;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
#if DEBUG
            }

#endif

        }

        private static CancellationTokenSource GetFramesImage()
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "getFrames", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif
            CancellationTokenSource tokenSource = new CancellationTokenSource();
          
                int count = 0;
                int errorCount = 0;
                while (!videoGrabCompleted)
                {
                    try
                    {

#if DEBUG
                        DateTime startTime = DateTime.Now;
#endif

                        FrameMetaData frameMetaData = GrabFrame(out bool status);

#if DEBUG
                        LogHandler.LogDebug("GetFrames Method of TimeTaken for GrabFrame {0} in msec, current frame count {1}",
                            LogHandler.Layer.FrameGrabber, DateTime.Now.Subtract(startTime).TotalMilliseconds, count);
#endif

                        if (status & frameMetaData != null)
                        {

#if DEBUG
                            DateTime processStartTime = DateTime.Now;
#endif
                            frameQueue.Enqueue(frameMetaData);
#if DEBUG
                            LogHandler.LogDebug("GetFrames Method of TimeTaken for Enqueue {0} in msec, time taken till now {1} in msec",
                                LogHandler.Layer.FrameGrabber, DateTime.Now.Subtract(processStartTime).TotalMilliseconds,
                                DateTime.Now.Subtract(startTime).TotalMilliseconds);
#endif
#if DEBUG
                            processStartTime = DateTime.Now;
#endif

                            if (FrameGrabberHelper.videoFeedType == "LIVE")
                            {

                                

                                if (canCalculateFrameGrabberFPR && count == previousFTPCycle)
                                {
#if DEBUG
                                    LogHandler.LogDebug("GetFrames Method and previousFTPCycle FrameCount value {0}",
                                        LogHandler.Layer.FrameGrabber, count);
#endif
                                    count = 0;
                                }

                                switch (count)
                                {
                                    case 0:
                                        prevTime = DateTime.UtcNow;
                                        break;
                                    case 1:
                                        CalculateFPS(count);
                                        break;
                                }

                            }
                            count++;
#if DEBUG

                            LogHandler.LogDebug("GetFrames Method of TimeTaken for FPS calculation {0} in msec, TimeTaken for overall {1} in msec, frame count {2}",
                               LogHandler.Layer.FrameGrabber, DateTime.Now.Subtract(processStartTime).TotalMilliseconds,
                               DateTime.Now.Subtract(startTime).TotalMilliseconds, count);
#endif

                        }
                        if (errorCount > 0)
                        {
                            errorCount = 0;
                        }
                    videoGrabCompleted = true;
                    }
                    catch (Exception ex)
                    {
                        LogHandler.LogError("Exception Occured in GetFrames method error message: {0}, exception trace {1} ",
                            LogHandler.Layer.Business, ex.Message, ex.StackTrace);
                        Thread.Sleep(emptyFrameWaitTime);
                        errorCount++;
                        if (errorCount >= FrameGrabberHelper.MaxFailureCount)
                        {
                            throw ex;
                        }
                    }
                }


            
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "GetFrames", "FrameGrabber"), LogHandler.Layer.FrameGrabber, null);
#endif  
            return tokenSource;
        }

        #region Program closing event handler code

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            

            if (!isUpdated)
            {
                if (!FrameGrabberHelper.UpdateFeedDetails(FrameGrabberHelper.MasterId, DateTime.UtcNow.Ticks))
                {
                    LogHandler.LogError("Failed to Update feed details into database on completion of execution as {0} for Device ID :{1} and Tenant ID:{2}", LogHandler.Layer.FrameGrabber, DateTime.UtcNow.ToString(), FrameGrabberHelper.deviceId, FrameGrabberHelper.tenantId);
                    
                }
            }
           


            switch (ctrlType)
            {
                case CtrlTypes.CTRL_C_EVENT:
                    isclosing = true;
                    Dispose();
                    
                    Environment.Exit(0);
                    break;
                case CtrlTypes.CTRL_CLOSE_EVENT:
                    isclosing = true;
                    Dispose();
                   
                    Environment.Exit(0);
                    break;
            }
          

            totalTime += DateTime.UtcNow.Subtract(ppST).TotalMilliseconds;
#if DEBUG
            LogHandler.LogInfo($"Master ID = {FrameGrabberHelper.MasterId}\nTotal Time :{totalTime}\nPre Process: {preProcessTime}\nTotal Frame Grab Time: {grabTotalTime}\n Frame Grab method : {grabinMethodTime}\nFrame Grab Cycle Time : {grabCycleTotalTime}\nBlob Insertion Time for Image :{FrameGrabberHelper.blobImageTime}\nBlob processing & Insertion Time for Lot :{FrameGrabberHelper.blobZipTime}\nQ Insertion Time:{FrameGrabberHelper.pushQTime}\nCompress Time:{FrameGrabberHelper.compressTime}\nTotal Frames in video :{FrameGrabberHelper.currentVideoTotalFrameCount}\nTotal Frames Grabbed: {FrameGrabberHelper.TotalFramesGrabbed}\nTotal Frames Processed: {FrameGrabberHelper.FrameCount}\nTotal Lots Processed: {FrameGrabberHelper.totalLotCount}\nTotal Images Processed: {FrameGrabberHelper.totalImgCount}\nTotal time to process Image Task: {FrameGrabberHelper.imageTask}\nTotal time to process Lot tasks: {FrameGrabberHelper.lotTask}", LogHandler.Layer.FrameGrabber, null);
#endif
            isclosing = true;
            return true;
        }

        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }
        #endregion





	}
}
