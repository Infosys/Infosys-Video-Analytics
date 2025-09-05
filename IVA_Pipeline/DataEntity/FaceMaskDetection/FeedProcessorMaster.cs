/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.VideoAnalytics
{
    public partial class FeedProcessorMaster
    {
        public int FeedProcessorMasterId { get; set; }
        public string ResourceId { get; set; }
        public string FileName { get; set; }
        public string FeedUri { get; set; }
        public long ProcessingStartTimeTicks { get; set; }
        public long? ProcessingEndTimeTicks { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int TenantId { get; set; }
        public int? Status { get; set; }
        public string MachineName { get; set; }
        public long FileSize { get; set; }
        public int VideoDuration { get; set; }
        public double Fps { get; set; }
        public int TotalFrameProcessed { get; set; }
        public double TimeTaken { get; set; }
        public double FrameProcessedRate { get; set; }
    }
}
