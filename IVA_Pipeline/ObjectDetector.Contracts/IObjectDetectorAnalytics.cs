/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;


namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts
{
    
    public interface IObjectDetectorAnalytics
    {
        

        ObjectDetectorAnalyticsResMsg GetLocationBasedCount(int tenantId,
            
            string locations,
          
            string startTime,
           
            string endTime, int timeInterval, bool uniquePersonCount);

        
        ObjectDetectorAnalyticsResMsg GetDeviceBasedCount(int tenantId,
           
            string deviceId,
          
            string startTime,
           
            string endTime, int timeInterval, bool uniquePersonCount);

        
        ObjectDetectorAnalyticsResMsg GetLocationBasedComplianceScore(int tenantId,
           
            string locations,
           
            string startTime,
           
            string endTime, int timeInterval, bool uniquePersonCount);

        
        ObjectDetectorAnalyticsResMsg GetLocationBasedCountTrend(int tenantId,
          
            string locations,
            
            string startTime,
            
            string endTime,
            int timeInterval, bool uniquePersonCount);

        
        ObjectDetectorAnalyticsResMsg GetLocationBasedComplianceScoreTrend(int tenantId,
           
            string locations,
            
            string startTime,
           
            string endTime,
            int timeInterval, bool uniquePersonCount);

    }

}
