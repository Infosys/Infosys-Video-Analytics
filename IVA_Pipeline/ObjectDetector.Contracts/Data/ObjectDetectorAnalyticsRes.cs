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
using System.Runtime.Serialization;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data
{
    [DataContract]
    public class ObjectDetectorAnalyticsRes
    {
        [DataMember]
        public string TenantId { get; set; }
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public string GrabberTime { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public string CompliancePercentage { get; set; }
        [DataMember]
        public List<ObjectDetectorAnalyticsData> Details { get; set; }
    }
    [DataContract]
    public class ObjectDetectorAnalyticsData
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Count { get; set; }
       
    }


    [DataContract]
    public class NoMaskAnalyticsRes
    {
        [DataMember]
        public DateTime Time { get; set; }
        [DataMember]
        public int NoMaskCount { get; set; }
     }
}
