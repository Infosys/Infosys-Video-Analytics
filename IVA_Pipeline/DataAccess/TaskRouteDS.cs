/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Lif.LegacyIntegrator;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class TaskRouteDS
    {
        public string Send<T>(T message,string transportRegion)
        {
            string msgResponse = null;
            try
            {
                AdapterManager adapterManager = new AdapterManager();
                string serializedPresentationMsg = Utility.SerialiseToJSON(message);
                msgResponse = adapterManager.Execute(serializedPresentationMsg, transportRegion);
                
               

            }
            catch (Exception ex)
            {
                string response = ex.Message;
                if (ex.InnerException != null)
                    response = response + ".Inner Exception- " + ex.InnerException.Message;
                LogHandler.LogError("Exception thrown while Sending message. Exception Message: {0} and Exception Stack Trace {1}",
                    LogHandler.Layer.Infrastructure, response, ex.StackTrace);
                throw ex;
            }
            return msgResponse;

        }


        public static bool IsMemoryDoc()
        {
            AdapterManager adapterManager = new AdapterManager();

            var transportMedium = adapterManager.GetTransportMedium(TaskRouteConstants.FrameRepositoryRegion) ;
            if (transportMedium == TaskRouteConstants.MemoryDoc)
            {
                return true;
            }
            return false;

        }
    }
}
