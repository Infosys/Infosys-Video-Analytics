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

namespace FrameDetailsServices.Models
{
    public class DeviceConfiguration
    {
        
        public string StorageBaseUrl { get; set; }
        
        public string CameraURl { get; set; }
        
        public string VideoFeedType { get; set; }
        
        public string OfflineVideoDirectory { get; set; }
        
        public string ArchiveDirectory { get; set; }
        
        public bool ArchiveEnabled { get; set; }
        
        public int LotSize { get; set; }
        
        public string ModelName { get; set; }
        
        public string QueueName { get; set; }
        
        public string DeviceId { get; set; }
        
        public int TenantId { get; set; }

    }
}