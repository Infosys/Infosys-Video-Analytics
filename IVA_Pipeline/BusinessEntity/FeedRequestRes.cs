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
    public class FeedRequestRes
    {
        public string RequestId { get; set; }
        public string ResourceId { get; set; }
        public Nullable<int> FeedProcessorMasterId { get; set; }
        public string VideoName { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public string Model { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public int TenantId { get; set; }
        public string LastFrameId { get; set; }
        public Nullable<System.DateTime> LastFrameGrabbedTime { get; set; }
        public Nullable<System.DateTime> LastFrameProcessedTime { get; set; }
        public Nullable<System.DateTime> StartFrameProcessedTime { get; set; }
    }
}
