/*=============================================================================================================== *
 * Copyright 2024 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/


using System;
using Infosys.Solutions.Ainauto.VideoAnalytics.Infrastructure.Common;
using DE = Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;
using BE = Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity.Queue;
using Newtonsoft.Json;
using Infosys.Solutions.Ainauto.VideoAnalytics.Services.MaskDetector.Contracts.Message;
using System.Linq;
using System.ComponentModel;
using Infosys.Solutions.Ainauto.VideoAnalytics.Resource.Entity.Queue;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity
{
    public class FaceMaskTranslator
    {

        public static BE.FrameProcessorMetaData FaceMaskDEToBE(DE.FrameProcessorMetaData message)
        {
            BE.FrameProcessorMetaData beMessage = new BE.FrameProcessorMetaData();

            if (message != null)
            {
                beMessage.Tid = message.Tid;
                beMessage.Did = message.Did;
                beMessage.Fid = message.Fid;
                beMessage.Sbu = message.Sbu;
                beMessage.Mod = message.Mod;
                beMessage.TE = message.TE;
                beMessage.FeedId = message.FeedId;
                beMessage.Fids = message.Fids;
                beMessage.SequenceNumber = message.SequenceNumber;
                beMessage.FrameNumber = message.FrameNumber;
                beMessage.Src = message.Src;
                beMessage.Stime = message.Stime;
                beMessage.Etime = message.Etime;
                
                beMessage.Ffp = message.Ffp;
                beMessage.Ltsize=message.Ltsize;
                beMessage.Lfp = message.Lfp;
                beMessage.videoFileName = message.videoFileName;
                beMessage.Pcd=message.Pcd;
                beMessage.Msk_img = message.Msk_img;
                beMessage.Rep_img = message.Rep_img;
                beMessage.Prompt = message.Prompt;
            }

            return beMessage;
        }

        public static DE.FrameRendererMetadata FaceMaskRendererBEToDE(string predictionMessage,DE.FrameProcessorMetaData dataMessage) {
            DE.FrameRendererMetadata deMessage=new DE.FrameRendererMetadata();
            var msg=JsonConvert.DeserializeObject<ObjectDetectorAPIResMessage>(predictionMessage);
            if(msg!=null) {
                deMessage.Fs=new DE.Predictions[msg.Fs.Count];
                for(var i=0;i<msg.Fs.Count;i++) {
                    deMessage.Fs[i]=new();
                    if(msg.Fs[i].Dm!=null) { 
                        if(msg.Fs[i].Dm.X!=null && msg.Fs[i].Dm.Y!=null && msg.Fs[i].Dm.W!=null && msg.Fs[i].Dm.H!=null) {
                            deMessage.Fs[i].Dm=msg.Fs[i].Dm;
                            /* Map other properties */
                            deMessage.Fs[i].Dm.X=msg.Fs[i].Dm.X;
                            deMessage.Fs[i].Dm.Y=msg.Fs[i].Dm.Y;
                            deMessage.Fs[i].Dm.W=msg.Fs[i].Dm.W;
                            deMessage.Fs[i].Dm.H=msg.Fs[i].Dm.H;
                        }
                    }
                    /* Add Kp value */
                    if(msg.Fs[i].Kp!=null) {
                        deMessage.Fs[i].Kp=msg.Fs[i].Kp;
                    }
                    if(msg.Fs[i].Tpc!=null) {
                        deMessage.Fs[i].Tpc=msg.Fs[i].Tpc;
                    }
                    if(msg.Fs[i].Bpc!=null) {
                        deMessage.Fs[i].Bpc=msg.Fs[i].Bpc;
                    }
                    deMessage.Fs[i].Info=msg.Fs[i].Info;
                    deMessage.Fs[i].Cs=msg.Fs[i].Cs;
                    deMessage.Fs[i].NoObj=msg.Fs[i].Nobj;
                    deMessage.Fs[i].Uid=msg.Fs[i].Uid;
                    deMessage.Fs[i].Lb=msg.Fs[i].Lb;
                }
                deMessage.Tid=msg.Tid;
                deMessage.Did=msg.Did;
                deMessage.Fid=msg.Fid;
                deMessage.Pts=DateTime.UtcNow.Ticks.ToString();
                deMessage.TE=dataMessage.TE;
                deMessage.Fids=dataMessage.Fids;
                deMessage.FeedId=dataMessage.FeedId;
                deMessage.SequenceNumber=dataMessage.SequenceNumber;
                deMessage.FrameNumber=dataMessage.FrameNumber;
                /* Added for identifying First and Last frame in Payload: Yoges Govindaraj */
                deMessage.Ffp=dataMessage.Ffp;
                deMessage.Ltsize=dataMessage.Ltsize;
                deMessage.Lfp=dataMessage.Lfp;
                deMessage.Ad=msg.Ad;
                deMessage.videoFileName=msg.I_fn;
                deMessage.Status=msg.Rm;
                deMessage.Mtp=msg.Mtp;
                deMessage.Obase_64=msg.Obase_64;
                deMessage.Img_url=msg.Img_url;
            }
            
                
            return deMessage;
        }


        public static DE.FrameCollectorMetadata FaceMaskCollectorBEToDE(string message, DE.FrameProcessorMetaData message1)
        {

            DE.FrameCollectorMetadata deMessage = new DE.FrameCollectorMetadata();

            var msg = JsonConvert.DeserializeObject<ObjectDetectorAPIResMessage>(message);
            deMessage.Fs = new DE.Predictions[msg.Fs.Count];

            if (message != null)
            {
                
                for (var i = 0; i < msg.Fs.Count; i++)
                {
                    deMessage.Fs[i] = new();

                    if (msg.Fs[i].Dm.X != null && msg.Fs[i].Dm.Y != null && msg.Fs[i].Dm.W != null && msg.Fs[i].Dm.H != null)
                    {
                        deMessage.Fs[i].Dm = msg.Fs[i].Dm;
                        
                        deMessage.Fs[i].Dm.X = msg.Fs[i].Dm.X;
                        deMessage.Fs[i].Dm.Y = msg.Fs[i].Dm.Y;
                        deMessage.Fs[i].Dm.W = msg.Fs[i].Dm.W;
                        deMessage.Fs[i].Dm.H = msg.Fs[i].Dm.H;
                    }

                    
                    if (msg.Fs[i].Kp != null)
                    {
                        deMessage.Fs[i].Kp = msg.Fs[i].Kp;
                    }

                    if (msg.Fs[i].Tpc != null)
                    {
                        deMessage.Fs[i].Tpc = msg.Fs[i].Tpc;
                    }
                    if (msg.Fs[i].Bpc != null)
                    {
                        deMessage.Fs[i].Bpc = msg.Fs[i].Bpc;
                    }

                    deMessage.Fs[i].Info = msg.Fs[i].Info;
                    deMessage.Fs[i].Cs = msg.Fs[i].Cs;
                    deMessage.Fs[i].NoObj = msg.Fs[i].Nobj;
                    deMessage.Fs[i].Uid = msg.Fs[i].Uid;
                    deMessage.Fs[i].Lb = msg.Fs[i].Lb;

                }

                deMessage.Tid = message1.Tid;
                deMessage.Did = message1.Did;
                deMessage.Fid = message1.Fid;
                deMessage.Pts = DateTime.Now.Ticks.ToString();
               
            }

            return deMessage;
        }

        public static BE.PersonCountMetaData PersonCountDEToBE(DE.PersonCountMetaData message)
        {
            BE.PersonCountMetaData beMessage = new BE.PersonCountMetaData();

            if (message != null)
            {
                beMessage.Tid = message.Tid;
                beMessage.Did = message.Did;
                beMessage.Fid = message.Fid;
                beMessage.Mod = message.Mod;
                beMessage.TE = message.TE;
                beMessage.FeedId = message.FeedId;
                beMessage.SequenceNumber = message.SequenceNumber;
                beMessage.FrameNumber = message.FrameNumber;                   
                            
      
           
                beMessage.Fids = message.Fids;
              
               
                beMessage.Src = message.Src;
                beMessage.Stime = message.Stime;
                beMessage.Etime = message.Etime;
                
                beMessage.Ffp = message.Ffp;
                beMessage.Ltsize = message.Ltsize;
                beMessage.Lfp = message.Lfp;
            }

            return beMessage;
        }

        public static DE.FrameRendererMetadata PersonCountBEToDE(string predictionMessage,DE.PersonCountMetaData dataMessage) {
            DE.FrameRendererMetadata deMessage=new DE.FrameRendererMetadata();
            
            var msg=JsonConvert.DeserializeObject<ObjectDetectorAPIResMessage>(predictionMessage);
            if(msg!=null) {
                deMessage.Fs=new DE.Predictions[msg.Fs.Count];
                for(var i=0;i<msg.Fs.Count;i++) {
                    deMessage.Fs[i]=new();
                    if(msg.Fs[i].Dm.X!=null && msg.Fs[i].Dm.Y!=null && msg.Fs[i].Dm.W!=null && msg.Fs[i].Dm.H!=null) {
                        deMessage.Fs[i].Dm=msg.Fs[i].Dm;
                        /* Map other properties */
                        deMessage.Fs[i].Dm.X=msg.Fs[i].Dm.X;
                        deMessage.Fs[i].Dm.Y=msg.Fs[i].Dm.Y;
                        deMessage.Fs[i].Dm.W=msg.Fs[i].Dm.W;
                        deMessage.Fs[i].Dm.H=msg.Fs[i].Dm.H;
                    }
                    deMessage.Fs[i].Info=msg.Fs[i].Info;
                    deMessage.Fs[i].Cs=msg.Fs[i].Cs;
                    deMessage.Fs[i].NoObj=msg.Fs[i].Nobj;
                    deMessage.Fs[i].Uid=msg.Fs[i].Uid;
                    deMessage.Fs[i].Lb=msg.Fs[i].Lb;
                }
                deMessage.Tid=msg.Tid;
                deMessage.Did=msg.Did;
                deMessage.Fid=msg.Fid;
                deMessage.Pts=DateTime.UtcNow.Ticks.ToString();
                deMessage.TE=dataMessage.TE;
                deMessage.Fids=dataMessage.Fids;
                deMessage.FeedId=dataMessage.FeedId;
                deMessage.SequenceNumber=dataMessage.SequenceNumber;
                deMessage.FrameNumber=dataMessage.FrameNumber;
                deMessage.Ffp=dataMessage.Ffp;
                deMessage.Ltsize=dataMessage.Ltsize;
                deMessage.Lfp=dataMessage.Lfp;
                deMessage.Ad=msg.Ad;
                deMessage.videoFileName=msg.I_fn;
                deMessage.Status=msg.Rm;
                deMessage.Mtp=msg.Mtp;
                deMessage.Obase_64=msg.Obase_64;
                deMessage.Img_url=msg.Img_url;
            }
            return deMessage;
        }


        public static DE.FrameRendererMetadata PersonCountFailureBEToDE(DE.PersonCountMetaData message)
        {
            DE.FrameRendererMetadata deMessage = new DE.FrameRendererMetadata();
            deMessage.Tid = message.Tid;
            deMessage.Did = message.Did;
            deMessage.Fid = message.Fid;
            deMessage.Pts = DateTime.UtcNow.Ticks.ToString();
            deMessage.Fs = new DE.Predictions[0];
            deMessage.SequenceNumber = message.SequenceNumber;
            deMessage.Status = ApplicationConstants.ProcessingStatus.FailedToPredict;
            deMessage.FrameNumber = message.FrameNumber;
            return deMessage;
        }
    }
}
