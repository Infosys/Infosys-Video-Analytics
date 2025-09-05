/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.TaskRoute;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    internal class PromptInjector : ProcessHandlerBase<QueueEntity.PromptInjectorMetaData>
    {
        AppSettings appSettings = Config.AppSettings;
        public string _taskCode;
        public PromptInjector() { }
        public PromptInjector(string processId)
        {
            _taskCode = TaskRoute.GetTaskCode(processId);
        }

        public override void Dump(QueueEntity.PromptInjectorMetaData message)
        {
            
        }

        public override bool Initialize(MaintenanceMetaData message)
        {
            appSettings = Config.AppSettings;
            return true;
        }

        public override bool Process(QueueEntity.PromptInjectorMetaData message, int robotId, int runInstanceId, int robotTaskMapId)
        {
            LogHandler.LogDebug($"Received prompt: {message.Prompt} for device id : {message.DeviceId}", LogHandler.Layer.PromptInjector);
            try
            {
                PromptHandlerMetaData promptData = new PromptHandlerMetaData()
                {
                    Prompt = message.Prompt,
                    Did = message.Prompt,
                    Hp = message.HyperParameters,
                };
                TaskRoute taskRoute = new TaskRoute();
                var taskList = taskRoute.GetTaskRouteDetails(appSettings.TenantID.ToString(), appSettings.DeviceID, _taskCode)[_taskCode];
                foreach (string moduleCode in taskList)
                {
                    taskRoute.SendMessageToQueue(Config.AppSettings.TenantID.ToString(), Config.AppSettings.DeviceID, moduleCode, promptData);
                }
            }
            catch (Exception e)
            {
                LogHandler.LogError("Error in Process method of Prompt Injector, exception: {0}, inner exception: {1}, stack trace: {2}",
                    LogHandler.Layer.PromptInjector, e.Message, e.InnerException, e.StackTrace);
            }
            return true;
        }
    }
}
