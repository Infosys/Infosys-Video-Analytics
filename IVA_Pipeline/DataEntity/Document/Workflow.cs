/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Document
{
    public class Workflow
    {
        
        public Stream File { get; set; }
       
        public string DeviceId { get; set; }
       
        
        public string StorageBaseURL { get; set; }
        
        public string FrameId { get; set; }
        
        public int TenantId { get; set; }
        
        public string UploadedBy { get; set; }
       
        public string StatusMessage { get; set; }
        
        public int StatusCode { get; set; }


    }
}
