/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using System;
using System.Collections;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;

using System.Configuration;
using Infosys.Solutions.Ainauto.VideoAnalytics.AIModels;
using Newtonsoft.Json;
using System.Runtime.Caching;
using System.Collections.Generic;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;

using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using System.Threading;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Nest;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class PersonTracking : ProcessHandlerBase<QueueEntity.PersonCountMetaData>
    {
        static string cacheKey = CacheConstants.UniquePersonCode + FrameGrabberHelper.tenantId + FrameGrabberHelper.deviceId;
        int exceptionCount = 0;
        static int exceptionCount_threshold = 0;
        static ObjectCache cache = MemoryCache.Default;
        CacheItemPolicy policy = new CacheItemPolicy();
        CacheItemPolicy framePolicy = new CacheItemPolicy();
        double cacheExpiration = 30.0;
        
        static Queue personCountQueue = (Queue)cache.Get(cacheKey);
        TaskRoute taskRouter = new TaskRoute();
        DeviceDetails deviceDetails;
        AppSettings appSettings = Config.AppSettings;

        public string _taskCode;
        public PersonTracking() { }
        public PersonTracking(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
            deviceDetails = ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(), appSettings.DeviceID, "PT");
        }

        public override void Dump(QueueEntity.PersonCountMetaData message)
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
                exceptionCount_threshold = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["ExceptionCount"]);
            }

        }


        public override bool Process(QueueEntity.PersonCountMetaData message, int robotId, int runInstanceId, int robotTaskMapId)
        {
           
            try
            {
                using (LogHandler.TraceOperations("PersonTracking:Process", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    if (!message.TE.ContainsKey(TaskRouteConstants.UniquePersonCode))
                    {
                        LogHandler.LogError("Message is not processed in PersonTracking for FrameId = {0} ,TenantId = {1}, deviceId = {2} , module = {3}", LogHandler.Layer.Business, message.Fid, message.Tid, message.Did, TaskRouteConstants.UniquePersonCode);
                        return true;
                    }
                    BE.Queue.PersonCountMetaData beReceivedMessage = new BE.Queue.PersonCountMetaData();
                    beReceivedMessage = BE.FaceMaskTranslator.PersonCountDEToBE(message);
                    DeviceDetails deviceDetails = ConfigHelper.SetDeviceDetails(beReceivedMessage.Tid, beReceivedMessage.Did, "PT");

                    if (beReceivedMessage.Fid != null & beReceivedMessage.Fid != "")
                    {
                        
                        ModelParameters maskDetection = new ModelParameters();
                        maskDetection.ModelName = beReceivedMessage.Mod;
                      
                        maskDetection.deviceId = beReceivedMessage.Did;
                        maskDetection.tId = beReceivedMessage.Tid;
                        maskDetection.Fid = beReceivedMessage.Fid;
                        maskDetection.Stime = beReceivedMessage.Stime;
                        maskDetection.Src = beReceivedMessage.Src;
                        maskDetection.Etime = beReceivedMessage.Etime;
                        maskDetection.Ts = DateTime.UtcNow.ToString("yyy-MM-dd,HH:mm:ss.fff tt");
                        
                        maskDetection.Ts_ntp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                        maskDetection.Msg_ver = deviceDetails.MsgVersion;
                        maskDetection.Inf_ver = deviceDetails.InfVersion;
                        maskDetection.Per = ""; 
                        maskDetection.Ad = beReceivedMessage.Ad; 
                        maskDetection.ModelName = beReceivedMessage.Mod;
                        maskDetection.ConfidenceThreshold = deviceDetails.ConfidenceThreshold;
                        maskDetection.OverlapThreshold = deviceDetails.OverlapThreshold;
                        maskDetection.FrameNumber = long.TryParse(beReceivedMessage.FrameNumber, out long value) == true ? value : 0;
                        maskDetection.Ffp = beReceivedMessage.Ffp;
                        maskDetection.Lfp = beReceivedMessage.Lfp;
                        maskDetection.Ltsize = beReceivedMessage.Ltsize;
                        maskDetection.videoFileName = beReceivedMessage.videoFileName;
                        maskDetection.Prompt = beReceivedMessage.Prompt;
                        maskDetection.Hp = beReceivedMessage.Hp;
                       
                        string predictedMetadata = "";

                        framePolicy.SlidingExpiration = TimeSpan.FromMinutes(cacheExpiration);

                       
                       
                        PersonCountPayload payload = new PersonCountPayload();
                        
                        personCountQueue = (Queue)cache[cacheKey];
                        if (personCountQueue == null)
                        {
                            personCountQueue = new Queue();
                            cache.Set(cacheKey, personCountQueue, framePolicy);
                          
                        }

                        #region  Commenting old request part for testing new response structure
                        /*
                       if (personCountQueue.Count > 0)
                       {
                           BE.Queue.PersonCount[] boundingboxblank = new BE.Queue.PersonCount[personCountQueue.Count];
                           int i = 0;
                           foreach (BE.Queue.PersonCount[] ele in personCountQueue)
                           {
                               boundingboxblank[i] = ele[0];
                               i = i + 1;
                           }
                           payload.Tid = message.Tid;
                           payload.Did = message.Did;
                           payload.Fid = message.Fid;
                           
                           payload.Cs = deviceDetails.SimilarityThreshold;
                           payload.Per = boundingboxblank;
                       }
                       else
                       {
                           var boundingboxblank = JsonConvert.DeserializeObject<BE.Queue.PersonCount[]>("[]");
                           payload.Tid = message.Tid;
                           payload.Did = message.Did;
                           payload.Fid = message.Fid;
                          
                           payload.Cs = deviceDetails.SimilarityThreshold;
                           payload.Per = boundingboxblank;

                       }
                       var input = JsonConvert.SerializeObject(payload);
                       */
                        #endregion


                       
                        var blobImage = VideoAnalytics.BusinessComponent.Helper.DownloadBlob(beReceivedMessage.Did, beReceivedMessage.Fid, beReceivedMessage.Tid, deviceDetails.StorageBaseUrl, ".jpg");
                        if (deviceDetails.SharedBlobStorage)
                        {
                            int messageStucktime = deviceDetails.FrameSequencingMessageStuckDuration;
                            int messageRetry = deviceDetails.FrameSequencingMessageRetry;
                           
                            if (blobImage == null)
                            {
                                for (int i = 0; i <= messageRetry; i++)
                                {
                                    Thread.Sleep(messageStucktime);
                                    blobImage = VideoAnalytics.BusinessComponent.Helper.DownloadBlob(beReceivedMessage.Did, beReceivedMessage.Fid, beReceivedMessage.Tid, deviceDetails.StorageBaseUrl, ".jpg");
                                    if (blobImage != null)
                                    {
                                        break;
                                    }
                                }
                            }


                            if (blobImage != null)
                            {

                                
                                predictedMetadata = ModelInferenceManager.ModelInference(maskDetection, blobImage.File, "");
                            }                           
                        }


                        else
                        {
                            
                            predictedMetadata = ModelInferenceManager.ModelInference(maskDetection, blobImage.File, "");

                        }

                        ObjectDetectorAPIResMsg responseCheck = JsonConvert.DeserializeObject<ObjectDetectorAPIResMsg>(predictedMetadata);
                        Per pobj = new Per();
                        pobj.Fid = responseCheck.Fid;
                        pobj.Fs = new CartPredictions[responseCheck.Fs.Count];
                        for (var i = 0; i < responseCheck.Fs.Count; i++)
                        {

                            pobj.Fs[i] = new();

                            if (responseCheck.Fs[i].Dm.X != null || responseCheck.Fs[i].Dm.Y != null || responseCheck.Fs[i].Dm.W != null || responseCheck.Fs[i].Dm.H != null)
                            {
                                pobj.Fs[i].Dm = responseCheck.Fs[i].Dm;
                                pobj.Fs[i].Dm.X = responseCheck.Fs[i].Dm.X;
                                pobj.Fs[i].Dm.Y = responseCheck.Fs[i].Dm.Y;
                                pobj.Fs[i].Dm.W = responseCheck.Fs[i].Dm.W;
                                pobj.Fs[i].Dm.H = responseCheck.Fs[i].Dm.H;
                            }

                           
                            if (responseCheck.Fs[i].Kp != null)
                            {
                                pobj.Fs[i].Kp = responseCheck.Fs[i].Kp;
                            }

                            pobj.Fs[i].Info = responseCheck.Fs[i].Info;
                            pobj.Fs[i].Cs = responseCheck.Fs[i].Cs;
                            pobj.Fs[i].NoObj = responseCheck.Fs[i].Nobj;
                            pobj.Fs[i].Uid = responseCheck.Fs[i].Uid;
                            pobj.Fs[i].Lb = responseCheck.Fs[i].Lb;
                            pobj.Fs[i].Np = responseCheck.Fs[i].Np;
                            pobj.Fs[i].TaskType = responseCheck.Fs[i].TaskType;
                        }


                      
                        if (responseCheck.Rc == 200)
                        {
                            
                            var queueItem = JsonConvert.SerializeObject(pobj);
                            if (personCountQueue.Count == int.Parse(deviceDetails.PreviousFrameCount))
                            {
                                personCountQueue.Dequeue();
                            }
                      
                            personCountQueue.Enqueue(queueItem);
                            cache.Set(cacheKey, personCountQueue, framePolicy);
                            if (pobj.Fs[0].Np!=null)
                            {
                               
                                cache.Remove(cacheKey);
                            }
                        
                            DE.Queue.FrameRendererMetadata deReceivedFrameRendererMessage = new DE.Queue.FrameRendererMetadata();
                           
                            deReceivedFrameRendererMessage = BE.FaceMaskTranslator.PersonCountBEToDE(predictedMetadata, message);
                            
                            sendMessage(deReceivedFrameRendererMessage);

                            



                        }
                    }

                    return true;
                }
            }
            catch (Exception exMP)
            {
                LogHandler.LogError("Exception in PersonTracking : {0}", LogHandler.Layer.Business, exMP.Message);
                DE.Queue.FrameRendererMetadata deReceivedFrameRendererMessage = BE.FaceMaskTranslator.PersonCountFailureBEToDE(message);
                sendMessage(deReceivedFrameRendererMessage, message);

                LogHandler.LogDebug(String.Format("Exception occured in Process method of PersonTracking class"), LogHandler.Layer.Business, null);
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
                        
                        if (exceptionCount == exceptionCount_threshold)
                            return false;
                        else return true;
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "Process", "PersonTracking"), LogHandler.Layer.Business, null);
                    
                    if (!failureLogged)
                    {
                        LogHandler.LogDebug(String.Format("Exception Occured while handling an exception. error message: {0}", ex.Message), LogHandler.Layer.Business, null);
                    }

                    if (exceptionCount == exceptionCount_threshold)
                        return false;
                    else return true;
                }
            }
        }

        private void sendMessage(DE.Queue.FrameRendererMetadata deReceivedFrameRendererMessage, QueueEntity.PersonCountMetaData message)
        {
            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(deReceivedFrameRendererMessage.Tid, deReceivedFrameRendererMessage.Did);
            List<string> taskList = message.TE[TaskRouteConstants.UniquePersonCode];
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    Dictionary<string, List<string>> te = new Dictionary<string, List<string>>();
                    te = taskRouter.GetTaskRouteDetails(deReceivedFrameRendererMessage.Tid, deReceivedFrameRendererMessage.Did, task);
                    deReceivedFrameRendererMessage.TE = te;
                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, TaskRouteConstants.UniquePersonCode, deReceivedFrameRendererMessage, task);
                }
            }
        }

        private void sendMessage(DE.Queue.FrameRendererMetadata deReceivedFrameRendererMessage)
        {
            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(deReceivedFrameRendererMessage.Tid, deReceivedFrameRendererMessage.Did);
            List<string> taskList = deReceivedFrameRendererMessage.TE[TaskRouteConstants.UniquePersonCode];
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    Dictionary<string, List<string>> te = new Dictionary<string, List<string>>();
                    te = taskRouter.GetTaskRouteDetails(deReceivedFrameRendererMessage.Tid, deReceivedFrameRendererMessage.Did, task);
                    deReceivedFrameRendererMessage.TE = te;
                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, TaskRouteConstants.UniquePersonCode, deReceivedFrameRendererMessage, task);
                }
            }
        }

        private void sendEventMessage(QueueEntity.MaintenanceMetaData message)
        {
            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(message.Tid, message.Did);
            var taskList = taskRouter.GetTaskRouteDetails(message.Tid, message.Did, TaskRouteConstants.UniquePersonCode)[TaskRouteConstants.UniquePersonCode];
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, TaskRouteConstants.UniquePersonCode, message, task);
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
