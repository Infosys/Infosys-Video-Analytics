/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
using System;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Translator;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Runtime.Caching;
using System.Data.SqlClient;
using System.Diagnostics;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail;
using System.Runtime.InteropServices;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using static Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common.ApplicationConstants;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.IO;
using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using System.Reflection;
using System.Data.SqlTypes;
using Infosys.Solutions.Ainauto.VideoAnalytics.DataCollector;



namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    public class DataCollectorProcess: ProcessHandlerBase<QueueEntity.FrameCollectorMetadata>
    {
        public string _taskCode;
        public static Dictionary<string,string> args;
        private static DeviceDetails deviceDetails = new DeviceDetails();
        public static AppSettings appSettings = Config.AppSettings;
        public DataCollectorProcess() { }
        public DataCollectorProcess(string processId,Dictionary<string,string> arguments) {
            args=arguments;
            _taskCode=TaskRoute.GetTaskCode(processId,args);
            deviceDetails=ConfigHelper.SetDeviceDetails(appSettings.TenantID.ToString(),appSettings.DeviceID,_taskCode,args);
            if(args!=null && args.Count>0) {
                string type=args[args.Keys.First()];
                if(type.ToLower()=="values") {
                    deviceDetails=Helper.UpdateConfigValues(args,deviceDetails);
                }
            }
        }

        public override void Dump(QueueEntity.FrameCollectorMetadata message)
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
        private void ReadFromConfig()
        {
            //Added App settings to get the predicted model type
            AppSettings appSettings = Config.AppSettings;
            //if (ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"] != null)
            //{
            //    frameCacheSlidingExpirationInMins = Convert.ToDouble(System.Configuration.ConfigurationManager.AppSettings["FrameCacheSlidingExpirationInMins"]);
            //}
            ////   if (ConfigurationManager.AppSettings["PredictionType"] != null)
            //if (appSettings.PredictionType != null)
            //{
            //    //  predictionType = System.Configuration.ConfigurationManager.AppSettings["PredictionType"];
            //    predictionType = appSettings.PredictionType;
            //}


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
      
        public override bool Process(QueueEntity.FrameCollectorMetadata message, int robotId, int runInstanceId, int robotTaskMapId)
        {
            appSettings = Config.AppSettings;
            string _filePath;
            string directory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string fullPath = Path.Combine(directory,deviceDetails.DataCollectorFileName);
            IDataCollector client = DataCollectorFactory.CreateInstance(deviceDetails.DBProvider);
            bool response;
            if (client != null)
            {
                response = client.LogCollector(message, fullPath);
            }
            return true;
        }
    }
}
