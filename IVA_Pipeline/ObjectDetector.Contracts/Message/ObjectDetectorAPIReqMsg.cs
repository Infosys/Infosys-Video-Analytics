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
    public class ObjectDetectorAPIReqMsg
    {
        

        
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
        public string Model { get; set; }
        [DataMember]
        public string Base_64 { get; set; }  
        [DataMember]
        public float C_threshold { get; set; } 
        [DataMember]
        public List<Mtp> Mtp { get; set; }
        [DataMember]
        public List<Per> Per { get; set; }
        [DataMember]
        public string Ad { get; set; }
       
        [DataMember]
        public string Ffp { get; set; } 
        [DataMember]
        public string Ltsize { get; set; } 
        [DataMember]
        public string Lfp { get; set; }
        [DataMember]
        public string I_fn { get; set; }
        [DataMember]
        public List<string> Msk_img { get; set; }
        [DataMember]
        public List<string> Rep_img { get; set; }
        [DataMember]
        public List<List<string>> Prompt { get; set; }
        [DataMember]
        public List<PersonDetails> Fs { get; set; }
        [DataMember]
        public string Hp { get; set; }
    }

    [DataContract]
    public class Per
    {
        [DataMember]
        public string Fid { get; set; }
        [DataMember]
        public CartPredictions[] Fs { get; set; }
        
    }
    [DataContract]
    public class Fs
    {
        [DataMember]
        public string Lb { get; set; }
        [DataMember]
        public Dm Dm { get; set; }
        [DataMember]
        public string Uid { get; set; }
        [DataMember]
        public string Nobj { get; set; }
        [DataMember]
        public string Cs { get; set; }

    }

    [DataContract]
    public class Dm
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
    public class ObjectDetectorImageBytesMsgAICloud
    {
        [DataMember]
        public ObjectDetectorAPIReqMsg  image_bytes { get; set; }
       
    }

    [DataContract]
    public class ObjectDetectorAPIReqMsgAICloud
    {
        [DataMember]
        public List<ObjectDetectorImageBytesMsgAICloud> instances { get; set; }

    }

    [DataContract]
    public class PersonCountAPIReqMsg
    {
        [DataMember]
        public string Tid { get; set; }
        [DataMember]
        public string Did { get; set; }
        [DataMember]
        public string Fid { get; set; }
        [DataMember]
        public string Cs { get; set; }
        [DataMember]
        public string Tok { get; set; }
        [DataMember]
        public PersonCount[] Per { get; set; }
        [DataMember]
        public string Base64_image { get; set; }  
    }

    [DataContract]
    public class PersonCount
    {
        [DataMember]
        public string Class { get; set; } 
        [DataMember]
        public string info { get; set; }
        [DataMember]
        public string Fid { get; set; }
        [DataMember]
        public PersonAndBounding[] Fs { get; set; } 
    }

    [DataContract]
    public class PersonAndBounding
    {
        [DataMember]
        public string Class { get; set; } 
        [DataMember]
        public BoundingBox Dm { get; set; } 
        [DataMember]
        public string Pid { get; set; }
    }

    [DataContract]
    public class LicensePlateDetectorAPIReqMsg
    {
        [DataMember]
        public string base64_image { get; set; }
        
    }

    [DataContract]
    public class CartPredictions
    {
        [DataMember]
        public BoundingBox Dm { get; set; } 
        [DataMember]
        public string Cs { get; set; } 
        [DataMember]
        public string Lb { get; set; } 
        [DataMember]
        public string Info { get; set; }
        [DataMember]
        public string NoObj { get; set; }
        [DataMember]
        public string Uid { get; set; }
        [DataMember]
        public string Pid { get; set; } 
        [DataMember]
        public string Np { get; set; }     
        [DataMember]
        public Dictionary<int, List<float>> Kp { get; set; }
        [DataMember]
        public string TaskType { get; set; }

    }
    [DataContract]
    public class CrowdCounting
    {
        [DataMember]
        public int[] x { get; set; }

        [DataMember]
        public int[] y { get; set; }
        [DataMember]
        public string Base_64 { get; set; }
    }

    [DataContract]
    public class ObjectDetectorAPIReqMsgExp
    {
       
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
        public string Model { get; set; }
        [DataMember]
        public List<string> Base_64 { get; set; }  
        [DataMember]
        public string C_threshold { get; set; }   



        [DataMember]
        public List<Mtp> Mtp { get; set; }
        [DataMember]
        public List<Per> Per { get; set; }
        [DataMember]
        public string Ad { get; set; }
        

        [DataMember]
        public string Ffp { get; set; } 
        [DataMember]
        public string Ltsize { get; set; } 
        [DataMember]
        public string Lfp { get; set; }

        [DataMember]
        public string I_fn { get; set; }

        [DataMember]
        public List<string> Msk_img { get; set; }

        [DataMember]
        public List<string> Rep_img { get; set; }

        [DataMember]
        public List<List<string>> Prompt { get; set; }
        [DataMember]
        public byte[] Pcd { get; set; }
        [DataMember]
        public string Exp_api_ver { get; set; }
        [DataMember]
        
        public List<string> Explainers_to_run { get; set; }
        [DataMember]
        public Dictionary<string, List<string>> TE { get; set; } 
        [DataMember]
        public  string Explainer_url { get; set; } 
    }
}
