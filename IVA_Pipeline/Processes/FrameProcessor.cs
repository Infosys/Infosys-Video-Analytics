/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;

using System.Configuration;

using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using System.Diagnostics;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using System.Runtime.Caching;
using System.Collections.Generic;

using Infosys.Solutions.Ainauto.VideoAnalytics.AIModels;
using System.IO;
using System.Drawing;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using Infosys.Solutions.Ainauto.VideoAnalytics.AIModels;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class FrameProcessor : ProcessHandlerBase<QueueEntity.FrameProcessorMetaData>
    {
        string counterInstanceName = "";
        static string dummyString = "";
        TaskRoute taskRouter = new TaskRoute();
        public FrameProcessor()
        {
                        
        }
        int exceptionCount = 0;
        static int exceptionCountLimit = 0;
        static double tokenCacheExpirationTime = 0.0;
        private static AppSettings appSettings = Config.AppSettings;

        public override void Dump(QueueEntity.FrameProcessorMetaData message)
        {

        }
        public override bool Initialize(QueueEntity.MaintenanceMetaData message)
        {

            if (message == null)
            {
                ReadFromConfig();

            }
            else
            {
                try
                {
                    if (message.EventType != null)
                    {
                        var eventList = message.EventType.Split(',');

                        for (var i = 0; i < eventList.Length; i++)
                        {
                            switch (eventList[i])
                            {
                                case "reload_config":
                                    ReadFromConfig();
                                    break;
                                case "cache_cleanup":
                                    if (message.ResourceId != null)
                                    {
                                        var resourceIdList = message.ResourceId.Split(',');
                                        CacheCleanUp(resourceIdList);
                                    }
                                    else
                                    {
                                        LogHandler.LogError("ResourceId is  null in maintenance message : {0}", LogHandler.Layer.Business, JsonConvert.SerializeObject(message));
                                    }

                                    break;

                            }
                        }
                    }
                    else
                    {
                        LogHandler.LogError("EventType is  null in maintenance message : {0}", LogHandler.Layer.Business, JsonConvert.SerializeObject(message));
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception in Initialize method of FramePreloaderMetadata : {0} ", LogHandler.Layer.Business, ex.Message);
                    return false;
                }


            }
            return true;
        }
        private void CacheCleanUp(string[] resourceIdList)
        {
            ObjectCache cache = MemoryCache.Default;
            for (var j = 0; j < resourceIdList.Length; j++)
            {
                if (cache.Contains(resourceIdList[j]))
                {
                    cache.Remove(resourceIdList[j]);

                }

            }

        }


        private void ReadFromConfig()
        {
            if (ConfigurationManager.AppSettings["ExceptionCount"] != null)
            {
                exceptionCountLimit = int.Parse(ConfigurationManager.AppSettings["ExceptionCount"]);

            }
            if (ConfigurationManager.AppSettings["TokenCacheExpirationTime"] != null)
            {
                tokenCacheExpirationTime = double.Parse(ConfigurationManager.AppSettings["TokenCacheExpirationTime"]);

            }

        }

        public override bool Process(QueueEntity.FrameProcessorMetaData message, int robotId, int runInstanceId, int robotTaskMapId)
        {
            
            counterInstanceName = message.Tid + "_" + message.Did;
#if DEBUG
            
            LogHandler.LogDebug("counterInstanceName in frameProcessor: {0}", LogHandler.Layer.Business, counterInstanceName);
            
            LogHandler.LogUsage(String.Format("The Process Method of FrameProcessor class is getting executed with parameters : FrameProcessor message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}; at {4}", JsonConvert.SerializeObject(message), robotId, runInstanceId, robotTaskMapId, DateTime.UtcNow.ToLongTimeString()), null);
#endif
            try
            {
                using (LogHandler.TraceOperations("FrameProcessor:Process", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    
                    BE.Queue.FrameProcessorMetaData beReceivedMessage = new BE.Queue.FrameProcessorMetaData();

                    beReceivedMessage = BE.FaceMaskTranslator.FaceMaskDEToBE(message);
                    if (!message.TE.ContainsKey(Infrastructure.Common.TaskRouteConstants.FrameProcessorCode))
                    {
                        LogHandler.LogError("Message is not processed in FrameProcessor for FrameId = {0} ,TenantId = {1}, deviceId = {2} , module = {3},message = {4}", LogHandler.Layer.Business, message.Fid, message.Tid, message.Did, TaskRouteConstants.FrameProcessorCode, JsonConvert.SerializeObject(message));
                        return true;
                    }
                    
                    DeviceDetails deviceDetails = ConfigHelper.SetDeviceDetails(beReceivedMessage.Tid, beReceivedMessage.Did, CacheConstants.FrameProcessorCode);
                    
                    processData(beReceivedMessage, deviceDetails, message, tokenCacheExpirationTime);
                    



                    return true;

                }
            }
            catch (Exception exMP)
            {
                
                DE.Queue.FrameRendererMetadata frameRendererMetadata = BE.FaceMaskTranslator.FaceMaskRendererBEToDE(dummyString, message);
                frameRendererMetadata.Status = ApplicationConstants.ProcessingStatus.FailedToPredict;
                sendMessage(frameRendererMetadata);

                LogHandler.LogDebug(String.Format("Exception occured in Process method of FrameProcessor class"), LogHandler.Layer.Business, null);
                bool failureLogged = false;
                exceptionCount++;
                
                try
                {
                    Exception ex = new Exception();
                    bool rethrow = ExceptionHandler.HandleException(exMP, ApplicationConstants.WORKER_EXCEPTION_HANDLING_POLICY, out ex);
                    failureLogged = true;

                    if (rethrow)
                    {
                        throw ex;
                    }
                    else
                    {
                        
                        if (exceptionCount == exceptionCountLimit)
                            return false;
                        else return true;
                    }
                }
                catch (Exception ex)
                {
                    
                    if (!failureLogged)
                    {
                        LogHandler.LogDebug(String.Format("Exception Occured while handling an exception. error message: {0}", ex.Message), LogHandler.Layer.Business, null);
                    }

                    if (exceptionCount == exceptionCountLimit)
                        return false;
                    else return true;
                }
            }
        }

        public void processData(BE.Queue.FrameProcessorMetaData beReceivedMessage, DeviceDetails deviceDetails, QueueEntity.FrameProcessorMetaData message, double tokenCacheExpirationTime)
        {
            

            try
            {
                Stopwatch processStopWatch = new Stopwatch();
                Stopwatch predictionStopWatch = new Stopwatch();
                predictionStopWatch.Reset();
                processStopWatch.Reset();
                processStopWatch.Start();
                int messageStucktime = deviceDetails.FrameSequencingMessageStuckDuration;
                int messageRetry = deviceDetails.FrameSequencingMessageRetry;
                
                DE.Document.Workflow blobImage = null;
                if (beReceivedMessage.Pcd == null || beReceivedMessage.Pcd.Length == 0)
                {
                    blobImage = BusinessComponent.Helper.DownloadBlob(beReceivedMessage.Did, beReceivedMessage.Fid, beReceivedMessage.Tid, beReceivedMessage.Sbu, ".jpg");
                }
                if (blobImage == null && (beReceivedMessage.Pcd == null || beReceivedMessage.Pcd.Length == 0))
                {
                    for (int i = 0; i <= messageRetry; i++)
                    {
                        Thread.Sleep(messageStucktime);
                        blobImage = BusinessComponent.Helper.DownloadBlob(beReceivedMessage.Did, beReceivedMessage.Fid, beReceivedMessage.Tid, beReceivedMessage.Sbu, ".jpg");
                        if (blobImage != null)
                        {
                            break;
                        }
                    }
                }


                if (blobImage != null || beReceivedMessage.Prompt != null || beReceivedMessage.Pcd.Length != 0)
                {
                    

                    ModelParameters maskDetection = new ModelParameters();
                    
                    maskDetection.deviceId = beReceivedMessage.Did;
                    maskDetection.tId = beReceivedMessage.Tid;
                    maskDetection.Fid = beReceivedMessage.Fid;
                    maskDetection.Stime = beReceivedMessage.Stime;
                    maskDetection.Src = beReceivedMessage.Src;
                    maskDetection.Etime = beReceivedMessage.Etime;
                    
                    maskDetection.Ts = DateTime.UtcNow.ToString("yyyy-MM-dd,HH:mm:ss.fff tt");
                    
                    maskDetection.Ts_ntp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                    maskDetection.Msg_ver = deviceDetails.MsgVersion;
                    maskDetection.Inf_ver = deviceDetails.InfVersion;
                    maskDetection.Per = ""; 
                    maskDetection.Ad = ""; 
                    maskDetection.ModelName = beReceivedMessage.Mod;
                    maskDetection.ConfidenceThreshold = deviceDetails.ConfidenceThreshold;
                    maskDetection.OverlapThreshold = deviceDetails.OverlapThreshold;
                    maskDetection.TokenCacheExpirationTime = tokenCacheExpirationTime;

                    maskDetection.Ffp = beReceivedMessage.Ffp;
                    maskDetection.Lfp = beReceivedMessage.Lfp;
                    maskDetection.Ltsize = beReceivedMessage.Ltsize;
                    maskDetection.FrameNumber = Convert.ToInt32(beReceivedMessage.FrameNumber);
                    maskDetection.videoFileName = beReceivedMessage.videoFileName;

                    maskDetection.Msk_img = beReceivedMessage.Msk_img;
                    maskDetection.Rep_img = beReceivedMessage.Rep_img;
                    maskDetection.Prompt = beReceivedMessage.Prompt;
                    maskDetection.Pcd = beReceivedMessage.Pcd;
                    predictionStopWatch.Start();
                    
                    string predictedMetadata = string.Empty;

                    if (!String.IsNullOrEmpty(appSettings.ImageDebugEnabled) && appSettings.ImageDebugEnabled.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!String.IsNullOrEmpty(appSettings.FpDebugImageFilePath) && Directory.Exists(appSettings.FpDebugImageFilePath))
                        {
                            Image image = Image.FromStream(blobImage.File);
                            image.Save(appSettings.FpDebugImageFilePath + beReceivedMessage.Fid + ".jpg");
                        }
                    }
                        Stream st = null;
                        if (blobImage != null)
                        {
                            st = blobImage.File;
                        }
                        predictedMetadata = ModelInferenceManager.ModelInference(maskDetection, st);
                        
                        if (blobImage != null)
                        {
                            blobImage.File.Dispose();
                            blobImage.File = null;
                        }
                        if (predictedMetadata != null)
                        {
#if DEBUG
                            
#endif
                            
                            DE.Queue.FrameRendererMetadata deReceivedPersonCountMessage = new DE.Queue.FrameRendererMetadata();
                            deReceivedPersonCountMessage = BE.FaceMaskTranslator.FaceMaskRendererBEToDE(predictedMetadata, message);
                            

                            
                            
                            sendMessage(deReceivedPersonCountMessage);
                           

                            
                            int maskCount = 0;
                            int nomaskCount = 0;
                            
                            var deserializeOutput = JsonConvert.DeserializeObject<Resource.Entity.Queue.Predictions>(predictedMetadata);
                            if (deserializeOutput != null)
                            {
                                

                                /**/
                                if (!string.IsNullOrEmpty(deserializeOutput.Lb) && deserializeOutput.Lb.StartsWith("Mask"))
                                {
                                    maskCount = maskCount + 1;
                                }
                                else
                                {
                                    nomaskCount = nomaskCount + 1;
                                }

                                int totalObjectDetected = maskCount + nomaskCount;
                                if (totalObjectDetected == 0)
                                {
                                    
                                }
                                else
                                {
                                    
                                }
                            }

                            LogHandler.LogUsage(String.Format("The Mask and NoMask count is : TenantId={0}; DeviceId={1}; FrameId={2}; MaskCount={3}; NoMaskCount={4}", deReceivedPersonCountMessage.Tid, deReceivedPersonCountMessage.Did, deReceivedPersonCountMessage.Fid, maskCount, nomaskCount), null);


                            
                            LogHandler.LogUsage(String.Format("The Process Method of FrameProcessor class finished execution with parameters : FrameProcessor message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}; at {4}", JsonConvert.SerializeObject(message), robotId, runInstanceId, robotTaskMapId, DateTime.UtcNow.ToLongTimeString()), null);
#if DEBUG
                            
#endif
                            processStopWatch.Stop();
                            

                        }
                        else
                        {
                            
                        }

                    }

            }
            catch (Exception ex)
            {
                
                LogHandler.LogError(String.Format("Exception Occured in Thread Task.Run of frame processor error message: {0}, exception trace {1} ", ex.Message, ex.StackTrace), LogHandler.Layer.Business, null);
                throw;
            }
        }

        private void sendMessage(DE.Queue.FrameRendererMetadata deReceivedPersonCountMessage)
        {
            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(deReceivedPersonCountMessage.Tid, deReceivedPersonCountMessage.Did);
            List<string> taskList = deReceivedPersonCountMessage.TE[TaskRouteConstants.FrameProcessorCode];
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    Dictionary<string, List<string>> te = new Dictionary<string, List<string>>();
                    te = taskRouter.GetTaskRouteDetails(deReceivedPersonCountMessage.Tid, deReceivedPersonCountMessage.Did, task);
                    deReceivedPersonCountMessage.TE = te;
                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, TaskRouteConstants.FrameProcessorCode, deReceivedPersonCountMessage, task);
                }
            }
        }

        private void sendEventMessage(QueueEntity.MaintenanceMetaData message)
        {
            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(message.Tid, message.Did);
            var taskList = taskRouter.GetTaskRouteDetails(message.Tid, message.Did, TaskRouteConstants.FrameProcessorCode)[TaskRouteConstants.FrameProcessorCode];
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, TaskRouteConstants.FrameProcessorCode, message, task);
                }
            }
        }

        public override bool HandleEventMessage(QueueEntity.MaintenanceMetaData message)
        {
            if (message != null)
            {
                string eventType = message.EventType;
                switch (eventType)
                {
                    case ProcessingStatus.StartOfFile:
                        sendEventMessage(message);
                        break;
                    case ProcessingStatus.EndOfFile:
                        sendEventMessage(message);
                        break;
                }
            }
            return true;
        }
    }
}
