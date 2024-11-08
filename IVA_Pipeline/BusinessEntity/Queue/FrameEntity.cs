/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/

using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue
{

    public class FrameProcessorMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Sbu { get; set; } 
        public string Mod { get; set; } 
        public string Que { get; set; } 
        public string FeedId { get; set; } 
        public Dictionary<string,  List<string>> TE { get; set; } 
        public List<string> Fids { get; set; } 
        public string SequenceNumber { get; set; } 
        public string FrameNumber { get; set; } 
        public string Etime { get; set; }
        public string Src { get; set; }
        public string Mtp { get; set; }
        public string Stime { get; set; }

        public string Ffp { get; set; } 

        public string Ltsize { get; set; } 

        public string Lfp { get; set; }

        public string videoFileName { get; set; }
        public List<List<string>> Prompt { get; set; }
        public List<string> Msk_img { get; set; }
        public List<string> Rep_img { get; set; }
        public byte[] Pcd { get; set; }
    }

    public class FrameCollectorMetadata {
        public string Tid {get;set;} /* Tenant Id */
        public string Did {get;set;} /* Device Id */
        public string Fid {get;set;} /* Frame Id */
        public string Pts {get;set;} /* Prediction Timestamp */
        public Predictions[] Fs {get;set;} /* Predicted Faces */
         public string  PredictionType {get;set;} /* Predicted Type */
        public string FeedId {get;set;} /* Feed Id */
        public string SequenceNumber {get;set;} /* Sequence Number */
        public string Status {get;set;} /* Predictor Status */
        public string FrameNumber {get;set;} /* Current Frame Number */
        public string FileName {get;set;}
        public List<string> Obase_64 {get;set;}
        public List<string> Img_url {get;set;}
    }


    public class MetricIngestorMetadata

    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Pts { get; set; }  
        public Predictions[] Fs { get; set; } 
        public string PredictionType { get; set; } 
        public string FeedId { get; set; } 
        public string SequenceNumber { get; set; } 
        public string FrameNumber { get; set; } 
    }

    public class Predictions
    {
        public BoundingBox Dm { get; set; } 
        public string Cs { get; set; } 
        public string Lb { get; set; } 

        public Dictionary<int, List<float>> Kp { get; set; }

        public List<List<float>> Tpc { get; set; }

        public List<List<float>> Bpc { get; set; }

    }
    public class BoundingBox
    {
        public string X { get; set; } 
        public string Y { get; set; } 
        public string H { get; set; } 
        public string W { get; set; } 
        

    }


    public class PersonCountMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Mod { get; set; } 
        public string FeedId { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public string SequenceNumber { get; set; } 
        public string FrameNumber { get; set; } 

        public string Fp { get; set; } 

        
        public string Etime { get; set; }
        public string Src { get; set; }
        public string Stime { get; set; }
        public string Ad { get; set; }
        public List<string> Fids { get; set; } 

        public string Ffp { get; set; } 

        public string Ltsize { get; set; } 

        public string Lfp { get; set; }
        public string videoFileName { get; set; }
    }
    public class DeepSortPersonCountMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Mod { get; set; } 
        public string FeedId { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public string SequenceNumber { get; set; } 
        public string FrameNumber { get; set; } 
    }

    public class PersonCountPayload
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        
        public string Cs { get; set; } 
        public PersonCount[] Per { get; set; } 
        public string Yt { get; set; } 
    }

    public class PersonAndBounding
    {
        public BoundingBox Dm { get; set; } 
        public string Pid { get; set; }
        public string Class { get; set; }

    }

    public class PersonCount
    {       
        public string Fid { get; set; } 
        public PersonAndBounding[] Fs { get; set; } 
        public string Dt { get; set; }

        public string info { get; set; }
        public string Class { get; set; }
    }

    public class ObjectDetectorAPIResMessage
    {
        
        public List<MaskDetectorAPIResMessage> Objects { get; set; }
        [DataMember]
        public int Rc { get; set; } 
        [DataMember]
        public string Rm { get; set; }

        /**/
        #region New changes for IVA request/response structure 

        [DataMember]
        public string Did { get; set; } // device id
        [DataMember]
        public string Fid { get; set; } // frame id
        [DataMember]
        public string Tid { get; set; } // tenant id

        [DataMember]
        public string Ts { get; set; }

        [DataMember]
        public string Ts_ntp { get; set; }

        [DataMember]
        public string Msg_ver { get; set; }

        [DataMember]
        public string Inf_ver { get; set; }

        [DataMember]
        public string Ad { get; set; }

        //[DataMember]
        //public PersonCountAPIResMsg[] Fs { get; set; }

        //[DataMember]
        //public PersonDetails[] Fs { get; set; }

        [DataMember]
        public List<PersonDetails> Fs { get; set; }

        [DataMember]
        public List<Mtp> Mtp { get; set; }
        #endregion
        public string Img_url { get; set; }
        public string Base_64 { get; set; }

    }

    public class GenerativeAIMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Sbu { get; set; } 
        public string Mod { get; set; } 
        public string Que { get; set; } 
        public string FeedId { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public List<string> Fids { get; set; } 
        public string SequenceNumber { get; set; } 
        public string FrameNumber { get; set; } 
        public string Etime { get; set; }
        public string Src { get; set; }
        public string Stime { get; set; }

        public string Ffp { get; set; } 

        public string Ltsize { get; set; } 

        public string Lfp { get; set; }

        public string videoFileName { get; set; }
        public List<List<string>> prompts { get; set; }
        public string Msk_img { get; set; }
        public string Rep_img { get; set; }    
    }

    public class Mtp
    {
        [DataMember]
        public DateTime Etime { get; set; }

        [DataMember]
        public string Src { get; set; }

        [DataMember]
        public DateTime Stime { get; set; }
    }


}