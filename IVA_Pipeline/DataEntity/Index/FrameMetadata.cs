/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Text;

using System.Threading.Tasks;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
 
using Nest;
namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Index
{
    [ElasticsearchType(RelationName = "framemetadata_staging_video")]
    public class FrameMetaData
    {
        public FrameMetaData()
        {
            Mtp = new List<Mtp>();
            
        }
        [Text(Name = "Did")]
        public string Did { get; set; }
        [Text(Name = "Fid")]
        public string Fid { get; set; }
        [Text(Name = "Tid")]
        public int Tid { get; set; }
        [Text(Name = "Ts")]
        public string Ts { get; set; }
        [Text(Name = "Ts_ntp")]
        public string Ts_ntp { get; set; }
        [Text(Name = "Msg_ver")]
        public string Msg_ver { get; set; }
        [Text(Name = "Inf_ver")]
        public string Inf_ver { get; set; }
       
        [Object]
        public List<Mtp> Mtp { get; set; }
        [Object]
        public  Predictions[] Fs { get; set; }
       
        [Text(Name = "I_fn")]
        public string I_fn { get; set; }
       

        [Text(Name = "FeedId")]
        public string FeedId { get; set; }

        [Text(Name = "FrameNumber")]
        public string FrameNumber { get; set; }

        [Text(Name = "PredictionType")]
        public string PredictionType { get; set; }
        [Text(Name = "Pts")]
        public string Pts { get; set; }
        [Text(Name = "SequenceNumber")]
        public string SequenceNumber { get; set; }
        [Text(Name = "Status")]
        public string Status { get; set; }
        [Text(Name = "CreatedBy")]
        public string CreatedBy { get; set; }
        [Date(Name = "CreatedDate")]
        public DateTime CreatedDate { get; set; }
        [Text(Name = "ModifiedBy")]
        public string ModifiedBy { get; set; }
        [Text(Name = "ModifiedDate")]
        public DateTime ModifiedDate { get; set; }
        [Text(Name = "Raw_base64_image")]
        public string Raw_base64_image { get; set; }
        [Text(Name = "Rendered_base64_image")]
        public string Rendered_base64_image { get; set; }

        
    }
    public class Mtp
    {
        [Text(Name = "Etime")]
        public string Etime { get; set; }

        [Text(Name = "Src")]
        public string Src { get; set; }

        [Text(Name = "Stime")]
        public string Stime { get; set; }
    }

    public class Predictions
    {
        public BoundingBox Dm { get; set; } 
        public string Cs { get; set; } 
        public string Lb { get; set; }

        public string Info { get; set; }
        public string NoObj { get; set; }
        public string Uid { get; set; }
        public string Pid { get; set; }
        public string Np { get; set; }

        public List<MaskDetectorAPIResMsgEntity> Objects { get; set; }

        public int Rc { get; set; } 

        public string Rm { get; set; }




        public Dictionary<int, List<float>> Kp { get; set; }

        public List<List<float>> Tpc { get; set; }

        public List<List<float>> Bpc { get; set; }

      

        
    }


    public class BoundingBox
    {
        [Text(Name = "X")]
        public string X { get; set; } 
        [Text(Name = "Y")]
        public string Y { get; set; } 
        [Text(Name = "H")]
        public string H { get; set; } 
        [Text(Name = "W")]
        public string W { get; set; }
    }

    public class FrameElasticSearchMetadata
    {
        public string Tid { get; set; } 
        public string Did { get; set; }
        public string Fid { get; set; } 
        public string Pts { get; set; } 
        public Predictions[] Fs { get; set; } 
        public string PredictionType { get; set; } 
        public string FeedId { get; set; } 
        public string SequenceNumber { get; set; } 
        public string Status { get; set; }
        public string FrameNumber { get; set; } 

        public string FileName { get; set; }

        public List<Index.Mtp> Mtp { get; set; }

        public string Raw_base64_image { get; set; }
        public string Rendered_base64_image { get; set; }
    }

   

    public class FrameMetaDataEntity
    {
        public string PredictionType { get; set; }

        public int Tid { get; set; }

        public string Did { get; set; }
        
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
        public long Fid { get; set; }
    }
}
