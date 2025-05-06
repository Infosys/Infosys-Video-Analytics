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

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Table
{
    public class FrameResourceDetails
    {
        public string ResourceID { get; set; }
        
        public string FrameID { get; set; }
        
    }

    public class TableDetails
    {
        public string JobName { get; set; }
        public string EntityName { get; set; }
    }


}
