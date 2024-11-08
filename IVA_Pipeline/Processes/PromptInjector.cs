/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.ProcessScheduler.Framework;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QueueEntity = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Processes
{
    internal class PromptInjector : ProcessHandlerBase<QueueEntity.PromptInjectorMetaData>
    {
        int exceptionCount = 0;
        static int exceptionCountLimit = 0;
        static double tokenCacheExpirationTime = 0.0;

        public override void Dump(QueueEntity.PromptInjectorMetaData message)
        {
            
        }

        public override bool Initialize(MaintenanceMetaData message)
        {
            ReadFromConfig();
            return true;
        }

        public override bool Process(QueueEntity.PromptInjectorMetaData message, int robotId, int runInstanceId, int robotTaskMapId)
        {
            if(FrameGrabberHelper.videoFeedType == "PROMPT")
            {
                File.WriteAllText(Path.Combine(FrameGrabberHelper.offlinePromptDirectory, "promptinput.txt"), message.Prompt);
            }
            else
            {
                FrameGrabberHelper.promptData = message.Prompt;
            }
            return true;
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
