/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using System; 
using QE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity;
using System.Text.Json.Nodes;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessComponent.Translator
{
    public class SensorDataEntityTranslator
    {
        public BE.Queue.SensorMetaData SensorDataCollectorTranslator(QE.SensorMetaData message)
        {
            BE.Queue.SensorMetaData sensorDataCollector = new BE.Queue.SensorMetaData();
            try
            {
                if (message != null)
                {
                    sensorDataCollector.Tid = message.Tid;
                    sensorDataCollector.Did = message.Did;
                  
                    sensorDataCollector.Ts = message.Ts;
                    sensorDataCollector.Msg_ver = message.Msg_ver;
                
                    sensorDataCollector.Status = message.Status;
                  

                }
                return sensorDataCollector;
            }
            catch (Exception ex)
            {
                LogHandler.LogError(String.Format(ErrorMessages.Exception_Failed, "SensorCollectorEntityTranslator", "SensorCollectorTranslator"), LogHandler.Layer.Business, null);
                throw ex;
            }
        }
    }
}
