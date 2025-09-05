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
    public class History_Details_Res_Msg
    {
        [DataMember]
        public List<History_Details> HistoryDetailsList { get; set; }
    }
    public class ResourceDetailsResMsg
    {
        [DataMember]
        public Resource_Details ResourceDetails { get; set; }
    }

    public class AllResourceDetailsResMsg
    {
        [DataMember]
        public All_Resource_Details ResourceDetails { get; set; }
    }

    [DataContract]
    public class FeedProcessorMasterMsg
    {
        [DataMember]
        public Feed_Processor_Master_Detail FeedProcessorMasterDetail { get; set; }
    }

    [DataContract]
    public class Media_MetaData_Msg_Res
    {
        [DataMember]
        public int MediaId { get; set; }
    }

    [DataContract]
    public class UpdateMediaMetaDataMsgResMsg
    {
        [DataMember]
        public bool Status { get; set; }
    }

    [DataContract]
    public class FeedStatusDetailsRes
    {
        [DataMember]
        public Feed_Status_Details FeedStatusDetails { get; set; }

    }

    [DataContract]
    public class Media_MetaData_Msg_Req
    {
        [DataMember]
        public Media_Metadata_Details MediaMetadataDetails { get; set; }
    }

    [DataContract]
    public class UploadResponseMsg
    {
        [DataMember]
        public string PreviewPath { get; set; }
        [DataMember]
        public string VideoFormat { get; set; }
        [DataMember]
        public string Message { get; set; }
    }

    [DataContract]
    public class FeedRequestResMsg
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
        public string Model { get; set; }
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
    }

    [DataContract]
    public class UpdateFeedRequestResMsg
    {
        [DataMember]
        public bool Status { get; set; }

    }
}
