/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
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
    public class FeedProcessorMasterDetails
    {
        public int FeedProcessorMasterId { get; set; }
        public string ResourceId { get; set; }
        public string FileName { get; set; }
        public string FeedURI { get; set; }
        public long ProcessingStartTimeTicks { get; set; }
        public Nullable<long> ProcessingEndTimeTicks { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public int TenantId { get; set; }
        public int Status { get; set; }
        public string MachineName { get; set; }

        public int TotalFrameProcessed { get; set; }
        public double TimeTaken { get; set; }
        public double FrameProcessedRate { get; set; }
    }
}
