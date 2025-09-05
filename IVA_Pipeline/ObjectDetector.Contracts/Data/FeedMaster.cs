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
using System.Runtime.Serialization;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Data
{
    [DataContract]
    public class Feed_Master
    {
        [DataMember]
        public int FeedProcessorMasterId { get; set; }
        [DataMember]
        public string ResourceId { get; set; }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public string FeedURI { get; set; }
        [DataMember]
        public long ProcessingStartTimeTicks { get; set; }
        [DataMember]
        public Nullable<long> ProcessingEndTimeTicks { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public System.DateTime CreatedDate { get; set; }
        [DataMember]
        public string ModifiedBy { get; set; }
        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        [DataMember]
        public int TenantId { get; set; }
        [DataMember]
        public int Status { get; set; }
        [DataMember]
        public string MachineName { get; set; }

    }
}
