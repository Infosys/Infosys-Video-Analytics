/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Table;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using System;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Elastic.CommonSchema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Net.WebRequestMethods;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using Nest;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    internal class PromptHandlerProcess : ProcessHandlerBase<QueueEntity.GenerativeAIMetaData>
    {
        string counterInstanceName = "";
        static string dummyString = "";

        TaskRoute taskRouter = new TaskRoute();
        public static bool clientStatus = true;

        int exceptionCount = 0;
        static int exceptionCountLimit = 0;
        static double tokenCacheExpirationTime = 0.0;
        private static AppSettings appSettings = Config.AppSettings;
        public override void Dump(QueueEntity.GenerativeAIMetaData message)
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

        public override bool Process(QueueEntity.GenerativeAIMetaData message, int robotId, int runInstanceId, int robotTaskMapId)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "FrameGrabberProcess"), LogHandler.Layer.Business, null);
            LogHandler.LogDebug(String.Format("The Process Method of FrameGrabberProcess class is getting executed with parameters : " +
                " message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", message, robotId, runInstanceId, robotTaskMapId),
                LogHandler.Layer.Business, null);
#endif
            try
            {
                using (LogHandler.TraceOperations("GenerativeAI:Process", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    
                    BE.Queue.FrameProcessorMetaData bereceivedmessage = new BE.Queue.FrameProcessorMetaData()
                    {
                        Fid = message.Fid,
                        Did = message.Did,
                        Sbu = message.Sbu,
                        Tid = message.Tid,
                        Mod = message.Mod,
                        TE = taskRouter.GetTaskRouteDetails(message.Tid.ToString(), message.Did, TaskRouteConstants.FrameProcessorCode),
                        FeedId = message.FeedId,
                        Fids = message.Fids,
                        SequenceNumber = message.SequenceNumber,
                        FrameNumber = message.FrameNumber,
                        Stime = message.Stime,
                        Src = message.Src,
                        Etime = message.Etime,
                        Msk_img = message.Msk_img,
                        
                        Ffp = message.Ffp,
                        Ltsize = message.Ltsize,
                        Lfp = message.Lfp,
                        videoFileName = message.videoFileName,
                    };

                    bereceivedmessage.Prompt = FormatPrompts(message.Prompt);  

                    if (message.Tid != null && message.Did != null)
                    {
                        if (message.TE[TaskRouteConstants.GenerativeAI].Contains(TaskRouteConstants.FrameProcessorCode))
                        {
                            string response = taskRouter.SendMessageToQueue(bereceivedmessage.Tid.ToString(), bereceivedmessage.Did, TaskRouteConstants.FrameProcessorCode, bereceivedmessage);
                        }
                    }

                    return true;
                }

            }
            catch (Exception exMP)
            {
                return false;
            }

        }



        public static List<List<string>> FormatPrompts(string promptData)
        {
            if (!string.IsNullOrEmpty(promptData))
            {
                promptData = promptData.Replace("\r", string.Empty);
                List<string> list = promptData.Split('\n').ToList<string>();
                List<List<string>> prompt = new List<List<string>>();
                
                prompt.Add(list);
               
                return prompt;
            }
            return null;
        }

        private void sendMessage(BE.Queue.FrameProcessorMetaData deReceivedPersonCountMessage)
        {
            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(deReceivedPersonCountMessage.Tid, deReceivedPersonCountMessage.Did);
            List<string> taskList = deReceivedPersonCountMessage.TE[TaskRouteConstants.GenerativeAI];
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    Dictionary<string, List<string>> te = new Dictionary<string, List<string>>();
                    te = taskRouter.GetTaskRouteDetails(deReceivedPersonCountMessage.Tid, deReceivedPersonCountMessage.Did, task);
                    deReceivedPersonCountMessage.TE = te;
                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, TaskRouteConstants.GenerativeAI, deReceivedPersonCountMessage, task);
                }
            }
        }

        private void sendEventMessage(QueueEntity.MaintenanceMetaData message)
        {
            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(message.Tid, message.Did);
            var taskList = taskRouter.GetTaskRouteDetails(message.Tid, message.Did, TaskRouteConstants.GenerativeAI)[TaskRouteConstants.GenerativeAI];
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, TaskRouteConstants.GenerativeAI, message, task);
                }
            }
        }
    }
}
