/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using System;
using System.Collections.Generic;
using static Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.FrameGrabber;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Newtonsoft.Json;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using System.Runtime.Caching;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent
{
    public class PromptHandler : ProcessHandlerBase<QueueEntity.PromptHandlerMetaData>
    {
        static AppSettings appSettings = Config.AppSettings;

        public static string _taskCode;

        public static Dictionary<string, List<string>> targetTaskRoute;

        public PromptHandler() { }

        public PromptHandler(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
            PromptHandlerHelper._taskCode = _taskCode;
            targetTaskRoute = new TaskRoute().GetTaskRouteDetails(appSettings.TenantID.ToString(), appSettings.DeviceID, _taskCode);
        }
        
        private static string Hp;

        public override void Dump(QueueEntity.PromptHandlerMetaData message)
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

        public override bool HandleEventMessage(QueueEntity.MaintenanceMetaData message)
        {
            if (message != null)
            {
                string eventType = message.EventType;
                switch (eventType)
                {
                    case ProcessingStatus.StartOfFile:
                        LogHandler.LogDebug($"Received Start of file event type message", LogHandler.Layer.PromptHandler);
                        GetFrameData(message);
                        sendEventMessage(message);
                        PromptHandlerHelper.GetPrompt();
                        break;
                    case ProcessingStatus.EndOfFile:
                        LogHandler.LogDebug($"Received End of file event type message", LogHandler.Layer.PromptHandler);
                        PromptHandlerHelper.StopBackgroundTask();
                        sendEventMessage(message);
                        break;
                }
            }
            return true;
        }

        public override bool Process(QueueEntity.PromptHandlerMetaData message, int robotId, int runInstanceId, int robotTaskMapId)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "PromptHandler"), LogHandler.Layer.Business, null);
            LogHandler.LogDebug(String.Format("The Process Method of PromptHandler class is getting executed with parameters : " +
                " message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message), robotId, runInstanceId, robotTaskMapId),
                LogHandler.Layer.Business, null);
#endif
            try
            {
                using (LogHandler.TraceOperations("PromptHandler:Process", LogHandler.Layer.PromptHandler, Guid.NewGuid(), null))
                {
                    BE.Queue.FrameProcessorMetaData bereceivedmessage = new BE.Queue.FrameProcessorMetaData()
                    {
                        Fid = message.Fid,
                        Did = message.Did,
                        Sbu = message.Sbu,
                        Tid = message.Tid,
                        TE = targetTaskRoute,
                        FeedId = message.FeedId,
                        Fids = message.Fids,
                        SequenceNumber = message.SequenceNumber,
                        FrameNumber = message.FrameNumber,
                        Stime = message.Stime,
                        Src = message.Src,
                        Etime = message.Etime,
                        Msk_img = message.Msk_img,
                        Rep_img = message.Rep_img,
                        
                        Ffp = message.Ffp,
                        Ltsize = message.Ltsize,
                        Lfp = message.Lfp,
                        videoFileName = message.videoFileName,
                        Prompt = message.Prompt,
                        Hp = !string.IsNullOrEmpty(PromptHandlerHelper.HyperParameters) ? PromptHandlerHelper.HyperParameters : message.Hp
                    };

                    if (!string.IsNullOrEmpty(message.Prompt))
                    {
                        LogHandler.LogDebug("Prompt received : {0}", LogHandler.Layer.PromptHandler, message.Prompt);
                        PromptHandlerHelper.promptText = message.Prompt;
                        PromptHandlerHelper.HyperParameters = message.Hp;
                        return true;
                    }
                    if (!string.IsNullOrEmpty(message.Mod))
                    {
                        if (message.Mod.Contains(_taskCode))
                            message.Mod = JsonConvert.DeserializeObject<Dictionary<string, string>>(message.Mod).GetValueOrDefault(_taskCode);
                        else
                            message.Mod = JsonConvert.DeserializeObject<Dictionary<string, string>>(message.Mod).GetValueOrDefault("default");
                    }
                    bool promptAvailable = PromptHandlerHelper.promptDictionary.TryGetValue(PromptHandlerHelper.feedKey, out string prompt);
                    LogHandler.LogDebug($"prompt being used: {prompt}", LogHandler.Layer.PromptHandler);
                    if (promptAvailable)
                    {
                        LogHandler.LogDebug($"Updated the prompt: {prompt} in the prompthandler message", LogHandler.Layer.PromptHandler);
                        bereceivedmessage.Prompt = prompt;
                    }
                    else
                    {
                        LogHandler.LogError($"no prompt available: {prompt}", LogHandler.Layer.PromptHandler);
                    }
                    LogHandler.LogDebug($"sending message to frameprocessor: {JsonConvert.SerializeObject(bereceivedmessage)}",
                        LogHandler.Layer.PromptHandler);
                    PromptHandlerHelper.SendMessage(bereceivedmessage, bereceivedmessage.Tid, bereceivedmessage.Did, bereceivedmessage.TE);

                    if (message.Lfp == "1")
                    {
                        PromptHandlerHelper.masterId = "";
                        PromptHandlerHelper.promptDictionary[PromptHandlerHelper.feedKey] = "";
                        PromptHandlerHelper.feedKey = "";
                        PromptHandlerHelper.HyperParameters = "";
                        PromptHandlerHelper.StopBackgroundTask();
                    }
                    return true;
                }

            }
            catch (Exception exMP)
            {
                LogHandler.LogError($"Exception occurred in the Process method of PromptHandler class, exception: {exMP.Message}, Inner exception: {exMP.InnerException}, stack trace: {exMP.StackTrace}",
                    LogHandler.Layer.PromptHandler);
                return false;
            }

        }

        private void sendEventMessage(QueueEntity.MaintenanceMetaData message)
        {
            TaskRouteMetadata taskRouteMetadata = new TaskRoute().GetTaskRouteConfig(message.Tid, message.Did);
            var taskList = new TaskRoute().GetTaskRouteDetails(message.Tid, message.Did, _taskCode)[_taskCode];
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    new TaskRoute().SendMessageToQueueWithTask(taskRouteMetadata, _taskCode, message, task);
                }
            }
        }

        private void GetFrameData(QueueEntity.MaintenanceMetaData message)
        {
            LogHandler.LogDebug($"Extracting frame data from event message", LogHandler.Layer.PromptHandler);
            FrameInformation frameInfo = JsonConvert.DeserializeObject<FrameInformation>(message.Data);
            PromptHandlerHelper.tenantId = frameInfo.TID;
            PromptHandlerHelper.deviceId = frameInfo.DID;
            PromptHandlerHelper.masterId = frameInfo.FeedId;
            PromptHandlerHelper.UpdateModelName(frameInfo.Model);
            PromptHandlerHelper.feedKey = PromptHandlerHelper.tenantId + "_" + PromptHandlerHelper.deviceId + "_" + PromptHandlerHelper.masterId;
            LogHandler.LogDebug($"Event data received, tenantId: {frameInfo.TID}, deviceId: {frameInfo.DID}, masterId: {frameInfo.FeedId}, modelName: {frameInfo.Model}, feedKey: {PromptHandlerHelper.feedKey}", LogHandler.Layer.PromptHandler);
        }
    }
}
