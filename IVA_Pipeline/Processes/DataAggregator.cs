/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class DataAggregator : ProcessHandlerBase<QueueEntity.FrameRendererMetadata>
    {
        static AppSettings appSettings = new AppSettings();
        static DeviceDetails deviceDetails = new DeviceDetails();
        static int routes;
        string _taskCode;
        static Dictionary<string, List<FrameRendererMetadata>> messageAggregator = new Dictionary<string, List<FrameRendererMetadata>>();
        static TaskRoute taskRouter = new TaskRoute();

        public DataAggregator() { }
        public DataAggregator(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
            Dictionary<string, object> taskRouteInfo = TaskRoute.TaskRouteMetaData.TasksRoute.ToObject<Dictionary<string, object>>();
            routes = taskRouteInfo.Values.Count(value => value.ToString().Contains(TaskRouteConstants.DataAggregatorCode));
        }

        public override void Dump(QueueEntity.FrameRendererMetadata message)
        {
            
        }

        public override bool Initialize(MaintenanceMetaData message)
        {
            if (message == null)
            {
                ReadFromConfig();
                messageAggregator.Clear();
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
                                    messageAggregator.Clear();
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
            appSettings = Config.AppSettings;
            deviceDetails = ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(), appSettings.DeviceID, TaskRouteConstants.DataAggregatorCode);
        }

        public override bool Process(QueueEntity.FrameRendererMetadata message, int robotId, int runInstanceId, int robotTaskMapId)
        {
            try
            {
                if (!message.TE.ContainsKey(_taskCode))
                {
                    LogHandler.LogError("Message is not processed in DataAggregator for frameId={0}, tenantId={1}, deviceId={2}, module={3}, message={4}", LogHandler.Layer.Business, message.Fid, message.Tid, message.Did, _taskCode, JsonConvert.SerializeObject(message));
                    return true;
                }
                
                LogHandler.LogDebug("Message with Fid: {0} received to DataAggregator: {1}", LogHandler.Layer.DataAggregator, message.Fid, JsonConvert.SerializeObject(message));
                bool exist = messageAggregator.TryGetValue(message.Fid, out List<FrameRendererMetadata> messageToAggregate);
                if (!exist)
                {
                    messageToAggregate = new List<FrameRendererMetadata>();
                    messageAggregator.Add(message.Fid, messageToAggregate);
                }
                messageToAggregate.Add(message);
                if (messageToAggregate.Count == routes)
                {
                    FrameRendererMetadata agrMessage = AggregateMessages(messageToAggregate);
                    sendMessage(agrMessage, agrMessage.Tid, agrMessage.Did, agrMessage.TE);
                    messageAggregator.Remove(message.Fid);
                }
            }
            catch(Exception ex)
            {
                LogHandler.LogError("Error in Process method of DataAggregator, exception: {0}\ninner exception: {1}\nstack trace: {2}\n\n",
                    LogHandler.Layer.DataAggregator, ex.Message, ex.InnerException, ex.StackTrace);
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

        private void sendEventMessage(QueueEntity.MaintenanceMetaData message)
        {
            TaskRouteMetadata taskRouteMetadata = taskRouter.GetTaskRouteConfig(message.Tid, message.Did);
            var taskList = taskRouter.GetTaskRouteDetails(message.Tid, message.Did, _taskCode)[_taskCode];
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    taskRouter.SendMessageToQueueWithTask(taskRouteMetadata, _taskCode, message, task);
                }
            }
        }

        public FrameRendererMetadata AggregateMessages(List<FrameRendererMetadata> msgs)
        {
            int count = 0;
            for(int i = 0; i < msgs.Count; i++)
            {
                count += msgs[i].Fs == null ? 0 : msgs[i].Fs.Length ;
            }
            FrameRendererMetadata agrMessage = msgs.FirstOrDefault();
            Predictions[] predictions = new Predictions[count];
            int k = 0;
            for(int i = 0; i < msgs.Count; i++)
            {
                if (msgs[i].Fs == null)
                {
                    continue;
                }
                for (int j = 0; j < msgs[i].Fs.Length; j++)
                {
                    predictions[k] = new();
                    predictions[k].TaskType = msgs[i].Fs[j].TaskType;
                    if (msgs[i].Fs[j].Dm != null)
                    {
                        if (msgs[i].Fs[j].Dm.X != null && msgs[i].Fs[j].Dm.Y != null && msgs[i].Fs[j].Dm.W != null && msgs[i].Fs[j].Dm.H != null)
                        {
                            predictions[k].Dm = msgs[i].Fs[j].Dm;
                            /* Map other properties */
                            predictions[k].Dm.X = msgs[i].Fs[j].Dm.X;
                            predictions[k].Dm.Y = msgs[i].Fs[j].Dm.Y;
                            predictions[k].Dm.W = msgs[i].Fs[j].Dm.W;
                            predictions[k].Dm.H = msgs[i].Fs[j].Dm.H;
                        }
                    }
                    /* Add Kp value */
                    if (msgs[i].Fs[j].Kp != null)
                    {
                        predictions[k].Kp = msgs[i].Fs[j].Kp;
                    }
                    if (msgs[i].Fs[j].Tpc != null)
                    {
                        predictions[k].Tpc = msgs[i].Fs[j].Tpc;
                    }
                    if (msgs[i].Fs[j].Bpc != null)
                    {
                        predictions[k].Bpc = msgs[i].Fs[j].Bpc;
                    }
                    predictions[k].Info = msgs[i].Fs[j].Info;
                    predictions[k].Cs = msgs[i].Fs[j].Cs;
                    predictions[k].NoObj = msgs[i].Fs[j].NoObj;
                    predictions[k].Uid = msgs[i].Fs[j].Uid;
                    predictions[k].Lb = msgs[i].Fs[j].Lb;
                    k++;
                }
            }

            agrMessage.Fs = predictions;

            return agrMessage;
        }

        private bool sendMessage<T>(T deReceivedPersonCountMessage, string tenantId, string deviceId, Dictionary<string, List<string>> TE)
        {
            List<string> taskList = TE[_taskCode];
            Dictionary<string, List<string>> te = new Dictionary<string, List<string>>();
            bool result = false;
            if (taskList != null)
            {
                foreach (var task in taskList)
                {
                    var type = deReceivedPersonCountMessage.GetType();
                    var property = type.GetProperty("TE");
                    te = taskRouter.GetTaskRouteDetails(tenantId, deviceId, task);
                    property.SetValue(deReceivedPersonCountMessage, te);
                    string response = taskRouter.SendMessageToQueueWithTask(TaskRoute.TaskRouteMetaData, _taskCode, deReceivedPersonCountMessage, task);
                    if (response.Contains("Success"))
                    {
                        result = true;
                    }
                }
            }
            return result;
        }
    }
}
