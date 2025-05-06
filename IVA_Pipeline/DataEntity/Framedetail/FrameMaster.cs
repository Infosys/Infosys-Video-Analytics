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
    public partial class FrameMaster
    {
        public FrameMaster()
        {
            FramePredictedClassDetails = new HashSet<FramePredictedClassDetail>();
        }

        public int FeedProcessorMasterId { get; set; }
        public int FrameMasterId { get; set; }
        public string ResourceId { get; set; }
        public string FrameId { get; set; }
        public DateTime FrameGrabTime { get; set; }
        public string Status { get; set; }
        public string ClassPredictionCount { get; set; }
        public int PartitionKey { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int TenantId { get; set; }

        public virtual ICollection<FramePredictedClassDetail> FramePredictedClassDetails { get; set; }

        public string FileName { get; set; }
        public string Mtp { get; set; }
    }
}
