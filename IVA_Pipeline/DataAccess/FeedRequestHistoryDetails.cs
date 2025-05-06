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

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.DataAccess
{
    public class FeedRequestHistoryDetails
    {
        public string RequestId { get; set; }
        public string ResourceId { get; set; }
        public Nullable<int> FeedProcessorMasterId { get; set; }
        public string VideoName { get; set; }
        public string Status { get; set; }
        public int TenantId { get; set; }
        public string LastFrameId { get; set; }
        public string LastFrameGrabbedTime { get; set; }
        public string LastFrameProcessedTime { get; set; }
        public string StartFrameProcessedTime { get; set; }
        public string FileName { get; set; }
        public string FeedURI { get; set; }
        public long ProcessingStartTimeTicks { get; set; }
        public Nullable<long> ProcessingEndTimeTicks { get; set; }
        public string MachineName { get; set; }
        public long FileSize { get; set; }
        public int VideoDuration { get; set; }
        public double Fps { get; set; }
        public int TotalFrameProcessed { get; set; }
        public double TimeTaken { get; set; }
        public double FrameProcessedRate { get; set; }
        public string ModelName { get; set; }
        public string VideoMetadata { get; set; }

    }


    public class MediaMetadata
    {

        public long FileSize { get; set; }
        public double VideoDuration { get; set; }
        public double Fps { get; set; }
        public string FileExtension { get; set; }
        public int BitRateKbs { get; set; }
        public string ColorModel { get; set; }
        public string Format { get; set; }
        public string FrameSize { get; set; }
    }
}
