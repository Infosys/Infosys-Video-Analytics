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

namespace Infosys.Solutions.Ainauto.VideoAnalytics.MaskDetector.Contracts.Data
{
    public class FrameDetails
    {
        public string TenantId { get; set; }
        public string DeviceId { get; set; }
        public string Location { get; set; }
        public byte[] Frame { get; set; }
        public List<FaceDetails> Faces { get; set; }
    }
    public class FaceDetails
    {
        public string Class { get; set; }
        public double ConfidenceScore { get; set; }
        public string Id { get; set; }
    }

}