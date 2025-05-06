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
using System.Text.Json.Nodes;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message
{

    [DataContract]
    public class ObjectDetectorAPIResMsg
    {
        
        [DataMember]
        public int Rc { get; set; } 
        [DataMember]
        public string Rm { get; set; }

        /**/
        #region New changes for IVA request/response structure 

        [DataMember]
        public string Did { get; set; } 
        [DataMember]
        public string Fid { get; set; }
        [DataMember]
        public string Tid { get; set; } 

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

       

        [DataMember]
        public List<PersonDetails> Fs { get; set; }

        [DataMember]
        public List<Mtp> Mtp { get; set; }

        [DataMember]
        public string Ffp { get; set; } 
        [DataMember]
        public string Ltsize { get; set; } 
        [DataMember]
        public string Lfp { get; set; }

        [DataMember]
        public string I_fn { get; set; }

        [DataMember]
        public List<string> Obase_64 { get; set; }

        [DataMember]
        public List<string> Img_url { get; set; }

        [DataMember]
        public List<List<string>> Prompt { get; set; }

        [DataMember]
        public string Hp { get; set; }

        #endregion


    }

    [DataContract]
    public class MaskDetectorAPIResMsg
    {
        [DataMember]
        public BoundingBox Dm { get; set; } 
        [DataMember]
        public string Cs { get; set; } 

        #region New changes for IVA request/response structure
        [DataMember]
        public string Lb { get; set; } 
        #endregion
    }

    [DataContract]
    public class ResponseStatus
    {
        [DataMember]
        public int Rc { get; set; } 
        [DataMember]
        public string Rm { get; set; } 

    }

    [DataContract]
    public class Mtp
    {
        [DataMember]
        public string Etime { get; set; }

        [DataMember]
        public string Src { get; set; }

        [DataMember]
        public string Stime { get; set; }
    }

    [DataContract]
    public class BoundingBox
    {
        [DataMember]
        public string X { get; set; } 
        [DataMember]
        public string Y { get; set; } 
        [DataMember]
        public string H { get; set; } 
        [DataMember]
        public string W { get; set; } 
    }

    [DataContract]
    public class PersonDetails
    {
        [DataMember]
        public BoundingBox Dm { get; set; } 
        [DataMember]
        public string Pid { get; set; } 
        [DataMember]
        public string Np { get; set; }
        [DataMember]
        public string Cs { get; set; } 
        

        #region New changes for IVA request/response structure
        [DataMember]
        public string Lb { get; set; } 

        [DataMember]
        public string Uid { get; set; }

        [DataMember]
        public string Nobj { get; set; }

        [DataMember]
        public string Info { get; set; }

        [DataMember]

        public Dictionary<int, List<float>> Kp { get; set; }
        [DataMember]
        public List<List<float>> Tpc { get; set; }
        [DataMember]
        public List<List<float>> Bpc { get; set; }
        [DataMember]
        public string TaskType { get; set; }

       
        #endregion
    }
    [DataContract]
    public class Ad
    {
        [DataMember]
        public int count { get; set; }
        [DataMember]
        public List<List<float>> points { get; set; }
    }
    [DataContract]

    public class points
    {
        [DataMember]
        public List<string> point{ get; set; }
    }

    [DataContract]
    public class Info
    {
        public Dictionary<string, List<float>> Keypoints { get; set; }
        public List<List<int>> Kp_skeleton { get; set; }
    }



    [DataContract]
    public class PersonCountAPIResMsg
    {
        [DataMember]
        public PersonDetails[] Fs { get; set; } 
        [DataMember]
        public string Tid { get; set; } 
        [DataMember]
        public string Did { get; set; } 
        [DataMember]
        public string Fid { get; set; } 
        [DataMember]
        public string Rc { get; set; } 
        [DataMember]
        public string Rm { get; set; } 
        [DataMember]
        public string Class { get; set; } 
        

    }

    [DataContract]
    public class ObjectDetectorAPIResMsgExp
    {
        
        [DataMember]
        public int Rc { get; set; } 
        [DataMember]
        public string Rm { get; set; }

        /**/
        #region New changes for IVA request/response structure 

        [DataMember]
        public string Did { get; set; } 
        [DataMember]
        public List<string> Fid { get; set; } 
        [DataMember]
        public string Tid { get; set; } 

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

        

        [DataMember]
        public List<PersonDetails> Fs { get; set; }

        [DataMember]
        public List<Mtp> Mtp { get; set; }

        [DataMember]
        public string Ffp { get; set; } 
        [DataMember]
        public string Ltsize { get; set; } 
        [DataMember]
        public string Lfp { get; set; }

        [DataMember]
        public string I_fn { get; set; }

        [DataMember]
        public List<string> Obase_64 { get; set; }

        [DataMember]
        public List<string> Img_url { get; set; }

        [DataMember]
        
        public List<Dictionary<string, Dictionary<string, string>>> Explainer_Metadata { get; set; }

        [DataMember]
        public List<string> ImageUrls { get; set; }
        [DataMember]
        public string ExplainerToRun { get; set; }
        [DataMember]
        public string ModelName { get; set; }
        [DataMember]
        public string ExpVersion { get; set; }
        #endregion
        [DataMember]
        public string Hp { get; set; }

    }


}
