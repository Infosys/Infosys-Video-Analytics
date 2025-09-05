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
    public class History_Details
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
        public int TenantId { get; set; }
        [DataMember]
        public string LastFrameId { get; set; }
        [DataMember]
        public string LastFrameGrabbedTime { get; set; }
        [DataMember]
        public string LastFrameProcessedTime { get; set; }
        [DataMember]
        public string StartFrameProcessedTime { get; set; }
        
        [DataMember]
        public string ModelName { get; set; }
        [DataMember]
        public string FeedURI { get; set; }
        [DataMember]
        public long ProcessingStartTimeTicks { get; set; }
        [DataMember]
        public Nullable<long> ProcessingEndTimeTicks { get; set; }
        [DataMember]
        public string MachineName { get; set; }
        [DataMember]
        public Media_Metadata VideoMetaData { get; set; }
        
        [DataMember]
        public int TotalFrameProcessed { get; set; }
        [DataMember]
        public double TimeTaken { get; set; }
        
    }
    public class Resource_Details
    {
        [DataMember]
        public int TotalResource { get; set; }
        [DataMember]
        public int TotalAvailableResource { get; set; }

    }
    public class All_Resource_Details
    {
        [DataMember]
        public List<string> AllDevices { get; set; }
        [DataMember]
        public List<string> InprogressDevices { get; set; }
        [DataMember]
        public List<string> IntiatedDevices { get; set; }
        [DataMember]
        public List<string> BusyDevices { get; set; }
        [DataMember]
        public List<string> AvailableDevices { get; set; }

    }

    [DataContract]
    public class Media_Metadata_Details
    {
        [DataMember]
        public int MediaId { get; set; }
        [DataMember]
        public int FeedProcessorMasterId { get; set; }
        [DataMember]
        public string RequestId { get; set; }
        
        [DataMember]
        public System.DateTime CreatedDate { get; set; }
        [DataMember]
        public string CreatedBy { get; set; }
        [DataMember]
        public string ModifiedBy { get; set; }
        [DataMember]
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        [DataMember]
        public int TenantId { get; set; }
        [DataMember]
        public string MetaData { get; set; }

    }



    [DataContract]
    public class Media_Metadata
    {

        [DataMember]
        public long FileSize { get; set; }
        [DataMember]
        public double VideoDuration { get; set; }
        [DataMember]
        public double Fps { get; set; }

        [DataMember]
        public string FileExtension { get; set; }
        [DataMember]
        public int BitRateKbs { get; set; }

        [DataMember]
        public string ColorModel { get; set; }
        [DataMember]
        public string Format { get; set; }
        [DataMember]
        public string FrameSize { get; set; }
    }

    [DataContract]
    public class Feed_Processor_Master_Detail
    {
        [DataMember]
        public int FeedProcessorMasterId { get; set; }
        [DataMember]
        public int MediaId { get; set; }
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
        public Nullable<int> Status { get; set; }
        [DataMember]
        public string MachineName { get; set; }
        
        [DataMember]
        public int TotalFrameProcessed { get; set; }
        [DataMember]
        public double TimeTaken { get; set; }
        [DataMember]
        public double FrameProcessedRate { get; set; }
    }

    [DataContract]
    public class Feed_Status_Details
    {
        [DataMember]
        public string VideoName { get; set; }
        [DataMember]
        public string FeedId { get; set; }
        [DataMember]
        public string Status { get; set; }
        [DataMember]
        public long ProcessingStartTime { get; set; }
        [DataMember]
        public long ProcessingEndTime { get; set; }

        [DataMember]
        public string StreamingUrl { get; set; }
        [DataMember]
        public long LastPredictorTime { get; set; }
    }
}
