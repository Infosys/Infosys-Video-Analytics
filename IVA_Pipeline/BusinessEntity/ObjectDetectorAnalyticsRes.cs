/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity
{
    public class ObjectDetectorAnalyticsRes
    {
        public int TenantId { get; set; }
        public string DeviceId { get; set; }
        public string GrabberTime { get; set; }
        public string Location { get; set; }
        public List<ObjectDetectorAnalyticsData> Details { get; set; }
        public double CompliancePercentage { get; set; }
    }
    public class ObjectDetectorAnalyticsData
    {
        public string PersonId { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
       
    }

    public class ObjectDetectorAnalyticsDetail
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public DateTime GrabberTime { get; set; }
        public int Count { get; set; }
        public string FrameId { get; set; }
        public string PersonId { get; set; }

    }
}
