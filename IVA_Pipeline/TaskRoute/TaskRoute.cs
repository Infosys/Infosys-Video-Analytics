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
        public static TaskRouteMetadata TaskRouteMetaData {
            get
            {
                if (_taskRouteMetaData == null)
                {
                    _taskRouteMetaData = new TaskRoute().GetTaskRouteConfig(Config.AppSettings.TenantID.ToString(), Config.AppSettings.DeviceID);
                }
                return _taskRouteMetaData;
            }
        }

        
        public Boolean AllowTaskRouting(string tenantId, string deviceId, List<string> moduleList)
        {
            Boolean res = false;
            TaskRouteMetadata taskRouteMetadata = GetTaskRouteConfig(tenantId, deviceId);
            foreach (string module in moduleList)
            {
                if (taskRouteMetadata.TasksRoute?.SelectToken(module) != null)
                {
                    res = true;

                }
            }

            return res;
        }

        
        public Dictionary<string, List<string>> GetTaskRouteDetails(string tenantId, string deviceId, string module)
        {
            TaskRouteMetadata taskRouteMetadata = GetTaskRouteConfig(tenantId, deviceId);
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

       
        public string SendMessageToQueue<T>(string tenantId, string deviceId, string module, T message)
        {
            TaskRouteDS taskRouteDS = new TaskRouteDS();
            string msgResponse = string.Empty;
            
#if DEBUG
            using (LogHandler.TraceOperations("TaskRouterHelper:SendMessageToQueue", LogHandler.Layer.Business, Guid.NewGuid()))
            {
                LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "SendMessageToQueue", "TaskRouterHelper"), LogHandler.Layer.Business, null);
#endif
                TaskRouteMetadata taskRouteMetadata = GetTaskRouteConfig(tenantId, deviceId);
               
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


       

        public TaskRouteMetadata GetTaskRouteConfig(string tenantId, string deviceId)
        {
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
                    DeviceDetails deviceDetails = ConfigHelper.SetDeviceDetails(tenantId, deviceId, CacheConstants.TaskRouteKey);
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

        public static string GetTaskCode(string taskCode)
        {
            Dictionary<string, string> taskRoute = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(TaskRouteMetaData.TransportRegionCodes));
            foreach(string key in taskRoute.Keys)
            {
                if(taskRoute.GetValueOrDefault(key) == taskCode)
                {
                    return key;
                }
            }
            return "";
        }

    }
}
