/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.Caching;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using TR = Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Table;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class PromptHandlerProcess : ProcessHandlerBase<TableDetails>
    {
        string counterInstanceName = "";
        static string dummyString = "";
        static bool initiateEventMessage;

        
        TR.TaskRoute taskRouter = new TR.TaskRoute();
        Dictionary<string, List<string>> taskRoute;
        int exceptionCount = 0;
        static int exceptionCountLimit = 0;
        static double tokenCacheExpirationTime = 0.0;

        public string _taskCode;
        public PromptHandlerProcess()
        {

        }
        public PromptHandlerProcess(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
        }

        public override void Dump(TableDetails message)
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
                                        LogHandler.LogError("ResourceId is  null in maintenance message : {0}", LogHandler.Layer.PromptHandler, JsonConvert.SerializeObject(message));
                                    }

                                    break;

                            }
                        }
                    }
                    else
                    {
                        LogHandler.LogError("EventType is  null in maintenance message : {0}", LogHandler.Layer.PromptHandler, JsonConvert.SerializeObject(message));
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError("Exception in Initialize method of MaintenanceMetadata of PromptHandlerProcess : {0} ", LogHandler.Layer.PromptHandler, ex.Message);
                    return false;
                }


            }
            return true;
        }

        public override bool Process(TableDetails message, int robotId, int runInstanceId, int robotTaskMapId)
        {
#if DEBUG
            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "PromptHandlerProcess"), LogHandler.Layer.PromptHandler, null);
            LogHandler.LogDebug(String.Format("The Process Method of PromptHandlerProcess class is getting executed with parameters : " +
                " message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", message, robotId, runInstanceId, robotTaskMapId),
                LogHandler.Layer.PromptHandler, null);
#endif
            try
            {
                using (LogHandler.TraceOperations("PromptHandlerProcess:Process", LogHandler.Layer.PromptHandler, Guid.NewGuid(), null))
                {
                    
                    return true;
                }
            }
            catch (Exception exMP)
            {

                LogHandler.LogError("Exception in PromptHandlerProcess : {0},stack trace : {1}", LogHandler.Layer.PromptHandler, exMP.Message, exMP.StackTrace);
                bool failureLogged = false;

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
                        
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "Process", "PromptHandlerProcess"),
                            LogHandler.Layer.PromptHandler, null);
                    
                    if (!failureLogged)
                    {
                        LogHandler.LogError(String.Format("Exception Occured while handling an exception in PromptHandlerProcess in Process method. error message: {0}", ex.Message), LogHandler.Layer.PromptHandler, null);
                    }

                    return false;
                }
            }

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
    }
}
