/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿
namespace Infosys.Solutions.Ainauto.VideoAnalytics.DataCollector
{
    public static class DataCollectorFactory
    {
        public static IDataCollector? CreateInstance(string logType)
        {
            IDataCollector? dataCollector = null;
            if (!string.IsNullOrEmpty(logType))
            {
                string datacollectornamespace = "Infosys.Solutions.Ainauto.VideoAnalytics.DataCollector";
                Type? type = Type.GetType(datacollectornamespace + "." + logType);
                dataCollector = (IDataCollector?)Activator.CreateInstance(type);
            }
            return dataCollector;
        }
    }
}
