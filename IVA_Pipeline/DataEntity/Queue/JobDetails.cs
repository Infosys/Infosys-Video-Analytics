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

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue
{
    public class JobDetails
    {
        public int jobTrigger { get; set; }
        public string executionId { get; set; }
        public string jobName { get; set; }

        public int jobId { get; set; }

        public string authentication_port { get; set; }

        public string r2w_port { get; set; }

        public string baseURI { get; set; }

        public string authentication_EndPoint { get; set; }

        public string r2w_EndPoint { get; set; }

        public string data_Limit { get; set; }

        public int lastProcessedDataId { get; set; }

        
        public string jobStatus { get; set; }
        public string createdBy { get; set; }
        public int tenantId { get; set; }

        public string entityName { get; set; }

        public string clientId { get; set; }
        public string clientSecret { get; set; }



    }
}
