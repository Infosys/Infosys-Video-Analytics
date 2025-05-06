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
    public partial class FramePredictedClassDetail
    {
        public int Id { get; set; }
        public int FeedProcessorMasterId { get; set; }
        public string ResourceId { get; set; }
        public string FrameId { get; set; }
        public DateTime? FrameGrabTime { get; set; }
        public int PredictedClassSequenceId { get; set; }
        public long FrameProcessedTime { get; set; }
        public DateTime FrameProcessedDateTime { get; set; }
        public string Region { get; set; }
        public string PredictedClass { get; set; }
        public double ConfidenceScore { get; set; }
        public int PartitionKey { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int TenantId { get; set; }
        public string PredictionType { get; set; }

        public virtual FrameMaster FrameMaster { get; set; }
    }
}
