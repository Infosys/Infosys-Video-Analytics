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
    public class ConfigData
    {

        public string BaseURI { get; set; }
        public string R2w_Port { get; set; }
        public string Authentication_Port { get; set; }
        public string Authentication_EndPoint { get; set; }
        public string R2w_EndPoint { get; set; }
        public string Data_Limit { get; set; }
        public string clientId { get; set; }
        public string clientSecret { get; set; }
        public string MetricIngestor_EndPoint { get; set; }
        public string Port { get; set; }
    }


    public class FrameResourceDetails
    {
        public string FrameID { get; set; }

        public string ResourceID { get; set; }
    }
}
