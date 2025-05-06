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
    public class FrameMetaData
    {
        public object Frame { get; set; }
        public DateTime FrameGrabberTime { get; set; }
        public long Fid { get; set; }
        public int FrameNumber { get; set; }
        public int SequenceNumber { get; set; }
    }
}
