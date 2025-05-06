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
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data;
using System.Runtime.Serialization;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message
{
    [DataContract]
    public class Attribute_Details_ResMsg
    {
        [DataMember]
        public string DeviceId { get; set; }
        [DataMember]
        public int TenantId { get; set; }
        [DataMember]
        public List<Attribute_Details> Attributes { get; set; } 

        
    }
}
