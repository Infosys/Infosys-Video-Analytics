/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data
{
    [DataContract]
    public class DeviceConfiguration
    {
        [DataMember]
        public string StorageBaseUrl { get; set; }
        [DataMember]
        public string CameraURl { get; set; }
        [DataMember]
        public string VideoFeedType { get; set; }
        [DataMember]
        public string OfflineVideoDirectory { get; set; }
        [DataMember]
        public string ArchiveDirectory { get; set; }
        [DataMember]
        public bool ArchiveEnabled { get; set; }
        [DataMember]
        public int LotSize { get; set; }
        [DataMember]
        public string ModelName { get; set; }
        [DataMember]
        public string QueueName { get; set; }
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public int TenantId { get; set; }

    }
}