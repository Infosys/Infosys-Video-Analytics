/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Translator;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System.Net.Http;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using System.Diagnostics;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using System.Configuration;
using System.Runtime.Caching;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class MetricIngestor : ProcessHandlerBase<QueueEntity.MetricIngestorMetadata>
    {
        public string _taskCode;
        public MetricIngestor() { }
        public MetricIngestor(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
        }
        public override void Dump(QueueEntity.MetricIngestorMetadata message)
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
                    LogHandler.LogError("Exception in Initialize method of metricIngestor : {0} ", LogHandler.Layer.Business, ex.Message);
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
                cache.Remove(resourceIdList[j]);
            }

        }


        private void ReadFromConfig()
        {
            if (ConfigurationManager.AppSettings["ApplicationName"] != null)
            {
               
            }
            if (ConfigurationManager.AppSettings["EventType"] != null)
            {
               
            }
            if (ConfigurationManager.AppSettings["metricIngestorJobName"] != null)
            {
                
            }
            if (ConfigurationManager.AppSettings["BlobService"] != null)
            {
               
            }
            
        }


        public override bool Process(QueueEntity.MetricIngestorMetadata message, int robotId, int runInstanceId, int robotTaskMapId)
        {
            string counterInstanceName = message.Tid + "_" + message.Did;
            Stopwatch processStopWatch = new Stopwatch();
            TaskRoute taskRouter = new TaskRoute();
#if DEBUG
            LogHandler.LogDebug("counterInstanceName in frameProcessor: {0}", LogHandler.Layer.Business, counterInstanceName);

            LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_Start, "Process", "MetricIngestor"), LogHandler.Layer.Business, null);
            LogHandler.LogDebug(String.Format("The Process Method of MetricIngestor class is getting executed with parameters :  message={0}; robotId={1};runInstanceId={2}; robotTaskMapId={3}", JsonConvert.SerializeObject(message), robotId, runInstanceId, robotTaskMapId),
                LogHandler.Layer.Business, null);
#endif
            try
            {
                using (LogHandler.TraceOperations("MetricIngestor:Process", LogHandler.Layer.Business, Guid.NewGuid(), null))
                {
                    if (!message.TE.ContainsKey(TaskRouteConstants.MetricIngestorCode))
                    {
                        LogHandler.LogError("Message is not processed in MetricIngestor for FrameId = {0} ,TenantId = {1}, deviceId = {2} , module = {3}", LogHandler.Layer.Business, message.Fid, message.Tid, message.Did, TaskRouteConstants.MetricIngestorCode);
                        return true;
                    }
                    
                        MetricIngestorEntityTranslator metricIngestorrEntityTranslator = new MetricIngestorEntityTranslator();
                        BE.Queue.MetricIngestorMetadata metricBotMetricIngestorMetadata = metricIngestorrEntityTranslator.MetricEntityTranslator(message);
                        
                   
                    return true;
                 }
            }
            catch (Exception exMP)
            {
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
                    LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "Process", "MetricIngestor"),
                            LogHandler.Layer.Business, null);
                   
                    if (!failureLogged)
                    {
                        LogHandler.LogError(String.Format("Exception Occured while handling an exception in MetricIngestor in Process method. error message: {0}", ex.Message), LogHandler.Layer.Business, null);
                    }
#if DEBUG
                    LogHandler.LogInfo(String.Format(InfoMessages.Method_Execution_End, "Process", "MetricIngestor"), LogHandler.Layer.Business, null);
#endif
                    return false;
                }
            }
        }

        }
    }
