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

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message
{
    [DataContract]
    public class ResourceAttributeMsg
    {
        [DataMember]
        public string ResourceId { get; set; }
        [DataMember]
        public string AttributeName { get; set; }
        [DataMember]
        public string AttributeValue { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public System.DateTime CreateDate { get; set; }
        [DataMember]
        public string ModifiedBy { get; set; }
        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        [DataMember]
        public int TenantId { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string VersionNumber { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public Nullable<bool> IsSecret { get; set; }
    }
    [DataContract]
    public class FeedRequestReqMsg
    {
        [DataMember]
        public string RequestId { get; set; }
        [DataMember]
        public string ResourceId { get; set; }
        [DataMember]
        public Nullable<int> FeedProcessorMasterId { get; set; }
        [DataMember]
        public string VideoName { get; set; }
        [DataMember]
        public string Status { get; set; }
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
        public string LastFrameId { get; set; }
        [DataMember]
        public Nullable<System.DateTime> LastFrameGrabbedTime { get; set; }
        [DataMember]
        public Nullable<System.DateTime> LastFrameProcessedTime { get; set; }
        [DataMember]
        public Nullable<System.DateTime> StartFrameProcessedTime { get; set; }
        [DataMember]
        public string Model { get; set; }
    }
}
