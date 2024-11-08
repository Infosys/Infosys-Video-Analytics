/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Framedetail
{
    public partial class FrameMetadatum
    {
        public int FeedProcessorMasterId { get; set; }
        public string ResourceId { get; set; }
        public string FrameId { get; set; }
        public int SequenceId { get; set; }
        public DateTime FrameGrabTime { get; set; }
        public string MetaData { get; set; }
        public int PartitionKey { get; set; }
        public string MachineName { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int TenantId { get; set; }
        public string PredictionType { get; set; }
    }
}
