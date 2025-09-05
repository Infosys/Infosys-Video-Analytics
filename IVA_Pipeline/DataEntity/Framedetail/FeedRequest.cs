/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail
{
    public partial class FeedRequest
    {
        public string RequestId { get; set; }
        public string ResourceId { get; set; }
        public int? FeedProcessorMasterId { get; set; }
        public string VideoName { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int TenantId { get; set; }
        public string LastFrameId { get; set; }
        public DateTime? LastFrameGrabbedTime { get; set; }
        public DateTime? LastFrameProcessedTime { get; set; }
        public DateTime? StartFrameProcessedTime { get; set; }
    }
}
