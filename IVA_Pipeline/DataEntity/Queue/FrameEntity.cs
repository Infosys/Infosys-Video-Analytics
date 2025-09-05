/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SE = Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue
{
    public class QueueMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; }
        public string Sbu { get; set; }
        public string Mod { get; set; } 
        public string Pts { get; set; } 
        public List<Predictions> Fs { get; set; } 
        public string Pid { get; set; } 
        public string Np { get; set; }
    }

    public class FrameProcessorMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Sbu { get; set; } 
        public string Mod { get; set; }
        public string FeedId { get; set; } 
        public string Fp { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public List<string> Fids { get; set; } 
        public string SequenceNumber { get; set; } 
        public string FrameNumber { get; set; } 
        public string Src { get; set; }
        public List<Mtp> Mtp { get; set; }
        public string Stime { get; set; }
        public string Etime { get; set; }

        public string Ffp { get; set; } 

        public string Ltsize { get; set; }

        public string Lfp { get; set; }

        public string LastFrame { get; set; } 

        public string Ad { get; set; }

        public string videoFileName { get; set; }
        public string Prompt { get; set; }
        public List<string> Msk_img { get; set; }
        public List<string> Rep_img { get; set; }
        public byte[] Pcd { get; set; }
        public List<PersonDetails> Fs { get; set; } 
        public string Hp { get; set; }
    }

    public class AzureCustomVisionMetaData
    {
        public double probability { get; set; } 
        public string tagId { get; set; } 
        public string tagName { get; set; } 
        public AzureCustomVisionBoundingBox boundingBox { get; set; } 
    }
    public class AzureCustomVisionBoundingBox
    {
        public double left { get; set; } 
        public double top { get; set; } 
        public double height { get; set; } 
        public string width { get; set; } 
    }

    public class FrameGrabberMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; }
        public string Fid { get; set; } 
        public string Sbu { get; set; }
        public string Mod { get; set; } 
        public string FeedId { get; set; }
        public string Fp { get; set; } 
        public Dictionary<string,List<string>> TE { get; set; } 
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
        public string Prompt { get; set; }
        public List<string> Msk_img { get; set; }
        public List<string> Rep_img { get; set; }
        public byte[] Pcd { get; set; }
        public string Hp { get; set; }

    }

    public class PcdHandlerMetaData {
        public string Tid {get;set;} 
        public string Did {get;set;} 
        public string Fid {get;set;} 
        public string Sbu {get;set;} 
        public string Mod {get;set;} 
        public string FeedId {get;set;} 
        public string Fp {get;set;} 
        public Dictionary<string,List<string>> TE {get;set;} 
        public List<string> Fids {get;set;} 
        public string SequenceNumber {get;set;} 
        public string FrameNumber {get;set;} 
        public string Etime {get;set;}
        public string Src {get;set;}
        public string Stime {get;set;}
        public string Ffp {get;set;} 
        public string Ltsize {get;set;} 
        public string Lfp {get;set;} 
        public string videoFileName {get;set;}
        public List<string> Msk_img {get;set;}
        public List<string> Rep_img {get;set;}
        public byte[] Pcd {get;set;}
        public string Hp { get; set; }
    }

    public class FrameRendererMetadata
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Pts { get; set; } 
        public string Class { get; set; } 
        public string Np { get; set; } 
        public Predictions[] Fs { get; set; }
        public string FeedId { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public List<string> Fids { get; set; } 
        public string SequenceNumber { get; set; }
        public string Status { get; set; }

        public string FrameNumber { get; set; } 

        public string Info { get; set; }
        public string Ffp { get; set; } 

        public string Ltsize { get; set; } 

        public string Lfp { get; set; }

        public string Ad { get; set; }

        public string videoFileName { get; set; }

        public List<Queue.Mtp> Mtp { get; set; }

        public List<string> Obase_64 { get; set; }
        public List<string> Img_url { get; set; }
        public string Prompt { get; set; }
        public string Sbu { get; set; } 
        public string Fp { get; set; } 
        public string Hp { get; set; }

    }

    public class ObjectDetectorAPIResMessage {
        
        public List<MaskDetectorAPIResMessage> Objects {get;set;}
        [DataMember]
        public int Rc {get;set;} 
        [DataMember]
        public string Rm {get;set;}

        #region New changes for IVA request/response structure 
        [DataMember]
        public string Did {get;set;} 
        [DataMember]
        public string Fid {get;set;} 
        [DataMember]
        public string Tid {get;set;} 
        [DataMember]
        public string Ts {get;set;}
        [DataMember]
        public string Ts_ntp {get;set;}
        [DataMember]
        public string Msg_ver {get;set;}
        [DataMember]
        public string Inf_ver {get;set;}
        [DataMember]
        public string Ad {get;set;}
        
        [DataMember]
        public List<PersonDetails> Fs {get;set;}
        [DataMember]
        public List<Mtp> Mtp {get;set;}
        [DataMember]
        public string I_fn {get;set;}
        #endregion

        [DataMember]
        public List<string> Obase_64 {get;set;}
        [DataMember]
        public List<string> Img_url {get;set;}
        [DataMember]
        public List<List<string>> Prompt { get;set;}

        [DataMember]
        public string Hp { get; set; }
    }

    [DataContract]
    public class MaskDetectorAPIResMessage
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

    public class FrameCollectorMetadata {
        public string Tid {get;set;} 
        public string Did {get;set;} 
        public string Fid {get;set;}
        public string Pts {get;set;} 
        public string Class {get;set;} 
        public string Np {get;set;} 
        public List<Predictions> Fs {get;set;} 
        public string FeedId {get;set;} 
        public Dictionary<string, List<string>> TE {get;set;} 
        public List<string> Fids {get;set;} 
        public string Status {get;set;} 
        public string SequenceNumber {get;set;} 
        public string FrameNumber {get;set;} 
        public string videoFileName {get;set;}
        public List<string> Obase_64 {get;set;}
        public List<string> Img_url {get;set;}
        public string Prompt { get; set; }
        public List<Dictionary<string, Dictionary<string, string>>> Explainer_Metadata { get; set; }
        public string Ad { get; set; }
        public List<SE.Mtp> Mtp { get; set; }
        public string ModelName { get; set; }
        public List<string> ExpToRun { get; set; }
        public string ExpVer { get; set; }
        public string Ffp { get; set; }
        public string Lfp { get; set; }
        public string Hp { get; set; }
    }

    public class Predictions_old
    {
        public BoundingBox Dm { get; set; } 
        public string Cs { get; set; }
        public string Lb { get; set; } 
        public string Pid { get; set; }
        public string Np { get; set; }     
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
        public string TaskType { get; set; }  

        

                


        
    }

    public class Mtp
    {
        [DataMember]
        public string Etime { get; set; }

        [DataMember]
        public string Src { get; set; }

        [DataMember]
        public string Stime { get; set; }
    }

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

    
    #region New changes for IVA request/response structure
    public class MaskDetectorAPIResMsgEntity
    {

        public BoundingBox Dm { get; set; }

        public string Cs { get; set; } 


        public string Lb { get; set; } 

    }
    #endregion

    public class BoundingBox
    {
        public string X { get; set; } 
        public string Y { get; set; } 
        public string H { get; set; }
        public string W { get; set; } 
        
    }

    public class FramePreloaderMetadata

    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string FeedId { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public string SequenceNumber { get; set; }

        public string FrameNumber { get; set; } 
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

        public List<string> Fids { get; set; }

        public string Fp { get; set; } 



        
        public string Etime { get; set; }
        public string Src { get; set; }
        public string Stime { get; set; }
        public string Ad { get; set; }

        public string Ffp { get; set; } 

        public string Ltsize { get; set; } 

        public string Lfp { get; set; }
        public string videoFileName { get; set; }
        public string Hp { get; set; }
    }


    public class PersonCountQueueMsg
    {
        public PersonDetails[] Fs { get; set; } 
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Class { get; set; } 
        public string FeedId { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public string SequenceNumber { get; set; } 
        public string Status { get; set; } 

        public string FrameNumber { get; set; } 

    }

    public class MetricIngestorMetadata

    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; }
        public string Pts { get; set; } 
        public List<Predictions> Fs { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public string SequenceNumber { get; set; } 

        public string FrameNumber { get; set; }
    }


    public class Metrics
    {
        public string Application { get; set; }
        public string Description { get; set; }
        public string EventType { get; set; }
        public string MetricName { get; set; }
        public string MetricTime { get; set; }
        public string MetricValue { get; set; }
        public string ResourceId { get; set; }
        public string SequenceNumber { get; set; }
        public string ServerIp { get; set; }
        public string Source { get; set; }
        public string FrameNumber { get; set; }
    }

    public class MetricData
    {
        public List<Metrics> MetricMessages { get; set; }
    }

    public class MaintenanceMetaData
    {
        public string MessageId { get; set; }
        public string MessageType { get; set; } 
        public DateTime Timestamp { get; set; } 
        public string SenderId { get; set; } 
        public string ResourceId { get; set; } 
        public string Source { get; set; }
        public string EventType { get; set; } 
        public string Data { get; set; } 
        public string Tid { get; set; } 
        public string Did { get; set; } 

    }

    public class FrameInformation
    {
        public string TID { get; set; }
        public string DID { get; set; } 
        public string TotalFrameCount { get; set; }  
        public string LastFrameNumberSendForPrediction { get; set; } 
        public string FeedId { get; set; } 
        public string TotalMessageSendForPrediction { get; set; }

        public Dictionary<int, string> FramesNotSendForRendering { get; set; }  
        public string Model { get; set; }

    }

    public class FrameElasticSearchMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Pts { get; set; } 
        public string Class { get; set; }
        public string Np { get; set; } 
        public List<Predictions> Fs { get; set; } 
        public string FeedId { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public List<string> Fids { get; set; } 
        public string Status { get; set; } 
        public string SequenceNumber { get; set; }

        public string FrameNumber { get; set; } 

        public string videoFileName { get; set; }

        public List<Index.Mtp> Mtp { get; set; }
    }

    public class PromptHandlerMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Sbu { get; set; } 
        public string Mod { get; set; } 
        public string FeedId { get; set; } 
        public string Fp { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public List<string> Fids { get; set; } 
        public string SequenceNumber { get; set; }
        public string FrameNumber { get; set; } 
        public string Etime { get; set; }
        public string Src { get; set; }
        public string Stime { get; set; }
        public string Mtp { get; set; }
        public string Ffp { get; set; } 
        public string Ltsize { get; set; } 
        public string Lfp { get; set; }
        public string videoFileName { get; set; }
        public string Prompt { get; set; }
        public List<string> Msk_img { get; set; }
        public List<string> Rep_img { get; set; }
        public string Hp { get; set; }
    }

    public class PromptInjectorMetaData
    {
        public string Prompt { get; set; }
        public string DeviceId { get; set; }
        public string HyperParameters { get; set; }
    }
    public class FrameExplainerModeMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        public string Fid { get; set; } 
        public string Pts { get; set; } 
        public string Class { get; set; } 
        public string Np { get; set; } 
        public List<Predictions> Fs { get; set; }
        public string FeedId { get; set; } 
        public Dictionary<string, List<string>> TE { get; set; } 
        public List<string> Fids { get; set; } 
        public string Status { get; set; }
        public string SequenceNumber { get; set; } 

        public string FrameNumber { get; set; } 

        public string videoFileName { get; set; }

        public List<Index.Mtp> Mtp { get; set; }

        public string Ad { get; set; } 
        public string ExpToRun { get; set; }

        public string I_Fn { get; set; }
        public string ExpVer { get; set; }
        public string ModelName { get; set; }    
        public List<string> Obase_64 { get; set; }
        public List<string> Img_url { get; set; }
        public List<List<string>> Prompt { get; set; }
        public List<Dictionary<string, Dictionary<string, string>>> Explainer_Metadata { get; set; }
        public string Ffp { get; set; } 
        public string Lfp { get; set; }
        public string Hp { get; set; }
    }

    public class TemplateAttributes
    {
        public string AttributeName { get; set; }

        public string AttributeCondition { get; set; }
        public string AttributeValue { get; set; }
        public string CompareAttributeValue { get; set; }
        public string AttributeComparison { get; set; }

    }
    public class SensorMetaData
    {
        public string Tid { get; set; } 
        public string Did { get; set; } 
        
        public SensorData Metadata { get; set; }
        public string Ts { get; set; }
        public string Msg_ver { get; set; } 
        
        public string Status { get; set; }
       

    }

    public class SensorData
    {
        public Double Temperature { get; set; }
    }

    



}
